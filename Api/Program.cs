using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HdhrProxy.Common.Misc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HdhrProxy.Api
{
    public static class Program
    {
        public static async Task Start(AppConfiguration configuration, CancellationToken cancellationToken)
        {
            await CreateHostBuilder(configuration)
                .Build()
                .RunAsync(cancellationToken);
        }

        private static IHostBuilder CreateHostBuilder(AppConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddSingleton(configuration))
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>().UseUrls(configuration.ListenAddresses.Select(a => $"http://{a}").ToArray()));
    }
}