using System.Runtime.Serialization;
using System.Text;

namespace HdhrProxy.Common.Http
{
    [DataContract]
    public class DiscoveryInfo
    {
        [DataMember]
        public string FriendlyName { get; set; }
        
        [DataMember]
        public string ModelNumber { get; set; }
        
        public bool IsLegacy { get; set; }

        [DataMember(Name="Legacy")]
        private int LegacyDataMember
        {
            get => IsLegacy ? 1 : 0;
            set => IsLegacy = value == 1;
        }
        
        [DataMember]
        public string FirmwareName { get; set; }
        
        [DataMember]
        public string FirmwareVersion { get; set; }
        
        [DataMember(Name="DeviceID")]
        public string DeviceId { get; set; }
        
        [DataMember]
        public string DeviceAuth { get; set; }
        
        [DataMember]
        public int TunerCount { get; set; }
        
        [DataMember]
        public string BaseUrl { get; set; }
        
        [DataMember(Name="LineupURL")]
        public string LineupUrl { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Friendly Name: {FriendlyName}");
            builder.AppendLine($"Model Number: {ModelNumber}");
            builder.AppendLine($"Is Legacy: {IsLegacy}");
            builder.AppendLine($"Firmware Name: {FirmwareName}");
            builder.AppendLine($"Firmware Version: {FirmwareVersion}");
            builder.AppendLine($"Device Id: {DeviceId}");
            builder.AppendLine($"Device Auth: {DeviceAuth}");
            builder.AppendLine($"Tuner Count: {TunerCount}");
            builder.AppendLine($"Base Url: {BaseUrl}");
            builder.AppendLine($"Lineup Url: {LineupUrl}");

            return builder.ToString().TrimEnd('\n');
        }
    }
}