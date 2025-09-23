using System.Collections.Generic;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SHUU.Utils.Helpers;

namespace SHUU.Utils.Data
{


#region General

#region XML doc
/// <summary>
/// Contains all the encryption types included in SHUU.
/// </summary>
#endregion
public enum EncryptionTypes{
    AES,
    RSA,
    BASE64
}


#region XML doc
/// <summary>
/// Manages all basic encryption-related methods.
/// </summary>
#endregion
public static class EncryptionBasics{
    #region XML doc
    /// <summary>
    /// Function that allows you to get all the prefixes for all the encryption types.
    /// </summary>
    /// <returns>Returns a list with the prefixes of each encryption type.</returns>
    #endregion
    public static List<string> GetEncryptionPrefixes(){
        return new List<string>{
            "AES_",
            "RSA_",
            "B64_"
        };
    }
    
    #region XML doc
    /// <summary>
    /// Function that allows you to get an encryption type prefix.
    /// </summary>
    /// <param name="encryptionType">The type of encryption you want the prefix of.</param>
    /// <returns>Returns the prefix used for the type of encryption you input.</returns>
    #endregion
    public static string GetEncryptionPrefixByEnum(EncryptionTypes encryptionType){
        return GetEncryptionPrefixes()[(int) encryptionType];
    }


    #region XML doc
    /// <summary>
    /// Gets the bytes of a string.
    /// </summary>
    /// <param name="data">The string you want to get the bytes of.</param>
    /// <returns>Returns the bytes (byte[]) of the string.</returns>
    #endregion
    public static byte[] GetBytesOfString(string data){
        return Encoding.UTF8.GetBytes(data);
    }

    #region XML doc
    /// <summary>
    /// Translates bytes into a string.
    /// </summary>
    /// <param name="bytes">The bytes you want to translate into a string.</param>
    /// <returns>Returns the translated string.</returns>
    #endregion
    public static string GetStringFromBytes(byte[] bytes){
        return Encoding.UTF8.GetString(bytes);
    }
}

#endregion



#region AES-256 encryption // Faster, less secure

#region XML doc
/// <summary>
/// Manages AES encryption.
/// </summary>
#endregion
public static class AES_Encryption{
    #region XML doc
    /// <summary>
    /// Generates an AES encryption key.
    /// </summary>
    /// <returns>Returns the AES key.</returns>
    #endregion
    public static string GenerateKey(){
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256; // AES-256
            aes.GenerateKey();

            return Convert.ToBase64String(aes.Key); // Convert key to Base64 for easy storage
        }
    }


    #region XML doc
    /// <summary>
    /// Encrypts a string using AES.
    /// </summary>
    /// <param name="stringToEncrypt">The string you want to encrypt.</param>
    /// <param name="aesKey">The AES key you wish to use to encrypt the string.</param>
    /// <param name="addPrefix">Whether you want a prefix (AES_) to be added after the encryption.</param>
    /// <returns>Returns a string encrypted with AES.</returns>
    #endregion
    public static string EncryptString(string stringToEncrypt, string aesKey, bool addPrefix){
        byte[] key = Convert.FromBase64String(aesKey); // Convert key from Base64


        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // Generate a new IV for every encryption

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length); // Store IV at the beginning

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                {
                    byte[] inputBytes = EncryptionBasics.GetBytesOfString(stringToEncrypt);

                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                    

                    string retS = Convert.ToBase64String(ms.ToArray()); // Convert to Base64 for storage
                    if (addPrefix)
                    {
                        retS = EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.AES) + retS;
                    }

                    return retS;
                }
            }
        }
    }

    #region XML doc
    /// <summary>
    /// Decrypts a string using AES.
    /// </summary>
    /// <param name="encryptedString">The string you want to decrypt.</param>
    /// <param name="aesKey">The AES key you wish to use to decrypt the string (must be the same one used to encrypt it).</param>
    /// <param name="checkPrefix">Whether you want to check for a prefix (AES_) from the encrypted string (must match the same bool param used in encryption).</param>
    /// <returns>Returns a string decrypted using AES.</returns>
    #endregion
    public static string DecryptString(string encryptedString, string aesKey, bool checkPrefix){
        if (checkPrefix && !HandyFunctions.CheckAndPopSubstring(ref encryptedString, EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.AES), 0, true))
        {
            return null;
        }



        byte[] key = Convert.FromBase64String(aesKey);
        byte[] cipherBytes = Convert.FromBase64String(encryptedString);


        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                byte[] iv = new byte[16]; // AES IV is always 16 bytes (128-bit)

                ms.Read(iv, 0, iv.Length); // Extract the IV from the encrypted data
                aes.IV = iv;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                        
                    }
                }
            }
        }
    }
}

#endregion



#region RSA-2048 encryption // Slower, more secure

