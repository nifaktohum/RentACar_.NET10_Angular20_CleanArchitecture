using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Users;

namespace Application.Services;

public interface IJwtProvider
{
  // // 🔥 SİHİRLİ DOKUNUŞ: Arayüz (interface) seviyesinde de rememberMe parametresini opsiyonel (default false) olarak tanımlıyoruz kanka.
  // // Böylece bu interface'i implement eden tüm sınıflar bu kuralla el sıkışmak zorunda kalacak.
  string CreateToken(User user, List<string> roles, string branchName, List<string> permissions, bool rememberMe = false);
}
