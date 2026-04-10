/** POST api/TermlyGrade/query — same filters as GET path + query, sent as JSON body. */
export interface TermlyGradeQueryPayload {
  termId: number;
  yearId: number;
  classId: number;
  subjectId: number;
  pageNumber: number;
  pageSize: number;
}
