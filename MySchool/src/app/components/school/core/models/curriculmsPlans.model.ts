export interface CurriculmsPlan {
    coursePlanID?: number;
    yearID?: number;
    termID?: number;
    subjectID?: number;
    classID?: number;
    teacherID: number;
    divisionID: number;
}
export interface CurriculmsPlans {
    coursePlanName?: string;
    divisionName: string;
    teacherName: string;
    termName: string;
    year: string;
}
export interface CurriculmsPlanSubject {
    subjectID: number;
    subjectName: string;
}