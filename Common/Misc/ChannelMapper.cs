using System;
using System.Collections.Generic;
using System.Linq;

namespace HdhrProxy.Common.Misc
{
    public class ChannelMapper
    {
        private struct ChannelBand
        {
            public readonly int MinChannel;
            public readonly int MaxChannel;
            public readonly decimal BaseFrequency;
            public readonly decimal FrequencyOffset;

            public ChannelBand(int minChannel, int maxChannel, decimal baseFrequency, decimal frequencyOffset = 6m)
            {
                MinChannel = minChannel;
                MaxChannel = maxChannel;
                BaseFrequency = baseFrequency;
                FrequencyOffset = frequencyOffset;
            }
        }

        private static readonly ChannelBand[] ChannelBands =
        {
            new ChannelBand(1, 1, 75m),
            new ChannelBand(2, 4, 57m),
            new ChannelBand(5, 6, 81m),
            new ChannelBand(95, 99, 93m),
            new ChannelBand(14, 22, 123m),
            new ChannelBand(7, 13, 177m),
            new ChannelBand(23, 94, 219m), 
            new ChannelBand(100, 158, 651m) 
        };
        
        private static Dictionary<int, decimal> ChannelMap { get; }
        private static Dictionary<decimal, int> FrequencyMap { get; }
        
        public static int MinChannel { get; }
        public static int MaxChannel { get; }

        static ChannelMapper()
        {
            ChannelMap = new Dictionary<int, decimal>();
            FrequencyMap = new Dictionary<decimal, int>();
            
            PopulateMaps();

            MinChannel = ChannelMap.Keys.Min();
            MaxChannel = ChannelMap.Keys.Max();
        }

        public static decimal? GetFrequency(int channelId)
        {
            if (channelId < MinChannel || channelId > MaxChannel)
                throw new ArgumentOutOfRangeException(nameof(channelId));

            if (!ChannelMap.ContainsKey(channelId))
                return null;

            return ChannelMap[channelId];
        }

        public static int? GetChannel(decimal frequency)
        {
            if (!FrequencyMap.ContainsKey(frequency))
                return null;

            return FrequencyMap[frequency];
        }

        private static void PopulateMaps()
        {
            foreach (var band in ChannelBands)
            {
                for (var channelId = band.MinChannel; channelId <= band.MaxChannel; channelId++)
                {
                    var frequency = CalculateFrequency(channelId, band);
                    
                    if (ChannelMap.ContainsKey(channelId))
                        throw new Exception($"Channel ID overlap detected for channel {channelId}. Please check channel bands.");
                    
                    if (FrequencyMap.ContainsKey(frequency))
                        throw new Exception($"Frequency overlap detected for frequency {frequency}Mhz. Please check channel bands.");

                    ChannelMap[channelId] = frequency;
                    FrequencyMap[frequency] = channelId;
                }
            }
        }

        private static decimal CalculateFrequency(int channelId, ChannelBand band)
        {
            var offset = (channelId - band.MinChannel) * band.FrequencyOffset;

            return band.BaseFrequency + offset;
        }
    }
}