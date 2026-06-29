namespace Application.Services;

public interface IUserContext
{
  string? GetSecurityStamp();
  Guid GetUserId();
  Guid GetBranchId();

  string GetRoleName();
}
