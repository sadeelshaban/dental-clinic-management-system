namespace DentalClinic.API.Constants;

/// <summary>
/// Canonical audit action values stored in audit_logs.action.
/// </summary>
public static class AuditActions
{
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string Activate = "ACTIVATE";
    public const string Deactivate = "DEACTIVATE";
    public const string PasswordReset = "PASSWORD_RESET";

    // Appointment status transitions (Phase 2)
    public const string Confirm = "CONFIRM";
    public const string Complete = "COMPLETE";
    public const string Cancel = "CANCEL";
    public const string NoShow = "NO_SHOW";

    // Billing (Phase 4)
    public const string PaymentCreated = "PAYMENT_CREATED";
    public const string PaymentVoided = "PAYMENT_VOIDED";

    // Expenses & suppliers (Phase 5)
    public const string ExpensePaymentCreated = "EXPENSE_PAYMENT_CREATED";
    public const string ExpensePaymentVoided = "EXPENSE_PAYMENT_VOIDED";
}

/// <summary>
/// Canonical entity names stored in audit_logs.entity_name.
/// </summary>
public static class AuditEntities
{
    public const string User = "user";
    public const string Doctor = "doctor";
    public const string Appointment = "appointment";
    public const string Visit = "visit";
    public const string TreatmentCategory = "treatment_category";
    public const string Treatment = "treatment";
    public const string PatientTreatment = "patient_treatment";
    public const string Payment = "payment";
    public const string PaymentMethod = "payment_method";
    public const string ExpenseCategory = "expense_category";
    public const string Supplier = "supplier";
    public const string Expense = "expense";
    public const string ExpensePayment = "expense_payment";
}
