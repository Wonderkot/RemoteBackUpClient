using System;
using System.IO;
using Newtonsoft.Json;
using RemoteBackUpClient.Data;

namespace RemoteBackUpClient.Utils
{
    public class SettingsReader
    {
        public static Settings GetSettings()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = Path.Combine(dir, "Settings.json");

            if (!File.Exists(fileName))
            {
                return null;
            }


            string source;
            try
            {
                source = File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                return null;
            }

            Settings settings = JsonConvert.DeserializeObject<Settings>(source);

            return settings;
        }

        public static void Save(Settings settings)
        {
            var jsonStr = JsonConvert.SerializeObject(settings);

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = Path.Combine(dir, "Settings.json");

            if (!File.Exists(fileName))
            {
                return;
            }

            try
            {
                File.WriteAllText(fileName, jsonStr);
            }
            catch (Exception e)
            {
                return;
            }

        }
    }
}