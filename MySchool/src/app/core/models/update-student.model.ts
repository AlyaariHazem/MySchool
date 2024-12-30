
export interface UpdateStudent {
    studentID: number;
    guardianEmail: string;
    guardianPassword: string;
    guardianAddress: string;
    guardianGender: string;
    guardianFullName: string;
    guardianType: string;
    guardianPhone: string;
    guardianDOB: string; // ISO string format for dates
    studentEmail: string;
    studentPassword: string;
    studentAddress: string;
    studentGender: string;
    studentFirstName: string;
    studentMiddleName: string;
    studentLastName: string;
    studentFirstNameEng: string | null;
    studentMiddleNameEng: string | null;
    studentLastNameEng: string | null;
    studentImageURL: string;
    divisionID: number;
    placeBirth: string;
    studentPhone: string;
    studentDOB: string; // ISO string format for dates
    hireDate: string; // ISO string format for dates
    amount: number;
    attachments: string[]; // URLs or identifiers
    discounts: Discount[];
  }
  
  export interface Discount {
    feeClassID: number;
    amountDiscount: number;
    noteDiscount: string;
  }
  