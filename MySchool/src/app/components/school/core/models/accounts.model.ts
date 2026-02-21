export interface Account {
    accountID: number;
    guardianName: string;
    accountName: string;
    state: boolean;
    Balance: number;
    Status: string;
    note: string;
    openBalance: string;
    typeOpenBalance: boolean;
    hireDate: string;
    typeAccountID: number;
}

export interface StudentAccounts{
    accountName: string;
    studentName: string;
    studentID: number;
    guardianID: number;
    accountStudentGuardianID:number;
}

export interface AccountTransaction {
    id: number;
    description: string;
    type: string; // "Debit" or "Credit"
    amount: number;
    date: Date | string;
}

export interface AccountSavings {
    id: number;
    description: string;
    type: boolean; // true for savings, false for withdrawal
    amount: number;
    date: Date | string;
}

export interface SchoolInfo {
    schoolName: string;
    schoolAddress: string;
    schoolPhone: string;
    schoolLogo: string;
    academicYear: string;
}

export interface AccountReport {
    accountID: number;
    accountName: string;
    hireDate: Date | string;
    openBalance?: number;
    typeOpenBalance: boolean;
    totalDebit: number;
    totalCredit: number;
    balance: number;
    transactions: AccountTransaction[];
    savings: AccountSavings[];
    schoolInfo: SchoolInfo;
}