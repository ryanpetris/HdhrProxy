using System.Text;

namespace HdhrProxy.Common.Control.Reply
{
    public class StatusReply : Reply
    {
        /// <summary>
        /// Channel Requested
        /// </summary>
        public string Channel { get; }
        
        /// <summary>
        /// Actual Modulation Detected
        /// </summary>
        public string Modulation { get; }
        
        /// <summary>
        /// Signal Strength. 80% is approximately -12dBmV.
        /// </summary>
        public int SignalStrength { get; }
        
        /// <summary>
        /// Signal to Noise Quality (based on analog signal to noise ratio).
        /// </summary>
        public int SignalToNoiseQuality { get; }
        
        /// <summary>
        /// Symbol Error Quality (number of uncorrectable digital errors detected).
        /// </summary>
        public int SymbolErrorQuality { get; }
        
        /// <summary>
        /// Raw channel bits per second.
        /// </summary>
        public int BitsPerSecond { get; }
        
        /// <summary>
        /// Packets per second sent through the network.
        /// </summary>
        public int PacketsPerSecond { get; }
        
        public StatusReply(string name, string value) : base(name, value)
        {
            foreach (var item in value.Split(" "))
            {
                var parts = item.Split("=");

                if (parts.Length != 2)
                    throw new InvalidReplyException($"Invalid status message.");

                switch (parts[0])
                {
                    case "ch":
                        Channel = parts[1];
                        break;
                    
                    case "lock":
                        Modulation = parts[1];
                        break;
                    
                    case "ss":
                        SignalStrength = int.Parse(parts[1]);
                        break;
                    
                    case "snq":
                        SignalToNoiseQuality = int.Parse(parts[1]);
                        break;
                    
                    case "seq":
                        SymbolErrorQuality = int.Parse(parts[1]);
                        break;
                    
                    case "bps":
                        BitsPerSecond = int.Parse(parts[1]);
                        break;
                    
                    case "pps":
                        PacketsPerSecond = int.Parse(parts[1]);
                        break;
                    
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Channel: {Channel}");
            builder.AppendLine($"Modulation: {Modulation}");
            builder.AppendLine($"Signal Strength: {SignalStrength}");
            builder.AppendLine($"Signal to Noise Quality: {SignalToNoiseQuality}");
            builder.AppendLine($"Symbol Error Quality: {SymbolErrorQuality}");
            builder.AppendLine($"Bits per Second: {BitsPerSecond}");
            builder.AppendLine($"Packets per Second: {PacketsPerSecond}");

            return builder.ToString().TrimEnd('\n');
        }
    }
}