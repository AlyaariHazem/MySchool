/** Backend enums as int (JSON). */

import { EmploymentStatus } from '../employees-hr/employees-hr.models';

export enum JobPostingStatus {
  Draft = 1,
  Open = 2,
  Closed = 3,
  Archived = 4,
}

export enum JobApplicationStatus {
  Submitted = 1,
  UnderReview = 2,
  InterviewScheduled = 3,
  Evaluated = 4,
  Accepted = 5,
  Rejected = 6,
  ConvertedToEmployee = 7,
  Withdrawn = 8,
}

export enum InterviewStatus {
  Scheduled = 1,
  Completed = 2,
  Cancelled = 3,
  NoShow = 4,
}

export enum EvaluationRecommendation {
  StrongReject = 1,
  Reject = 2,
  Consider = 3,
  Recommend = 4,
  StrongRecommend = 5,
}

export enum HiringDecisionStatus {
  Pending = 1,
  Accepted = 2,
  Rejected = 3,
  Cancelled = 4,
}

/** Filters (query params). */
export interface JobPostingFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  employeeJobTypeID?: number | null;
  status?: JobPostingStatus | null;
  isActive?: boolean | null;
}

export interface JobApplicationFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  jobPostingID?: number | null;
  status?: JobApplicationStatus | null;
  email?: string | null;
  nationalID?: string | null;
  isActive?: boolean | null;
}

export interface JobPostingCreateDto {
  schoolID: number;
  academicYearID?: number | null;
  employeeJobTypeID: number;
  title: string;
  department?: string | null;
  description?: string | null;
  requirements?: string | null;
  responsibilities?: string | null;
  employmentType?: string | null;
  numberOfOpenings: number;
  postingDate: string;
  closingDate?: string | null;
  status: JobPostingStatus;
  notes?: string | null;
  isActive: boolean;
}

export type JobPostingUpdateDto = JobPostingCreateDto;

