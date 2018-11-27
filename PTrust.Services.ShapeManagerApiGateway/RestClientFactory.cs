using System;
using System.Net.Cache;

using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class RestClientFactory : IRestClientFactory
    {
        private static IOptions<RouteSettings> _routeSettings;

        private static readonly Lazy<IRestClient> LazySmsRouterServiceClient = new Lazy<IRestClient>(() => CreateRestSharpClient(_routeSettings.Value.ShapeManagerRouterServiceUri));

        public RestClientFactory(IOptions<RouteSettings> routeSettings)
        {
            _routeSettings = routeSettings;
        }

        public IRestClient GetShapeManagerRouterServiceClient()
        {
            return LazySmsRouterServiceClient.Value;
        }

        private static RestClient CreateRestSharpClient(string uri)
        {
            var shapeManagerRestSharpClient = new RestClient
            {
                BaseUrl = new Uri(uri),
                Authenticator = new NtlmAuthenticator(),
            };

            shapeManagerRestSharpClient.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/x-json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/javascript", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("*+json", NewtonsoftJsonSerializer.Default);
            return shapeManagerRestSharpClient;
        }

        private static RestClient CreateRestSharpClientWithCaching(string uri)
        {
            var shapeManagerRestSharpClient = new RestClient
            {
                BaseUrl = new Uri(uri),
                Authenticator = new NtlmAuthenticator(),
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default)
            };

            shapeManagerRestSharpClient.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/x-json", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("text/javascript", NewtonsoftJsonSerializer.Default);
            shapeManagerRestSharpClient.AddHandler("*+json", NewtonsoftJsonSerializer.Default);

            return shapeManagerRestSharpClient;
        }
    }
}