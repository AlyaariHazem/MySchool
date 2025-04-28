export interface MonthlyResult {
    studentID: number;
    studentName: string;

    schoolName: string;
    schoolURL?: string;

    year?: string;
    month?: string;
    term?: string;
    class?: string;
    division?: string;
    teacher?: string;

    grade?: number;
    gradeSubjects?: GradeSubject[];
}

export interface GradeSubject {
    subjectID: number;
    subjectName: string;
    grade?: number;
}
