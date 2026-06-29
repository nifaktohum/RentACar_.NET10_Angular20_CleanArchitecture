using Domain.Abstractions;

namespace Domain.Branchs;

public sealed class Branch : BaseEntity
{
  private Branch() : base(Guid.Empty) { }
  public Branch(string name, Address address, Guid createdBy) : base(createdBy)
  {
    Name = name;
    Address = address;
  }

  public Branch( Guid id, string name, Address address, Guid createdBy): base(id, createdBy)
  {
    Name = name;
    Address = address;
  }
  public string Name { get; private set; } = default!;
  // ŞUBE-ADI: "İstanbul Kadıköy Şubesi" gibi şubemin vitrin ismidir.
  public Address Address { get; private set; } = default!;
  // ADRES: Şubenin fiziksel olarak dünyadaki yerini açıkça belirtirim.

  #region  Behaviors
  public void SetName(string name) { Name = name; }
  public void SetAddress(Address address) { Address = address; }
  #endregion
}
