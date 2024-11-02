export interface Class{
    id:string,
    level:string,
    name:string,
    state:boolean,
    totalStudents:number,
    type:string,
    semester:string  
}

export interface Semester{
    id:number,
    name:string,
    className:string,
    totalStudents:number,
    note:string,
    state:boolean
}