using Application.Behaviors;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Roles.Commands;

[Permission("Roles.Update")]
public sealed record UpdateRoleCommand(Guid Id, string Name, string Description) : IRequest<Result<Guid>>;

public sealed class UpdateRoleCommandValidation : AbstractValidator<UpdateRoleCommand>
{
  public UpdateRoleCommandValidation()
  {
    RuleFor(r => r.Id)
            .NotEmpty().WithMessage("Güncellenecek rolün Id bilgisi boş olamaz!");

    RuleFor(r => r.Name)
        .NotEmpty().WithMessage("Rol adı boş olamaz!")
        .NotNull().WithMessage("Rol adı mutlaka girilmelidir!")
        .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır!")
        .MaximumLength(50).WithMessage("Rol adı en fazla 50 karakter olabilir (Veritabanı sınırı)!")
        .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$").WithMessage("Rol adı sadece harflerden oluşmalıdır!");

    RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage("Rol açıklaması en fazla 250 karakter olabilir!");
  }
}


public sealed class UpdateRoleCommandHandler(IRoleRepository _roleRepo, IUnitOfWork _unit) : IRequestHandler<UpdateRoleCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(UpdateRoleCommand _req, CancellationToken _token)
  {
    // 1. Güncellenecek rol sistemde var mı?
    var role = await _roleRepo.FirstOrDefaultAsync(x => x.Id == _req.Id, _token, isTrackingActive: true);
    if (role is null)
      return Result<Guid>.Failure("Güncellenmek istenen rol sistemde bulunamadı!");

    // 2. İş Kuralı: Yeni isimle başka bir rol zaten var mı? (Unique Index çakışmasın)
    var cleanedName = _req.Name.Trim();
    if (role.Name != cleanedName)
    {
      var isNameExists = await _roleRepo.AnyAsync(r => r.Name == cleanedName, _token);
      if (isNameExists)
        return Result<Guid>.Failure($"'{cleanedName}' isimli başka bir rol sistemde zaten mevcut!");
    }

    // 3. Domain Entity üzerindeki güncelleme metodunu çağırıyoruz
    // (Entity içinde yazdığın ChangeName veya direkt Name property'si üzerinden)
    role.SetName(cleanedName);
    role.SetDescription(_req.Description);

    // Repository'deki Update metodu sadece state'i Modified çeker
    _roleRepo.Update(role);

    // 4. 🔥 Değişiklikleri Unit of Work ile veritabanına tek seferde gömüyoruz!
    await _unit.SaveChangesAsync(_token);

    return Result<Guid>.Succeed(role.Id);

  }
}