export interface Guardians{
    guardianID:number;
    fullName:string;
    type:string;
    gender:string;
    guardianPhone:number;
    guardianEmail:string;
    guardianAddress:string;
    guardianDOB:Date;
    userID:number;
}

export interface GuardianInfo{
    guardianID: number;
    fullName: string;
    studentCount: number;
    requiredFee: number;
    piad:number;
    remaining: number;
    gender: string;
    phone: number;
    dob: Date;
    address: string;
    accountId: number;
}
export interface GuardianExist{
    accountStudentGuardianID?: number;
    guardianName: string;
    guardianID: number;
}