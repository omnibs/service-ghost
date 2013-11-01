namespace ServiceGhost.Core
{
    using System.Collections.Generic;
    using System.Linq;

    using FakeItEasy.Core;

    public class CallQueue
    {
        private readonly Dictionary<string, List<CallMetadata>> calls = new Dictionary<string, List<CallMetadata>>();

        public int Count
        {
            get
            {
                return count;
            }
        }

        private int count = 0;

        public void LoadList(List<CallMetadata> data)
        {
            foreach (var item in data)
            {
                this.AddItem(item);
            }
        }

        public void AddItem(CallMetadata item)
        {
            if (!calls.ContainsKey(item.UniqueId))
            {
                calls[item.UniqueId] = new List<CallMetadata>();
            }

            calls[item.UniqueId].Add(item);
            count++;
        }

        public List<CallMetadata> ToList()
        {
            var result = new List<CallMetadata>();
            foreach (var callList in calls.Values)
            {
                result.AddRange(callList);
            }

            return result;
        }

        public CallMetadata GetResponseForCall(IInterceptedFakeObjectCall fakeObjectCall)
        {
            var key = KeyGenerator.GetMethodKey(
                fakeObjectCall.FakedObject.GetType(),
                fakeObjectCall.Method,
                CommonExtensions.GetCurrentStack());

            if (!calls.ContainsKey(key))
            {
                return null;
            }

            return calls[key].FirstOrDefault(x => !x.HasBeenApplied);
        }
    }
}
