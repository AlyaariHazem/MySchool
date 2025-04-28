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
}

export interface updateMonthlyGrades{
    studentID:number;
    subjectID:number;
    monthID:number;
    yearID:number;
    classID:number;
    gradeTypeID:number;
    termID:number;
    grade:number;
    
}