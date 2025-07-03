using System;
using System.Security.Cryptography;

namespace WEBSITE_TRAVELBOOKING.Utilities
{
    public static class OtpUtility
    {
        public static string GenerateOtp(int length = 6)
        {
            // Generate a cryptographically secure random number
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);

                // Convert to numeric OTP
                string otp = "";
                for (int i = 0; i < length; i++)
                {
                    otp += (data[i] % 10).ToString();
                }
                return otp;
            }
        }
    }
} 