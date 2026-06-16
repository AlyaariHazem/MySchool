export interface Employee {
    employeeID?: number;
    /** Stable row id: `${jopName}-${employeeID}` (Teacher/Manager can share numeric ids). */
    employeeRowKey?: string;
    firstName: string;
    middleName: string;
    lastName: string;
    jopName: string;
    address: string | null;
    mobile: string;
    gender: string;
    hireDate: Date;
    dob:Date;
    email: string | null;
    imageURL: string | null;
    managerID: number | null;
    schoolID?: number | null;
    divisionID?: number | null;
    guardianID?: number | null;
    password?: string | null;
    teacherID?: number;
    userID?: string;
    userName?: string;
  }
  