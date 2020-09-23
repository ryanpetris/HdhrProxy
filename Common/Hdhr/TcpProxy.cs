using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

                    try
                    {
                        RunConnection(await Listener.AcceptTcpClientAsync());
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                }
            }
        }

        public async void RunConnection(TcpClient connection)
        {
            await using (CancellationToken.Register(connection.Close))
            {
                using (var client = new TcpClient(Host, Port))
                {
                    await using (CancellationToken.Register(client.Close))
                    {
                        while (connection.Connected && client.Connected)
                        {
                            var upstream = connection.GetStream();
                            var downstream = client.GetStream();

                            await Task.WhenAll(new[]
                            {
                                RunConnectionReceive(upstream, downstream),
                                RunConnectionTransmit(upstream, downstream)
                            });
                        }
                    }
                }
            }
        }

        private async Task RunConnectionReceive(Stream upstream, Stream downstream)
        {
            await Task.Yield();

            var bufferSize = 8192;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                while (true)
                {
                    var received = await upstream.ReadAsync(buffer, 0, bufferSize, CancellationToken);

                    if (received > 0)
                        await downstream.WriteAsync(buffer, 0, received, CancellationToken);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        
        private async Task RunConnectionTransmit(Stream upstream, Stream downstream)
        {
            await Task.Yield();

            var bufferSize = 8192;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                while (true)
                {
                    var received = await downstream.ReadAsync(buffer, 0, bufferSize, CancellationToken);

                    if (received > 0)
                        await upstream.WriteAsync(buffer, 0, received, CancellationToken);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public void Dispose()
        {
            Stop().Wait();
        }
    }
}