using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HdhrProxy.Common.Control.Command
{
    public class GetCommand : Command
    {
        public GetCommand(int tuner, string name) : base(tuner, name)
        {
        }

        protected override Task<IEnumerable<byte>> GetDataSection()
        {
            return Task.FromResult(Enumerable.Empty<byte>());
        }
    }
}