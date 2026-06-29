using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services;

public interface IEmailService
{
  // // 🔥 Kurumsal mail motorumuzun asenkron kontratı kanka
  Task SendEmailAsync(string to, string subject, string body);
}
