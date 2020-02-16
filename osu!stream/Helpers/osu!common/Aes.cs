using System.Security.Cryptography;
using System.Security.Permissions;

namespace osum.Helpers
{
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public abstract class Aes : SymmetricAlgorithm
    {
        // Fields
        private static readonly KeySizes[] s_legalBlockSizes = { new KeySizes(0x80, 0x80, 0) };
        private static readonly KeySizes[] s_legalKeySizes = { new KeySizes(0x80, 0x100, 0x40) };

        // Methods
        protected Aes()
        {
            LegalBlockSizesValue = s_legalBlockSizes;
            LegalKeySizesValue = s_legalKeySizes;
            BlockSizeValue = 0x80;
            FeedbackSizeValue = 8;
            KeySizeValue = 0x100;
            ModeValue = CipherMode.CBC;
        }

        /*
        public static Aes Create()
        {
            return Create(typeof(AesCryptoServiceProvider).FullName);
        }
        
        public static Aes Create(string algorithmName)
        {
            if (algorithmName == null)
            {
                throw new ArgumentNullException("algorithmName");
            }
            return CoreCryptoConfig.CreateFromName<Aes>(algorithmName);
        }
        */
    }
}