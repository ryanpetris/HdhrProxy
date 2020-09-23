using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HdhrProxy.Common.Http
{
    public class HttpClient
    {
        public string Host { get; }
        public int Port { get; }

        public HttpClient(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public async Task<DiscoveryInfo> Discover()
        {
            using var client = new System.Net.Http.HttpClient();

            var result = await client.GetAsync($"http://{Host}:{Port}/discover.json");
            var resultStr = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<DiscoveryInfo>(resultStr);
        }
    }
}