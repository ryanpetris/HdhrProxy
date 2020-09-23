using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HdhrProxy.Common.Hdhr;
using HdhrProxy.Common.Misc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HdhrProxy.Cli
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Yield();

            var config = GetConfiguration();

            if (config.Proxies == null || !config.Proxies.Any())
            {
                Console.WriteLine("Please add Proxies to appsettings.override.json.");
                return;
            }

            if (args.Length > 0 && args[0] == "--scan")
            {
                await HandleScan(config, args[1]);
                return;
            }

            var cts = new CancellationTokenSource();
            var runTasks = new List<Task>();

            Console.CancelKeyPress += (sender, eventArgs) => cts.Cancel();

            runTasks.Add(Api.Program.Start(config, cts.Token));
            runTasks.AddRange(await GetTcpProxyTasks(config, cts.Token));
            
            await Task.WhenAll(runTasks);
        }

        private static async Task HandleScan(AppConfiguration config, string host)
        {
            var scanProxy = config.Proxies.FirstOrDefault(p => p.Host == host);

            if (scanProxy == null)
            {
                Console.WriteLine($"Could not find host {host}.");
                return;
            }
                
            var scanner = new ChannelScanner(scanProxy.Host, scanProxy.HttpPort, scanProxy.ControlPort);
            var channels = await scanner.Scan();
            var json = JsonConvert.SerializeObject(channels);

            await using var stream = File.OpenWrite(Path.Join(config.DataDir, scanProxy.GetChannelFilename()));
            await using var streamWriter = new StreamWriter(stream);

            await streamWriter.WriteAsync(json);
        }

        private static AppConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.override.json");

            var configRoot = builder.Build();
            var config = new AppConfiguration();

            configRoot.Bind(config);

            return config;
        }

        private static async Task<IEnumerable<Task>> GetTcpProxyTasks(AppConfiguration config, CancellationToken token)
        {
            await Task.Yield();

            var tasks = new List<Task>();

            foreach (var proxy in config.Proxies)
            {
                var tcpProxy = new TcpProxy(proxy.Host, proxy.ProxyHost, proxy.ControlPort, token);
                
                tasks.Add(tcpProxy.Run());
            }

            return tasks;
        }
    }
}