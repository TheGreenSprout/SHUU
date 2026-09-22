using System;
using System.Collections.Generic;
using UnityEngine;

using Alchemy.Inspector;

using SHUU.UserSide.Addons.SavingSystem;
using SHUU.Utils.Data;

namespace SHUU.UserSide.Addons
{
    [RequireComponent(typeof(SavingManager))]
    public class Sample_DataEncryptor : MonoBehaviour
    {
        #region Variables
        #region XML doc
        /// <summary>
        /// Applied in order when encrypting, and undone in the REVERSE order when decrypting (like peeling the layers of an onion back off).
        /// </summary>
        #endregion
        [Title("Encryption Modes")]
        [SerializeField] private EncryptionModeSettings[] modes = new EncryptionModeSettings[0];
        #endregion




        #region Hooks
        public void Encrypt(SavePayload payload) => payload.text = RunModes(payload.text, true);

        public void Decrypt(SavePayload payload) => payload.text = RunModes(payload.text, false);
        #endregion



        #region Logic
        private string RunModes(string text, bool forward)
        {
            if (modes == null || !this.enabled) return text;


            for (int i = forward ? 0 : modes.Length - 1; forward ? i < modes.Length : i >= 0; i += forward ? 1 : -1)
            {
                EncryptionModeSettings mode = modes[i];

                try { text = forward ? EncryptOnce(text, mode) : DecryptOnce(text, mode); }
                catch (Exception e) { throw new InvalidOperationException($"{(forward ? "Encryption" : "Decryption")} step {i} ({mode.type}) failed: {e.Message}", e); }

                if (text == null)
                {
                    string reason = forward ? "produced no output (check its key)" : "failed (its prefix wasn't found - wrong key, the modes don't match what encrypted it, or this data was never encrypted)";

                    throw new InvalidOperationException($"{(forward ? "Encryption" : "Decryption")} step {i} ({mode.type}) {reason}.");
                }
            }

            return text;
        }


        private static string EncryptOnce(string text, EncryptionModeSettings mode)
        {
            switch (mode.type)
            {
                case EncryptionTypes.AES:
                    RequireField(mode.aesKey, "an AES key");
                    return AES_Encryption.EncryptString(text, mode.aesKey, addPrefix: true);

                case EncryptionTypes.RSA:
                    RequireField(mode.rsaPublicKey, "an RSA public key");
                    return RSA_Encryption.EncryptString(text, mode.rsaPublicKey, addPrefix: true);

                case EncryptionTypes.BASE64:
                    return BASE64_Encryption.EncryptString(text, addPrefix: true);

                default:
                    throw new NotSupportedException($"Unhandled encryption type: {mode.type}");
            }
        }

        private static string DecryptOnce(string text, EncryptionModeSettings mode)
        {
            switch (mode.type)
            {
                case EncryptionTypes.AES:
                    RequireField(mode.aesKey, "an AES key");
                    return AES_Encryption.DecryptString(text, mode.aesKey, checkPrefix: true);

                case EncryptionTypes.RSA:
                    RequireField(mode.rsaPrivateKey, "an RSA private key");
                    return RSA_Encryption.DecryptString(text, mode.rsaPrivateKey, checkPrefix: true);

                case EncryptionTypes.BASE64:
                    return BASE64_Encryption.DecryptString(text, checkPrefix: true);

                default:
                    throw new NotSupportedException($"Unhandled encryption type: {mode.type}");
            }
        }

        private static void RequireField(string value, string what)
        {
            if (string.IsNullOrEmpty(value)) throw new InvalidOperationException($"This mode needs {what} (set it in the inspector).");
        }
        #endregion



        #region Editor Helpers
        #if UNITY_EDITOR
        [Button]
        private void GenerateAESKey() => Debug.Log($"New AES-256 key (paste into an AES mode's 'Aes Key' field):\n{AES_Encryption.GenerateKey()}");

        [Button]
        private void GenerateRSAKeyPair()
        {
            List<string> keys = RSA_Encryption.GenerateKeys();

            Debug.Log($"New RSA-2048 key pair (paste into an RSA mode's fields):\nPublic ('Rsa Public Key'): {keys[0]}\nPrivate ('Rsa Private Key', keep this one secret): {keys[1]}");
        }
        #endif
        #endregion




        #region Mode
        [Serializable]
        public class EncryptionModeSettings
        {
            #region XML doc
            /// <summary>
            /// Which encryption to apply for this step.
            /// RSA can only encrypt small amounts of data directly (about 245 bytes for a 2048-bit key) - it throws above that, it doesn't truncate or corrupt anything.
            /// A save file's json is almost always bigger than that, so RSA on its own isn't practical for Save Data Write/Read; AES or BASE64 have no such limit.
            /// </summary>
            #endregion
            public EncryptionTypes type = EncryptionTypes.BASE64;


            [ShowIf(nameof(NeedsAESKey))]
            [Tooltip("Base64 AES-256 key, the same one is used to encrypt and decrypt. Generate one with the button below, or from code with AES_Encryption.GenerateKey().")]
            public string aesKey = "";


            [ShowIf(nameof(NeedsRSAKeys))]
            [Tooltip("RSA public key (XML), used to encrypt. Only good for small payloads (~245 bytes for a 2048-bit key) - too big and this step throws.")]
            public string rsaPublicKey = "";

            [ShowIf(nameof(NeedsRSAKeys))]
            [Tooltip("RSA private key (XML), used to decrypt. Keep this one out of builds that only ever need to encrypt.")]
            public string rsaPrivateKey = "";


            private bool NeedsAESKey => type == EncryptionTypes.AES;
            private bool NeedsRSAKeys => type == EncryptionTypes.RSA;
        }
        #endregion
    }
}
