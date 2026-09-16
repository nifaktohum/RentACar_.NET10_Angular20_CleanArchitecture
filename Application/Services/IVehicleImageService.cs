using Application.Features.VehicleImages.Dto;
using Domain.Entities.Vehicles;

namespace Application.Services;

public interface IVehicleImageService
{
  Task<List<UploadedImageInfo>> SaveVehicleImagesAsync(
                                              Vehicle vehicle,
                                              List<FileUploadDto> files,
                                              bool isMain = false,
                                              CancellationToken cancellationToken = default);

}