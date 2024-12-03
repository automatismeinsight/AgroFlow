using System;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;

namespace GlobalsOPCUA
{
    /// <summary>
    /// Cette classe permet de chiffrer et déchiffrer des chaines de caractères
    /// </summary>
    class CryptString
    {
        #region Properties

        private bool _UseHashing;

        /// <summary>
        ///  Clé de sécurité pour le chiffrage
        /// </summary>
        private string _SecurityKey = "HMA0601!";

        #endregion

        #region Constructor
        /// <summary>
        /// Constructeur de classe
        /// </summary>
        /// <param name="bUseHashing"> Indique si l'on utilise une clé de hachage </param>
        public CryptString(bool bUseHashing)
        {
            _UseHashing = bUseHashing;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Permet de chiffrer une chaine de caractères
        /// avec ou sans une clé de sécurité
        /// </summary>
        /// <param name="toEncrypt"> Chaine a chiffrer </param>
        /// <returns></returns>
        public string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string key = _SecurityKey; // On récupére la chaine de sécurité

            // Test si l'on applique une clé de sécurité sur notre chaine d'entrée
            if (_UseHashing)
            {
                // On chiffre notre chaine d'entrée par notre clé de sécurité
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
            {
                // Si pas de clé de sécurité alors on prend la chaine d'entrée
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            // On applique la clé secrète pour l'algo tripleDES
            tdes.Key = keyArray;
            // On choisi le mode Electronic Code Book
            tdes.Mode = CipherMode.ECB;
            // On applique le mode padding de type PKCS7
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            // On applique le chiffrement sur notre tableau de caractères
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            // Nettoyage des ressources pour l'algo de chiffrement
            tdes.Clear();

            // On retourne la chaine chiffrée en string
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// Permet de déchiffrer une chaine de caractères
        /// avec ou sans une clé de sécurité
        /// </summary>
        /// <param name="cipherString"></param>
        /// <returns></returns>
        public string Decrypt(string cipherString)
        {
            byte[] keyArray;
            string key = _SecurityKey;
            byte[] resultArray = null;

            try
            {

                // Récupération du tableau de byte représentant la chaine de caractères d'entrée
                byte[] toencryptArray = Convert.FromBase64String(cipherString);

                // Test si on utilise une clé de hachage de sécurité ?
                if (_UseHashing)
                {
                    // Calcul du code de hachage en fonction de la clé de sécurité
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                {
                    // Si pas de clé de sécurité alors on prend la chaine d'entrée
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);
                }

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                // Affectation de la clé de sécurité pour l'algo tripleDES
                tdes.Key = keyArray;
                // On choisi le mode Electronic Code Book
                tdes.Mode = CipherMode.ECB;
                // On applique le padding PKCS7
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransForm = tdes.CreateDecryptor();
                resultArray = cTransForm.TransformFinalBlock(toencryptArray, 0, toencryptArray.Length);
                // Nettoyage des ressources de l'algo
                tdes.Clear();
            }
            catch
            {
                return string.Empty;
            }

            // On retourne en clair la chaine déchiffrée
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// Permet de convertir une chaine sécurisée en string
        /// </summary>
        /// <param name="secstring"></param>
        /// <returns></returns>
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
