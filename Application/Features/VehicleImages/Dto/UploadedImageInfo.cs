namespace Application.Features.VehicleImages.Dto;

public sealed record UploadedImageInfo(  
    string ImageUrl,
    bool IsMain
);