export interface Employee {
    employeeID?: number;
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
  }
  