using System;
using System.Collections.Generic;
using DentalClinic.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Data;

public partial class DentalClinicDbContext : DbContext
{
    public DentalClinicDbContext(DbContextOptions<DentalClinicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<ClinicSetting> ClinicSettings { get; set; }

    public virtual DbSet<ClinicWorkingHour> ClinicWorkingHours { get; set; }

    public virtual DbSet<DailyFinancialSummary> DailyFinancialSummaries { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    public virtual DbSet<ExpenseFinancial> ExpenseFinancials { get; set; }

    public virtual DbSet<ExpensePayment> ExpensePayments { get; set; }

    public virtual DbSet<MonthlyFinancialSummary> MonthlyFinancialSummaries { get; set; }

    public virtual DbSet<MonthlyPerformanceComparison> MonthlyPerformanceComparisons { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientContact> PatientContacts { get; set; }

    public virtual DbSet<PatientDirectory> PatientDirectories { get; set; }

    public virtual DbSet<PatientFinancialSummary> PatientFinancialSummaries { get; set; }

    public virtual DbSet<PatientPayment> PatientPayments { get; set; }

    public virtual DbSet<PatientTreatment> PatientTreatments { get; set; }

    public virtual DbSet<PatientTreatmentFinancial> PatientTreatmentFinancials { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierFinancialSummary> SupplierFinancialSummaries { get; set; }

    public virtual DbSet<Treatment> Treatments { get; set; }

    public virtual DbSet<TreatmentCategory> TreatmentCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PRIMARY");

            entity.ToTable("appointments");

            entity.HasIndex(e => e.CreatedBy, "fk_appointments_created_by");

            entity.HasIndex(e => new { e.ClinicId, e.AppointmentDate }, "idx_appointments_clinic_date");

            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate, e.StartTime }, "idx_appointments_doctor_date");

            entity.HasIndex(e => new { e.PatientId, e.AppointmentDate }, "idx_appointments_patient");

            entity.HasIndex(e => new { e.ClinicId, e.Status }, "idx_appointments_status");

            entity.Property(e => e.AppointmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("appointment_id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("created_by");
            entity.Property(e => e.DoctorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("doctor_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("end_time");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(250)
                .HasColumnName("reason");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'SCHEDULED'")
                .HasColumnType("enum('SCHEDULED','CONFIRMED','COMPLETED','CANCELLED','NO_SHOW')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_appointments_clinic");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_appointments_created_by");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointments_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointments_patient");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PRIMARY");

            entity.ToTable("attachments");

            entity.HasIndex(e => e.ClinicId, "fk_attachments_clinic");

            entity.HasIndex(e => e.UploadedBy, "fk_attachments_uploaded_by");

            entity.HasIndex(e => e.PatientId, "idx_attachments_patient");

            entity.HasIndex(e => e.PatientTreatmentId, "idx_attachments_treatment");

            entity.Property(e => e.AttachmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("attachment_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSize)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("file_size");
            entity.Property(e => e.FileType)
                .HasMaxLength(100)
                .HasColumnName("file_type");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(1000)
                .HasColumnName("file_url");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.PatientTreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_treatment_id");
            entity.Property(e => e.UploadedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("uploaded_by");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_attachments_clinic");

            entity.HasOne(d => d.Patient).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_attachments_patient");

            entity.HasOne(d => d.PatientTreatment).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.PatientTreatmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_attachments_treatment");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_attachments_uploaded_by");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PRIMARY");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => new { e.ClinicId, e.CreatedAt }, "idx_audit_logs_clinic");

            entity.HasIndex(e => new { e.EntityName, e.EntityId }, "idx_audit_logs_entity");

            entity.HasIndex(e => e.UserId, "idx_audit_logs_user");

            entity.Property(e => e.AuditId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("audit_id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("entity_id");
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .HasColumnName("entity_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValues)
                .HasColumnType("json")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasColumnType("json")
                .HasColumnName("old_values");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Clinic).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_audit_logs_clinic");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_audit_logs_user");
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId).HasName("PRIMARY");

            entity.ToTable("clinics");

            entity.HasIndex(e => e.IsActive, "idx_clinics_active");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Palestine'")
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'ILS'")
                .IsFixedLength()
                .HasColumnName("currency_code");
            entity.Property(e => e.CurrencySymbol)
                .HasMaxLength(10)
                .HasDefaultValueSql("'Ôé¬'")
                .HasColumnName("currency_symbol");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LegalName)
                .HasMaxLength(200)
                .HasColumnName("legal_name");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Timezone)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Asia/Gaza'")
                .HasColumnName("timezone");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ClinicSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PRIMARY");

            entity.ToTable("clinic_settings");

            entity.HasIndex(e => new { e.ClinicId, e.SettingKey }, "uq_clinic_setting").IsUnique();

            entity.Property(e => e.SettingId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("setting_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingValue)
                .HasColumnType("text")
                .HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ClinicSettings)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_clinic_settings_clinic");
        });

        modelBuilder.Entity<ClinicWorkingHour>(entity =>
        {
            entity.HasKey(e => e.WorkingHourId).HasName("PRIMARY");

            entity.ToTable("clinic_working_hours");

            entity.HasIndex(e => new { e.ClinicId, e.DayOfWeek }, "uq_clinic_day").IsUnique();

            entity.Property(e => e.WorkingHourId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("working_hour_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.ClosingTime)
                .HasColumnType("time")
                .HasColumnName("closing_time");
            entity.Property(e => e.DayOfWeek)
                .HasColumnType("tinyint(4)")
                .HasColumnName("day_of_week");
            entity.Property(e => e.IsOpen)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_open");
            entity.Property(e => e.OpeningTime)
                .HasColumnType("time")
                .HasColumnName("opening_time");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ClinicWorkingHours)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_working_hours_clinic");
        });

        modelBuilder.Entity<DailyFinancialSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("daily_financial_summary");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.Expenses)
                .HasPrecision(34, 2)
                .HasColumnName("expenses");
            entity.Property(e => e.FinancialDate).HasColumnName("financial_date");
            entity.Property(e => e.NetProfit)
                .HasPrecision(35, 2)
                .HasColumnName("net_profit");
            entity.Property(e => e.Revenue)
                .HasPrecision(34, 2)
                .HasColumnName("revenue");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PRIMARY");

            entity.ToTable("doctors");

            entity.HasIndex(e => e.ClinicId, "idx_doctors_clinic");

            entity.HasIndex(e => e.UserId, "uq_doctors_user").IsUnique();

            entity.Property(e => e.DoctorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("doctor_id");
            entity.Property(e => e.Bio)
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(100)
                .HasColumnName("license_number");
            entity.Property(e => e.Specialization)
                .HasMaxLength(150)
                .HasColumnName("specialization");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_doctors_clinic");

            entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_doctors_user");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PRIMARY");

            entity.ToTable("expenses");

            entity.HasIndex(e => e.CreatedBy, "fk_expenses_created_by");

            entity.HasIndex(e => e.CategoryId, "idx_expenses_category");

            entity.HasIndex(e => new { e.ClinicId, e.ExpenseDate }, "idx_expenses_clinic_date");

            entity.HasIndex(e => new { e.ClinicId, e.Status }, "idx_expenses_status");

            entity.HasIndex(e => e.SupplierId, "idx_expenses_supplier");

            entity.Property(e => e.ExpenseId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("expense_id");
            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ExpenseDate)
                .HasDefaultValueSql("curdate()")
                .HasColumnName("expense_date");
            entity.Property(e => e.ExpenseType)
                .HasDefaultValueSql("'GENERAL'")
                .HasColumnType("enum('GENERAL','SUPPLIER_PURCHASE','RENT','UTILITIES','EQUIPMENT','MAINTENANCE','LABORATORY','MATERIALS','OTHER')")
                .HasColumnName("expense_type");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'UNPAID'")
                .HasColumnType("enum('UNPAID','PARTIALLY_PAID','PAID','VOIDED')")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("supplier_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expenses_category");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_expenses_clinic");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expenses_created_by");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expenses_supplier");
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("expense_categories");

            entity.HasIndex(e => new { e.ClinicId, e.Name }, "uq_expense_category").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ExpenseCategories)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_expense_categories_clinic");
        });

