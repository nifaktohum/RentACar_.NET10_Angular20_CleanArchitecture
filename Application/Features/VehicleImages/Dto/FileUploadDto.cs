namespace Application.Features.VehicleImages.Dto;

public sealed record FileUploadDto(
    string FileName,
    Stream FileStream,
    string ContentType,
    long Length
);