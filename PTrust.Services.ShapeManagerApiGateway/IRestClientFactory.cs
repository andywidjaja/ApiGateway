using RestSharp;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public interface IRestClientFactory
    {
        IRestClient GetShapeManagerRouterServiceClient();
    }
}