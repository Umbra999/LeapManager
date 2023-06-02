namespace LeapManager
{
    internal class Boot
    {
        public static Leap.Controller LeapController;
        public static void Main()
        {
            Initialize();
            PipeConnection.StartPipe();
        }

        public static void Initialize()
        {
            LeapController = new Leap.Controller();
            LeapController.Device += OnLeapDeviceInitialized;
            LeapController.StartConnection();
        }

        public static void UnInitialize()
        {
            LeapController.StopConnection();
        }

        private static void OnLeapDeviceInitialized(object p_sender, Leap.DeviceEventArgs p_args)
        {
            LeapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
            LeapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }
    }
}
