using System.Collections.Generic;
using System.Linq;

namespace ServiceGhost.Core
{
    using FakeItEasy.SelfInitializedFakes;

    public class DefaultStorage : ICallStorage
    {
        public List<CallData> RecordedCalls { get; set; }

        public IEnumerable<CallData> Load()
        {
            return this.RecordedCalls;
        }

        public void Save(IEnumerable<CallData> calls)
        {
            this.RecordedCalls = calls.ToList();
        }
    }
}
