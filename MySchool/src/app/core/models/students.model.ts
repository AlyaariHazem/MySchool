import { Discount } from "./discount.model";
import { Guardian } from "./guardian.model";
import { NameAlisDTO, NameDTO } from "./name.model";

export interface AddStudent {
  existingGuardianId?: number;
  studentID: number;
  guardianEmail?: string;
  guardianPassword?: string;
  guardianAddress?: string;
  guardianGender?: string;
  guardianFullName?: string;
  guardianType?: string;
  guardianPhone?: string;
  guardianDOB?: string;
  studentEmail?: string;
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
  studentDOB: string;
  amount: number;
  classID: number;
  files: File[];
  attachments: string[]; // Array of strings representing URLs
  discounts: Discount[]; // Array of Discount objects
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
  guardians: Guardian;
}

export interface StudentPayload {
  studentID: number;
  studentEmail?: string;
  studentPhone?: string;
  studentAddress?: string;
  studentPassword?: string;
  studentFirstName: string;
  studentMiddleName?: string;
  studentLastName: string;
  studentFirstNameEng?: string;
  studentMiddleNameEng?: string;
  studentLastNameEng?: string;
  divisionID: number;
  placeBirth?: string;
  studentDOB?: string; // or Date if parsed
  studentImageURL?: string;
  studentGender?: string;
  hireDate?: string; // or Date if parsed

  guardianID: number;
  existingGuardianId?: number;
  guardianEmail?: string;
  guardianPhone?: string;
  guardianAddress?: string;
  guardianFullName: string;
  guardianGender?: string;
  guardianDOB?: string; // or Date
  guardianType: string;

  attachments?: string[]; // URLs or filenames

  updateDiscounts?: UpdateDiscount[];
}

export interface UpdateDiscount {
  studentClassFeeID?: number;
  studentID: number;
  feeClassID: number;
  amountDiscount?: number;
  noteDiscount?: string;
  mandatory?: boolean;
}

export interface ApplicationUserDTO {
  id: string;
  userName: string;
  email: string;
  gender: string;
}

