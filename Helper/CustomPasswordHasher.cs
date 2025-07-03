using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WEBSITE_TRAVELBOOKING.Helper
{
    public class CustomPasswordHasher
    {
        private readonly bool _useAspNetCore;
        private readonly byte _formatMarker;
        private readonly KeyDerivationPrf _prf; // Requires Microsoft.AspNetCore
        private readonly HashAlgorithmName _hashAlgorithmName;
        private readonly bool _includeHeaderInfo;
        private readonly int _saltLength;
        private readonly int _requestedLength;
        private readonly int _iterCount;
        public CustomPasswordHasher()
        {
            _useAspNetCore = true;
            _formatMarker = 0x01;
            _prf = KeyDerivationPrf.HMACSHA256;
            _hashAlgorithmName = HashAlgorithmName.SHA256;
            _includeHeaderInfo = true;
            _saltLength = 128 / 8;
            _requestedLength = 256 / 8;
            _iterCount = 10000;
        }
        public bool VerifyPasswordMD5(string hashedPassword, string enteredPassword)
        {
            return hashedPassword == CreateBase64(enteredPassword);
        }
        public string CreateBase64(string input)
        {
            byte[] bytes = new UnicodeEncoding().GetBytes(input);
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(bytes));
        }
    }
}
