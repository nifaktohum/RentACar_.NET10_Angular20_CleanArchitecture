namespace Application.Features.VehicleImages.Dto;

public sealed record AddVehicleImageResponse(
     List<UploadedImageInfo> UploadedImages,
    int TotalUploaded,
    int TotalFailed
);