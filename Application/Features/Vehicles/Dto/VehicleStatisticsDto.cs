namespace Application.Features.Vehicles.Dto;

public sealed record VehicleStatisticsDto(
    int TotalVehicleCount,
    int AvailableVehicleCount,
    int RentedVehicleCount,
    int PassiveVehicleCount,
    decimal AverageDailyPrice
);