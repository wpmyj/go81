using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MongoDB
{
    public static class Hash
    {
        private const string desPubKey = "Id`gD9~K";

        public enum HashAlgorithm
        {
            MD5,
            SHA1,
            SHA256,
        }
        public const HashAlgorithm DefaultHashAlgorithm = HashAlgorithm.SHA1;
        public enum EncodeAlgorithm
        {
            DES,
        }
        public const EncodeAlgorithm DefaultEncodeAlgorithm = EncodeAlgorithm.DES;

        #region Hex-Binary
        private static char NibbleToHex(byte nibble)
        {
            return ((nibble < 10) ? ((char)(nibble + 0x30)) : ((char)((nibble - 10) + 0x41)));
        }
        private static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }
        public static string BinaryToHex(byte[] data)
        {
            if (null == data) return null;

            char[] chArray = new char[data.Length * 2];
            for (int i = data.Length - 1; i >= 0 ; --i)
			{
                byte num2 = data[i];
                chArray[2 * i] = NibbleToHex((byte)(num2 >> 4));
                chArray[(2 * i) + 1] = NibbleToHex((byte)(num2 & 15));
			}
            return new string(chArray);
        }
        public static byte[] HexToBinary(string data)
        {
            if ((data == null) || ((data.Length % 2) != 0))
            {
                return null;
            }
            byte[] buffer = new byte[data.Length / 2];
            for (int i = 0; i < buffer.Length; i++)
            {
                int num2 = HexToInt(data[2 * i]);
                int num3 = HexToInt(data[(2 * i) + 1]);
                if ((num2 == -1) || (num3 == -1))
                {
                    return null;
                }
                buffer[i] = (byte)((num2 << 4) | num3);
            }
            return buffer;
        }
        #endregion

        #region 散列
        public static byte[] ComputeBinary(string s, HashAlgorithm ha = DefaultHashAlgorithm)
        {
            switch (ha)
            {
                case HashAlgorithm.MD5: return MD5Binary(s);
                case HashAlgorithm.SHA1: return SHA1Binary(s);
                case HashAlgorithm.SHA256: return SHA256Binary(s);
            }
            return SHA1Binary(s);
        }
        public static string Compute(string s, HashAlgorithm ha = DefaultHashAlgorithm)
        {
            return BinaryToHex(ComputeBinary(s, ha));
        }

        public static byte[] MD5Binary(string s)
        {
            using (var md5 = new MD5CryptoServiceProvider())
                return md5.ComputeHash(Encoding.UTF8.GetBytes(s));
        }
        public static string MD5(string s)
        {
            return BinaryToHex(MD5Binary(s));
        }

        public static byte[] SHA1Binary(string s)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
        }
        public static string SHA1(string s)
        {
            return BinaryToHex(SHA1Binary(s));
        }

        public static byte[] SHA256Binary(string s)
        {
            using (var sha2 = new SHA256CryptoServiceProvider())
                return sha2.ComputeHash(Encoding.UTF8.GetBytes(s));
        }
        public static string SHA256(string s)
        {
            return BinaryToHex(SHA256Binary(s));
        }
        #endregion

        #region 加解密
        public static string Enc(string data, EncodeAlgorithm ea = DefaultEncodeAlgorithm, string pubKey = desPubKey)
        {
            return BinaryToHex(EncToBinary(data, ea, pubKey));
        }
        public static byte[] EncToBinary(string data, EncodeAlgorithm ea = DefaultEncodeAlgorithm, string pubKey = desPubKey)
        {
            switch (ea)
            {
                case EncodeAlgorithm.DES: return DesEncToBinary(data, pubKey);
            }
            return DesEncToBinary(data, pubKey);
        }
        public static string Dec(string data, EncodeAlgorithm ea = DefaultEncodeAlgorithm, string pubKey = desPubKey)
        {
            return DecFromBinary(HexToBinary(data), ea, pubKey);
        }
        public static string DecFromBinary(byte[] data, EncodeAlgorithm ea = DefaultEncodeAlgorithm, string pubKey = desPubKey)
        {
            switch (ea)
            {
                case EncodeAlgorithm.DES: return DesDecFromBinary(data, pubKey);
            }
            return DesDecFromBinary(data, pubKey);
        }

        public static string DesEnc(string data, string pubKey = desPubKey)
        {
            return BinaryToHex(DesEncToBinary(data, pubKey));
        }
        public static byte[] DesEncToBinary(string data, string pubKey = desPubKey)
        {
            if (null == data || null == pubKey) return null;
            using (var des = new DESCryptoServiceProvider())
            {
                des.Key = des.IV = Encoding.UTF8.GetBytes(desPubKey);
                var bytes = Encoding.UTF8.GetBytes(data);
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }
        public static string DesDec(string data, string pubKey = desPubKey)
        {
            return DesDecFromBinary(HexToBinary(data), pubKey);
        }
        public static string DesDecFromBinary(byte[] data, string pubKey = desPubKey)
        {
            if (null == data || null == pubKey) return null;
            using (var des = new DESCryptoServiceProvider())
            {
                des.Key = des.IV = Encoding.UTF8.GetBytes(desPubKey);
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        #endregion
    }
}