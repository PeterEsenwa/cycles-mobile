using System.Runtime.InteropServices;
using Java.Lang;

namespace Cycles.Droid.Utils
{
    public class NativeController
    {
        public NativeController()
        {
            try
            {
                JavaSystem.LoadLibrary("jimi-native-lib");
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getClientCharacteristicConfig")]
        public static extern string getClientCharacteristicConfig();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getBltServerUUID")]
        public static extern string getBltServerUUID();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getLockReadwriteUuid")]
        public static extern string getLockReadwriteUuid();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getWriteDataUUID")]
        public static extern string getWriteDataUUID();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getReadDataUUID")]
        public static extern string getReadDataUUID();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getCmdDisuoUp")]
        public static extern string getCmdDisuoUp();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getCmdDisuoUp2")]
        public static extern byte[] getCmdDisuoUp2();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getEncyptKey")]
        public static extern byte[] getEncyptKey();

        [DllImport("jimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getTokenCmd")]
        public static extern byte[] getTokenCmd();

        //[DllImport("libjimi-native-lib", EntryPoint = "Java_architecture_com_jimi_NativeController_getBltServerUUID")]
        //public static extern string GetBltServerUUID();
    }
}