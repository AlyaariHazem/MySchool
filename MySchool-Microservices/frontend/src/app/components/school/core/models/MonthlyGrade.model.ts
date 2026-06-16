export interface MonthlyGrade{
    studentID:number;
    studentName:string;
    studentURL:string;
    subjectID:number;
    subjectName:string;
    grades:Grades[];
}

export interface Grades{
    gradeTypeID:number;
    maxGrade:number;
    gradeTypeName?: string;
}

export interface updateMonthlyGrades{
    studentID:number;
    subjectID:number;
    monthID:number;
    classID:number;
    gradeTypeID:number;
    termID:number;
    grade:number;
}

/** Guardian monthly report API row (student + subject + month aggregate). */
export interface GuardianMonthlyGradeRow {
  studentID: number;
  studentName: string;
  yearID: number;
  termID: number;
  termName?: string | null;
  monthID: number;
  monthName?: string | null;
  classID: number;
  className?: string | null;
  subjectID: number;
  subjectName: string;
  grades: {
    gradeTypeID: number;
    maxGrade: number | null;
    gradeTypeName?: string | null;
  }[];
}