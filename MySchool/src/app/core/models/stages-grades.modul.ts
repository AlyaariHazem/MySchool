export interface Stages{
    id:string,
    stage:string,
    note:string,
    state:boolean
}
export interface AddStage{
    StageName:string,
    Note:string,
}

export interface Grades{
    id:string,
    grade:string,
    stage:string,
    totalStudents:number,
    note:string,
    state:boolean
}

export interface Division{
    id:string,
    division:string,
    grade:string,
    note:string,
    state:boolean
}
// stage.model.ts
export interface Class {
    ClassID: number;
    className: string;
    // Include additional fields present in the class structure if necessary
}

export interface Student {
    StudentID: number;
    StudentName: string;
    // Include additional fields present in the student structure if necessary
}
  
  export interface Stage {
    stageID: number; // Changed to match your API model
    stageName: string;
    note: string;
    active: boolean;
    classes: Class[]; // List of classes for this stage
    studentCount: number; // Total number of students in classes
}