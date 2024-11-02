export interface Stages{
    id:string,
    stage:string,
    note:string,
    state:boolean
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