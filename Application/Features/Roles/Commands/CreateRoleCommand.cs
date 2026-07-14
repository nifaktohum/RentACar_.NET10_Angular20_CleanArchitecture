using Application.Behaviors;
using Domain.Repositories;
using Domain.Roles;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Roles.Commands;

[Permission("Roles.Create")]
public sealed record CreateRoleCommand(string Name, string Description) : IRequest<Result<Guid>>;

public sealed class CreateRoleCommandValidation : AbstractValidator<CreateRoleCommand>
{
  public CreateRoleCommandValidation()
  {
    RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Rol adı boş olamaz!")
            .NotNull().WithMessage("Rol adı mutlaka girilmelidir!")
            .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır!")
            .MaximumLength(50).WithMessage("Rol adı en fazla 50 karakter olabilir (Veritabanı sınırı)!")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$").WithMessage("Rol adı sadece harflerden oluşmalıdır, sayı veya özel karakter içeremez!");

    RuleFor(r => r.Description)
        .MaximumLength(250).WithMessage("Rol açıklaması en fazla 250 karakter olabilir!");
  }
}

public sealed class CreateRoleCommandHandler(
                        IRoleRepository _roleRepo,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateRoleCommand _req, CancellationToken _token)
  {
    var cleanedName = _req.Name.Trim();
    var cleanedDescription = _req.Description?.Trim() ?? string.Empty;
    // 1. İş Kuralı: Aynı isimde iki rol olamaz! (DB'de de Unique index çakmıştık zaten)
    var isRoleExists = await _roleRepo.AnyAsync(r => r.Name == _req.Name.Trim(), _token);
    if (isRoleExists)
      throw new Exception($"'{_req.Name}' isimli rol sistemde zaten mevcut!");

    // 2. Yeni Role entity'sini ayağa kaldır (Senin yazdığın o yakışıklı constructor)
    // CreatedBy alanına o an bu rolü ekleyen Admin'in ID'sini geçebilirsin (Şimdilik simüle ettik)
    var newRole = new Role(cleanedName, cleanedDescription);

    // 3. Repository üzerinden kuyruğa ekle
    await _roleRepo.AddAsync(newRole, _token);
    await _unit.SaveChangesAsync(_token);

    return Result<Guid>.Succeed(newRole.Id);
  }
}