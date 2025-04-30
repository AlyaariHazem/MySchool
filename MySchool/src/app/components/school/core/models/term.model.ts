export interface Terms{
    termID: number;
    name: string;
}
export interface TermGrades{
    termlyGradeID?: number;
    studentID?: number;
    studentName?: string;
    studentURL?: string;
    grade?: number;
    termID?:number;
    subjectID?:number;
    note?:string;
    subjectName?:string;
}
export interface TermlyGrade{
    termlyGradeID:number;
    studentID:number;
    grade:number;
    classID:number;
    termID:number;
    subjectID:number;
    note?:string;
}