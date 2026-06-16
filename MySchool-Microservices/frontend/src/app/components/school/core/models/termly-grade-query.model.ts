/** POST api/TermlyGrade/page — filters and paging; server uses active academic year (no yearId). */
export interface TermlyGradeQueryPayload {
  termId: number;
  classId: number;
  subjectId: number;
  pageNumber: number;
  pageSize: number;
}
