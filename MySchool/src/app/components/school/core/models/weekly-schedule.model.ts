export interface WeeklySchedule {
  weeklyScheduleID: number;
  dayOfWeek: number;
  dayName: string;
  periodNumber: number;
  periodName: string;
  startTime: string;
  endTime: string;
  classID: number;
  className: string;
  termID: number;
  termName: string;
  subjectID?: number;
  subjectName?: string;
  teacherID?: number;
  teacherName?: string;
  yearID: number;
  divisionID?: number;
  divisionName?: string;
}

export interface AddWeeklySchedule {
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  classID: number;
  termID: number;
  subjectID?: number;
  teacherID?: number;
  yearID: number;
  divisionID?: number;
}

export interface UpdateWeeklySchedule {
  weeklyScheduleID: number;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  classID: number;
  termID: number;
  subjectID?: number;
  teacherID?: number;
  yearID: number;
  divisionID?: number;
}

export interface WeeklyScheduleGrid {
  classID: number;
  className: string;
  termID: number;
  termName: string;
  yearID: number;
  scheduleItems: WeeklySchedule[];
  periods: Period[];
}

export interface Period {
  periodNumber: number;
  periodName: string;
  startTime: string;
  endTime: string;
}
