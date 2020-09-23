using System.Collections.Generic;

namespace HdhrProxy.Common.Misc
{
    public class AppConfiguration
    {
        public List<AppProxy> Proxies { get; set; }
        public string DataDir { get; set; }
    }

    public class AppProxy
    {
        public string Host { get; set; }
        public string ProxyHost { get; set; }
        public int HttpPort { get; set; }
        public int ControlPort { get; set; }

        public string GetChannelFilename()
        {
            return $"{Host}-channels.json";
        }
    }
}