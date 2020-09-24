using System.Linq;
using System.Threading.Tasks;
using HdhrProxy.Common.Http;
using HdhrProxy.Common.Misc;
using Microsoft.AspNetCore.Http;

namespace HdhrProxy.Api.Services
{
    public class DiscoverService
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private AppConfiguration Configuration { get; }
        
        public DiscoverService(IHttpContextAccessor httpContextAccessor, AppConfiguration configuration)
        {
            HttpContextAccessor = httpContextAccessor;
            Configuration = configuration;
        }

        public async Task<DiscoveryInfo> GetDiscoveryInfo()
        {
            await Task.Yield();

            var localIp = HttpContextAccessor.HttpContext.Connection.LocalIpAddress;

            if (localIp.IsIPv4MappedToIPv6)
                localIp = localIp.MapToIPv4();

            var localIpStr = localIp.ToString();
            var proxy = Configuration.Proxies.FirstOrDefault(p => p.ProxyHost == localIpStr);

            if (proxy == null)
                return null;

            var discoverInfo = await new HttpClient(proxy.Host, proxy.HttpPort).Discover();

            discoverInfo.DeviceAuth = "";
            discoverInfo.BaseUrl = discoverInfo.BaseUrl.Replace(proxy.Host, proxy.ProxyHost);
            discoverInfo.LineupUrl = $"http://{proxy.ProxyHost}:{proxy.HttpPort}/lineup.json";

            if (proxy.FauxDeviceId != null)
                discoverInfo.DeviceId = proxy.FauxDeviceId;

            return discoverInfo;
        }
    }
}