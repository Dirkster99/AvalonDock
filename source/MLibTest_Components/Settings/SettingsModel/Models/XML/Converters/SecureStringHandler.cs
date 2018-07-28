namespace SettingsModel.Models.XML.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;

    /// <summary>
    /// Source of string encryption and decryption:
    /// http://weblogs.asp.net/jongalloway//encrypting-passwords-in-a-net-app-config-file
    /// </summary>
    internal class SecureStringHandler : SettingsModel.Models.XML.Converters.IAlternativeDataTypeHandler
    {
        #region fields
        private static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("Salt Is Usually Not A Password");
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public SecureStringHandler()
        {
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the type of the original data type that is to be replaced
        /// with an alternative (typed) representation.
        /// </summary>
        public Type SourceDataType
        {
            get
            {
                return typeof(SecureString);
            }
        }

        /// <summary>
        /// Gets the type of the target data type that is to be used
        /// instead of the original (typed) representation.
        /// </summary>
        public Type TargetDataType
        {
            get
            {
                return typeof(string);
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Converts from the source datatype into the target data type representation.
        /// </summary>
        /// <param name="objectInput"></param>
        /// <returns></returns>
        public object Convert(object objectInput)
        {
            SecureString input = objectInput as SecureString;

            if (input == null)
                return null;

            byte[] encryptedData = null;
            try
            {
                encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                    System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                string result = System.Convert.ToBase64String(encryptedData);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (encryptedData != null)
                {
                    for (int i = 0; i < encryptedData.Length; i++)
                    {
                        encryptedData[i] = 0;
                    }
                    encryptedData = null;
                }
            }
        }

        /// <summary>
        /// Converts from the target datatype into the source data type representation.
        /// </summary>
        /// <param name="objectEncryptedData"></param>
        /// <returns></returns>
        public object ConvertBack(object objectEncryptedData)
        {
            string encryptedData = objectEncryptedData as string;

            if (encryptedData == null)
                return null;

            byte[] decryptedData = null;
            try
            {
                decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    System.Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                SecureString s = ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));

                return s;
            }
            catch
            {
                return new SecureString();
            }
            finally
            {
                if (decryptedData != null)
                {
                    for (int i = 0; i < decryptedData.Length; i++)
                    {
                        decryptedData[i] = 0;
                    }
                    decryptedData = null;
                }
            }
        }

        #region private methods
        private SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        private string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
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
        #endregion private methods
        #endregion methods
    }
}
