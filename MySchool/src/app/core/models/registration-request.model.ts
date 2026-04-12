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
