using System.Threading.Tasks;
using HdhrProxy.Api.Services;
using HdhrProxy.Common.Http;
using Microsoft.AspNetCore.Mvc;

namespace HdhrProxy.Api.Controllers
{
    [Controller]
    [Route("/")]
    public class DiscoverController
    {
        private DiscoverService DiscoverService { get; }
        
        public DiscoverController(DiscoverService discoverService)
        {
            DiscoverService = discoverService;
        }
        
        [HttpGet]
        [Route("discover.json")]
        public async Task<DiscoveryInfo> Json()
        {
            await Task.Yield();

            return await DiscoverService.GetDiscoveryInfo();
        }
    }
}