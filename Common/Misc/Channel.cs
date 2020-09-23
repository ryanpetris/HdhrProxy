namespace HdhrProxy.Common.Misc
{
    public class Channel
    {
        public int ChannelId { get; set; }
        public decimal Frequency { get; set; }
        public int ProgramId { get; set; }
        public string DisplayChannel { get; set; }
        public string Name { get; set; }

        public Channel()
        {
            
        }

        public Channel(int channelId, decimal frequency, int programId, string displayChannel, string name)
        {
            ChannelId = channelId;
            Frequency = frequency;
            ProgramId = programId;
            DisplayChannel = displayChannel;
            Name = name;
        }
    }
}