namespace DentalClinic.API.DTOs.Reports;

public record PatientDirectoryDto(
    ulong PatientId,
    string PatientNumber,
    string FullName,
    string? Phone,
    string? Email,
    DateOnly? DateOfBirth,
    string Gender,
    bool IsActive,
    decimal TotalTreatments,
    decimal TotalPaid,
    decimal TotalRemaining
);
