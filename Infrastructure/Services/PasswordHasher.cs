using Application.Services;

namespace Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
  public string HashPassword(string password)
  {
    // Düz metin şifreyi alır ve güvenli bir şekilde hash'ler
    return BCrypt.Net.BCrypt.HashPassword(password);
  }

  public bool VerifyPassword(string password, string passwordHash)
  {
    // Giriş yapılırken şifrelerin eşleşip eşleşmediğini kontrol eder
    return BCrypt.Net.BCrypt.Verify(password, passwordHash);
  }
}
