using Application.Behaviors;
using Application.Features.Branches.Dto;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Branches.Queries;

// Tek bir şube ID'si alır ve geriye tek bir Response nesnesi döner
[Permission("Branches.Read")]
public sealed record GetBranchQuery(Guid Id) : IRequest<BranchDto>;

public sealed class GetBranchQueryHandler(
    IBranchRepository _branchRepo,
    IUserRepository _userRepo
) : IRequestHandler<GetBranchQuery, BranchDto>
{
  public async Task<BranchDto> Handle(GetBranchQuery _req, CancellationToken _token)
  {
    // 1. ADIM: Şubeyi veritabanından tracking kapalı şekilde çekiyoruz.
    var branch = await _branchRepo.FirstOrDefaultAsync(
        x => x.Id == _req.Id && !x.IsDeleted,
        _token,
        isTrackingActive: false
    );

    if (branch is null) throw new KeyNotFoundException("İstenen şube sistemde bulunamadı veya silinmiş.");

    // 2. ADIM: Şubeye ait kullanıcı ID'lerini listeye topluyoruz (Oluşturan ve varsa Güncelleyen).
    var userIds = new List<Guid> { branch.CreatedBy };

    if (branch.UpdatedBy.HasValue)
    {
      userIds.Add(branch.UpdatedBy.Value);
    }

    // 3. ADIM: Yazdığımız metotla isimleri tek sorguda sözlük olarak çekiyoruz.
    var userNamesDictionary = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);

    // Sözlükten isimleri güvenle ayıklıyoruz.
    userNamesDictionary.TryGetValue(branch.CreatedBy, out var createdByName);

    string? updatedByName = null;
    if (branch.UpdatedBy.HasValue)
    {
      userNamesDictionary.TryGetValue(branch.UpdatedBy.Value, out updatedByName);
    }

    // 4. ADIM: Genişletilmiş response modelini doldurarak dönüyoruz.
    return new BranchDto (
        Id: branch.Id,
        Name: branch.Name,
        Address: branch.Address,
        IsActive: branch.IsActive,
        CreatedAt: branch.CreatedAt,
        CreatedByName: createdByName ?? "Bilinmeyen Kullanıcı",
        UpdatedAt: branch.UpdatedAt,
        UpdatedByName: updatedByName
    );
  }
}