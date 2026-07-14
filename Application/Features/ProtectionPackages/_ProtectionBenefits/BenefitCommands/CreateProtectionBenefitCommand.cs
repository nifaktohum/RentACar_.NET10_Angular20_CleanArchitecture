using Application.Features.ProtectionPackages.Dto;
using ProtectionBenefitEntity = Domain.Entities.Protection.ProtectionBenefit;
using Domain.Protection;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitCommands;

public sealed record CreateProtectionBenefitCommand(
                                    string Name,
                                    string? Description,
                                    string? Icon,
                                    int DisplayOrder,
                                    BenefitCategory Category
                      ) : IRequest<Result<ProtectionBenefitDto>>;

public sealed class CreateProtectionBenefitValidator : AbstractValidator<CreateProtectionBenefitCommand>
{
  public CreateProtectionBenefitValidator()
  {
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

public sealed class CreateProtectionBenefitCommandHandler(
                            IProtectionBenefitRepository _benefitRepo,
                            IUserRepository _userRepo,
                            IUnitOfWork _unit,
                            IConfiguration _config
                      ) : IRequestHandler<CreateProtectionBenefitCommand, Result<ProtectionBenefitDto>>
{
  public async Task<Result<ProtectionBenefitDto>> Handle(CreateProtectionBenefitCommand _req, CancellationToken _token)
  {

    // 1. Aynı isimde benefit var mı?
    var existing = await _benefitRepo
        .FirstOrDefaultAsync(b => b.Name == _req.Name && !b.IsDeleted, _token);

    if (existing is not null)
      return Result<ProtectionBenefitDto>.Failure(400, $"'{_req.Name}' adında bir benefit zaten mevcut.");

    // 2. Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData.AdminUserId"]!);

    // 3. Benefit oluştur
    var benefit = new ProtectionBenefitEntity(
        name: _req.Name,
        description: _req.Description,
        icon: _req.Icon,
        displayOrder: _req.DisplayOrder,
        category: _req.Category,
        createdBy: userId
    );



    await _benefitRepo.AddAsync(benefit, _token);
    await _unit.SaveChangesAsync(_token);

    // 4. Response
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
