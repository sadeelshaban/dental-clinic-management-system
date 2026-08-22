-- ============================================================
-- DENTAL CLINIC MANAGEMENT SYSTEM
-- COMPLETE MYSQL / MARIADB DATABASE
-- MySQL 8.0+ / MariaDB 10.4+ / XAMPP / phpMyAdmin
-- ============================================================

DROP DATABASE IF EXISTS dental_clinic_db;

CREATE DATABASE dental_clinic_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE dental_clinic_db;


-- ============================================================
-- 1. CLINICS
-- ============================================================

CREATE TABLE clinics (
    clinic_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    name VARCHAR(150) NOT NULL,
    legal_name VARCHAR(200),

    logo_url VARCHAR(500),

    phone VARCHAR(50),
    email VARCHAR(150),

    address TEXT,
    city VARCHAR(100),
    country VARCHAR(100) DEFAULT 'Palestine',

    currency_code CHAR(3) NOT NULL DEFAULT 'ILS',
    currency_symbol VARCHAR(10) NOT NULL DEFAULT '₪',

    timezone VARCHAR(100) NOT NULL DEFAULT 'Asia/Gaza',

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_clinics_active (is_active)
) ENGINE=InnoDB;


-- ============================================================
-- 2. USERS
-- ============================================================

CREATE TABLE users (
    user_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL,

    password_hash VARCHAR(255) NOT NULL,

    role ENUM(
        'ADMIN',
        'DOCTOR',
        'SECRETARY'
    ) NOT NULL DEFAULT 'SECRETARY',

    phone VARCHAR(50),

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    last_login_at DATETIME NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_users_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_users_clinic_email (clinic_id, email),

    INDEX idx_users_clinic (clinic_id),
    INDEX idx_users_role (clinic_id, role)

) ENGINE=InnoDB;


-- ============================================================
-- 3. DOCTORS
-- ============================================================

CREATE TABLE doctors (
    doctor_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    user_id BIGINT UNSIGNED NOT NULL,

    license_number VARCHAR(100),
    specialization VARCHAR(150),
    bio TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_doctors_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_doctors_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id)
        ON DELETE RESTRICT,

    UNIQUE KEY uq_doctors_user (user_id),

    INDEX idx_doctors_clinic (clinic_id)

) ENGINE=InnoDB;


-- ============================================================
-- 4. PATIENTS
-- ============================================================

CREATE TABLE patients (
    patient_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_number VARCHAR(50) NOT NULL,

    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,

    phone VARCHAR(50),
    email VARCHAR(150),

    date_of_birth DATE,

    gender ENUM(
        'MALE',
        'FEMALE',
        'OTHER',
        'UNKNOWN'
    ) NOT NULL DEFAULT 'UNKNOWN',

    national_id VARCHAR(100),

    address TEXT,

    emergency_contact_name VARCHAR(150),
    emergency_contact_phone VARCHAR(50),

    medical_alerts TEXT,
    allergies TEXT,
    medications TEXT,
    medical_history TEXT,

    notes TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_patients_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_patient_number (
        clinic_id,
        patient_number
    ),

    INDEX idx_patients_clinic (clinic_id),
    INDEX idx_patients_name (
        clinic_id,
        last_name,
        first_name
    ),
    INDEX idx_patients_phone (
        clinic_id,
        phone
    ),
    INDEX idx_patients_national_id (
        clinic_id,
        national_id
    )

) ENGINE=InnoDB;


-- ============================================================
-- 5. PATIENT CONTACTS
-- ============================================================

CREATE TABLE patient_contacts (
    contact_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    patient_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(150) NOT NULL,
    relationship VARCHAR(100),

    phone VARCHAR(50),

    is_primary BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_patient_contacts_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE CASCADE,

    INDEX idx_patient_contacts_patient (patient_id)

) ENGINE=InnoDB;


-- ============================================================
-- 6. TREATMENT CATEGORIES
-- ============================================================

CREATE TABLE treatment_categories (
    category_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(150) NOT NULL,
    description TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_treatment_categories_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_treatment_category (
        clinic_id,
        name
    )

) ENGINE=InnoDB;


-- ============================================================
-- 7. TREATMENT CATALOG
-- ============================================================

