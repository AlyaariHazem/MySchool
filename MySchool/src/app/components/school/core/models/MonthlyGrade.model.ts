export interface MonthlyGrade{
    studentID:number;
    studentName:string;
    subjectID:number;
    subjectName:string;
    grades:Grades[];
}

export interface Grades{
    gradeTypeID:number;
    maxGrade:number;
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