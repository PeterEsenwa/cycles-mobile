namespace Cycles.Droid.Services
{
    public static class Constants
    {
        public const int DELAY_BETWEEN_LOG_MESSAGES = 1000; // milliseconds
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string BROADCAST_MESSAGE_KEY = "broadcast_message";
        public const string NOTIFICATION_BROADCAST_ACTION = "RideHandlerService.Notification.Action";

        public const string ACTION_START_SERVICE = "RideHandlerService.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "RideHandlerService.action.STOP_SERVICE";
        public const string ACTION_RESTART_TIMER = "RideHandlerService.action.RESTART_TIMER";
        public const string ACTION_MAIN_ACTIVITY = "RideHandlerService.action.MAIN_ACTIVITY";

        public const int SUCCESS_RESULT = 0;
        public const int FAILURE_RESULT = 1;
        public const string PACKAGE_NAME = "com.cycles.bikeshare";
        public const string RECEIVER = PACKAGE_NAME + ".RECEIVER";
        public const string RESULT_DATA_KEY = PACKAGE_NAME + ".RESULT_DATA_KEY";
        public const string RESULT_DATA_KEY2 = PACKAGE_NAME + ".RESULT_DATA_KEY2";
        public const string LOCATION_DATA_EXTRA = PACKAGE_NAME + ".LOCATION_DATA_EXTRA";
    }
}