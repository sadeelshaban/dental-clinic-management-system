import type {
  AttachmentDto,
  AppointmentDetailDto,
  AppointmentListItemDto,
  AppointmentSearchQuery,
  ChangePasswordRequest,
  CreateAppointmentRequest,
  CreateExpenseCategoryRequest,
  CreateExpensePaymentRequest,
  CreateExpenseRequest,
  CreatePatientRequest,
  CreatePatientTreatmentRequest,
  CreatePaymentMethodRequest,
  CreatePaymentRequest,
  CreateSupplierRequest,
  CreateTreatmentCategoryRequest,
  CreateTreatmentRequest,
  CreateUserRequest,
  CreateVisitRequest,
  DailyFinancialReportDto,
  DoctorDetailDto,
  DoctorListItemDto,
  DoctorSearchQuery,
  ExpenseCategoryDto,
  ExpenseDetailDto,
  ExpenseListItemDto,
  ExpensePaymentDetailDto,
  ExpensePaymentListItemDto,
  ExpensePaymentSearchQuery,
  ExpenseSearchQuery,
  LoginRequest,
  LoginResponse,
  MonthlyFinancialSummaryDto,
  MonthlyPerformanceComparisonDto,
  PagedResult,
  PatientDetailDto,
  PatientDirectoryDto,
  PatientFinancialStatementDto,
  PatientListItemDto,
  PatientSearchQuery,
  PatientTreatmentDetailDto,
  PatientTreatmentListItemDto,
  PatientTreatmentSearchQuery,
  PaymentDetailDto,
  PaymentListItemDto,
  PaymentMethodDto,
  PaymentSearchQuery,
  ResetPasswordRequest,
  SupplierDto,
  SupplierFinancialStatementDto,
  SupplierSearchQuery,
  TreatmentCategoryDto,
  TreatmentCategorySearchQuery,
  TreatmentDetailDto,
  TreatmentListItemDto,
  TreatmentSearchQuery,
  UpdateAppointmentRequest,
  UpdateDoctorRequest,
  UpdateExpenseCategoryRequest,
  UpdateExpenseRequest,
  UpdatePatientRequest,
  UpdatePatientTreatmentRequest,
  UpdatePaymentMethodRequest,
  UpdateSupplierRequest,
  UpdateTreatmentCategoryRequest,
  UpdateTreatmentRequest,
  UpdateUserRequest,
  UpdateVisitRequest,
  UserDetailDto,
  UserDto,
  UserListItemDto,
  UserSearchQuery,
  VisitDetailDto,
  VisitListItemDto,
  VisitSearchQuery,
  VoidExpensePaymentRequest,
  VoidExpenseRequest,
  VoidPaymentRequest,
} from '@/types/api'
import { apiRequest, del, get, post, postForm, put, toQuery } from '@/api/client'

export const authApi = {
  login: (body: LoginRequest) =>
    apiRequest<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(body),
      skipAuth: true,
    }),
  me: () => get<UserDto>('/api/auth/me'),
  changePassword: (body: ChangePasswordRequest) =>
    post<Record<string, never>>('/api/auth/change-password', body),
}

export const patientsApi = {
  list: (query: PatientSearchQuery = {}) =>
    get<PagedResult<PatientListItemDto>>(`/api/patients${toQuery(query)}`),
  get: (patientId: number) => get<PatientDetailDto>(`/api/patients/${patientId}`),
  create: (body: CreatePatientRequest) => post<PatientDetailDto>('/api/patients', body),
  update: (patientId: number, body: UpdatePatientRequest) =>
    put<PatientDetailDto>(`/api/patients/${patientId}`, body),
  deactivate: (patientId: number) => del<Record<string, never>>(`/api/patients/${patientId}`),
  financial: (patientId: number) =>
    get<PatientFinancialStatementDto>(`/api/patients/${patientId}/financial`),
}

export const usersApi = {
  list: (query: UserSearchQuery = {}) =>
    get<PagedResult<UserListItemDto>>(`/api/users${toQuery(query)}`),
  get: (userId: number) => get<UserDetailDto>(`/api/users/${userId}`),
  create: (body: CreateUserRequest) => post<UserDetailDto>('/api/users', body),
  update: (userId: number, body: UpdateUserRequest) =>
    put<UserDetailDto>(`/api/users/${userId}`, body),
  activate: (userId: number) => post<Record<string, never>>(`/api/users/${userId}/activate`),
  deactivate: (userId: number) => post<Record<string, never>>(`/api/users/${userId}/deactivate`),
  resetPassword: (userId: number, body: ResetPasswordRequest) =>
    post<Record<string, never>>(`/api/users/${userId}/reset-password`, body),
}

export const doctorsApi = {
  list: (query: DoctorSearchQuery = {}) =>
    get<PagedResult<DoctorListItemDto>>(`/api/doctors${toQuery(query)}`),
  get: (doctorId: number) => get<DoctorDetailDto>(`/api/doctors/${doctorId}`),
  update: (doctorId: number, body: UpdateDoctorRequest) =>
    put<DoctorDetailDto>(`/api/doctors/${doctorId}`, body),
}

