export interface Voucher {
    voucherID: number;
    accountName: string;
    receipt: number;
    hireDate: Date;
    payBy: string;
    note: string;
    accountAttachments: number;
    studentID: number;
}
export interface VoucherAdd {
    voucherID?: number;
    receipt: number;
    hireDate: Date;
    note: string;
    payBy: string;
    accountStudentGuardianID: number;
    attachments: string[];
    studentID: number;
    files?: File[];
}