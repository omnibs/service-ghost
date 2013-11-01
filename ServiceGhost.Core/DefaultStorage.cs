using System.Collections.Generic;
using System.Linq;

namespace ServiceGhost.Core
{
    public class DefaultStorage : IStorage
    {
        public List<CallMetadata> RecordedCalls { get; set; }

        public IEnumerable<CallMetadata> Load()
        {
            return this.RecordedCalls;
        }

        public void Save(IEnumerable<CallMetadata> calls)
        {
            this.RecordedCalls = calls.ToList();
        }
    }
}
