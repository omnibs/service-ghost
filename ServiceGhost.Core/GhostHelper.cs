namespace ServiceGhost.Core
{
    using FakeItEasy;
    using FakeItEasy.SelfInitializedFakes;

    public static class GhostHelper
    {
        /// <summary>
        /// Gravador
        /// </summary>
        private static DefaultRecorder recorder;

        /// <summary>
        /// Is initialized ?
        /// </summary>
        private static bool initialized;

        public static FakerModeEnum Mode
        {
            get
            {
                if (recorder == null)
                {
                    return FakerModeEnum.Save;
                }

                return recorder.Mode;
            }
            set
            {
                if (recorder != null)
                {
                    recorder.Mode = value;
                    recorder.IsRecording = recorder.Mode == FakerModeEnum.Save;
                }
            }
        }

        public static void SetStore(ICallStorage storage)
        {
            recorder = new DefaultRecorder(storage);
            initialized = true;
        }

        public static T CreateFake<T>(T instance)
        {
            if (!initialized)
            {
                recorder = new DefaultRecorder(new DefaultStorage());
                initialized = true;
            }

            var fake = A.Fake<T>(x => x.Wrapping(instance).RecordedBy(recorder));
            Fake.GetFakeManager(fake).AddInterceptionListener(recorder);
            return fake;
        }
    }
}
