using System;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;

namespace GlobalsOPCUA
{
    /// <summary>
    /// Cette classe permet de chiffrer et déchiffrer des chaînes de caractères, avec ou sans clé de hachage.
    /// </summary>
    class CryptString
    {
        #region Properties

        private bool _UseHashing;

        /// <summary>
        /// Clé de sécurité pour le chiffrage.
        /// </summary>
        private string _SecurityKey = "HMA0601!";

        #endregion

        #region Constructor

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="CryptString"/>.
        /// </summary>
        /// <param name="bUseHashing">Indique si l'on utilise une clé de hachage pour le chiffrement/déchiffrement.</param>
        public CryptString(bool bUseHashing)
        {
            _UseHashing = bUseHashing;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Chiffre une chaîne de caractères avec ou sans clé de sécurité.
        /// </summary>
        /// <param name="toEncrypt">Chaîne à chiffrer.</param>
        /// <returns>La chaîne chiffrée en base64.</returns>
        public string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string key = _SecurityKey;

            if (_UseHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// Déchiffre une chaîne de caractères avec ou sans clé de sécurité.
        /// </summary>
        /// <param name="cipherString">Chaîne chiffrée à déchiffrer (base64).</param>
        /// <returns>La chaîne déchiffrée ou <c>string.Empty</c> si erreur.</returns>
        public string Decrypt(string cipherString)
        {
            byte[] keyArray;
            string key = _SecurityKey;
            byte[] resultArray = null;

            try
            {
                byte[] toencryptArray = Convert.FromBase64String(cipherString);

                if (_UseHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                {
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);
                }

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider
                {
                    Key = keyArray,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransForm = tdes.CreateDecryptor();
                resultArray = cTransForm.TransformFinalBlock(toencryptArray, 0, toencryptArray.Length);
                tdes.Clear();
            }
            catch
            {
                return string.Empty;
            }

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// Convertit une chaîne sécurisée (<see cref="SecureString"/>) en string non sécurisée.
        /// </summary>
        /// <param name="secstring">La chaîne sécurisée à convertir.</param>
        /// <returns>La chaîne déchiffrée ou vide en cas d'échec.</returns>
        public string ConvertToUNSecureString(SecureString secstring)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                try
                {
                    unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secstring);
                    return Marshal.PtrToStringUni(unmanagedString);
                }
                catch
                {
                    return "";
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        #endregion
    }
}