export const appointmentsApi = {
  list: (query: AppointmentSearchQuery = {}) =>
    get<PagedResult<AppointmentListItemDto>>(`/api/appointments${toQuery(query)}`),
  get: (appointmentId: number) => get<AppointmentDetailDto>(`/api/appointments/${appointmentId}`),
  create: (body: CreateAppointmentRequest) => post<AppointmentDetailDto>('/api/appointments', body),
  update: (appointmentId: number, body: UpdateAppointmentRequest) =>
    put<AppointmentDetailDto>(`/api/appointments/${appointmentId}`, body),
  confirm: (appointmentId: number) =>
    post<AppointmentDetailDto>(`/api/appointments/${appointmentId}/confirm`),
  complete: (appointmentId: number) =>
    post<AppointmentDetailDto>(`/api/appointments/${appointmentId}/complete`),
  cancel: (appointmentId: number) =>
    post<AppointmentDetailDto>(`/api/appointments/${appointmentId}/cancel`),
  noShow: (appointmentId: number) =>
    post<AppointmentDetailDto>(`/api/appointments/${appointmentId}/no-show`),
}

export const visitsApi = {
  list: (query: VisitSearchQuery = {}) =>
    get<PagedResult<VisitListItemDto>>(`/api/visits${toQuery(query)}`),
  get: (visitId: number) => get<VisitDetailDto>(`/api/visits/${visitId}`),
  create: (body: CreateVisitRequest) => post<VisitDetailDto>('/api/visits', body),
  update: (visitId: number, body: UpdateVisitRequest) =>
    put<VisitDetailDto>(`/api/visits/${visitId}`, body),
}

export const treatmentCategoriesApi = {
  list: (query: TreatmentCategorySearchQuery = {}) =>
    get<PagedResult<TreatmentCategoryDto>>(`/api/treatmentcategories${toQuery(query)}`),
  get: (categoryId: number) => get<TreatmentCategoryDto>(`/api/treatmentcategories/${categoryId}`),
  create: (body: CreateTreatmentCategoryRequest) =>
    post<TreatmentCategoryDto>('/api/treatmentcategories', body),
  update: (categoryId: number, body: UpdateTreatmentCategoryRequest) =>
    put<TreatmentCategoryDto>(`/api/treatmentcategories/${categoryId}`, body),
}

export const treatmentsApi = {
  list: (query: TreatmentSearchQuery = {}) =>
    get<PagedResult<TreatmentListItemDto>>(`/api/treatments${toQuery(query)}`),
  get: (treatmentId: number) => get<TreatmentDetailDto>(`/api/treatments/${treatmentId}`),
  create: (body: CreateTreatmentRequest) => post<TreatmentDetailDto>('/api/treatments', body),
  update: (treatmentId: number, body: UpdateTreatmentRequest) =>
    put<TreatmentDetailDto>(`/api/treatments/${treatmentId}`, body),
}

export const patientTreatmentsApi = {
  list: (query: PatientTreatmentSearchQuery = {}) =>
    get<PagedResult<PatientTreatmentListItemDto>>(`/api/patienttreatments${toQuery(query)}`),
  get: (id: number) => get<PatientTreatmentDetailDto>(`/api/patienttreatments/${id}`),
  create: (body: CreatePatientTreatmentRequest) =>
    post<PatientTreatmentDetailDto>('/api/patienttreatments', body),
  update: (id: number, body: UpdatePatientTreatmentRequest) =>
    put<PatientTreatmentDetailDto>(`/api/patienttreatments/${id}`, body),
}

export const paymentsApi = {
  list: (query: PaymentSearchQuery = {}) =>
    get<PagedResult<PaymentListItemDto>>(`/api/payments${toQuery(query)}`),
  get: (paymentId: number) => get<PaymentDetailDto>(`/api/payments/${paymentId}`),
  create: (body: CreatePaymentRequest) => post<PaymentDetailDto>('/api/payments', body),
  void: (paymentId: number, body: VoidPaymentRequest) =>
    post<PaymentDetailDto>(`/api/payments/${paymentId}/void`, body),
}

export const paymentMethodsApi = {
  list: (isActive?: boolean) =>
    get<PagedResult<PaymentMethodDto>>(`/api/paymentmethods${toQuery({ isActive })}`),
  get: (paymentMethodId: number) => get<PaymentMethodDto>(`/api/paymentmethods/${paymentMethodId}`),
  create: (body: CreatePaymentMethodRequest) => post<PaymentMethodDto>('/api/paymentmethods', body),
  update: (paymentMethodId: number, body: UpdatePaymentMethodRequest) =>
    put<PaymentMethodDto>(`/api/paymentmethods/${paymentMethodId}`, body),
}

