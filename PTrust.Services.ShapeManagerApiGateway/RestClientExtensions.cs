using System;
using System.Diagnostics;
using System.Text;

using Polly;
using RestSharp;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public static class RestClientExtensions
    {
        private const int WaitAndRetryDelayTimespan = 250;

        public static IRestResponse<T> ExecutePostWithRetry<T>(this IRestClient restClient, string uri, object body, int? timeOutInSeconds = null) where T : new()
        {
            var request = CreatePostRequest(uri, body, timeOutInSeconds);

            var response = Policy.Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromMilliseconds(1 * WaitAndRetryDelayTimespan),
                    TimeSpan.FromMilliseconds(2 * WaitAndRetryDelayTimespan),
                    TimeSpan.FromMilliseconds(3 * WaitAndRetryDelayTimespan)
                }, (ex, timespan, retryCount, context) => { Debug.WriteLine($"Request for {uri} failed. Executing retry #{retryCount}.", ex); })
                .Execute(() => restClient.ExecutePost<T>(request, uri, body));

            response.ThrowIfError(restClient, request);

            return response;
        }

        public static IRestResponse<T> ExecutePost<T>(this IRestClient restClient, string uri, object body, int? timeOutInSeconds = null) where T : new()
        {
            var request = CreatePostRequest(uri, body, timeOutInSeconds);
            return restClient.ExecutePost<T>(request, uri, body);
        }

        public static T ExecuteGet<T>(this IRestClient restClient, string uri) where T : new()
        {
            var request = new RestRequest(uri, Method.GET);

            var sw = Stopwatch.StartNew();
            var response = restClient.ExecuteRestRequest<T>(request);

            Debug.WriteLine($"Query for {uri} took {sw.ElapsedMilliseconds}ms.");
            return response.Data;
        }

        private static IRestResponse<T> ExecutePost<T>(this IRestClient restClient, IRestRequest request, string requestUri, object body) where T : new()
        {
            var sw = Stopwatch.StartNew();
            var response = restClient.ExecuteRestRequest<T>(request);
            Debug.WriteLine($"Post to {requestUri} of type {body.GetType().Name} took {sw.ElapsedMilliseconds}ms.");
            return response;
        }

        private static IRestRequest CreatePostRequest(string requestUri, object body, int? timeOutInSeconds)
        {
            var request = new RestRequest(requestUri)
            {
                Method = Method.POST,
                JsonSerializer = NewtonsoftJsonSerializer.Default
            };
            request.AddJsonBody(body);
            request.RequestFormat = DataFormat.Json;

            //if timeout is overridden, apply it.  Otherwise leave it as default.
            if (timeOutInSeconds.HasValue)
            {
                request.Timeout = timeOutInSeconds.Value * 1000;
            }

            return request;
        }

        // Simple Wrapper that calls ThrowIfError().
        private static IRestResponse<T> ExecuteRestRequest<T>(this IRestClient client, IRestRequest request) where T : new()
        {
            var response = client.Execute<T>(request);
            response.ThrowIfError(client, request);
            return response;
        }

        private static bool IsSuccessStatusCode(this IRestResponse restResponse)
        {
            return (int)restResponse.StatusCode < 400;
        }

        private static void ThrowIfError(this IRestResponse restResponse, IRestClient client, IRestRequest request)
        {
            if (restResponse.IsSuccessStatusCode())
            {
                return;
            }

            var sb = new StringBuilder($"Processing request {client.BuildUri(request)} resulted in errors.  ");

            if (restResponse.ErrorException != null)
            {
                sb.Append(restResponse.ErrorException.Message);
            }
            else
            {
                sb.Append("Raw error content: " + restResponse.Content);
            }

            throw new Exception(sb.ToString());
        }
    }
}