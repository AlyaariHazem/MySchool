export interface divisions{
    divisionID:number;
    divisionName:string;
    state:boolean;
    classID:number;
    classesName:string;
    stageName?:string;
    studentCount:number;
}

export interface Division{
    divisionName:string;
    classID:number;
  }

  export interface DivisionName{
    divisionID:number;
    divisionName:string;
  }