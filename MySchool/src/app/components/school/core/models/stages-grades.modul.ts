export interface AddStage {
    StageName: string,
    Note: string,
}


//these are model for Asp.net 

// stage.model.ts
export interface Class {
    ClassID: number;
    className: string;
    StudentCount: number;
}

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