using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ABSAProject.Utility
{
    public static class PasswordHasher
    {
        public static string HashPassword(string RawPassword)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string result = Convert.ToBase64String(KeyDerivation.Pbkdf2(RawPassword, salt, KeyDerivationPrf.HMACSHA1, 10000, 32));
            return result;
        }
    }
}
