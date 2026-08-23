export interface ApiEnvelope<T> {
  success: boolean
  message?: string | null
  data?: T | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface UserDto {
  userId: number
  clinicId: number
  fullName: string
  email: string
  role: string
  phone?: string | null
  isActive: boolean
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: UserDto
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface PatientListItemDto {
  patientId: number
  patientNumber: string
  firstName: string
  lastName: string
  fullName: string
  phone?: string | null
  email?: string | null
  dateOfBirth?: string | null
  gender: string
  isActive: boolean
  createdAt: string
}

export interface PatientDetailDto extends PatientListItemDto {
  nationalId?: string | null
  address?: string | null
  emergencyContactName?: string | null
  emergencyContactPhone?: string | null
  medicalAlerts?: string | null
  allergies?: string | null
  medications?: string | null
  medicalHistory?: string | null
  notes?: string | null
  updatedAt: string
}

export interface CreatePatientRequest {
  firstName: string
  lastName: string
  phone?: string | null
  email?: string | null
  dateOfBirth?: string | null
  gender: string
  nationalId?: string | null
  address?: string | null
  emergencyContactName?: string | null
  emergencyContactPhone?: string | null
  medicalAlerts?: string | null
  allergies?: string | null
  medications?: string | null
  medicalHistory?: string | null
  notes?: string | null
}

export interface UpdatePatientRequest extends CreatePatientRequest {
  isActive?: boolean | null
}

export interface PatientSearchQuery {
  search?: string
  isActive?: boolean | null
  page?: number
  pageSize?: number
}

export interface UserListItemDto {
  userId: number
  fullName: string
  email: string
  role: string
  phone?: string | null
  isActive: boolean
  hasDoctorProfile: boolean
  lastLoginAt?: string | null
  createdAt: string
}

export interface DoctorProfileDto {
  doctorId: number
  licenseNumber?: string | null
  specialization?: string | null
  bio?: string | null
  isActive: boolean
}

export interface UserDetailDto extends UserListItemDto {
  clinicId: number
  doctorProfile?: DoctorProfileDto | null
  updatedAt: string
}

export interface CreateDoctorProfileRequest {
  licenseNumber?: string | null
  specialization?: string | null
  bio?: string | null
}

export interface CreateUserRequest {
  fullName: string
  email: string
  password: string
  role: string
  phone?: string | null
  doctorProfile?: CreateDoctorProfileRequest | null
}

export interface UpdateUserRequest {
  fullName?: string | null
  email?: string | null
  phone?: string | null
  role?: string | null
}

export interface ResetPasswordRequest {
  newPassword: string
}

export interface UserSearchQuery {
  search?: string
  role?: string
  isActive?: boolean | null
  page?: number
  pageSize?: number
}

export interface DoctorListItemDto {
  doctorId: number
  userId: number
  fullName: string
  email: string
  phone?: string | null
  specialization?: string | null
  licenseNumber?: string | null
  isActive: boolean
}

export interface DoctorDetailDto extends DoctorListItemDto {
  bio?: string | null
  createdAt: string
}

export interface UpdateDoctorRequest {
  licenseNumber?: string | null
  specialization?: string | null
  bio?: string | null
}

export interface DoctorSearchQuery {
  search?: string
  isActive?: boolean | null
  page?: number
  pageSize?: number
}

export interface AppointmentListItemDto {
  appointmentId: number
  patientId: number
  patientName: string
  doctorId: number
  doctorName: string
  appointmentDate: string
  startTime: string
  endTime: string
  status: string
  reason?: string | null
}

export interface AppointmentDetailDto extends AppointmentListItemDto {
  notes?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateAppointmentRequest {
  patientId: number
  doctorId?: number | null
  appointmentDate: string
  startTime: string
  endTime: string
  reason?: string | null
  notes?: string | null
}

export interface UpdateAppointmentRequest {
  patientId?: number | null
  doctorId?: number | null
  appointmentDate?: string | null
  startTime?: string | null
  endTime?: string | null
  reason?: string | null
  notes?: string | null
}

export interface AppointmentSearchQuery {
  date?: string
  from?: string
  to?: string
  doctorId?: number
  patientId?: number
  status?: string
  page?: number
  pageSize?: number
}

export interface VisitListItemDto {
  visitId: number
  patientId: number
  patientName: string
  doctorId: number
  doctorName: string
  visitDate: string
  chiefComplaint?: string | null
  followUpDate?: string | null
}

export interface VisitDetailDto extends VisitListItemDto {
  diagnosis?: string | null
  clinicalNotes?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateVisitRequest {
  patientId: number
  doctorId?: number | null
  visitDate: string
  chiefComplaint?: string | null
  diagnosis?: string | null
  clinicalNotes?: string | null
  followUpDate?: string | null
}

export interface UpdateVisitRequest {
  doctorId?: number | null
  visitDate?: string | null
  chiefComplaint?: string | null
  diagnosis?: string | null
  clinicalNotes?: string | null
  followUpDate?: string | null
}

export interface VisitSearchQuery {
  patientId?: number
  doctorId?: number
  date?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export interface TreatmentCategoryDto {
  categoryId: number
  name: string
  description?: string | null
  isActive: boolean
  createdAt: string
}

export interface CreateTreatmentCategoryRequest {
  name: string
  description?: string | null
}

export interface UpdateTreatmentCategoryRequest {
  name?: string | null
  description?: string | null
  isActive?: boolean | null
}

export interface TreatmentCategorySearchQuery {
  search?: string
  isActive?: boolean | null
  page?: number
  pageSize?: number
}

export interface TreatmentListItemDto {
  treatmentId: number
  categoryId?: number | null
  categoryName?: string | null
  name: string
  defaultPrice: number
  durationMinutes?: number | null
  isActive: boolean
}

export interface TreatmentDetailDto extends TreatmentListItemDto {
  description?: string | null
  createdAt: string
}

export interface CreateTreatmentRequest {
  name: string
  categoryId?: number | null
  description?: string | null
  defaultPrice: number
  durationMinutes?: number | null
}

export interface UpdateTreatmentRequest {
  name?: string | null
  categoryId?: number | null
  description?: string | null
  defaultPrice?: number | null
  durationMinutes?: number | null
  isActive?: boolean | null
}

export interface TreatmentSearchQuery {
  search?: string
  categoryId?: number
  isActive?: boolean | null
  page?: number
  pageSize?: number
}

export interface PatientTreatmentListItemDto {
  patientTreatmentId: number
  patientId: number
  patientName: string
  doctorId: number
  doctorName: string
  visitId?: number | null
  treatmentId?: number | null
  treatmentName: string
  treatmentDate: string
  quantity: number
  unitPrice: number
  discountAmount: number
  finalAmount: number
  status: string
}

export interface PatientTreatmentDetailDto extends PatientTreatmentListItemDto {
  notes?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreatePatientTreatmentRequest {
  patientId: number
  doctorId?: number | null
  visitId?: number | null
  treatmentId?: number | null
  treatmentName?: string | null
  treatmentDate?: string | null
  quantity?: number | null
  unitPrice?: number | null
  discountAmount?: number | null
  notes?: string | null
}

export interface UpdatePatientTreatmentRequest {
  quantity?: number | null
  unitPrice?: number | null
  discountAmount?: number | null
  notes?: string | null
  visitId?: number | null
  treatmentDate?: string | null
}

export interface PatientTreatmentSearchQuery {
  patientId?: number
  doctorId?: number
  visitId?: number
  treatmentId?: number
  from?: string
  to?: string
  status?: string
  page?: number
  pageSize?: number
}

export interface PaymentListItemDto {
  paymentId: number
  patientId: number
  patientName: string
  patientTreatmentId: number
  treatmentName: string
  amount: number
  paymentDate: string
  method: string
  paymentMethodId?: number | null
  referenceNumber?: string | null
  isVoided: boolean
}

export interface PaymentDetailDto extends PaymentListItemDto {
  notes?: string | null
  voidReason?: string | null
  voidedAt?: string | null
  createdAt: string
}

export interface CreatePaymentRequest {
  patientTreatmentId: number
  amount: number
  method?: string
  paymentMethodId?: number | null
  paymentDate?: string | null
  referenceNumber?: string | null
  notes?: string | null
}

export interface VoidPaymentRequest {
  reason: string
}

export interface PaymentSearchQuery {
  patientId?: number
  patientTreatmentId?: number
  method?: string
  from?: string
  to?: string
  isVoided?: boolean | null
  page?: number
  pageSize?: number
}

export interface PaymentMethodDto {
  paymentMethodId: number
  name: string
  isActive: boolean
}

export interface CreatePaymentMethodRequest {
  name: string
}

export interface UpdatePaymentMethodRequest {
  name?: string | null
  isActive?: boolean | null
}

export interface StatementLineDto {
  patientTreatmentId: number
  treatmentDate: string
  treatmentName: string
  doctorId: number
  doctorName: string
  treatmentTotal: number
  paid: number
  remaining: number
  status: string
}

export interface PatientFinancialStatementDto {
  patientId: number
  patientName: string
  patientNumber: string
  totalTreatments: number
  totalPaid: number
  totalRemaining: number
  lines: StatementLineDto[]
  payments: PaymentListItemDto[]
}

export interface ExpenseCategoryDto {
  categoryId: number
  name: string
  description?: string | null
  isActive: boolean
  createdAt: string
}

export interface CreateExpenseCategoryRequest {
  name: string
  description?: string | null
}

export interface UpdateExpenseCategoryRequest {
  name?: string | null
  description?: string | null
  isActive?: boolean | null
}

export interface SupplierDto {
  supplierId: number
  name: string
  phone?: string | null
  email?: string | null
  address?: string | null
  contactPerson?: string | null
  notes?: string | null
  isActive: boolean
  createdAt: string
}

export interface CreateSupplierRequest {
  name: string
  phone?: string | null
  email?: string | null
  address?: string | null
  contactPerson?: string | null
  notes?: string | null
}

export interface UpdateSupplierRequest {
  name?: string | null
  phone?: string | null
  email?: string | null
  address?: string | null
  contactPerson?: string | null
  notes?: string | null
  isActive?: boolean | null
}

export interface SupplierSearchQuery {
  search?: string
  isActive?: boolean | null
}

export interface ExpenseListItemDto {
  expenseId: number
  supplierId?: number | null
  supplierName?: string | null
  categoryId?: number | null
  categoryName?: string | null
  expenseType: string
  description: string
  expenseDate: string
  dueDate?: string | null
  totalAmount: number
  status: string
}

export interface ExpenseDetailDto extends ExpenseListItemDto {
  notes?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateExpenseRequest {
  supplierId?: number | null
  categoryId?: number | null
  expenseType?: string
  description: string
  expenseDate?: string | null
  dueDate?: string | null
  totalAmount: number
  notes?: string | null
}

export interface UpdateExpenseRequest {
  supplierId?: number | null
  categoryId?: number | null
  expenseType?: string | null
  description?: string | null
  expenseDate?: string | null
  dueDate?: string | null
  totalAmount?: number | null
  notes?: string | null
}

export interface VoidExpenseRequest {
  reason: string
}

export interface ExpenseSearchQuery {
  supplierId?: number
  categoryId?: number
  expenseType?: string
  from?: string
  to?: string
  status?: string
  page?: number
  pageSize?: number
}

export interface ExpensePaymentListItemDto {
  expensePaymentId: number
  expenseId: number
  supplierName?: string | null
  amount: number
  paymentDate: string
  method: string
  paymentMethodId?: number | null
  referenceNumber?: string | null
  isVoided: boolean
}

export interface ExpensePaymentDetailDto extends ExpensePaymentListItemDto {
  notes?: string | null
  voidReason?: string | null
  voidedAt?: string | null
  createdAt: string
}

export interface CreateExpensePaymentRequest {
  expenseId: number
  amount: number
  method?: string
  paymentMethodId?: number | null
  paymentDate?: string | null
  referenceNumber?: string | null
  notes?: string | null
}

export interface VoidExpensePaymentRequest {
  reason: string
}

export interface ExpensePaymentSearchQuery {
  expenseId?: number
  supplierId?: number
  method?: string
  from?: string
  to?: string
  isVoided?: boolean | null
  page?: number
  pageSize?: number
}

export interface SupplierStatementLineDto {
  expenseId: number
  expenseDate: string
  dueDate?: string | null
  expenseType: string
  categoryName?: string | null
  description: string
  totalAmount: number
  paid: number
  remaining: number
  status: string
}

export interface SupplierFinancialStatementDto {
  supplierId: number
  supplierName: string
  totalTransactions: number
  totalPurchases: number
  totalPaid: number
  totalRemaining: number
  lines: SupplierStatementLineDto[]
  payments: ExpensePaymentListItemDto[]
}

export interface DailyFinancialSummaryDto {
  financialDate: string
  revenue: number
  expenses: number
  netProfit: number
}

export interface DailyFinancialReportDto {
  outstandingPatientBalances: number
  items: DailyFinancialSummaryDto[]
}

export interface MonthlyFinancialSummaryDto {
  month: string
  revenue: number
  expenses: number
  netProfit: number
  outstandingPatientBalances: number
  patients: number
  appointments: number
}

export interface MonthlyPerformanceComparisonDto {
  month: string
  revenue: number
  expenses: number
  netProfit: number
  outstandingPatientBalances: number
  patients: number
  appointments: number
  previousMonthRevenue?: number | null
  previousMonthExpenses?: number | null
  previousMonthProfit?: number | null
  previousMonthPatients?: number | null
  previousMonthAppointments?: number | null
  revenueChangePercent?: number | null
  expenseChangePercent?: number | null
  profitChangePercent?: number | null
  patientChangePercent?: number | null
  appointmentChangePercent?: number | null
}

export interface PatientDirectoryDto {
  patientId: number
  patientNumber: string
  fullName: string
  phone?: string | null
  email?: string | null
  dateOfBirth?: string | null
  gender: string
  isActive: boolean
  totalTreatments: number
  totalPaid: number
  totalRemaining: number
}

export interface AttachmentDto {
  attachmentId: number
  clinicId: number
  patientId?: number | null
  patientTreatmentId?: number | null
  fileName: string
  fileUrl: string
  fileType?: string | null
  fileSize?: number | null
  uploadedBy?: number | null
  createdAt: string
}

export const AppointmentStatus = {
  Scheduled: 'SCHEDULED',
  Confirmed: 'CONFIRMED',
  Completed: 'COMPLETED',
  Cancelled: 'CANCELLED',
  NoShow: 'NO_SHOW',
} as const

export const TreatmentStatus = {
  Unpaid: 'UNPAID',
  PartiallyPaid: 'PARTIALLY_PAID',
  Paid: 'PAID',
  Voided: 'VOIDED',
} as const

export const PaymentMethodEnum = {
  Cash: 'CASH',
  Card: 'CARD',
  BankTransfer: 'BANK_TRANSFER',
  Cheque: 'CHEQUE',
  Other: 'OTHER',
} as const

export const ExpenseType = {
  General: 'GENERAL',
  SupplierPurchase: 'SUPPLIER_PURCHASE',
  Rent: 'RENT',
  Utilities: 'UTILITIES',
  Equipment: 'EQUIPMENT',
  Maintenance: 'MAINTENANCE',
  Laboratory: 'LABORATORY',
  Materials: 'MATERIALS',
  Other: 'OTHER',
} as const

export const Gender = {
  Male: 'MALE',
  Female: 'FEMALE',
  Other: 'OTHER',
  Unknown: 'UNKNOWN',
} as const
