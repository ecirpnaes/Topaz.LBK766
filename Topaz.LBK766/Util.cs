#region

using System.Configuration;

#endregion

namespace Topaz.LBK766
{
    public class Util
    {
        public const string KEY_IMAGE_SAVE_PATH = "IMAGE_SAVE_PATH";
        public const string IMAGE_SAVE_FILENAME = "IMAGE_SAVE_FILENAME";

        public static string GetValueFromConfigFile(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}