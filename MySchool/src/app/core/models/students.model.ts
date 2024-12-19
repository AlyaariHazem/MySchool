export interface AddStudent{
    existingGuardianId?:number;
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
    files: File[];
    attachments: string[]; // Array of strings representing URLs
    discounts: Discount[]; // Array of Discount objects
  }
  
  export interface Discount{
    classID: number,
    feeID: number,
    amountDiscount:number,
    noteDiscount: string
  }

  export interface StudentDetailsDTO {
    studentID: number;
    fullName: NameDTO;
    fullNameAlis?: NameAlisDTO;
    divisionID: number;
    fee: number;
    divisionName?: string;
    className?: string;
    stageName?: string;
    age?: number;
    gender?: string;
    photoUrl?: string;
    placeBirth?: string;
    studentPhone?: string;
    hireDate?: Date;
    studentAddress?: string;
    userID?: string;
    applicationUser: ApplicationUserDTO;
    guardians: GuardianDTO;
  }

  export interface NameDTO {
    firstName: string;
    middleName: string;
    lastName: string;
  }
  
  export interface NameAlisDTO {
    firstNameEng: string;
    middleNameEng: string;
    lastNameEng: string;
  }
  
  export interface ApplicationUserDTO {
    id: string;
    userName: string;
    email: string;
    gender: string;
  }
  
  export interface GuardianDTO {
    guardianFullName: string;
    guardianType: string;
    guardianEmail: string;
    guardianPhone: string;
    guardianDOB?: Date;
    guardianAddress: string;
  }