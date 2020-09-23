using System.Collections.Generic;
using System.Threading.Tasks;
using HdhrProxy.Common.Http;
using HdhrProxy.Common.Misc;

namespace HdhrProxy.Common.Hdhr
{
    public static class ConversionUtilities
    {
        public static async Task<IEnumerable<LineupItem>> ChannelsToHdhrLineup(string deviceId, IEnumerable<Channel> channels)
        {
            await Task.Yield();

            var items = new List<LineupItem>();

            foreach (var channel in channels)
            {
                var item = new LineupItem();
                var frequency = $"{channel.Frequency:#.000000}".Replace(".", "");

                item.GuideNumber = channel.DisplayChannel;
                item.GuideName = channel.Name;
                item.Url = $"hdhomerun://{deviceId}/ch{frequency}-{channel.ProgramId:d}";
                
                items.Add(item);
            }

            return items;
        }
    }
}