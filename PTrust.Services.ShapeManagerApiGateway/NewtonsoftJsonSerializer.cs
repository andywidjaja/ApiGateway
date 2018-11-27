using System.IO;

using Newtonsoft.Json;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class NewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
        // Note:
        /// We use Newtonsoft's JSON seralizer is to workaround RestSharp inability to seralize/deserailize IEnumerables
        //  Details from: http://bytefish.de/blog/restsharp_custom_json_serializer/

        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public NewtonsoftJsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public string ContentType
        {
            get => "application/json";
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(RestSharp.IRestResponse response)
        {
            using (var stringReader = new StringReader(response.Content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static NewtonsoftJsonSerializer Default =>
            new NewtonsoftJsonSerializer(new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            });
    }
}