export interface JobPostingReadDto {
  jobPostingID: number;
  schoolID: number;
  academicYearID?: number | null;
  employeeJobTypeID: number;
  jobTypeCode?: string | null;
  jobTypeName?: string | null;
  title: string;
  department?: string | null;
  description?: string | null;
  requirements?: string | null;
  responsibilities?: string | null;
  employmentType?: string | null;
  numberOfOpenings: number;
  postingDate: string;
  closingDate?: string | null;
  status: JobPostingStatus;
  notes?: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface JobPostingListDto {
  jobPostingID: number;
  schoolID: number;
  academicYearID?: number | null;
  title: string;
  department?: string | null;
  status: JobPostingStatus;
  postingDate: string;
  closingDate?: string | null;
  numberOfOpenings: number;
  jobTypeName?: string | null;
}

export interface JobApplicationCreateDto {
  jobPostingID: number;
  academicYearID?: number | null;
  applicantFirstName: string;
  applicantLastName: string;
  applicantArabicName?: string | null;
  applicantEnglishName?: string | null;
  nationalID?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  highestQualification?: string | null;
  specialization?: string | null;
  yearsOfExperience?: number | null;
  currentEmployer?: string | null;
  resumeFileUrl?: string | null;
  coverLetter?: string | null;
  source?: string | null;
  notes?: string | null;
}

export interface JobApplicationUpdateDto {
  applicantFirstName?: string | null;
  applicantLastName?: string | null;
  applicantArabicName?: string | null;
  applicantEnglishName?: string | null;
  nationalID?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  highestQualification?: string | null;
  specialization?: string | null;
  yearsOfExperience?: number | null;
  currentEmployer?: string | null;
  resumeFileUrl?: string | null;
  coverLetter?: string | null;
  source?: string | null;
  notes?: string | null;
  isActive?: boolean | null;
}

export interface JobApplicationReadDto {
  jobApplicationID: number;
  jobPostingID: number;
  jobPostingTitle?: string | null;
  schoolID: number;
  academicYearID: number;
  applicantFirstName: string;
  applicantLastName: string;
  applicantArabicName?: string | null;
  applicantEnglishName?: string | null;
  nationalID?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  highestQualification?: string | null;
  specialization?: string | null;
  yearsOfExperience?: number | null;
  currentEmployer?: string | null;
  resumeFileUrl?: string | null;
  coverLetter?: string | null;
  source?: string | null;
  status: JobApplicationStatus;
  appliedAt: string;
  notes?: string | null;
  convertedEmployeeProfileID?: number | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface JobApplicationListDto {
  jobApplicationID: number;
  jobPostingID: number;
  jobPostingTitle?: string | null;
  schoolID: number;
  applicantFirstName: string;
  applicantLastName: string;
  email?: string | null;
  status: JobApplicationStatus;
  appliedAt: string;
  convertedEmployeeProfileID?: number | null;
}

export interface JobApplicationStatusMoveDto {
  newStatus: JobApplicationStatus;
}

export interface InterviewCreateDto {
  interviewDate: string;
  interviewType?: string | null;
  locationOrMeetingLink?: string | null;
  interviewerName?: string | null;
  interviewerUserID?: string | null;
  interviewerEmployeeProfileID?: number | null;
  notes?: string | null;
}

export interface InterviewUpdateDto {
  interviewDate?: string | null;
  interviewType?: string | null;
  locationOrMeetingLink?: string | null;
  interviewerName?: string | null;
  interviewerUserID?: string | null;
  interviewerEmployeeProfileID?: number | null;
  status?: InterviewStatus | null;
  summary?: string | null;
  notes?: string | null;
  score?: number | null;
}

export interface InterviewReadDto {
  interviewID: number;
  jobApplicationID: number;
  schoolID: number;
  academicYearID: number;
  interviewDate: string;
  interviewType?: string | null;
  locationOrMeetingLink?: string | null;
  interviewerName?: string | null;
  interviewerUserID?: string | null;
  interviewerEmployeeProfileID?: number | null;
  status: InterviewStatus;
  summary?: string | null;
  notes?: string | null;
  score?: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CandidateEvaluationCreateDto {
  interviewID?: number | null;
  evaluatorUserID?: string | null;
  evaluatorEmployeeProfileID?: number | null;
  technicalScore?: number | null;
  communicationScore?: number | null;
  classManagementScore?: number | null;
  cultureFitScore?: number | null;
  overallScore?: number | null;
  strengths?: string | null;
  weaknesses?: string | null;
  recommendation: EvaluationRecommendation;
  notes?: string | null;
  evaluatedAt?: string | null;
}

export interface CandidateEvaluationUpdateDto {
  interviewID?: number | null;
  evaluatorUserID?: string | null;
  evaluatorEmployeeProfileID?: number | null;
  technicalScore?: number | null;
  communicationScore?: number | null;
  classManagementScore?: number | null;
  cultureFitScore?: number | null;
  overallScore?: number | null;
  strengths?: string | null;
  weaknesses?: string | null;
  recommendation?: EvaluationRecommendation | null;
  notes?: string | null;
  evaluatedAt?: string | null;
}

export interface CandidateEvaluationReadDto {
  candidateEvaluationID: number;
  jobApplicationID: number;
  interviewID?: number | null;
  schoolID: number;
  academicYearID: number;
  evaluatorUserID?: string | null;
  evaluatorEmployeeProfileID?: number | null;
  technicalScore?: number | null;
  communicationScore?: number | null;
  classManagementScore?: number | null;
  cultureFitScore?: number | null;
  overallScore?: number | null;
  strengths?: string | null;
  weaknesses?: string | null;
  recommendation: EvaluationRecommendation;
  notes?: string | null;
  evaluatedAt: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface HiringDecisionCreateDto {
  decisionStatus: HiringDecisionStatus;
  decisionDate?: string | null;
  decidedByUserID?: string | null;
  decidedByEmployeeProfileID?: number | null;
  offerJobTypeID: number;
  proposedHireDate?: string | null;
  proposedSalaryNotes?: string | null;
  reason?: string | null;
  internalNotes?: string | null;
  skipEvaluationCheck?: boolean;
}

export interface HiringDecisionUpdateDto {
  decisionStatus?: HiringDecisionStatus | null;
  decisionDate?: string | null;
  decidedByUserID?: string | null;
  decidedByEmployeeProfileID?: number | null;
  offerJobTypeID?: number | null;
  proposedHireDate?: string | null;
  proposedSalaryNotes?: string | null;
  reason?: string | null;
  internalNotes?: string | null;
}

export interface HiringDecisionReadDto {
  hiringDecisionID: number;
  jobApplicationID: number;
  schoolID: number;
  academicYearID: number;
  decisionStatus: HiringDecisionStatus;
  decisionDate: string;
  decidedByUserID?: string | null;
  decidedByEmployeeProfileID?: number | null;
  offerJobTypeID: number;
  offerJobTypeName?: string | null;
  proposedHireDate?: string | null;
  proposedSalaryNotes?: string | null;
  reason?: string | null;
  internalNotes?: string | null;
  convertedEmployeeProfileID?: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface JobApplicationFullDto {
  application: JobApplicationReadDto;
  posting?: JobPostingReadDto | null;
  interviews: InterviewReadDto[];
  evaluations: CandidateEvaluationReadDto[];
  decision?: HiringDecisionReadDto | null;
}

export interface ConvertApplicantToEmployeeDto {
  employeeCode?: string | null;
  userId?: string | null;
  employeeJobTypeID?: number | null;
  hireDate?: string | null;
  employmentStatus: EmploymentStatus;
  notes?: string | null;
  mapQualificationAndSpecialization?: boolean;
}

export interface ConvertApplicantToEmployeeResultDto {
  employeeProfileID: number;
  employeeCode: string;
  jobApplicationID: number;
}
