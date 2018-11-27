using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class ShapeManagerRequestHandler : DelegatingHandler
    {
        private readonly IPtLogger _ptLogger;

        private readonly IRestClientFactory _restClientFactory;

        public ShapeManagerRequestHandler(IPtLogger ptLogger, IRestClientFactory restClientFactory)
        {
            _ptLogger = ptLogger;
            _restClientFactory = restClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IRestClient client;
            IRestResponse<SmsRouteResponse> response;
            SmsRouteResponse routeResponse;
            UriBuilder uriBuilder;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _ptLogger.LogInfo($"{request.RequestUri.AbsoluteUri}: Request processing started");

            // GET
            if (request.Method == HttpMethod.Get)
            {
                GetDefaultRoute(request);

                stopwatch.Stop();
                _ptLogger.LogInfo($"{request.Method} request routed to {request.RequestUri.AbsoluteUri} in {stopwatch.ElapsedMilliseconds} ms");

                return await base.SendAsync(request, cancellationToken);
            }

            // Other than GET
            var requestContent = request.Content.ReadAsStringAsync().Result;
            if (requestContent == null)
            {
                GetDefaultRoute(request);

                stopwatch.Stop();
                _ptLogger.LogInfo($"{request.Method} request content is blank: {request.RequestUri.AbsoluteUri}. Routed to default route in {stopwatch.ElapsedMilliseconds} ms");
                return await base.SendAsync(request, cancellationToken);
            }

            dynamic requestObject = JsonConvert.DeserializeObject<object>(requestContent);
            if (requestObject == null)
            {
                GetDefaultRoute(request);

                stopwatch.Stop();
                _ptLogger.LogInfo($"{request.Method} request has no parameter(s): {request.RequestUri.AbsoluteUri}. Routed to default route in {stopwatch.ElapsedMilliseconds} ms");
                return await base.SendAsync(request, cancellationToken);
            }

            var requestKeyValueList = GetRequestPropertyKeys(requestObject);

            // Request may not have a username
            if (!requestKeyValueList.Contains("UserName"))
            {
                GetDefaultRoute(request);

                stopwatch.Stop();
                _ptLogger.LogInfo($"{request.Method} request is missing a UserName: {request.RequestUri.AbsoluteUri}. Routed to default route in {stopwatch.ElapsedMilliseconds} ms");

                return await base.SendAsync(request, cancellationToken);
            }

            // For now partition request by JobId
            // Else partition by username only
            var userName = requestObject.UserName;
            var routeElements = new StringBuilder(userName.ToString());

            int? id = null;
            if (requestKeyValueList.Contains("JobId"))
            {
                id = requestObject.JobId;
                routeElements.Append($"|{id.ToString()}");
            }

            // Get route by username and Id if available
            client = _restClientFactory.GetShapeManagerRouterServiceClient();
            response = client.ExecutePostWithRetry<SmsRouteResponse>("router/route", new SmsRouteRequest { HttpMethod = request.Method, Id = id, UserName = userName });
            routeResponse = response.Data;
            uriBuilder = new UriBuilder(request.RequestUri) { Host = routeResponse.Host, Port = routeResponse.Port };

            request.RequestUri = uriBuilder.Uri;

            stopwatch.Stop();
            _ptLogger.LogInfo($"{request.Method} request [{routeElements}] routed to {request.RequestUri.AbsoluteUri} in {stopwatch.ElapsedMilliseconds} ms");

            return await base.SendAsync(request, cancellationToken);
        }

        private void GetDefaultRoute(HttpRequestMessage request)
        {
            var client = _restClientFactory.GetShapeManagerRouterServiceClient();
            var response = client.ExecutePostWithRetry<SmsRouteResponse>("router/route", new SmsRouteRequest { HttpMethod = request.Method });
            var routeResponse = response.Data;
            var uriBuilder = new UriBuilder(request.RequestUri) { Host = routeResponse.Host, Port = routeResponse.Port };

            request.RequestUri = uriBuilder.Uri;
        }

        private static List<string> GetRequestPropertyKeys(dynamic dynamicObject)
        {
            if (dynamicObject == null)
            {
                return null;
            }

            JObject attributesAsJObject = dynamicObject;
            var values = attributesAsJObject.ToObject<Dictionary<string, object>>();

            return values.Keys.ToList();
        }
    }
}