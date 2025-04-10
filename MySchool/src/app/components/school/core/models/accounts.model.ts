export interface Account {
    accountID: number;
    guardianName: string;
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
    accountStudentGuardianID:number;
}