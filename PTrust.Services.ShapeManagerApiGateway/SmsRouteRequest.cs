using System.Net.Http;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class SmsRouteRequest
    {
        public HttpMethod HttpMethod { get; set; }

        public int? Id { get; set; }

        public string UserName { get; set; }
    }
}