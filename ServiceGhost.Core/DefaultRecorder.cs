namespace ServiceGhost.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using FakeItEasy.Core;
    using FakeItEasy.SelfInitializedFakes;

    /// <summary>
    /// Manages the applying of recorded calls and recording of new calls when
    /// using self initialized fakes.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Implements the Disposable method to support the using statement only.")]
    public class DefaultRecorder : ISelfInitializingFakeRecorder, IInterceptionListener
    {
        private readonly CallQueue recordedCalls;
        private readonly ICallStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        public DefaultRecorder(ICallStorage storage)
        {
            this.storage = storage;

            var calls = storage.Load();

            this.IsRecording = calls == null;
            this.recordedCalls = CreateCallsList(calls);
        }

        /// <summary>
        /// Represents a factory responsible for creating recording manager
        /// instances.
        /// </summary>
        /// <param name="storage">The storage the manager should use.</param>
        /// <returns>A RecordingManager instance.</returns>
        internal delegate RecordingManager Factory(ICallStorage storage);

        /// <summary>
        /// Gets a value indicating whether the recorder is currently recording.
        /// </summary>
        /// <value></value>
        public bool IsRecording { get; internal set; }

        /// <summary>
        /// Applies the call if the call has been recorded.
        /// </summary>
        /// <param name="fakeObjectCall">The call to apply to from recording.</param>
        public void ApplyNext(IInterceptedFakeObjectCall fakeObjectCall)
        {
            var callToApply = this.recordedCalls.GetResponseForCall(fakeObjectCall);

            if (callToApply == null)
            {
                throw new Exception("Nenhum item gravado disponível para a operação");
            }

            // AssertThatMethodsMatches(fakeObjectCall, callToApply);
            ApplyOutputArguments(fakeObjectCall, callToApply);

            fakeObjectCall.SetReturnValue(callToApply.RecordedCall.ReturnValue);
            callToApply.HasBeenApplied = true;
        }

        /// <summary>
        /// Records the specified call.
        /// </summary>
        /// <param name="fakeObjectCall">The call to record.</param>
        public virtual void RecordCall(ICompletedFakeObjectCall fakeObjectCall)
        {
            var hash = fakeObjectCall.GetHashCode();
            var watch = watches[hash];
            watches.Remove(hash);
            watch.Stop();

            var callData = new CallData(fakeObjectCall.Method, CommonExtensions.GetOutputArgumentsForCall(fakeObjectCall), fakeObjectCall.ReturnValue);

            var item = new CallMetadata
                       {
                           HasBeenApplied = false,
                           RecordedCall = callData,
                           ElapsedTime = watch.ElapsedMilliseconds,
                           UniqueId = KeyGenerator.GetMethodKey(fakeObjectCall.FakedObject.GetType(), fakeObjectCall.Method, CommonExtensions.GetCurrentStack())
                       };

            this.recordedCalls.AddItem(item);
        }

        /// <summary>
        /// Saves all recorded calls to the storage.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Does not have a finalizer.")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "The dispose method is provided for enabling using statement only, virtual for testability.")]
        public virtual void Dispose()
        {
            this.storage.Save(this.recordedCalls.ToList().Select(x => x.RecordedCall));
        }

        private static void ApplyOutputArguments(IInterceptedFakeObjectCall call, CallMetadata callToApply)
        {
            foreach (var outputArgument in GetIndicesAndValuesOfOutputParameters(call, callToApply.RecordedCall))
            {
                call.SetArgumentValue(outputArgument.Item1, outputArgument.Item2);
            }
        }

        private static CallQueue CreateCallsList(IEnumerable<CallData> callsFromStorage)
        {
            if (callsFromStorage == null)
            {
                return new CallQueue();
            }

            var result = new CallQueue();
            foreach (var call in callsFromStorage)
            {
                result.AddItem(new CallMetadata { RecordedCall = call });
            }

            return result;
        }

        private static IEnumerable<Tuple<int, object>> GetIndicesAndValuesOfOutputParameters(IInterceptedFakeObjectCall call, CallData recordedCall)
        {
            return
                (from argument in call.Method.GetParameters().Zip(Enumerable.Range(0, int.MaxValue))
                 where argument.Item1.ParameterType.IsByRef
                 select argument.Item2).Zip(recordedCall.OutputArguments);
        }


        private Dictionary<int, Stopwatch> watches = new Dictionary<int, Stopwatch>();

        public void OnBeforeCallIntercepted(IFakeObjectCall interceptedCall)
        {
            var watch = new Stopwatch();
            watch.Start();
            watches[interceptedCall.GetHashCode()] = watch;
        }

        public FakerModeEnum Mode { get; set; }

        public void OnAfterCallIntercepted(ICompletedFakeObjectCall interceptedCall, IFakeObjectCallRule ruleThatWasApplied)
        {
        }
    }
}