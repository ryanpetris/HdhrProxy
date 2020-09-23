using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HdhrProxy.Common.Control.Reply
{
    public class Reply
    {
        private static readonly int ExpectedMessageType = 5;
        
        public string RawName { get; }
        public string RawValue { get; }

        public Reply(string name, string value)
        {
            RawName = name;
            RawValue = value;
        }
        
        public static async Task<Reply> ParseReply(IEnumerable<byte> bytes)
        {
            await Task.Yield();
            
            var reply = new ReplyBytes(bytes);

            if (reply.Count < 8)
                throw new InvalidReplyException("Message too short.");
            
            reply.VerifyCrc();

            var messageType = reply.ReadUShort();
            
            if (messageType != ExpectedMessageType)
                throw new InvalidReplyException($"Unknown message type {messageType}.");

            var messageLength = reply.ReadUShort();
            
            if (messageLength != reply.Count - 8)
                throw new InvalidReplyException($"Invalid message length {messageLength}.");

            var name = string.Empty;
            var value = string.Empty;
            
            for (var i = 0; i < 2; i++)
            {
                var fieldValue = reply.ReadStringField(out var fieldType);

                switch (fieldType)
                {
                    case (byte) MessageSection.Name:
                        name = fieldValue;
                        break;
                    
                    case (byte) MessageSection.Value:
                        value = fieldValue;
                        break;
                    
                    case (byte) MessageSection.Error:
                        throw new InvalidOperationException(fieldValue);
                    
                    default:
                        throw new InvalidReplyException($"Unknown message section {fieldType}.");
                    
                }
            }
            
            var responseType = name.Split("/").Last();

            switch (responseType)
            {
                case "streaminfo":
                    return new StreamInfoReply(name, value);
                
                case "status":
                    return new StatusReply(name, value);
                
                default:
                    return new TextReply(name, value);
                
            }
        }

        public override string ToString()
        {
            return $"{RawName}: {RawValue}";
        }
    }
}