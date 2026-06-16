import { Class } from "./classes.model";

export interface AddStage {
    StageName: string,
    Note: string,
    YearID:number
}


//these are model for Asp.net 

// stage.model.ts


export interface Student {
    StudentID: number;
    StudentName: string;
}

export interface Stage {
    stageID: number; // Changed to match your API model
    stageName: string;
    note: string;
    active: boolean;
    classes: Class[]; // List of classes for this stage
    studentCount: number; // Total number of students in classes
}

export interface updateStage {
    id: number;
    stageName: string;
    note: string;
    active: boolean;
}