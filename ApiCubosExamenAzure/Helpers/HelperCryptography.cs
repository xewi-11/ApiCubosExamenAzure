using System.Security.Cryptography;
using System.Text;

namespace ApiOAuthEmpleados.Helpers
{
    public static class HelperCryptography
    {
        private static string KeyCifrado;

        public static void Initialize(IConfiguration configuration)
        {
            // Leemos con la "h" (Chyper) y minúsculas (key) ya que así está en el secreto de Key Vault en tu captura de pantalla de Azure ("Chyper--key").
            KeyCifrado = configuration.GetValue<string>("Chyper:key");
        }

        public static string CifrarString(string data)
        {
            byte[] keyData = GetKeyData();
            string res = EncryptString(keyData, data);
            return res;
        }

        public static string DescifrarString(string data)
        {
            byte[] keyData = GetKeyData();
            string res = DecryptString(keyData, data);
            return res;
        }

        private static byte[] GetKeyData()
        {
            string key = KeyCifrado ?? string.Empty;
            return SHA256.HashData(Encoding.UTF8.GetBytes(key));
        }

        private static string EncryptString(byte[] key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string DecryptString(byte[] key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
