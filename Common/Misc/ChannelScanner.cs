using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HdhrProxy.Common.Control;
using HdhrProxy.Common.Control.Command;
using HdhrProxy.Common.Http;

namespace HdhrProxy.Common.Misc
{
    public class ChannelScanner
    {
        private class ScanItem
        {
            public int Tuner { get; }
            public Task<IEnumerable<Channel>> Task { get; }

            public ScanItem(int tuner, Task<IEnumerable<Channel>> task)
            {
                Tuner = tuner;
                Task = task;
            }
        }
        
        public string Host { get; }
        public int HttpPort { get; }
        public int ControlPort { get; }
        
        public ChannelScanner(string host, int httpPort, int controlPort)
        {
            Host = host;
            HttpPort = httpPort;
            ControlPort = controlPort;
        }

        public async Task<IEnumerable<Channel>> Scan()
        {
            await Task.Yield();
            
            var httpClient = new HttpClient(Host, HttpPort);
            var discoverData = await httpClient.Discover();

            var clients = new List<ControlClient>();

            foreach (var tunerId in Enumerable.Range(0, discoverData.TunerCount))
                clients.Add(new ControlClient(Host, ControlPort, tunerId));

            await Task.WhenAll(clients.Select(StartupClient));

            var tasks = new Collection<ScanItem>();
            var channels = new List<Channel>();

            for (var channelId = ChannelMapper.MinChannel; channelId <= ChannelMapper.MaxChannel; channelId++)
            {
                var client = clients.FirstOrDefault(c => tasks.All(t => t.Tuner != c.Tuner));

                if (client == null)
                {
                    var task = await Task.WhenAny(tasks.Select(t => t.Task));
                    var taskItem = tasks.First(t => t.Task == task);
                    var result = await task;
                    
                    channels.AddRange(result);
                    tasks.Remove(taskItem);

                    client = clients.First(c => c.Tuner == taskItem.Tuner);
                }

                tasks.Add(new ScanItem(client.Tuner, ScanChannel(channelId, client)));
            }
            
            foreach (var task in tasks)
                channels.AddRange(await task.Task);
            
            tasks.Clear();
            
            await Task.WhenAll(clients.Select(ShutdownClient));

            return channels;
        }

        private async Task StartupClient(ControlClient client)
        {
            await Task.Yield();

            await client.Connect();
            
            var token = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            
            while (true)
            {
                if (token.IsCancellationRequested)
                    throw new Exception("Could not lock tuner.");
                
                try
                {
                    await client.Lock();

                    break;
                }
                catch
                {
                    // This space intentionally left blank.
                }
            }
        }

        private async Task ShutdownClient(ControlClient client)
        {
            await Task.Yield();

            await client.Disconnect();
        }

        private async Task<IEnumerable<Channel>> ScanChannel(int channelId, ControlClient client)
        {
            await Task.Yield();

            var frequency = ChannelMapper.GetFrequency(channelId);

            if (frequency == null)
                return Enumerable.Empty<Channel>();
            
            Console.WriteLine($"Scanning {client.Host}:{client.Port} tuner {client.Tuner} for channel {channelId} ({frequency:#.00}Mhz)...");

            await client.SetFrequency(frequency.Value);
            
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            while (true)
            {
                if (token.IsCancellationRequested)
                    return Enumerable.Empty<Channel>();
                
                var status = await client.GetStatus();
                
                if (status.Modulation == CommandValues.None)
                    continue;

                break;
            }
            
            token = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            while (true)
            {
                var info = await client.GetStreamInfo();
                
                if (!token.IsCancellationRequested && (info.Channels.Count == 0 || info.IsWaitingForData))
                    continue;

                var results = info.Channels
                    .Where(c => !c.IsEncrypted && !c.IsControl && !c.IsNoData && !string.IsNullOrEmpty(c.DisplayChannel) )
                    .Select(c => new Channel(channelId, frequency.Value, c.Program, c.DisplayChannel, c.Name))
                    .ToList();
                
                if (results.Any())
                    Console.WriteLine($"Found {results.Count} virtual channels on {client.Host}:{client.Port} tuner {client.Tuner} for channel {channelId} ({frequency:#.00}Mhz).");

                return results;
            }
        }
    }
}