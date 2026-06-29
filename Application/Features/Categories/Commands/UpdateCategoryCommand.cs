using Application.Behaviors;
using Application.Features.Categories.Dto;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Categories.Commands;

[Permission("Categories.Update")]
public sealed record UpdateCategoryCommand(
                            Guid Id,
                            string Name,
                            string Slug,
                            string? Description,
                            int? DisplayOrder,
                            Guid? ParentCategoryId
                        ) : IRequest<Result<CategoryDto>>;

public sealed class UpdateCategoryCommandHandler(
                          ICategoryRepository _categoryRepo,
                          IUserRepository _userRepo,
                          IConfiguration _confi,
                          IUnitOfWork unit
                    ) : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{

  public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
  {
    public UpdateCategoryCommandValidator()
    {
      RuleFor(x => x.Id)
          .NotEmpty().WithMessage("Kategori ID boş olamaz.");

      RuleFor(x => x.Name)
          .NotEmpty().WithMessage("Kategori adı boş olamaz.")
          .MaximumLength(100).WithMessage("Kategori adı 100 karakterden uzun olamaz.");

      RuleFor(x => x.Slug)
          .NotEmpty().WithMessage("Slug boş olamaz.")
          .MaximumLength(100).WithMessage("Slug 100 karakterden uzun olamaz.")
          .Matches(@"^[a-z0-9-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");

      RuleFor(x => x.Description)
          .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");
    }
  }
  public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand _req, CancellationToken _token)
  {
    // 1. Kategori var mı?
    var category = await _categoryRepo.GetByExpressionWithTrackingAsync(c => c.Id == _req.Id, _token);
    if (category is null) return Result<CategoryDto>.Failure(404, "Kategori bulunamadı.");

    // 2. Slug kontrolü (kendi hariç)
    var isSlugExist = await _categoryRepo.AnyAsync(c => c.Slug == _req.Slug.ToLowerInvariant() && c.Id != _req.Id, _token);
    if (isSlugExist) return Result<CategoryDto>.Failure(400, "Bu slug zaten başka bir kategori tarafından kullanılıyor.");

    // 3. Parent kategori kontrolü (kendi kendine parent olmaz)
    if (_req.ParentCategoryId.HasValue)
    {
      if (_req.ParentCategoryId.Value == _req.Id)
      {
        return Result<CategoryDto>.Failure(400, "Bir kategori kendi kendisinin alt kategorisi olamaz.");
      }

      var parentExists = await _categoryRepo.AnyAsync(c => c.Id == _req.ParentCategoryId.Value, _token);
      if (!parentExists) return Result<CategoryDto>.Failure(404, "Belirtilen üst kategori bulunamadı.");
    }

    // 4. Güncelleyen kullanıcı
    var updatedBy = _userRepo.GetCurrentUserId();
    if (updatedBy == Guid.Empty)
    {
      // ✅ appsettings'den SystemUserId al
      var systemUserId = _confi["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001";
      updatedBy = Guid.Parse(systemUserId);
    }

    // 5. Kategoriyi güncelle
    category.UpdateDetails(
        name: _req.Name,
        slug: _req.Slug.ToLowerInvariant(),
        description: _req.Description,
        displayOrder: _req.DisplayOrder,
        parentCategoryId: _req.ParentCategoryId
    );
    // 6. Update metadata
    category.UpdateMetadata(updatedBy);

    _categoryRepo.Update(category);

    await unit.SaveChangesAsync(_token);

    var response = new CategoryDto(
          Id: category.Id,
          Name: category.Name,
          Slug: category.Slug,
          Description: category.Description,
          DisplayOrder: category.DisplayOrder,
          ParentCategoryId: category.ParentCategoryId,
          ParentCategoryName: category.ParentCategory?.Name
      );

    return Result<CategoryDto>.Succeed(response);
  }
}