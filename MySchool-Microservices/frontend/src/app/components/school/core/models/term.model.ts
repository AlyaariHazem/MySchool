import { TermEnum } from "../enums/term.enum";

export interface Terms {
    termID: number;
    name: string;
}
export interface TermGrades {
    termlyGradeID?: number;
    studentID?: number;
    studentName?: string;
    studentURL?: string;
    grade?: number;
    termID?: number;
    subjectID?: number;
    note?: string;
    subjectName?: string;
}
export interface TermlyGrade {
    /** Present when row exists in DB; required for PUT updates. */
    termlyGradeID?: number;
    studentID: number;
    grade: number;
    classID: number;
    termID: number;
    /** Not sent on PUT — server keeps the row’s year. Optional on create. */
    yearID?: number;
    subjectID: number;
    note?: string;
}

export interface ITerm {
    id: TermEnum;
    name: string;
}

