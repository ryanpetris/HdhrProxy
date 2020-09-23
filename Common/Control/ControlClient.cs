using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HdhrProxy.Common.Control.Command;
using HdhrProxy.Common.Control.Reply;

namespace HdhrProxy.Common.Control
{
    public class ControlClient : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public int Tuner { get; }
        
        private TcpClient TcpClient { get; }
        private bool IsConnected { get; set; }
        private bool IsClosed { get; set; }
        
        private uint? LockKey { get; set; }
        
        public ControlClient(string host, int port, int tuner)
        {
            Host = host;
            Port = port;
            Tuner = tuner;
            TcpClient = new TcpClient();
        }

        public async Task Connect()
        {
            await Task.Yield();
            
            CheckConnectionStatus(checkConnected: false);

            if (IsConnected)
                return;

            IsConnected = true;
            
            await TcpClient.ConnectAsync(Host, Port);
        }

        public async Task Disconnect()
        {
            await Task.Yield();
            
            CheckConnectionStatus(checkClosed: false);

            if (IsClosed)
                return;

            try
            {
                await ClearTarget();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            try
            {
                await Unlock();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            IsClosed = true;

            TcpClient.Close();
        }

        public async Task Lock()
        {
            await Task.Yield();

            if (LockKey != null)
                return;

            try
            {
                LockKey = (uint) new Random().Next(int.MinValue, int.MaxValue);

                var command = new SetCommand(Tuner, CommandNames.LockKey, $"{LockKey:d}");

                await Send(command);
            }
            catch
            {
                LockKey = null;

                throw;
            }
        }

        public async Task Unlock()
        {
            await Task.Yield();

            if (LockKey == null)
                return;

            LockKey = null;
            
            var command = new SetCommand(Tuner, CommandNames.LockKey, CommandValues.None, LockKey);
            
            await Send(command);
        }

        public async Task SetTarget(IPEndPoint endpoint)
        {
            var address = endpoint.Address;
            var port = endpoint.Port;

            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            var command = new SetCommand(Tuner, CommandNames.Target, $"rtp://{address}:{port}", LockKey);

            await Send(command);
        }

        public async Task ClearTarget()
        {
            var command = new SetCommand(Tuner, CommandNames.Target, CommandValues.None, LockKey);

            await Send(command);
        }

        public async Task SetChannelMap(string channelMap)
        {
            await Task.Yield();
            
            var command = new SetCommand(Tuner, CommandNames.ChannelMap, channelMap, LockKey);

            await Send(command);
        }

        public async Task SetFrequency(decimal frequency)
        {
            await Task.Yield();
            
            var command = new SetCommand(Tuner, CommandNames.Channel, $"auto:{frequency:#.000000}".Replace(".", ""), LockKey);

            await Send(command);
        }

        public async Task SetProgram(int program)
        {
            await Task.Yield();
            
            var command = new SetCommand(Tuner, CommandNames.Program, $"{program:d}", LockKey);

            await Send(command);
        }

        public async Task<StatusReply> GetStatus()
        {
            await Task.Yield();
            
            var command = new GetCommand(Tuner, CommandNames.Status);
            var result = await Send(command);

            if (result is StatusReply statusResult)
                return statusResult;
            
            throw new InvalidReplyException($"Expected StatusReply, got {result.GetType().Name}.");
        }

        public async Task<StreamInfoReply> GetStreamInfo()
        {
            await Task.Yield();
            
            var command = new GetCommand(Tuner, CommandNames.StreamInfo);
            var result = await Send(command);

            if (result is StreamInfoReply streamInfoResult)
                return streamInfoResult;
            
            throw new InvalidReplyException($"Expected StreamInfoReply, got {result.GetType().Name}.");
        }

        private async Task<Reply.Reply> Send(Command.Command command)
        {
            await Task.Yield();

            CheckConnectionStatus();
            
            var stream = TcpClient.GetStream();
            var commandBytes = await command.GetBytes();
            var receiveBuffer = new byte[8192];

            await stream.WriteAsync(commandBytes.ToArray(), 0, commandBytes.Count);

            var receivedBytes = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

            return await Reply.Reply.ParseReply(receiveBuffer.Take(receivedBytes));
        }

        private void CheckConnectionStatus(bool checkClosed = true, bool checkConnected = true)
        {
            if (checkClosed && IsClosed)
                throw new InvalidOperationException("Client is closed.");
            
            if (checkConnected && !IsConnected)
                throw new InvalidOperationException("Client is not connected.");
        }

        public void Dispose()
        {
            try
            {
                Disconnect().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            TcpClient?.Dispose();
        }
    }
}