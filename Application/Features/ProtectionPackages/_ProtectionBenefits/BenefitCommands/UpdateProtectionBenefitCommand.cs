using Application.Features.ProtectionPackages.Dto;
using Domain.Protection;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitCommands;

public sealed record UpdateProtectionBenefitCommand(
                                Guid Id,
                                string Name,
                                string? Description,
                                string? Icon,
                                int DisplayOrder,
                                BenefitCategory Category
                      ) : IRequest<Result<ProtectionBenefitDto>>;

public sealed class UpdateProtectionBenefitCommandValidator : AbstractValidator<UpdateProtectionBenefitCommand>
{
  public UpdateProtectionBenefitCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Benefit ID zorunludur.");

    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Benefit adı zorunludur.")
        .MaximumLength(100).WithMessage("Benefit adı 100 karakterden uzun olamaz.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");

    RuleFor(x => x.Icon)
        .MaximumLength(50).WithMessage("İkon adı 50 karakterden uzun olamaz.");

    RuleFor(x => x.DisplayOrder)
        .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");

    RuleFor(x => x.Category)
        .IsInEnum().WithMessage("Geçerli bir kategori seçiniz.");
  }
}

public sealed class UpdateProtectionBenefitCommandHandler(
                            IProtectionBenefitRepository _benefitRepo,
                            IUserRepository _userRepo,
                            IUnitOfWork _unit,
                            IConfiguration _config
                    ) : IRequestHandler<UpdateProtectionBenefitCommand, Result<ProtectionBenefitDto>>
{
  public async Task<Result<ProtectionBenefitDto>> Handle(UpdateProtectionBenefitCommand _req, CancellationToken _token)
  {

    // 1. Benefit var mı?
    var benefit = await _benefitRepo
        .FirstOrDefaultAsync(b => b.Id == _req.Id && !b.IsDeleted, _token);

    if (benefit is null) return Result<ProtectionBenefitDto>.Failure(404, "Benefit bulunamadı.");

    // 2. Aynı isimde başka benefit var mı?
    var existing = await _benefitRepo
        .FirstOrDefaultAsync(b => b.Name == _req.Name && b.Id != _req.Id && !b.IsDeleted, _token);

    if (existing is not null)
      return Result<ProtectionBenefitDto>.Failure(400, $"'{_req.Name}' adında bir benefit zaten mevcut.");

    // 3. Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData.AdminUserId"]!);

    // 4. Benefit'i güncelle
    benefit.UpdateDetails(
        name: _req.Name,
        description: _req.Description,
        icon: _req.Icon,
        displayOrder: _req.DisplayOrder,
        category: _req.Category
    );

    benefit.UpdateMetadata(userId);
    _benefitRepo.Update(benefit);
    await _unit.SaveChangesAsync(_token);

    // 5. Response
    var dto = new ProtectionBenefitDto(
        Id: benefit.Id,
        Name: benefit.Name,
        Description: benefit.Description,
        Icon: benefit.Icon,
        DisplayOrder: benefit.DisplayOrder,
        Category: benefit.Category.ToString(),
        IsActive: benefit.IsActive,
        CreatedAt: benefit.CreatedAt,
        CreatedBy: benefit.CreatedBy,
        CreatedByName: "",
        UpdatedAt: benefit.UpdatedAt,
        UpdatedBy: benefit.UpdatedBy,
        UpdatedByName: null
    );

    return Result<ProtectionBenefitDto>.Succeed(dto);
  }
}