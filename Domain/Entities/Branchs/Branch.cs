using Domain.Abstractions;
using Domain.Entities.Reservations;

namespace Domain.Entities.Branchs;

public sealed class Branch : BaseEntity
{
  private Branch() : base(Guid.Empty)
  {
    PickupReservations = new List<Reservation>();
    ReturnReservations = new List<Reservation>();
  }
  public Branch(string name, Address address, Guid createdBy) : base(createdBy)
  {
    Name = name;
    Address = address;
    PickupReservations = new List<Reservation>();
    ReturnReservations = new List<Reservation>();
  }

  public Branch(Guid id, string name, Address address, Guid createdBy) : base(id, createdBy)
  {
    Name = name;
    Address = address;
    PickupReservations = new List<Reservation>();
    ReturnReservations = new List<Reservation>();
  }
  public string Name { get; private set; } = default!;
  // ŞUBE-ADI: "İstanbul Kadıköy Şubesi" gibi şubemin vitrin ismidir.
  public Address Address { get; private set; } = default!;
  // ADRES: Şubenin fiziksel olarak dünyadaki yerini açıkça belirtirim.

  // ⭐ Navigation
  public ICollection<Reservation> PickupReservations { get; private set; } = new List<Reservation>();
  public ICollection<Reservation> ReturnReservations { get; private set; } = new List<Reservation>();

  #region  Behaviors
  public void SetName(string name) { Name = name; }
  public void SetAddress(Address address) { Address = address; }
  #endregion
}
