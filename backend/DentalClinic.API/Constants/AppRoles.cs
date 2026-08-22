namespace DentalClinic.API.Constants;

public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Doctor = "DOCTOR";
    public const string Secretary = "SECRETARY";

    public const string AdminOnly = Admin;
    public const string ClinicalStaff = $"{Admin},{Doctor},{Secretary}";
    public const string AdminOrSecretary = $"{Admin},{Secretary}";
    public const string AdminOrDoctor = $"{Admin},{Doctor}";

    /// <summary>All valid role values, used for input validation.</summary>
    public static readonly IReadOnlyList<string> AllRoles = [Admin, Doctor, Secretary];
}

public static class ClaimTypesCustom
{
    public const string UserId = "user_id";
    public const string ClinicId = "clinic_id";
    public const string FullName = "full_name";
}
