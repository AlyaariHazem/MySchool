import { MonthEnum } from "../enums/month.enum";
import { TermEnum } from "../enums/term.enum";

export interface IMonth {
  id:      MonthEnum;
  name:    string;
  termId:  TermEnum;
}

/** Row from GET api/Month (YearTermMonths for active year, or Months fallback). */
export interface MonthDto {
  monthID: number;
  name: string;
  /** Set when rows come from YearTermMonths; omitted/null when using Months fallback. */
  termID?: number | null;
}