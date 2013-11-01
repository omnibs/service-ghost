using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceGhost.Core
{
    using System.Globalization;

    using FakeItEasy.SelfInitializedFakes;

    public class CallMetadata
    {
        public CallData RecordedCall { get; set; }

        public string UniqueId { get; set; }

        public long ElapsedTime { get; set; }

        public bool HasBeenApplied { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendFormat(CultureInfo.CurrentCulture, "Applied: {0}", this.HasBeenApplied)
                .AppendLine()
                .Append(this.RecordedCall.Method.Name)
                .Append(" ")
                .Append(this.RecordedCall.ReturnValue)
                .ToString();
        }
    }

}
