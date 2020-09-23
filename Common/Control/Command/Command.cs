using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HdhrProxy.Common.Control.Command
{
    public abstract class Command
    {
        public int Tuner { get; }
        public string Name { get; }
        
        protected Command(int tuner, string name)
        {
            Tuner = tuner;
            Name = name;
        }
        
        public async Task<List<byte>> GetBytes()
        {
            await Task.Yield();
            
            var data = new List<byte>();

            data.AddRange(await GetCommandSection());
            data.AddRange(await GetDataSection());
            data.InsertRange(0, await GetHeaderSection(data.Count));
            data.AddRange(GetBytes(MessageCrc.Get(data)));

            return data;
        }
        
        private async Task<IEnumerable<byte>> GetHeaderSection(int messageLength)
        {
            await Task.Yield();
            
            var data = new List<byte>();
            
            data.AddRange(GetBytes((ushort) MessageSection.Header));
            data.AddRange(GetBytes((ushort) messageLength));

            return data;
        }

        private async Task<IEnumerable<byte>> GetCommandSection()
        {
            await Task.Yield();

            var data = new List<byte>();
            var commandBytes = GetBytes($"/tuner{Tuner}/{Name}\0");

            data.AddRange(GetBytes((byte) MessageSection.Name));
            data.AddRange(GetBytes((byte) commandBytes.Length));
            data.AddRange(commandBytes);

            return data;
        }

        protected abstract Task<IEnumerable<byte>> GetDataSection();

        protected static byte[] GetBytes(byte value)
        {
            return new[] {value};
        }

        protected static byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        protected static byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        protected static byte[] GetBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }
}