export interface AddStudent{
    studentID:number;
    guardianEmail: string;
    guardianPassword: string;
    guardianAddress: string;
    guardianGender: string;
    guardianFullName: string;
    guardianType: string;
    guardianPhone: string;
    guardianDOB: string; // Use `Date` type if you'll parse it into a Date object in your Angular code
    studentEmail: string;
    studentPassword: string;
    studentAddress: string;
    studentGender: string;
    studentFirstName: string;
    studentMiddleName: string;
    studentLastName: string;
    studentFirstNameEng: string | null; // Optional fields should allow null
    studentMiddleNameEng: string | null;
    studentLastNameEng: string | null;
    divisionID: number;
    placeBirth: string;
    studentPhone: string;
    studentDOB: string; // Use `Date` if parsing
    amount: number;
    classID: number;
    attachments: string[]; // Array of strings representing URLs
    discounts: Discount[]; // Array of Discount objects
  }
  
  export interface Discount{
    classID: number,
    feeID: number,
    amountDiscount:number,
    noteDiscount: string
  }