export const expenseCategoriesApi = {
  list: () => get<PagedResult<ExpenseCategoryDto>>('/api/expensecategories'),
  get: (categoryId: number) => get<ExpenseCategoryDto>(`/api/expensecategories/${categoryId}`),
  create: (body: CreateExpenseCategoryRequest) =>
    post<ExpenseCategoryDto>('/api/expensecategories', body),
  update: (categoryId: number, body: UpdateExpenseCategoryRequest) =>
    put<ExpenseCategoryDto>(`/api/expensecategories/${categoryId}`, body),
}

export const suppliersApi = {
  list: (query: SupplierSearchQuery = {}) =>
    get<PagedResult<SupplierDto>>(`/api/suppliers${toQuery(query)}`),
  get: (supplierId: number) => get<SupplierDto>(`/api/suppliers/${supplierId}`),
  create: (body: CreateSupplierRequest) => post<SupplierDto>('/api/suppliers', body),
  update: (supplierId: number, body: UpdateSupplierRequest) =>
    put<SupplierDto>(`/api/suppliers/${supplierId}`, body),
  statement: (supplierId: number) =>
    get<SupplierFinancialStatementDto>(`/api/suppliers/${supplierId}/statement`),
}

export const expensesApi = {
  list: (query: ExpenseSearchQuery = {}) =>
    get<PagedResult<ExpenseListItemDto>>(`/api/expenses${toQuery(query)}`),
  get: (expenseId: number) => get<ExpenseDetailDto>(`/api/expenses/${expenseId}`),
  create: (body: CreateExpenseRequest) => post<ExpenseDetailDto>('/api/expenses', body),
  update: (expenseId: number, body: UpdateExpenseRequest) =>
    put<ExpenseDetailDto>(`/api/expenses/${expenseId}`, body),
  void: (expenseId: number, body: VoidExpenseRequest) =>
    post<ExpenseDetailDto>(`/api/expenses/${expenseId}/void`, body),
}

export const expensePaymentsApi = {
  list: (query: ExpensePaymentSearchQuery = {}) =>
    get<PagedResult<ExpensePaymentListItemDto>>(`/api/expensepayments${toQuery(query)}`),
  get: (expensePaymentId: number) =>
    get<ExpensePaymentDetailDto>(`/api/expensepayments/${expensePaymentId}`),
  create: (body: CreateExpensePaymentRequest) =>
    post<ExpensePaymentDetailDto>('/api/expensepayments', body),
  void: (expensePaymentId: number, body: VoidExpensePaymentRequest) =>
    post<ExpensePaymentDetailDto>(`/api/expensepayments/${expensePaymentId}/void`, body),
}

export const attachmentsApi = {
  listByPatient: (patientId: number) =>
    get<AttachmentDto[]>(`/api/attachments/patient/${patientId}`),
  listByTreatment: (patientTreatmentId: number) =>
    get<AttachmentDto[]>(`/api/attachments/treatment/${patientTreatmentId}`),
  upload: (file: File, patientId?: number, patientTreatmentId?: number) => {
    const form = new FormData()
    form.append('file', file)
    if (patientId !== undefined) form.append('patientId', String(patientId))
    if (patientTreatmentId !== undefined) form.append('patientTreatmentId', String(patientTreatmentId))
    return postForm<AttachmentDto>('/api/attachments/upload', form)
  },
  remove: (id: number) => del<boolean>(`/api/attachments/${id}`),
  download: async (id: number) => {
    const response = await apiRequest<Response>(`/api/attachments/${id}/download`, {
      method: 'GET',
      raw: true,
    })
    const blob = await response.blob()
    const contentType = response.headers.get('Content-Type') || 'application/octet-stream'
    const disposition = response.headers.get('Content-Disposition') ?? ''
    const utfMatch = /filename\*=UTF-8''([^;]+)/i.exec(disposition)
    const asciiMatch = /filename="?([^";]+)"?/i.exec(disposition)
    const fileName = decodeURIComponent(utfMatch?.[1] ?? asciiMatch?.[1] ?? 'attachment')
    return { blob, contentType, fileName }
  },
}

export const reportsApi = {
  daily: (from?: string, to?: string) =>
    get<DailyFinancialReportDto>(`/api/reports/daily${toQuery({ from, to })}`),
  monthly: (year?: number, month?: number) =>
    get<MonthlyFinancialSummaryDto>(`/api/reports/monthly${toQuery({ year, month })}`),
  comparison: (year?: number, month?: number) =>
    get<MonthlyPerformanceComparisonDto>(`/api/reports/comparison${toQuery({ year, month })}`),
  patientDirectory: (query: { page?: number; pageSize?: number; search?: string; isActive?: boolean } = {}) =>
    get<PagedResult<PatientDirectoryDto>>(`/api/reports/patient-directory${toQuery(query)}`),
  outstandingBalances: (query: { page?: number; pageSize?: number; search?: string } = {}) =>
    get<PagedResult<PatientDirectoryDto>>(`/api/reports/outstanding-balances${toQuery(query)}`),
}
