using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HdhrProxy.Common.Hdhr;
using HdhrProxy.Common.Http;
using HdhrProxy.Common.Misc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HdhrProxy.Api.Services
{
    public class LineupService
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private AppConfiguration Configuration { get; }
        
        public LineupService(IHttpContextAccessor httpContextAccessor, AppConfiguration configuration)
        {
            HttpContextAccessor = httpContextAccessor;
            Configuration = configuration;
        }

        public async Task<IEnumerable<LineupItem>> GetLineup()
        {
            await Task.Yield();

            var localIp = HttpContextAccessor.HttpContext.Connection.LocalIpAddress;

            if (localIp.IsIPv4MappedToIPv6)
                localIp = localIp.MapToIPv4();

            var localIpStr = localIp.ToString();
            var proxy = Configuration.Proxies.FirstOrDefault(p => p.ProxyHost == localIpStr);

            if (proxy == null)
                return Enumerable.Empty<LineupItem>();

            var filePath = Path.Join(Configuration.DataDir, proxy.GetChannelFilename());
            
            if (!File.Exists(filePath))
                return Enumerable.Empty<LineupItem>();
            
            var discoverInfo = await new HttpClient(proxy.Host, proxy.HttpPort).Discover();

            await using var stream = File.OpenRead(filePath);
            using var streamReader = new StreamReader(stream);

            var json = await streamReader.ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<IEnumerable<Channel>>(json);

            return await ConversionUtilities.ChannelsToHdhrLineup(discoverInfo.DeviceId, data);
        }
    }
}