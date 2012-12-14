using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using osu_common.Libraries;

namespace osu_common.Libraries
{
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class AesManaged : Aes
    {
        // Fields
        private RijndaelManaged m_rijndael;

        // Methods
        public AesManaged()
        {
            /*
            if (CoreCryptoConfig.EnforceFipsAlgorithms)
            {
                throw new InvalidOperationException(SR.GetString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            */
            this.m_rijndael = new RijndaelManaged();
            this.m_rijndael.BlockSize = this.BlockSize;
            this.m_rijndael.KeySize = this.KeySize;
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return this.m_rijndael.CreateDecryptor();
        }

        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException("Invalid key size", "key");
            }
            if ((iv != null) && ((iv.Length * 8) != base.BlockSizeValue))
            {
                throw new ArgumentException("Invalid IV size", "iv");
            }
            return this.m_rijndael.CreateDecryptor(key, iv);
        }

        public override ICryptoTransform CreateEncryptor()
        {
            return this.m_rijndael.CreateEncryptor();
        }

        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException("Invalid key size", "key");
            }
            if ((iv != null) && ((iv.Length * 8) != base.BlockSizeValue))
            {
                throw new ArgumentException("Invalid IV size", "iv");
            }
            return this.m_rijndael.CreateEncryptor(key, iv);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ((IDisposable)this.m_rijndael).Dispose();
            }
        }

        public override void GenerateIV()
        {
            this.m_rijndael.GenerateIV();
        }

        public override void GenerateKey()
        {
            this.m_rijndael.GenerateKey();
        }

        // Properties
        public override int FeedbackSize
        {
            get
            {
                return this.m_rijndael.FeedbackSize;
            }
            set
            {
                this.m_rijndael.FeedbackSize = value;
            }
        }

        public override byte[] IV
        {
            get
            {
                return this.m_rijndael.IV;
            }
            set
            {
                this.m_rijndael.IV = value;
            }
        }

        public override byte[] Key
        {
            get
            {
                return this.m_rijndael.Key;
            }
            set
            {
                this.m_rijndael.Key = value;
            }
        }

        public override int KeySize
        {
            get
            {
                return this.m_rijndael.KeySize;
            }
            set
            {
                this.m_rijndael.KeySize = value;
            }
        }

        public override CipherMode Mode
        {
            get
            {
                return this.m_rijndael.Mode;
            }
            set
            {
                if ((value == CipherMode.CFB) || (value == CipherMode.OFB))
                {
                    throw new CryptographicException("Invalid cipher mode");
                }
                this.m_rijndael.Mode = value;
            }
        }

        public override PaddingMode Padding
        {
            get
            {
                return this.m_rijndael.Padding;
            }
            set
            {
                this.m_rijndael.Padding = value;
            }
        }
    }
}