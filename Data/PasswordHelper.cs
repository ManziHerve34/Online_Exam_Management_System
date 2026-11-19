using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlineExamsSystem.Data
{
    public static class PasswordHelper
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hash = Convert.ToBase64String(hashBytes);
                return (hash, salt);
            }
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + storedSalt);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string computedHash = Convert.ToBase64String(hashBytes);
                return computedHash == storedHash;
            }
        }
    }
}
