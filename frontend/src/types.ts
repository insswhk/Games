export type UserRole = 'Admin' | 'Manager' | 'Cashier'
export type TransactionType = 'AddMoney' | 'WithdrawMoney' | 'BonusPoints'
export type ShiftType = 'Day' | 'Night'
export type AccountType = 'Income' | 'Expense' | 'Asset' | 'Liability'
export type ExpenseType = 'Startup' | 'Rent' | 'Salary' | 'Furniture' | 'Games' | 'Refreshments'

export interface AuthResponse {
  token: string
  userName: string
  fullName: string
  role: UserRole
  locationId?: string
}

export interface Permission {
  formName: string
  canOpen: boolean
  canAdd: boolean
  canDelete: boolean
  canViewReports: boolean
}

export interface DashboardKpi {
  totalCashIn: number
  totalCashOut: number
  netProfitToday: number
  bonusPointsIssued: number
  activeCustomers: number
  activeGames: number
}

export interface LocationDto {
  id: string
  clubName: string
  city: string
  state: string
  country: string
}

export interface CashierDto {
  id: string
  locationId: string
  locationName: string
  cashierCode: string
  fullName: string
  cashRegisterBalance: number
}

export interface CustomerDto {
  id: string
  locationId: string
  locationName: string
  customerCode: string
  fullName: string
  mobile: string
  balance: number
  bonusPoints: number
}

export interface GameModeDto {
  id: string
  code: string
  name: string
  modeType: string
}

export interface AccountDto {
  id: string
  accountNumber: string
  accountName: string
  accountType: AccountType
}

export type DataRow = Record<string, string | number | boolean | null | undefined>