        modelBuilder.Entity<ExpenseFinancial>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("expense_financials");

            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ExpenseDate)
                .HasDefaultValueSql("curdate()")
                .HasColumnName("expense_date");
            entity.Property(e => e.ExpenseId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("expense_id");
            entity.Property(e => e.ExpenseType)
                .HasDefaultValueSql("'GENERAL'")
                .HasColumnType("enum('GENERAL','SUPPLIER_PURCHASE','RENT','UTILITIES','EQUIPMENT','MAINTENANCE','LABORATORY','MATERIALS','OTHER')")
                .HasColumnName("expense_type");
            entity.Property(e => e.RemainingBalance)
                .HasPrecision(35, 2)
                .HasColumnName("remaining_balance");
            entity.Property(e => e.SupplierId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("supplier_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalPaid)
                .HasPrecision(34, 2)
                .HasColumnName("total_paid");
        });

        modelBuilder.Entity<ExpensePayment>(entity =>
        {
            entity.HasKey(e => e.ExpensePaymentId).HasName("PRIMARY");

            entity.ToTable("expense_payments");

            entity.HasIndex(e => e.PaymentMethodId, "fk_expense_payments_method");

            entity.HasIndex(e => e.PaidBy, "fk_expense_payments_paid_by");

            entity.HasIndex(e => e.VoidedBy, "fk_expense_payments_voided_by");

            entity.HasIndex(e => new { e.ClinicId, e.PaymentDate }, "idx_expense_payments_date");

            entity.HasIndex(e => e.ExpenseId, "idx_expense_payments_expense");

            entity.HasIndex(e => new { e.ClinicId, e.IsVoided }, "idx_expense_payments_voided");

            entity.Property(e => e.ExpensePaymentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("expense_payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpenseId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("expense_id");
            entity.Property(e => e.IsVoided).HasColumnName("is_voided");
            entity.Property(e => e.Method)
                .HasDefaultValueSql("'CASH'")
                .HasColumnType("enum('CASH','CARD','BANK_TRANSFER','CHEQUE','OTHER')")
                .HasColumnName("method");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.PaidBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("paid_by");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethodId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(150)
                .HasColumnName("reference_number");
            entity.Property(e => e.VoidReason)
                .HasColumnType("text")
                .HasColumnName("void_reason");
            entity.Property(e => e.VoidedAt)
                .HasColumnType("datetime")
                .HasColumnName("voided_at");
            entity.Property(e => e.VoidedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("voided_by");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ExpensePayments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_expense_payments_clinic");

            entity.HasOne(d => d.Expense).WithMany(p => p.ExpensePayments)
                .HasForeignKey(d => d.ExpenseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_expense_payments_expense");

            entity.HasOne(d => d.PaidByNavigation).WithMany(p => p.ExpensePaymentPaidByNavigations)
                .HasForeignKey(d => d.PaidBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expense_payments_paid_by");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.ExpensePayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expense_payments_method");

            entity.HasOne(d => d.VoidedByNavigation).WithMany(p => p.ExpensePaymentVoidedByNavigations)
                .HasForeignKey(d => d.VoidedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_expense_payments_voided_by");
        });

        modelBuilder.Entity<MonthlyFinancialSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("monthly_financial_summary");

            entity.Property(e => e.Appointments)
                .HasColumnType("bigint(21)")
                .HasColumnName("appointments");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.Expenses)
                .HasPrecision(34, 2)
                .HasColumnName("expenses");
            entity.Property(e => e.Month)
                .HasMaxLength(10)
                .HasColumnName("month")
                .UseCollation("utf8mb4_general_ci");
            entity.Property(e => e.NetProfit)
                .HasPrecision(35, 2)
                .HasColumnName("net_profit");
            entity.Property(e => e.Patients)
                .HasColumnType("bigint(21)")
                .HasColumnName("patients");
            entity.Property(e => e.Revenue)
                .HasPrecision(34, 2)
                .HasColumnName("revenue");
        });

        modelBuilder.Entity<MonthlyPerformanceComparison>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("monthly_performance_comparison");

            entity.Property(e => e.AppointmentChangePercent)
                .HasPrecision(28, 4)
                .HasColumnName("appointment_change_percent");
            entity.Property(e => e.Appointments)
                .HasColumnType("bigint(21)")
                .HasColumnName("appointments");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.ExpenseChangePercent)
                .HasPrecision(44, 6)
                .HasColumnName("expense_change_percent");
            entity.Property(e => e.Expenses)
                .HasPrecision(34, 2)
                .HasColumnName("expenses");
            entity.Property(e => e.Month)
                .HasMaxLength(10)
                .HasColumnName("month")
                .UseCollation("utf8mb4_general_ci");
            entity.Property(e => e.NetProfit)
                .HasPrecision(35, 2)
                .HasColumnName("net_profit");
            entity.Property(e => e.PatientChangePercent)
                .HasPrecision(28, 4)
                .HasColumnName("patient_change_percent");
            entity.Property(e => e.Patients)
                .HasColumnType("bigint(21)")
                .HasColumnName("patients");
            entity.Property(e => e.PreviousMonthAppointments)
                .HasColumnType("bigint(21)")
                .HasColumnName("previous_month_appointments");
            entity.Property(e => e.PreviousMonthExpenses)
                .HasPrecision(34, 2)
                .HasColumnName("previous_month_expenses");
            entity.Property(e => e.PreviousMonthPatients)
                .HasColumnType("bigint(21)")
                .HasColumnName("previous_month_patients");
            entity.Property(e => e.PreviousMonthProfit)
                .HasPrecision(35, 2)
                .HasColumnName("previous_month_profit");
            entity.Property(e => e.PreviousMonthRevenue)
                .HasPrecision(34, 2)
                .HasColumnName("previous_month_revenue");
            entity.Property(e => e.ProfitChangePercent)
                .HasPrecision(45, 6)
                .HasColumnName("profit_change_percent");
            entity.Property(e => e.Revenue)
                .HasPrecision(34, 2)
                .HasColumnName("revenue");
            entity.Property(e => e.RevenueChangePercent)
                .HasPrecision(44, 6)
                .HasColumnName("revenue_change_percent");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PRIMARY");

            entity.ToTable("patients");

            entity.HasIndex(e => e.ClinicId, "idx_patients_clinic");

            entity.HasIndex(e => new { e.ClinicId, e.LastName, e.FirstName }, "idx_patients_name");

            entity.HasIndex(e => new { e.ClinicId, e.NationalId }, "idx_patients_national_id");

            entity.HasIndex(e => new { e.ClinicId, e.Phone }, "idx_patients_phone");

            entity.HasIndex(e => new { e.ClinicId, e.PatientNumber }, "uq_patient_number").IsUnique();

            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.Allergies)
                .HasColumnType("text")
                .HasColumnName("allergies");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyContactName)
                .HasMaxLength(150)
                .HasColumnName("emergency_contact_name");
            entity.Property(e => e.EmergencyContactPhone)
                .HasMaxLength(50)
                .HasColumnName("emergency_contact_phone");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'UNKNOWN'")
                .HasColumnType("enum('MALE','FEMALE','OTHER','UNKNOWN')")
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MedicalAlerts)
                .HasColumnType("text")
                .HasColumnName("medical_alerts");
            entity.Property(e => e.MedicalHistory)
                .HasColumnType("text")
                .HasColumnName("medical_history");
            entity.Property(e => e.Medications)
                .HasColumnType("text")
                .HasColumnName("medications");
            entity.Property(e => e.NationalId)
                .HasMaxLength(100)
                .HasColumnName("national_id");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.PatientNumber)
                .HasMaxLength(50)
                .HasColumnName("patient_number");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Patients)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_patients_clinic");
        });

        modelBuilder.Entity<PatientContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PRIMARY");

            entity.ToTable("patient_contacts");

            entity.HasIndex(e => e.PatientId, "idx_patient_contacts_patient");

            entity.Property(e => e.ContactId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("contact_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Relationship)
                .HasMaxLength(100)
                .HasColumnName("relationship");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientContacts)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("fk_patient_contacts_patient");
        });

        modelBuilder.Entity<PatientDirectory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("patient_directory");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(201)
                .HasDefaultValueSql("''")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'UNKNOWN'")
                .HasColumnType("enum('MALE','FEMALE','OTHER','UNKNOWN')")
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.PatientNumber)
                .HasMaxLength(50)
                .HasColumnName("patient_number");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.TotalPaid)
                .HasPrecision(56, 2)
                .HasColumnName("total_paid");
            entity.Property(e => e.TotalRemaining)
                .HasPrecision(57, 2)
                .HasColumnName("total_remaining");
            entity.Property(e => e.TotalTreatments)
                .HasPrecision(34, 2)
                .HasColumnName("total_treatments");
        });

        modelBuilder.Entity<PatientFinancialSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("patient_financial_summary");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(201)
                .HasDefaultValueSql("''")
                .HasColumnName("full_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.PatientNumber)
                .HasMaxLength(50)
                .HasColumnName("patient_number");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.TotalPaid)
                .HasPrecision(56, 2)
                .HasColumnName("total_paid");
            entity.Property(e => e.TotalRemaining)
                .HasPrecision(57, 2)
                .HasColumnName("total_remaining");
            entity.Property(e => e.TotalTreatments)
                .HasPrecision(34, 2)
                .HasColumnName("total_treatments");
        });

        modelBuilder.Entity<PatientPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("patient_payments");

            entity.HasIndex(e => e.PaymentMethodId, "fk_patient_payments_method");

            entity.HasIndex(e => e.ReceivedBy, "fk_patient_payments_received_by");

            entity.HasIndex(e => e.VoidedBy, "fk_patient_payments_voided_by");

            entity.HasIndex(e => new { e.ClinicId, e.PaymentDate }, "idx_patient_payments_date");

            entity.HasIndex(e => new { e.PatientId, e.PaymentDate }, "idx_patient_payments_patient");

            entity.HasIndex(e => e.PatientTreatmentId, "idx_patient_payments_treatment");

            entity.HasIndex(e => new { e.ClinicId, e.IsVoided }, "idx_patient_payments_voided");

            entity.Property(e => e.PaymentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsVoided).HasColumnName("is_voided");
            entity.Property(e => e.Method)
                .HasDefaultValueSql("'CASH'")
                .HasColumnType("enum('CASH','CARD','BANK_TRANSFER','CHEQUE','OTHER')")
                .HasColumnName("method");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.PatientTreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_treatment_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethodId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.ReceivedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("received_by");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(150)
                .HasColumnName("reference_number");
            entity.Property(e => e.VoidReason)
                .HasColumnType("text")
                .HasColumnName("void_reason");
            entity.Property(e => e.VoidedAt)
                .HasColumnType("datetime")
                .HasColumnName("voided_at");
            entity.Property(e => e.VoidedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("voided_by");

            entity.HasOne(d => d.Clinic).WithMany(p => p.PatientPayments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_patient_payments_clinic");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientPayments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_patient_payments_patient");

            entity.HasOne(d => d.PatientTreatment).WithMany(p => p.PatientPayments)
                .HasForeignKey(d => d.PatientTreatmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_patient_payments_treatment");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.PatientPayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_payments_method");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.PatientPaymentReceivedByNavigations)
                .HasForeignKey(d => d.ReceivedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_payments_received_by");

            entity.HasOne(d => d.VoidedByNavigation).WithMany(p => p.PatientPaymentVoidedByNavigations)
                .HasForeignKey(d => d.VoidedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_payments_voided_by");
        });

        modelBuilder.Entity<PatientTreatment>(entity =>
        {
            entity.HasKey(e => e.PatientTreatmentId).HasName("PRIMARY");

            entity.ToTable("patient_treatments");

            entity.HasIndex(e => e.CreatedBy, "fk_patient_treatments_created_by");

            entity.HasIndex(e => e.TreatmentId, "fk_patient_treatments_treatment");

            entity.HasIndex(e => e.VisitId, "fk_patient_treatments_visit");

            entity.HasIndex(e => new { e.ClinicId, e.TreatmentDate }, "idx_patient_treatments_clinic");

            entity.HasIndex(e => new { e.DoctorId, e.TreatmentDate }, "idx_patient_treatments_doctor");

            entity.HasIndex(e => new { e.PatientId, e.TreatmentDate }, "idx_patient_treatments_patient");

            entity.Property(e => e.PatientTreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_treatment_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("created_by");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(12, 2)
                .HasColumnName("discount_amount");
            entity.Property(e => e.DoctorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("doctor_id");
            entity.Property(e => e.FinalAmount)
                .HasPrecision(12, 2)
                .HasComputedColumnSql("greatest(`quantity` * `unit_price` - `discount_amount`,0)", true)
                .HasColumnName("final_amount");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'UNPAID'")
                .HasColumnType("enum('UNPAID','PARTIALLY_PAID','PAID','VOIDED')")
                .HasColumnName("status");
            entity.Property(e => e.TreatmentDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("treatment_date");
            entity.Property(e => e.TreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("treatment_id");
            entity.Property(e => e.TreatmentName)
                .HasMaxLength(200)
                .HasColumnName("treatment_name");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VisitId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("visit_id");

            entity.HasOne(d => d.Clinic).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_patient_treatments_clinic");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_treatments_created_by");

            entity.HasOne(d => d.Doctor).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_patient_treatments_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_patient_treatments_patient");

            entity.HasOne(d => d.Treatment).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.TreatmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_treatments_treatment");

            entity.HasOne(d => d.Visit).WithMany(p => p.PatientTreatments)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_patient_treatments_visit");
        });

        modelBuilder.Entity<PatientTreatmentFinancial>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("patient_treatment_financials");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(12, 2)
                .HasColumnName("discount_amount");
            entity.Property(e => e.DoctorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("doctor_id");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.PatientTreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_treatment_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.RemainingBalance)
                .HasPrecision(35, 2)
                .HasColumnName("remaining_balance");
            entity.Property(e => e.TotalPaid)
                .HasPrecision(34, 2)
                .HasColumnName("total_paid");
            entity.Property(e => e.TreatmentDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("treatment_date");
            entity.Property(e => e.TreatmentName)
                .HasMaxLength(200)
                .HasColumnName("treatment_name");
            entity.Property(e => e.TreatmentTotal)
                .HasPrecision(12, 2)
                .HasColumnName("treatment_total");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PRIMARY");

            entity.ToTable("payment_methods");

            entity.HasIndex(e => new { e.ClinicId, e.Name }, "uq_payment_method").IsUnique();

            entity.Property(e => e.PaymentMethodId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Clinic).WithMany(p => p.PaymentMethods)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_payment_methods_clinic");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PRIMARY");

            entity.ToTable("suppliers");

            entity.HasIndex(e => e.ClinicId, "idx_suppliers_clinic");

            entity.HasIndex(e => new { e.ClinicId, e.Name }, "uq_supplier_name").IsUnique();

            entity.Property(e => e.SupplierId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("supplier_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(150)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_suppliers_clinic");
        });

        modelBuilder.Entity<SupplierFinancialSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("supplier_financial_summary");

            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SupplierId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("supplier_id");
            entity.Property(e => e.TotalPaid)
                .HasPrecision(56, 2)
                .HasColumnName("total_paid");
            entity.Property(e => e.TotalPurchases)
                .HasPrecision(34, 2)
                .HasColumnName("total_purchases");
            entity.Property(e => e.TotalRemaining)
                .HasPrecision(57, 2)
                .HasColumnName("total_remaining");
            entity.Property(e => e.TotalTransactions)
                .HasColumnType("bigint(21)")
                .HasColumnName("total_transactions");
        });

        modelBuilder.Entity<Treatment>(entity =>
        {
            entity.HasKey(e => e.TreatmentId).HasName("PRIMARY");

            entity.ToTable("treatments");

            entity.HasIndex(e => e.CategoryId, "idx_treatments_category");

            entity.HasIndex(e => e.ClinicId, "idx_treatments_clinic");

            entity.Property(e => e.TreatmentId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("treatment_id");
            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DefaultPrice)
                .HasPrecision(12, 2)
                .HasColumnName("default_price");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DurationMinutes)
                .HasColumnType("int(11)")
                .HasColumnName("duration_minutes");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Treatments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_treatments_category");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Treatments)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_treatments_clinic");
        });

        modelBuilder.Entity<TreatmentCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("treatment_categories");

            entity.HasIndex(e => new { e.ClinicId, e.Name }, "uq_treatment_category").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.TreatmentCategories)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_treatment_categories_clinic");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.ClinicId, "idx_users_clinic");

            entity.HasIndex(e => new { e.ClinicId, e.Role }, "idx_users_role");

            entity.HasIndex(e => new { e.ClinicId, e.Email }, "uq_users_clinic_email").IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'SECRETARY'")
                .HasColumnType("enum('ADMIN','DOCTOR','SECRETARY')")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Users)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_users_clinic");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PRIMARY");

            entity.ToTable("visits");

            entity.HasIndex(e => e.CreatedBy, "fk_visits_created_by");

            entity.HasIndex(e => new { e.ClinicId, e.VisitDate }, "idx_visits_clinic");

            entity.HasIndex(e => new { e.DoctorId, e.VisitDate }, "idx_visits_doctor");

            entity.HasIndex(e => new { e.PatientId, e.VisitDate }, "idx_visits_patient");

            entity.Property(e => e.VisitId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("visit_id");
            entity.Property(e => e.ChiefComplaint)
                .HasColumnType("text")
                .HasColumnName("chief_complaint");
            entity.Property(e => e.ClinicId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("clinic_id");
            entity.Property(e => e.ClinicalNotes)
                .HasColumnType("text")
                .HasColumnName("clinical_notes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("created_by");
            entity.Property(e => e.Diagnosis)
                .HasColumnType("text")
                .HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("doctor_id");
            entity.Property(e => e.FollowUpDate).HasColumnName("follow_up_date");
            entity.Property(e => e.PatientId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("patient_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VisitDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("visit_date");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("fk_visits_clinic");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Visits)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_visits_created_by");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Visits)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_visits_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Visits)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_visits_patient");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
