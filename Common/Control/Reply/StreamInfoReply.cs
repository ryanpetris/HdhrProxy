using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HdhrProxy.Common.Control.Reply
{
    public class StreamInfoReply : Reply
    {
        public IReadOnlyList<StreamChannel> Channels { get; }
        public bool IsWaitingForData { get; }
        
        public StreamInfoReply(string name, string value) : base(name, value)
        {
            Channels = StreamChannel.ParseChannels(value).ToList().AsReadOnly();
            IsWaitingForData = Channels.Any(c => c.IsNoData);
        }

        public override string ToString()
        {
            if (Channels.Count == 0)
                return "No Channels.";
            
            var builder = new StringBuilder();

            foreach (var channel in Channels)
                builder.AppendLine($"{channel}");
                
            return builder.ToString().TrimEnd('\n');
        }
    }

    public class StreamChannel
    {
        public int Program { get; }
        public string DisplayChannel { get; }
        public string Name { get; }
        
        public bool IsEncrypted { get; }
        public bool IsControl { get; }
        public bool IsNoData { get; }

        public StreamChannel(int program, string displayChannel, string name)
        {
            Program = program;
            
            if (displayChannel != "0")
                DisplayChannel = displayChannel;

            if (name == "(encrypted)")
            {
                IsEncrypted = true;
            }
            else if (name == "(control)")
            {
                IsControl = true;
            }
            else if (name == "(no data)" || string.IsNullOrEmpty(name))
            {
                IsNoData = true;
            }
            else if (name.Contains("(no data)"))
            {
                IsNoData = true;
                Name = name.Replace("(no data)", "").Trim();
            }
            else
            {
                Name = name.Trim();
            }
        }

        public static IEnumerable<StreamChannel> ParseChannels(string value)
        {
            foreach (var channel in value.Split("\n", StringSplitOptions.RemoveEmptyEntries))
            {
                if (channel == "none")
                    continue;
                
                var match = Regex.Match(channel, @"^(?<program>\d+): (?<display>[\d.]+)( (?<name>.*))?$");

                if (!match.Success)
                    continue;

                var program = match.Groups["program"].Value;
                var display = match.Groups["display"].Value;
                var name = match.Groups["name"].Value;
                
                yield return new StreamChannel(int.Parse(program), display, name);
            }
        }

        public override string ToString()
        {
            var displayChannel = DisplayChannel;
            var name = Name;

            if (string.IsNullOrEmpty(displayChannel))
                displayChannel = "0";

            if (IsEncrypted)
                name = "(encrypted)";
            else if (IsControl)
                name = "(control)";

            return $"Program: {Program}; Channel: {displayChannel}; Name: {name}";
        }
    }
}