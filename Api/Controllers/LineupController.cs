using System.Collections.Generic;
using System.Threading.Tasks;
using HdhrProxy.Api.Services;
using HdhrProxy.Common.Http;
using Microsoft.AspNetCore.Mvc;

namespace HdhrProxy.Api.Controllers
{
    [Controller]
    [Route("/")]
    public class LineupController
    {
        private LineupService LineupService { get; }
        
        public LineupController(LineupService lineupService)
        {
            LineupService = lineupService;
        }
        
        [Route("lineup.json")]
        [HttpGet]
        public async Task<IEnumerable<LineupItem>> Json()
        {
            await Task.Yield();

            return await LineupService.GetLineup();
        }
    }
}