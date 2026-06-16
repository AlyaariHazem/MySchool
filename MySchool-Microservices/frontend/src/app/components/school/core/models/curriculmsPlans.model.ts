export interface CurriculmsPlan {
    coursePlanID?: number;
    yearID?: number;
    termID?: number;
    subjectID?: number;
    classID?: number;
    teacherID: number;
    divisionID: number;
    /** Weekly lesson count for automatic schedule generation (default 1 on server if omitted). */
    periodsPerWeek?: number;
}
export interface CurriculmsPlans {
    coursePlanName?: string;
    divisionName: string;
    teacherName: string;
    termName: string;
    year: string;
    subjectID: number;
    classID: number;
    divisionID: number;
    teacherID: number;
    termID: number;
    yearID: number;
    periodsPerWeek?: number;
}
export interface CurriculmsPlanSubject {
    subjectID: number;
    subjectName: string;
}