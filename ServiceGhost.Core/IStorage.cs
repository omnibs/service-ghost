namespace ServiceGhost.Core
{
    using System.Collections.Generic;

    public interface IStorage
    {
        IEnumerable<CallMetadata> Load();

        void Save(IEnumerable<CallMetadata> calls);

    }
}