CREATE TABLE treatments (
    treatment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    category_id BIGINT UNSIGNED NULL,

    name VARCHAR(200) NOT NULL,
    description TEXT,

    default_price DECIMAL(12,2) NOT NULL DEFAULT 0.00,

    duration_minutes INT NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_treatments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_treatments_category
        FOREIGN KEY (category_id)
        REFERENCES treatment_categories(category_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_treatment_price
        CHECK (default_price >= 0),

    CONSTRAINT chk_treatment_duration
        CHECK (
            duration_minutes IS NULL
            OR duration_minutes > 0
        ),

    INDEX idx_treatments_clinic (clinic_id),
    INDEX idx_treatments_category (category_id)

) ENGINE=InnoDB;


-- ============================================================
-- 8. VISITS
-- ============================================================

CREATE TABLE visits (
    visit_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_id BIGINT UNSIGNED NOT NULL,

    doctor_id BIGINT UNSIGNED NOT NULL,

    visit_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    chief_complaint TEXT,
    diagnosis TEXT,
    clinical_notes TEXT,

    follow_up_date DATE NULL,

    created_by BIGINT UNSIGNED NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_visits_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_visits_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_visits_doctor
        FOREIGN KEY (doctor_id)
        REFERENCES doctors(doctor_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_visits_created_by
        FOREIGN KEY (created_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    INDEX idx_visits_patient (
        patient_id,
        visit_date DESC
    ),

    INDEX idx_visits_doctor (
        doctor_id,
        visit_date DESC
    ),

    INDEX idx_visits_clinic (
        clinic_id,
        visit_date DESC
    )

) ENGINE=InnoDB;


-- ============================================================
-- 9. PATIENT TREATMENTS
-- ============================================================

CREATE TABLE patient_treatments (
    patient_treatment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_id BIGINT UNSIGNED NOT NULL,

    doctor_id BIGINT UNSIGNED NOT NULL,

    visit_id BIGINT UNSIGNED NULL,

    treatment_id BIGINT UNSIGNED NULL,

    treatment_name VARCHAR(200) NOT NULL,

    treatment_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    quantity DECIMAL(10,2) NOT NULL DEFAULT 1.00,

    unit_price DECIMAL(12,2) NOT NULL DEFAULT 0.00,

    discount_amount DECIMAL(12,2) NOT NULL DEFAULT 0.00,

    final_amount DECIMAL(12,2)
        GENERATED ALWAYS AS (
            GREATEST(
                (quantity * unit_price) - discount_amount,
                0
            )
        ) STORED,

    status ENUM(
        'UNPAID',
        'PARTIALLY_PAID',
        'PAID',
        'VOIDED'
    ) NOT NULL DEFAULT 'UNPAID',

    notes TEXT,

    created_by BIGINT UNSIGNED NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_patient_treatments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_patient_treatments_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_patient_treatments_doctor
        FOREIGN KEY (doctor_id)
        REFERENCES doctors(doctor_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_patient_treatments_visit
        FOREIGN KEY (visit_id)
        REFERENCES visits(visit_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_patient_treatments_treatment
        FOREIGN KEY (treatment_id)
        REFERENCES treatments(treatment_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_patient_treatments_created_by
        FOREIGN KEY (created_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_patient_treatment_quantity
        CHECK (quantity > 0),

    CONSTRAINT chk_patient_treatment_unit_price
        CHECK (unit_price >= 0),

    CONSTRAINT chk_patient_treatment_discount
        CHECK (discount_amount >= 0),

    CONSTRAINT chk_patient_treatment_discount_limit
        CHECK (
            discount_amount <= quantity * unit_price
        ),

    INDEX idx_patient_treatments_patient (
        patient_id,
        treatment_date DESC
    ),

    INDEX idx_patient_treatments_doctor (
        doctor_id,
        treatment_date DESC
    ),

    INDEX idx_patient_treatments_clinic (
        clinic_id,
        treatment_date DESC
    )

) ENGINE=InnoDB;


-- ============================================================
-- 10. APPOINTMENTS
-- ============================================================

CREATE TABLE appointments (
    appointment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_id BIGINT UNSIGNED NOT NULL,

    doctor_id BIGINT UNSIGNED NOT NULL,

    appointment_date DATE NOT NULL,

    start_time TIME NOT NULL,
    end_time TIME NOT NULL,

    status ENUM(
        'SCHEDULED',
        'CONFIRMED',
        'COMPLETED',
        'CANCELLED',
        'NO_SHOW'
    ) NOT NULL DEFAULT 'SCHEDULED',

    reason VARCHAR(250),

    notes TEXT,

    created_by BIGINT UNSIGNED NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_appointments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_appointments_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_appointments_doctor
        FOREIGN KEY (doctor_id)
        REFERENCES doctors(doctor_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_appointments_created_by
        FOREIGN KEY (created_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_appointment_time
        CHECK (end_time > start_time),

    INDEX idx_appointments_clinic_date (
        clinic_id,
        appointment_date
    ),

    INDEX idx_appointments_doctor_date (
        doctor_id,
        appointment_date,
        start_time
    ),

    INDEX idx_appointments_patient (
        patient_id,
        appointment_date DESC
    ),

    INDEX idx_appointments_status (
        clinic_id,
        status
    )

) ENGINE=InnoDB;


-- ============================================================
-- 11. PAYMENT METHODS
-- ============================================================

CREATE TABLE payment_methods (
    payment_method_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(100) NOT NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_payment_methods_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_payment_method (
        clinic_id,
        name
    )

) ENGINE=InnoDB;


-- ============================================================
-- 12. PATIENT PAYMENTS
-- ============================================================

CREATE TABLE patient_payments (
    payment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_id BIGINT UNSIGNED NOT NULL,

    patient_treatment_id BIGINT UNSIGNED NOT NULL,

    amount DECIMAL(12,2) NOT NULL,

    payment_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    method ENUM(
        'CASH',
        'CARD',
        'BANK_TRANSFER',
        'CHEQUE',
        'OTHER'
    ) NOT NULL DEFAULT 'CASH',

    payment_method_id BIGINT UNSIGNED NULL,

    reference_number VARCHAR(150),

    notes TEXT,

    received_by BIGINT UNSIGNED NULL,

    is_voided BOOLEAN NOT NULL DEFAULT FALSE,

    voided_at DATETIME NULL,

    voided_by BIGINT UNSIGNED NULL,

    void_reason TEXT,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_patient_payments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_patient_payments_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_patient_payments_treatment
        FOREIGN KEY (patient_treatment_id)
        REFERENCES patient_treatments(patient_treatment_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_patient_payments_method
        FOREIGN KEY (payment_method_id)
        REFERENCES payment_methods(payment_method_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_patient_payments_received_by
        FOREIGN KEY (received_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_patient_payments_voided_by
        FOREIGN KEY (voided_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_patient_payment_amount
        CHECK (amount > 0),

    INDEX idx_patient_payments_patient (
        patient_id,
        payment_date DESC
    ),

    INDEX idx_patient_payments_treatment (
        patient_treatment_id
    ),

    INDEX idx_patient_payments_date (
        clinic_id,
        payment_date
    ),

    INDEX idx_patient_payments_voided (
        clinic_id,
        is_voided
    )

) ENGINE=InnoDB;


-- ============================================================
-- 13. EXPENSE CATEGORIES
-- ============================================================

CREATE TABLE expense_categories (
    category_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(150) NOT NULL,
    description TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_expense_categories_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_expense_category (
        clinic_id,
        name
    )

) ENGINE=InnoDB;


-- ============================================================
-- 14. SUPPLIERS
-- ============================================================

CREATE TABLE suppliers (
    supplier_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(200) NOT NULL,

    phone VARCHAR(50),
    email VARCHAR(150),

    address TEXT,

    contact_person VARCHAR(150),

    notes TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_suppliers_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_supplier_name (
        clinic_id,
        name
    ),

    INDEX idx_suppliers_clinic (clinic_id)

) ENGINE=InnoDB;


-- ============================================================
-- 15. EXPENSES / OBLIGATIONS
-- ============================================================

CREATE TABLE expenses (
    expense_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    category_id BIGINT UNSIGNED NULL,

    supplier_id BIGINT UNSIGNED NULL,

    expense_type ENUM(
        'GENERAL',
        'SUPPLIER_PURCHASE',
        'RENT',
        'UTILITIES',
        'EQUIPMENT',
        'MAINTENANCE',
        'LABORATORY',
        'MATERIALS',
        'OTHER'
    ) NOT NULL DEFAULT 'GENERAL',

    description VARCHAR(300) NOT NULL,

    expense_date DATE NOT NULL DEFAULT (CURRENT_DATE),

    due_date DATE NULL,

    total_amount DECIMAL(12,2) NOT NULL,

    status ENUM(
        'UNPAID',
        'PARTIALLY_PAID',
        'PAID',
        'VOIDED'
    ) NOT NULL DEFAULT 'UNPAID',

    notes TEXT,

    created_by BIGINT UNSIGNED NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_expenses_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_expenses_category
        FOREIGN KEY (category_id)
        REFERENCES expense_categories(category_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_expenses_supplier
        FOREIGN KEY (supplier_id)
        REFERENCES suppliers(supplier_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_expenses_created_by
        FOREIGN KEY (created_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_expense_amount
        CHECK (total_amount > 0),

    INDEX idx_expenses_clinic_date (
        clinic_id,
        expense_date DESC
    ),

    INDEX idx_expenses_supplier (
        supplier_id
    ),

    INDEX idx_expenses_status (
        clinic_id,
        status
    ),

    INDEX idx_expenses_category (
        category_id
    )

) ENGINE=InnoDB;


-- ============================================================
-- 16. EXPENSE PAYMENTS
-- ============================================================

CREATE TABLE expense_payments (
    expense_payment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    expense_id BIGINT UNSIGNED NOT NULL,

    amount DECIMAL(12,2) NOT NULL,

    payment_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    method ENUM(
        'CASH',
        'CARD',
        'BANK_TRANSFER',
        'CHEQUE',
        'OTHER'
    ) NOT NULL DEFAULT 'CASH',

    payment_method_id BIGINT UNSIGNED NULL,

    reference_number VARCHAR(150),

    notes TEXT,

    paid_by BIGINT UNSIGNED NULL,

    is_voided BOOLEAN NOT NULL DEFAULT FALSE,

    voided_at DATETIME NULL,

    voided_by BIGINT UNSIGNED NULL,

    void_reason TEXT,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_expense_payments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_expense_payments_expense
        FOREIGN KEY (expense_id)
        REFERENCES expenses(expense_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_expense_payments_method
        FOREIGN KEY (payment_method_id)
        REFERENCES payment_methods(payment_method_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_expense_payments_paid_by
        FOREIGN KEY (paid_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_expense_payments_voided_by
        FOREIGN KEY (voided_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_expense_payment_amount
        CHECK (amount > 0),

    INDEX idx_expense_payments_expense (
        expense_id
    ),

    INDEX idx_expense_payments_date (
        clinic_id,
        payment_date
    ),

    INDEX idx_expense_payments_voided (
        clinic_id,
        is_voided
    )

) ENGINE=InnoDB;


-- ============================================================
-- 17. ATTACHMENTS
-- ============================================================

CREATE TABLE attachments (
    attachment_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    patient_id BIGINT UNSIGNED NULL,

    patient_treatment_id BIGINT UNSIGNED NULL,

    file_name VARCHAR(255) NOT NULL,

    file_url VARCHAR(1000) NOT NULL,

    file_type VARCHAR(100),

    file_size BIGINT UNSIGNED,

    uploaded_by BIGINT UNSIGNED NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_attachments_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_attachments_patient
        FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_attachments_treatment
        FOREIGN KEY (patient_treatment_id)
        REFERENCES patient_treatments(patient_treatment_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_attachments_uploaded_by
        FOREIGN KEY (uploaded_by)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    CONSTRAINT chk_attachment_parent
        CHECK (
            patient_id IS NOT NULL
            OR patient_treatment_id IS NOT NULL
        ),

    INDEX idx_attachments_patient (
        patient_id
    ),

    INDEX idx_attachments_treatment (
        patient_treatment_id
    )

) ENGINE=InnoDB;


-- ============================================================
-- 18. WORKING HOURS
-- ============================================================

CREATE TABLE clinic_working_hours (
    working_hour_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    day_of_week TINYINT NOT NULL,

    is_open BOOLEAN NOT NULL DEFAULT TRUE,

    opening_time TIME NULL,
    closing_time TIME NULL,

    CONSTRAINT fk_working_hours_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    CONSTRAINT chk_day_of_week
        CHECK (
            day_of_week BETWEEN 0 AND 6
        ),

    CONSTRAINT chk_working_hours
        CHECK (
            is_open = FALSE
            OR (
                opening_time IS NOT NULL
                AND closing_time IS NOT NULL
                AND closing_time > opening_time
            )
        ),

    UNIQUE KEY uq_clinic_day (
        clinic_id,
        day_of_week
    )

) ENGINE=InnoDB;


-- ============================================================
-- 19. CLINIC SETTINGS
-- ============================================================

CREATE TABLE clinic_settings (
    setting_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NOT NULL,

    setting_key VARCHAR(100) NOT NULL,

    setting_value TEXT,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_clinic_settings_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE CASCADE,

    UNIQUE KEY uq_clinic_setting (
        clinic_id,
        setting_key
    )

) ENGINE=InnoDB;


-- ============================================================
-- 20. AUDIT LOGS
-- ============================================================

CREATE TABLE audit_logs (
    audit_id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    clinic_id BIGINT UNSIGNED NULL,

    user_id BIGINT UNSIGNED NULL,

    action VARCHAR(50) NOT NULL,

    entity_name VARCHAR(100) NOT NULL,

    entity_id BIGINT UNSIGNED NULL,

    old_values JSON NULL,

    new_values JSON NULL,

    ip_address VARCHAR(45),

    user_agent TEXT,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_audit_logs_clinic
        FOREIGN KEY (clinic_id)
        REFERENCES clinics(clinic_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_audit_logs_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id)
        ON DELETE SET NULL,

    INDEX idx_audit_logs_clinic (
        clinic_id,
        created_at DESC
    ),

    INDEX idx_audit_logs_entity (
        entity_name,
        entity_id
    ),

    INDEX idx_audit_logs_user (
        user_id
    )

) ENGINE=InnoDB;


-- ============================================================
-- 21. PATIENT TREATMENT FINANCIAL VIEW
-- ============================================================

CREATE OR REPLACE VIEW patient_treatment_financials AS

SELECT

    pt.patient_treatment_id,
    pt.clinic_id,
    pt.patient_id,
    pt.doctor_id,

    pt.treatment_date,
    pt.treatment_name,

    pt.quantity,
    pt.unit_price,
    pt.discount_amount,

    pt.final_amount AS treatment_total,

    COALESCE(
        SUM(
            CASE
                WHEN pp.is_voided = FALSE
                THEN pp.amount
                ELSE 0
            END
        ),
        0
    ) AS total_paid,

    pt.final_amount
    -
    COALESCE(
        SUM(
            CASE
                WHEN pp.is_voided = FALSE
                THEN pp.amount
                ELSE 0
            END
        ),
        0
    ) AS remaining_balance

FROM patient_treatments pt

LEFT JOIN patient_payments pp
    ON pp.patient_treatment_id =
       pt.patient_treatment_id

GROUP BY

    pt.patient_treatment_id,
    pt.clinic_id,
    pt.patient_id,
    pt.doctor_id,
    pt.treatment_date,
    pt.treatment_name,
    pt.quantity,
    pt.unit_price,
    pt.discount_amount,
    pt.final_amount;


-- ============================================================
-- 22. PATIENT FINANCIAL SUMMARY
-- ============================================================

CREATE OR REPLACE VIEW patient_financial_summary AS

SELECT

    p.patient_id,
    p.clinic_id,

    p.patient_number,

    p.first_name,
    p.last_name,

    CONCAT(
        p.first_name,
        ' ',
        p.last_name
    ) AS full_name,

    p.phone,

    COALESCE(
        SUM(ptf.treatment_total),
        0
    ) AS total_treatments,

    COALESCE(
        SUM(ptf.total_paid),
        0
    ) AS total_paid,

    COALESCE(
        SUM(ptf.remaining_balance),
        0
    ) AS total_remaining

FROM patients p

LEFT JOIN patient_treatment_financials ptf
    ON ptf.patient_id = p.patient_id

WHERE p.is_active = TRUE

GROUP BY

    p.patient_id,
    p.clinic_id,
    p.patient_number,
    p.first_name,
    p.last_name,
    p.phone;


-- ============================================================
-- 23. EXPENSE FINANCIAL VIEW
-- ============================================================

CREATE OR REPLACE VIEW expense_financials AS

SELECT

    e.expense_id,
    e.clinic_id,

    e.supplier_id,
    e.category_id,

    e.description,
    e.expense_type,

    e.expense_date,
    e.due_date,

    e.total_amount,

    COALESCE(
        SUM(
            CASE
                WHEN ep.is_voided = FALSE
                THEN ep.amount
                ELSE 0
            END
        ),
        0
    ) AS total_paid,

    e.total_amount
    -
    COALESCE(
        SUM(
            CASE
                WHEN ep.is_voided = FALSE
                THEN ep.amount
                ELSE 0
            END
        ),
        0
    ) AS remaining_balance

FROM expenses e

LEFT JOIN expense_payments ep
    ON ep.expense_id = e.expense_id

GROUP BY

    e.expense_id,
    e.clinic_id,
    e.supplier_id,
    e.category_id,
    e.description,
    e.expense_type,
    e.expense_date,
    e.due_date,
    e.total_amount;


-- ============================================================
-- 24. DAILY FINANCIAL SUMMARY
-- MariaDB/MySQL: FULL OUTER JOIN replaced with UNION pattern
-- ============================================================

CREATE OR REPLACE VIEW daily_financial_summary AS

WITH revenue AS (

    SELECT

        clinic_id,

        DATE(payment_date) AS financial_date,

        SUM(amount) AS revenue

    FROM patient_payments

    WHERE is_voided = FALSE

    GROUP BY
        clinic_id,
        DATE(payment_date)

),

expenses_paid AS (

    SELECT

        clinic_id,

        DATE(payment_date) AS financial_date,

        SUM(amount) AS expenses

    FROM expense_payments

    WHERE is_voided = FALSE

    GROUP BY
        clinic_id,
        DATE(payment_date)

),

combined AS (

    SELECT
        clinic_id,
        financial_date
    FROM revenue

    UNION

    SELECT
        clinic_id,
        financial_date
    FROM expenses_paid

)

SELECT

    c.clinic_id,

    c.financial_date,

    COALESCE(r.revenue, 0) AS revenue,

    COALESCE(e.expenses, 0) AS expenses,

    COALESCE(r.revenue, 0) - COALESCE(e.expenses, 0) AS net_profit

FROM combined c

LEFT JOIN revenue r
    ON r.clinic_id = c.clinic_id
   AND r.financial_date = c.financial_date

LEFT JOIN expenses_paid e
    ON e.clinic_id = c.clinic_id
   AND e.financial_date = c.financial_date;


-- ============================================================
-- 25. MONTHLY FINANCIAL SUMMARY
-- ============================================================

CREATE OR REPLACE VIEW monthly_financial_summary AS

WITH revenue AS (

    SELECT

        clinic_id,

        DATE_FORMAT(
            payment_date,
            '%Y-%m-01'
        ) AS month,

        SUM(amount) AS revenue

    FROM patient_payments

    WHERE is_voided = FALSE

    GROUP BY

        clinic_id,

        DATE_FORMAT(
            payment_date,
            '%Y-%m-01'
        )

),

expenses_paid AS (

    SELECT

        clinic_id,

        DATE_FORMAT(
            payment_date,
            '%Y-%m-01'
        ) AS month,

        SUM(amount) AS expenses

    FROM expense_payments

    WHERE is_voided = FALSE

    GROUP BY

        clinic_id,

        DATE_FORMAT(
            payment_date,
            '%Y-%m-01'
        )

),

patient_counts AS (

    SELECT

        clinic_id,

        DATE_FORMAT(
            created_at,
            '%Y-%m-01'
        ) AS month,

        COUNT(*) AS patients

    FROM patients

    GROUP BY

        clinic_id,

        DATE_FORMAT(
            created_at,
            '%Y-%m-01'
        )

),

appointment_counts AS (

    SELECT

        clinic_id,

        DATE_FORMAT(
            appointment_date,
            '%Y-%m-01'
        ) AS month,

        COUNT(*) AS appointments

    FROM appointments

    GROUP BY

        clinic_id,

        DATE_FORMAT(
            appointment_date,
            '%Y-%m-01'
        )

),

combined AS (

    SELECT clinic_id, month FROM revenue
    UNION
    SELECT clinic_id, month FROM expenses_paid
    UNION
    SELECT clinic_id, month FROM patient_counts
    UNION
    SELECT clinic_id, month FROM appointment_counts

)

SELECT

    c.clinic_id,

    c.month,

    COALESCE(r.revenue, 0) AS revenue,

    COALESCE(e.expenses, 0) AS expenses,

    COALESCE(r.revenue, 0) - COALESCE(e.expenses, 0) AS net_profit,

    COALESCE(p.patients, 0) AS patients,

    COALESCE(a.appointments, 0) AS appointments

FROM combined c

LEFT JOIN revenue r
    ON r.clinic_id = c.clinic_id
   AND r.month = c.month

LEFT JOIN expenses_paid e
    ON e.clinic_id = c.clinic_id
   AND e.month = c.month

LEFT JOIN patient_counts p
    ON p.clinic_id = c.clinic_id
   AND p.month = c.month

LEFT JOIN appointment_counts a
    ON a.clinic_id = c.clinic_id
   AND a.month = c.month;


-- ============================================================
-- 26. MONTHLY PERFORMANCE COMPARISON
-- ============================================================

CREATE OR REPLACE VIEW monthly_performance_comparison AS

SELECT

    current_month.clinic_id,

    current_month.month,

    current_month.revenue,

    current_month.expenses,

    current_month.net_profit,

    current_month.patients,

    current_month.appointments,

    previous_month.revenue
        AS previous_month_revenue,

    previous_month.expenses
        AS previous_month_expenses,

    previous_month.net_profit
        AS previous_month_profit,

    previous_month.patients
        AS previous_month_patients,

    previous_month.appointments
        AS previous_month_appointments,

    CASE
        WHEN previous_month.revenue IS NULL
             OR previous_month.revenue = 0
        THEN NULL
        ELSE
            (
                (
                    current_month.revenue
                    -
                    previous_month.revenue
                )
                /
                previous_month.revenue
            ) * 100
    END AS revenue_change_percent,

    CASE
        WHEN previous_month.expenses IS NULL
             OR previous_month.expenses = 0
        THEN NULL
        ELSE
            (
                (
                    current_month.expenses
                    -
                    previous_month.expenses
                )
                /
                previous_month.expenses
            ) * 100
    END AS expense_change_percent,

    CASE
        WHEN previous_month.net_profit IS NULL
             OR previous_month.net_profit = 0
        THEN NULL
        ELSE
            (
                (
                    current_month.net_profit
                    -
                    previous_month.net_profit
                )
                /
                previous_month.net_profit
            ) * 100
    END AS profit_change_percent,

    CASE
        WHEN previous_month.patients IS NULL
             OR previous_month.patients = 0
        THEN NULL
        ELSE
            (
                (
                    current_month.patients
                    -
                    previous_month.patients
                )
                /
                previous_month.patients
            ) * 100
    END AS patient_change_percent,

    CASE
        WHEN previous_month.appointments IS NULL
             OR previous_month.appointments = 0
        THEN NULL
        ELSE
            (
                (
                    current_month.appointments
                    -
                    previous_month.appointments
                )
                /
                previous_month.appointments
            ) * 100
    END AS appointment_change_percent

FROM monthly_financial_summary current_month

LEFT JOIN monthly_financial_summary previous_month

    ON previous_month.clinic_id =
       current_month.clinic_id

    AND previous_month.month =
        DATE_FORMAT(
            DATE_SUB(
                STR_TO_DATE(
                    current_month.month,
                    '%Y-%m-%d'
                ),
                INTERVAL 1 MONTH
            ),
            '%Y-%m-%d'
        );


-- ============================================================
-- 27. SUPPLIER FINANCIAL SUMMARY
-- ============================================================

CREATE OR REPLACE VIEW supplier_financial_summary AS

SELECT

    s.supplier_id,
    s.clinic_id,

    s.name,

    COUNT(e.expense_id)
        AS total_transactions,

    COALESCE(
        SUM(e.total_amount),
        0
    ) AS total_purchases,

    COALESCE(
        SUM(
            COALESCE(
                ef.total_paid,
                0
            )
        ),
        0
    ) AS total_paid,

    COALESCE(
        SUM(
            COALESCE(
                ef.remaining_balance,
                0
            )
        ),
        0
    ) AS total_remaining

FROM suppliers s

LEFT JOIN expenses e
    ON e.supplier_id =
       s.supplier_id

LEFT JOIN expense_financials ef
    ON ef.expense_id =
       e.expense_id

GROUP BY

    s.supplier_id,
    s.clinic_id,
    s.name;


-- ============================================================
-- 28. PATIENT DIRECTORY
-- ============================================================

CREATE OR REPLACE VIEW patient_directory AS

SELECT

    p.patient_id,

    p.clinic_id,

    p.patient_number,

    p.first_name,
    p.last_name,

    CONCAT(
        p.first_name,
        ' ',
        p.last_name
    ) AS full_name,

    p.phone,
    p.email,

    p.date_of_birth,
    p.gender,

    p.is_active,

    COALESCE(
        pfs.total_treatments,
        0
    ) AS total_treatments,

    COALESCE(
        pfs.total_paid,
        0
    ) AS total_paid,

    COALESCE(
        pfs.total_remaining,
        0
    ) AS total_remaining

FROM patients p

LEFT JOIN patient_financial_summary pfs
    ON pfs.patient_id =
       p.patient_id;


-- ============================================================
-- 29. DEMO CLINIC
-- ============================================================

INSERT INTO clinics (
    name,
    legal_name,
    phone,
    email,
    address,
    city,
    country,
    currency_code,
    currency_symbol,
    timezone
)

VALUES (

    'Demo Dental Clinic',

    'Demo Dental Clinic',

    '+970000000000',

    'demo@example.com',

    'Demo Address',

    'Nablus',

    'Palestine',

    'ILS',

    '₪',

    'Asia/Gaza'
);


-- ============================================================
-- 30. DEMO DATA
-- ============================================================

SET @clinic_id = (
    SELECT clinic_id
    FROM clinics
    WHERE name = 'Demo Dental Clinic'
    LIMIT 1
);


INSERT INTO payment_methods (
    clinic_id,
    name
)

VALUES
    (@clinic_id, 'Cash'),
    (@clinic_id, 'Card'),
    (@clinic_id, 'Bank Transfer'),
    (@clinic_id, 'Cheque');


INSERT INTO expense_categories (
    clinic_id,
    name,
    description
)

VALUES

(
    @clinic_id,
    'Rent',
    'Clinic rent'
),

(
    @clinic_id,
    'Materials',
    'Dental materials and consumables'
),

(
    @clinic_id,
    'Laboratory',
    'Dental laboratory expenses'
),

(
    @clinic_id,
    'Equipment',
    'Dental equipment'
),

(
    @clinic_id,
    'Maintenance',
    'Clinic and equipment maintenance'
),

(
    @clinic_id,
    'Utilities',
    'Electricity, water and internet'
),

(
    @clinic_id,
    'Other',
    'Other expenses'
);


INSERT INTO treatment_categories (
    clinic_id,
    name
)

VALUES

(@clinic_id, 'Diagnostic'),
(@clinic_id, 'Restorative'),
(@clinic_id, 'Endodontics'),
(@clinic_id, 'Prosthodontics'),
(@clinic_id, 'Oral Surgery'),
(@clinic_id, 'Preventive'),
(@clinic_id, 'Orthodontics'),
(@clinic_id, 'Other');


SET @diagnostic_category = (
    SELECT category_id
    FROM treatment_categories
    WHERE clinic_id = @clinic_id
      AND name = 'Diagnostic'
    LIMIT 1
);

SET @restorative_category = (
    SELECT category_id
    FROM treatment_categories
    WHERE clinic_id = @clinic_id
      AND name = 'Restorative'
    LIMIT 1
);

SET @endodontics_category = (
    SELECT category_id
    FROM treatment_categories
    WHERE clinic_id = @clinic_id
      AND name = 'Endodontics'
    LIMIT 1
);

SET @surgery_category = (
    SELECT category_id
    FROM treatment_categories
    WHERE clinic_id = @clinic_id
      AND name = 'Oral Surgery'
    LIMIT 1
);

SET @preventive_category = (
    SELECT category_id
    FROM treatment_categories
    WHERE clinic_id = @clinic_id
      AND name = 'Preventive'
    LIMIT 1
);


INSERT INTO treatments (
    clinic_id,
    category_id,
    name,
    description,
    default_price,
    duration_minutes
)

VALUES

(
    @clinic_id,
    @diagnostic_category,
    'Dental Examination',
    'General dental examination',
    50.00,
    30
),

(
    @clinic_id,
    @diagnostic_category,
    'Dental X-Ray',
    'Dental radiographic examination',
    30.00,
    15
),

(
    @clinic_id,
    @restorative_category,
    'Composite Filling',
    'Composite dental filling',
    100.00,
    45
),

(
    @clinic_id,
    @endodontics_category,
    'Root Canal Treatment',
    'Root canal treatment',
    500.00,
    90
),

(
    @clinic_id,
    @surgery_category,
    'Tooth Extraction',
    'Routine tooth extraction',
    150.00,
    30
),

(
    @clinic_id,
    @preventive_category,
    'Dental Cleaning',
    'Professional dental cleaning',
    100.00,
    45
);


INSERT INTO clinic_settings (
    clinic_id,
    setting_key,
    setting_value
)

VALUES

(
    @clinic_id,
    'appointment_slot_minutes',
    '30'
),

(
    @clinic_id,
    'allow_online_booking',
    'false'
),

(
    @clinic_id,
    'default_payment_method',
    'Cash'
);


INSERT INTO clinic_working_hours (
    clinic_id,
    day_of_week,
    is_open,
    opening_time,
    closing_time
)

VALUES

(@clinic_id, 0, TRUE, '09:00:00', '17:00:00'),
(@clinic_id, 1, TRUE, '09:00:00', '17:00:00'),
(@clinic_id, 2, TRUE, '09:00:00', '17:00:00'),
(@clinic_id, 3, TRUE, '09:00:00', '17:00:00'),
(@clinic_id, 4, TRUE, '09:00:00', '17:00:00'),
(@clinic_id, 5, FALSE, NULL, NULL),
(@clinic_id, 6, FALSE, NULL, NULL);


-- ============================================================
-- 31. FINISH
-- ============================================================

SELECT
    'Dental Clinic Database successfully created'
    AS status;

SHOW TABLES;
