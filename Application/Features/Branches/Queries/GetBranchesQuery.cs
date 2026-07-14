using Application.Behaviors;
using Application.Features.Branches.Dto;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Branches.Queries;

[Permission("Branches.Read")]
public sealed record GetBranchesQuery() : IRequest<Result<GetBranchesQueryResponse>>;


public sealed class GetBranchesQueryHandler(
    IBranchRepository _branchRepo, // IRepository<Branch>'den türediğini varsayıyoruz
    IUserRepository _userRepo
) : IRequestHandler<GetBranchesQuery,  Result<GetBranchesQueryResponse>>
{
  public async Task<Result<GetBranchesQueryResponse>> Handle(GetBranchesQuery _req, CancellationToken _token)
  {
    // 1. Reponun gerçek 'Where' metodunu kullanıyoruz. 
    // Geriye IQueryable döndüğü için EF Core üzerinden veritabanına async olarak sorgu atıyoruz.
    var branches = await _branchRepo
        .Where(b => !b.IsDeleted)
        .ToListAsync(_token);

    if (!branches.Any())
    {
      return new GetBranchesQueryResponse(new List<BranchDto>(), 0);
    }

    // 2. N+1 sorgu problemini engellemek için benzersiz kullanıcı ID'lerini topluyoruz.
    var createdUserIds = branches.Select(b => b.CreatedBy);
    var updatedUserIds = branches.Where(b => b.UpdatedBy.HasValue).Select(b => b.UpdatedBy!.Value);

    var allUserIds = createdUserIds
        .Concat(updatedUserIds)
        .Distinct()
        .ToList();

    // 3. Tek bir sorguyla bu ID'lere ait kullanıcı isimlerini çekiyoruz.
    var userNamesDictionary = await _userRepo.GetUserNamesByIdsAsync(allUserIds, _token);

    // 4. Entity verilerini, isimleri de sözlükten (Dictionary) çözerek DTO'ya map'liyoruz.
    var branchDtos = branches.Select(b =>
    {
      userNamesDictionary.TryGetValue(b.CreatedBy, out var createdByName);

      string? updatedByName = null;
      if (b.UpdatedBy.HasValue)
      {
        userNamesDictionary.TryGetValue(b.UpdatedBy.Value, out updatedByName);
      }

      return new BranchDto(
              Id: b.Id,
              Name: b.Name,
              Address: b.Address,
              CreatedAt: b.CreatedAt,
              CreatedByName: createdByName ?? "Bilinmeyen Kullanıcı",
              UpdatedAt: b.UpdatedAt,
              UpdatedByName: updatedByName,
              IsActive: b.IsActive
          );
    }).ToList();

    // 5. Tam istediğin response formatında (Items ve TotalCount) teslim ediyoruz.
    // 5. ✅ Result.Success ile dön!
    return Result<GetBranchesQueryResponse>.Succeed(
        new GetBranchesQueryResponse(
            Items: branchDtos,
            TotalCount: branchDtos.Count
        )
    );
  }
}
  /*
    public async Task<GetBranchesQueryResponse> Handle(GetBranchesQuery _req, CancellationToken _token)
    {
      // 1. ADIM: Güçlü IRepository arayüzündeki GetAll() metodunu çağırıyoruz.
      // Bu metot arkada 'AsNoTracking()' çalıştığı için EF Core nesneleri takip etmez ve harika bir performans sağlar.
      // Ayrıca senin BaseEntity kurallarına göre silinmiş (IsDeleted == true) olan şubeleri tamamen liste dışı bırakıyoruz.
      var branchesQuery = _branchRepo.GetAll()
          .Where(x => !x.IsDeleted);

      // 2. ADIM: Filtreye uyan toplam kayıt sayısını (Count) asenkron olarak veritabanından alıyoruz.
      // Bu sayede listeyi çekmeden önce filtrelenmiş net sayıyı öğreniyoruz.
      var totalCount = await branchesQuery.CountAsync(_token);

      // 2. ADIM: Veritabanından gelen ham 'Branch' entity nesnelerini, 
      // Angular'ın anlayacağı temiz 'GetBranchQueryResponse' modeline haritalıyoruz (Select).
      var items = await branchesQuery
          .Select(x => new GetBranchQueryResponse(
              x.Id,
              x.Name,
              x.Address.City,
              x.Address.District,
              x.Address.FullAddress,
              x.Address.Phone1,
              x.Address.Phone2,
              x.Address.Email,
              x.IsActive
          )).ToListAsync(_token); // Sorguyu PostgreSQL tarafında asenkron olarak çalıştırıp listeyi çekiyoruz.

      // 4. ADIM: Hem listeyi hem de toplam sayıyı barındıran yeni objemizi Angular'a fırlatıyoruz.
      return new GetBranchesQueryResponse(items, totalCount);
    }

  */
