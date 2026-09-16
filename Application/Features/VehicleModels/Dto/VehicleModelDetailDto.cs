namespace Application.Features.VehicleModels.Dto;

public sealed record VehicleModelDetailDto(
    Guid Id,
    string Brand,
    string Name,
    string? Description,
    int Stock,
    int AvailableStock,
    bool IsInStock,
    Guid VehicleTypeId,
    string VehicleTypeName,
    string? VehicleTypeDescription,
    bool IsActive,
    DateTimeOffset? CreatedAt,        
    Guid? CreatedBy,                  
    string? CreatedByName,            
    DateTimeOffset? UpdatedAt,       
    Guid? UpdatedBy,                 
    string? UpdatedByName
);