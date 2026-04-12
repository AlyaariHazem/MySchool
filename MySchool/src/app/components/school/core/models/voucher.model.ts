export interface Voucher {
    voucherID: number;
    accountName: string;
    studentName?: string;
    receipt: number;
    hireDate: Date;
    payBy: string;
    note: string;
    accountAttachments: number;
    /** Relative paths from API, e.g. /uploads/Vouchers/Vouchers_1_file.pdf */
    attachmentUrls?: string[];
    accountStudentGuardianID: number;
    studentID: number;
}
export interface VoucherAddUpdate {
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