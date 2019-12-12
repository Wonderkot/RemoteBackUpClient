using System;
using System.IO;
using System.Text;

namespace RemoteBackUpClient.Utils
{
    public static class FileUtils
    {
        public static void SaveFile(string b64, string dest, string fileName)
        {
            if (string.IsNullOrEmpty(b64) || string.IsNullOrEmpty(dest) || string.IsNullOrEmpty(fileName))
            {
                throw new Exception("Cannot save data.");
            }

            var bytes = Encoding.UTF8.GetBytes(b64);
            var fullPath = Path.Combine(dest, fileName);


            File.WriteAllBytes(fullPath, bytes);
        }
    }
}