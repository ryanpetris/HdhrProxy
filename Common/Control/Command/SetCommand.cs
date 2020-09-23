using System.Collections.Generic;
using System.Threading.Tasks;

namespace HdhrProxy.Common.Control.Command
{
    public class SetCommand : Command
    {
        public string Value { get; }
        public uint? LockKey { get; }
        
        public SetCommand(int tuner, string name, string value, uint? lockKey = null) : base(tuner, name)
        {
            Value = value;
            LockKey = lockKey;
        }

        protected override async Task<IEnumerable<byte>> GetDataSection()
        {
            await Task.Yield();

            var data = new List<byte>();

            if (Value != null)
            {
                var valueBytes = GetBytes($"{Value}\0");
                
                data.AddRange(GetBytes((byte) MessageSection.Value));
                data.AddRange(GetBytes((byte) valueBytes.Length));
                data.AddRange(valueBytes);
            }

            if (LockKey != null)
            {
                var lockKeyBytes = GetBytes(LockKey.Value);
                
                data.AddRange(GetBytes((byte) MessageSection.LockKey));
                data.AddRange(GetBytes((byte) lockKeyBytes.Length));
                data.AddRange(lockKeyBytes);
            }

            return data;
        }
    }
}