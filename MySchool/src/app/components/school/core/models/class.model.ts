export interface ClassDTO {
    classID: number;
    className: string;
    classYear: string;
    stageID: number;
    stageName:string;
    state: boolean;
    studentCount: number;
    divisions: DivisionInClassDTO[]; // Array of divisions
  }
  export interface DivisionInClassDTO {
    divisionID: number;
    divisionName: string;
    studentCount: number;
  }
  export interface CLass{
    className:string;
    stageID:number;
    yearID:number;
  }
    
  export interface updateClass{
    classID:number;
    className:string;
    stageID:number;
  }
    