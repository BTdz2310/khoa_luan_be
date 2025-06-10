using System.Security.Cryptography;
using learniverse_be.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public static class PasswordHelper
{
  public static HashSalt EncryptPassword(string password)
  {
    byte[] salt = new byte[128 / 8]; // Generate a 128-bit salt using a secure PRNG
    using (var rng = RandomNumberGenerator.Create())
    {
      rng.GetBytes(salt);
    }
    string encryptedPassw = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8
    ));
    return new HashSalt { Hash = encryptedPassw, Salt = salt };
  }

  public static bool VerifyPassword(string enteredPassword, byte[] salt, string storedPassword)
  {
    string encryptedPassw = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: enteredPassword,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8
    ));
    return encryptedPassw == storedPassword;
  }
}
