using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HdhrProxy.Common.Control.Reply
{
    public class ReplyBytes
    {
        private byte[] Bytes { get; }
        
        public int Position { get; private set; }
        public int Count => Bytes.Length;
        
        public ReplyBytes(IEnumerable<byte> bytes)
        {
            Bytes = bytes.ToArray();
        }

        public void Reset()
        {
            Position = 0;
        }

        public byte[] GetBytes()
        {
            var value = new byte[Bytes.Length];
            
            Array.Copy(Bytes, value, Bytes.Length);

            return value;
        }

        public byte[] GetNextBytes(int numBytes)
        {
            if (numBytes <= 0)
                throw new ArgumentException("At least one byte must be requested.");
            
            if (Position + numBytes > Bytes.Length)
                throw new ArgumentException($"Requested {numBytes} bytes but fewer are left.");
            
            var value = new byte[numBytes];
            
            Array.Copy(Bytes, Position, value, 0, numBytes);

            Position += numBytes;

            return value;
        }

        public byte[] PeekNextBytes(int numBytes)
        {
            if (numBytes <= 0)
                throw new ArgumentException("At least one byte must be requested.");
            
            if (Position + numBytes > Bytes.Length)
                throw new ArgumentException($"Requested {numBytes} bytes but fewer are left.");
            
            var value = new byte[numBytes];
            
            Array.Copy(Bytes, Position, value, 0, numBytes);

            return value;
        }
        
        public byte ReadByte()
        {
            var byteArray = GetNextBytes(1);
            
            return byteArray.First();
        }

        public ushort ReadUShort()
        {
            var byteArray = GetNextBytes(2);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byteArray);

            return BitConverter.ToUInt16(byteArray, 0);
        }

        public string ReadStringField(out byte fieldType)
        {
            fieldType = GetNextBytes(1).First();

            var fieldLength = (int) GetNextBytes(1).First();

            if (fieldLength > 128)
            {
                fieldLength ^= 0b_1000_0000;

                var additionalByte = (int) GetNextBytes(1).First();

                fieldLength ^= additionalByte << 7;
            }

            var fieldData = GetNextBytes(fieldLength);

            var result = Encoding.UTF8.GetString(fieldData).Replace("\0", "");

            return result;
        }

        public void VerifyCrc()
        {
            var messageBytes = Bytes.SkipLast(4);
            var messageCrcBytes = Bytes.TakeLast(4).ToArray();
            
            var crc = MessageCrc.Get(messageBytes);
            var crcBytes = BitConverter.GetBytes(crc);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(crcBytes);

            for (var i = 0; i < 4; i++)
            {
                if (messageCrcBytes[i] != crcBytes[i])
                    throw new InvalidReplyException($"CRC does not match.");
            }
        }
    }
}