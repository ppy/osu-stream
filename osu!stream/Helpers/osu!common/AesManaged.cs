using System;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace osum.Helpers
{
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class AesManaged : Aes
    {
        // Fields
        private readonly RijndaelManaged m_rijndael;

        // Methods
        public AesManaged()
        {
            /*
            if (CoreCryptoConfig.EnforceFipsAlgorithms)
            {
                throw new InvalidOperationException(SR.GetString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            */
            m_rijndael = new RijndaelManaged();
            m_rijndael.BlockSize = BlockSize;
            m_rijndael.KeySize = KeySize;
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return m_rijndael.CreateDecryptor();
        }

        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException("Invalid key size", "key");
            }

            if ((iv != null) && ((iv.Length * 8) != BlockSizeValue))
            {
                throw new ArgumentException("Invalid IV size", "iv");
            }

            return m_rijndael.CreateDecryptor(key, iv);
        }

        public override ICryptoTransform CreateEncryptor()
        {
            return m_rijndael.CreateEncryptor();
        }

        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException("Invalid key size", "key");
            }

            if ((iv != null) && ((iv.Length * 8) != BlockSizeValue))
            {
                throw new ArgumentException("Invalid IV size", "iv");
            }

            return m_rijndael.CreateEncryptor(key, iv);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ((IDisposable)m_rijndael).Dispose();
            }
        }

        public override void GenerateIV()
        {
            m_rijndael.GenerateIV();
        }

        public override void GenerateKey()
        {
            m_rijndael.GenerateKey();
        }

        // Properties
        public override int FeedbackSize
        {
            get => m_rijndael.FeedbackSize;
            set => m_rijndael.FeedbackSize = value;
        }

        public override byte[] IV
        {
            get => m_rijndael.IV;
            set => m_rijndael.IV = value;
        }

        public override byte[] Key
        {
            get => m_rijndael.Key;
            set => m_rijndael.Key = value;
        }

        public override int KeySize
        {
            get => m_rijndael.KeySize;
            set => m_rijndael.KeySize = value;
        }

        public override CipherMode Mode
        {
            get => m_rijndael.Mode;
            set
            {
                if ((value == CipherMode.CFB) || (value == CipherMode.OFB))
                {
                    throw new CryptographicException("Invalid cipher mode");
                }

                m_rijndael.Mode = value;
            }
        }

        public override PaddingMode Padding
        {
            get => m_rijndael.Padding;
            set => m_rijndael.Padding = value;
        }
    }
}