#region XML doc
/// <summary>
/// Manages RSA encryption.
/// </summary>
#endregion
public static class RSA_Encryption{
    #region XML doc
    /// <summary>
    /// Generates 2 RSA encryption keys, a public one and a private one.
    /// </summary>
    /// <returns>Returns both of the RSA keys. [First one is the public and second one is the private]</returns>
    #endregion
    public static List<string> GenerateKeys(){
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
        {
            return new List<string>{
                rsa.ToXmlString(false) /*Public key*/,
                rsa.ToXmlString(true) /*Private key*/
            };
        }
    }

    
    #region XML doc
    /// <summary>
    /// Encrypts a string using RSA.
    /// </summary>
    /// <param name="stringToEncrypt">The string you want to encrypt.</param>
    /// <param name="publicKey">The RSA public key you wish to use to encrypt the string.</param>
    /// <param name="addPrefix">Whether you want a prefix (RSA_) to be added after the encryption.</param>
    /// <returns>Returns a string encrypted with RSA.</returns>
    #endregion
    public static string EncryptString(string stringToEncrypt, string publicKey, bool addPrefix){
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.FromXmlString(publicKey);

            byte[] data = EncryptionBasics.GetBytesOfString(stringToEncrypt);
            byte[] encryptedData = rsa.Encrypt(data, false);


            string retS = Convert.ToBase64String(encryptedData);
            if (addPrefix)
            {
                retS = EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.RSA) + retS;
            }

            return retS;
        }
    }

    #region XML doc
    /// <summary>
    /// Decrypts a string using RSA.
    /// </summary>
    /// <param name="encryptedString">The string you want to decrypt.</param>
    /// <param name="privateKey">The RSA private key you wish to use to decrypt the string (must be the same one generated along the public one used to encrypt it).</param>
    /// <param name="checkPrefix">Whether you want to check for a prefix (RSA_) from the encrypted string (must match the same bool param used in encryption).</param>
    /// <returns>Returns a string decrypted using RSA.</returns>
    #endregion
    public static string DecryptString(string encryptedString, string privateKey, bool checkPrefix){
        if (checkPrefix && !HandyFunctions.CheckAndPopSubstring(ref encryptedString, EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.RSA), 0, true))
        {
            return null;
        }



        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.FromXmlString(privateKey);

            byte[] data = Convert.FromBase64String(encryptedString);
            byte[] decryptedData = rsa.Decrypt(data, false);

            return EncryptionBasics.GetStringFromBytes(decryptedData);
        }
    }
}

#endregion



#region Base64 encryption // Very fast, not secure

#region XML doc
/// <summary>
/// Manages Base64 encryption.
/// </summary>
#endregion
public static class BASE64_Encryption{
    #region XML doc
    /// <summary>
    /// Encrypts a string using Base64.
    /// </summary>
    /// <param name="stringToEncrypt">The string you want to encrypt.</param>
    /// <param name="addPrefix">Whether you want a prefix (B64_) to be added after the encryption.</param>
    /// <returns>Returns a string encrypted with Base64.</returns>
    #endregion
    public static string EncryptString(string stringToEncrypt, bool addPrefix){
        byte[] data = EncryptionBasics.GetBytesOfString(stringToEncrypt);


        string retS = Convert.ToBase64String(data);
        if (addPrefix)
        {
            retS = EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.BASE64) + retS;
        }

        return retS;
    }
    #region XML doc
    /// <summary>
    /// Encrypts a byte sequence using Base64.
    /// </summary>
    /// <param name="bytesToEncrypt">The byte sequence you want to encrypt.</param>
    /// <param name="addPrefix">Whether you want a prefix (B64_) to be added after the encryption.</param>
    /// <returns>Returns a string encrypted with Base64.</returns>
    #endregion
    public static string EncryptString(byte[] bytesToEncrypt, bool addPrefix){
        string retS = Convert.ToBase64String(bytesToEncrypt);
        if (addPrefix)
        {
            retS = EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.BASE64) + retS;
        }

        return retS;
    }

    #region XML doc
    /// <summary>
    /// Decrypts a string using Base64.
    /// </summary>
    /// <param name="encryptedString">The string you want to decrypt.</param>
    /// <param name="checkPrefix">Whether you want to check for a prefix (B64_) from the encrypted string (must match the same bool param used in encryption).</param>
    /// <returns>Returns a string decrypted using AES.</returns>
    #endregion
    public static string DecryptString(string encryptedString, bool checkPrefix){
        if (checkPrefix && !HandyFunctions.CheckAndPopSubstring(ref encryptedString, EncryptionBasics.GetEncryptionPrefixByEnum(EncryptionTypes.BASE64), 0, true))
        {
            return null;
        }


        byte[] data = Convert.FromBase64String(encryptedString);

        return EncryptionBasics.GetStringFromBytes(data);
    }
}

#endregion

}
