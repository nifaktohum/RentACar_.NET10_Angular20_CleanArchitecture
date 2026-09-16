namespace Domain.Exceptions;

public class DomainValidationException : DomainException
{
  public string? PropertyName { get; }

  public DomainValidationException(string message) : base(message)
  {
  }

  public DomainValidationException(string message, string propertyName)
      : base(message)
  {
    PropertyName = propertyName;
  }
}