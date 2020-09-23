using System.Linq;

namespace HdhrProxy.Common.Control.Reply
{
    public class TextReply : Reply
    {
        public TextReply(string name, string value) : base(name, value)
        {
        }

        public override string ToString()
        {
            return $"{RawName.Split("/").Last()}: {RawValue}";
        }
    }
}