using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services;

public interface IEmailService
{
  Task SendEmailAsync(string to, string subject, string body);
  Task<bool> SendVerificationCodeAsync(string to, string code, CancellationToken cancellationToken = default);
}
