using System.Runtime.Serialization;

namespace HdhrProxy.Common.Http
{
    [DataContract]
    public class LineupItem
    {
        [DataMember]
        public string GuideNumber { get; set; }
        
        [DataMember]
        public string GuideName { get; set; }
        
        [DataMember(Name = "URL")]
        public string Url { get; set; }
    }
}