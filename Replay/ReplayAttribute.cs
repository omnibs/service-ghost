using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replay
{
    using System.Diagnostics;
    using System.Reflection;

    using PostSharp.Aspects;
    using PostSharp.Extensibility;

    [Serializable]
    public class ReplayAttribute : MethodInterceptionAspect
    {
        [NonSerialized]
        private string _methodName;

        private static bool replayMode = false;

        static ReplayAttribute()
        {
            if (!PostSharpEnvironment.IsPostSharpRunning)
            {
            }
        }

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            _methodName = method.Name;
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var stackPath = this.GetStackPath();
            var key = this.GetCallInstanceKey(args.Arguments);

            if (replayMode) args.ReturnValue = this.GetReplayValue(key, stackPath);
            else
            {
                var returnVal = args.Invoke(args.Arguments);
                args.ReturnValue = returnVal;
                Record(key, stackPath, returnVal);
            }
        }

        private void Record(string key, string stack, object returnVal)
        {
            Console.WriteLine("Gravou valor do replay: " + stack + "(" + key + ")");
        }

        private object GetReplayValue(string key, string stack)
        {
            Console.WriteLine("Leu valor do replay: " + stack + "(" + key + ")");
            return null;
        }

        private string GetStackPath()
        {
            var stackTrace = new System.Diagnostics.StackTrace();

            Debug.Assert(stackTrace.GetFrames() != null);
            Debug.Assert(stackTrace != null);

            return string.Join("\r\n\t<-", stackTrace.GetFrames().Reverse().Select(x => x.GetMethod().ReflectedType + "." + x.GetMethod().Name));
        }

        private string GetCallInstanceKey(Arguments arguments)
        {
            return string.Join(", ", arguments.Select(x => x.ToString()));
        }
    }
}
