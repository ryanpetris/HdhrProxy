using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HdhrProxy.Common.Extensions;

namespace HdhrProxy.Common.Hdhr
{
    public class TcpProxy : IDisposable
    {
        public string Host { get; }
        public string ProxyHost { get; }
        public int Port { get; }
        private CancellationToken CancellationToken { get; }
        private TcpListener Listener { get; }
        
        public TcpProxy(string host, string proxyHost, int port, CancellationToken cancellationToken)
        {
            Host = host;
            ProxyHost = proxyHost;
            Port = port;
            CancellationToken = cancellationToken;
            
            Listener = new TcpListener(new IPEndPoint(IPAddress.Parse(proxyHost), port));
        }

        public async Task Stop()
        {
            await Task.Yield();
            
            Listener.Stop();
        }

        public async Task Run()
        {
            await Task.Yield();

            Console.WriteLine($"Proxy listening on {Host}:{Port}.");

            await RunInternal().IgnoreStreamExceptions();

            Console.WriteLine($"Proxy stopped on {Host}:{Port}.");
        }

        private async Task RunInternal()
        {
            Listener.Start();
            
            await using (CancellationToken.Register(Listener.Stop))
            {
                while (true)
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        await Stop();
                        return;
                    }

                    RunConnection(await Listener.AcceptTcpClientAsync());
                }
            }
        }

        private async void RunConnection(TcpClient connection)
        {
            await Task.Yield();

            await using var reg1 = CancellationToken.Register(connection.Close);
            using var client = new TcpClient(Host, Port);
            await using var reg2 = CancellationToken.Register(client.Close);

            var connectionEndpoint = connection.Client.RemoteEndPoint;
            var clientEndpoint = client.Client.RemoteEndPoint;

            Console.WriteLine($"Proxy from {connectionEndpoint} to {clientEndpoint} established.");

            var upstream = connection.GetStream();
            var downstream = client.GetStream();

            await Task.WhenAll(new[]
            {
                upstream.CopyToAsync(downstream, CancellationToken).IgnoreStreamExceptions(),
                downstream.CopyToAsync(upstream, CancellationToken).IgnoreStreamExceptions(),
                MonitorConnection(connection, client)
            });

            Console.WriteLine($"Proxy from {connectionEndpoint} to {clientEndpoint} closed.");
        }

        private async Task MonitorConnection(params TcpClient[] clients)
        {
            await Task.Yield();

            while (clients.All(c => c.IsRemoteConnected()) && !CancellationToken.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken);

            foreach (var client in clients)
                await Task.Run(() => client.Close()).IgnoreStreamExceptions();
        }

        public void Dispose()
        {
            Stop().Wait();
        }
    }
}