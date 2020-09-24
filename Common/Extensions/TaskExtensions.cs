using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HdhrProxy.Common.Extensions
{
    public static class TaskExtensions
    {
        public static async Task IgnoreStreamExceptions(this Task task)
        {
            try
            {
                await task;
            }
            catch (ObjectDisposedException)
            {
                // This space intentionally left blank.
            }
            catch (IOException)
            {
                // This space intentionally left blank.
            }
            catch (SocketException)
            {
                // This space intentionally left blank.
            }
        }
    }
}