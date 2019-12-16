using System;
using System.IO;
using System.Security;
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
            catch (Exception)
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
            catch (Exception)
            {
                // ignored
            }
        }

        static readonly byte[] Entropy = System.Text.Encoding.Unicode.GetBytes("Salt Is Not A Password");

        public static string EncryptString(SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                Entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    Entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}