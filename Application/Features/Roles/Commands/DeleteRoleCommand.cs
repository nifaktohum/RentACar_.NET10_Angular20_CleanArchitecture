using Application.Behaviors;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Roles.Commands;

[Permission("Roles.Delete")]
public sealed record DeleteRoleCommand(Guid Id) : IRequest<Result<Guid>>;

public sealed class DeleteRoleCommandValidation : AbstractValidator<DeleteRoleCommand>
{
  public DeleteRoleCommandValidation()
  {
    RuleFor(r => r.Id)
            .NotEmpty().WithMessage("Silinecek rolün Id bilgisi boş olamaz!");
  }
}

public sealed class DeleteRoleCommandHandler(
                                IRoleRepository _roleRepo,
                                IUserRepository _userRepo,  // Rolün boşta olup olmadığını kontrol etmek için
                                IUnitOfWork _unit
                          ) : IRequestHandler<DeleteRoleCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(DeleteRoleCommand _req, CancellationToken _token)
  {
    // 1. Silinecek rol gerçekten var mı?
    var role = await _roleRepo.FirstOrDefaultAsync(x => x.Id == _req.Id, _token);
    if (role is null)
      return Result<Guid>.Failure("Silinmek istenen rol sistemde bulunamadı!");

    // 2. KRİTİK İŞ KURALI: Bu role sahip aktif kullanıcı var mı kontrol et!
    // Paketten gelen AnyAsync metoduyla UserRoles üzerinden kontrol atıyoruz
    var isRoleInUse = await _userRepo.AnyAsync(u => u.UserRoles.Any(ur => ur.RoleId == _req.Id), _token);
    if (isRoleInUse)
      return Result<Guid>.Failure("Bu role ait aktif kullanıcılar bulunduğu için rolü silemezsiniz! Önce kullanıcıların rollerini değiştirin.");

    // 3. Silme işlemini tetikle
    // Paketteki Delete metodu bizim DbContext interceptor'ı sayesinde 
    // arka planda IsDeleted = true yapacak (Soft Delete)
    _roleRepo.Delete(role);

    // 4. Veritabanına tek transaction'da işle
    await _unit.SaveChangesAsync(_token);

    return Result<Guid>.Succeed(role.Id);

  }
}