export interface PublicSchoolOption {
  tenantId: number;
  schoolName: string;
}

export interface RegistrationAttachment {
  fileName: string;
  url: string;
}

export interface PendingRegistrationRequest {
  id: number;
  userName: string;
  phoneNumber: string;
  fullName?: string | null;
  gender: string;
  dateOfBirth?: string | null;
  requestedRole: string;
  tenantId: number;
  schoolName: string;
  createdAt: string;
  attachments: RegistrationAttachment[];
}

export interface RequestRegistrationPayload {
  tenantId: number;
  userName: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  requestedRole: 'STUDENT' | 'GUARDIAN';
  fullName?: string | null;
  gender: string;
  dateOfBirth?: string | null;
  files: File[];
}

/** Body for POST /auth/ApproveRequest/{id}. Required fields apply only when approving a STUDENT. */
export interface ApproveRegistrationPayload {
  divisionID?: number;
  existingGuardianId?: number | null;
  amount?: number;
  guardianEmail?: string | null;
  guardianPassword?: string | null;
  guardianFullName?: string | null;
  guardianPhone?: string | null;
  guardianGender?: string | null;
  guardianAddress?: string | null;
  guardianType?: string | null;
  guardianDOB?: string | null;
  studentFirstName?: string | null;
  studentMiddleName?: string | null;
  studentLastName?: string | null;
  discounts?: Array<{
    feeClassID: number;
    amountDiscount?: number | null;
    noteDiscount?: string | null;
    mandatory?: boolean;
  }>;
}
