export interface managerInfo{
    managerID:number;
    fullName: {
        firstName: string;
        middleName: string;
        lastName: string;
    };
    hireDate?: Date;
    schoolName: string;
    tenantID?: number;
    tenantName?: string;
    userName: string;
    email: string;
    userType: string;
    phoneNumber: number;
}