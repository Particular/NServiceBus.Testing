namespace NServiceBus.Testing
{
    using Encryption;

    public class FakeEncryptor : IEncryptionService
    {
        public EncryptedValue Encrypt(string value)
        {
            return new EncryptedValue
            {
                Base64Iv = value
            };
        }

        public string Decrypt(EncryptedValue encryptedValue)
        {
            return encryptedValue.Base64Iv;
        }
    }
}
