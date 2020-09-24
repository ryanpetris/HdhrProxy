using System.Net.Sockets;

namespace HdhrProxy.Common.Extensions
{
    public static class TcpClientExtensions
    {
        public static bool IsRemoteConnected(this TcpClient client)
        {
            return !client.Client.Poll(1, SelectMode.SelectRead) || client.GetStream().DataAvailable;
        }
    }
}