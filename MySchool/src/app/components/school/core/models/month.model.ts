import { MonthEnum } from "../enums/month.enum";
import { TermEnum } from "../enums/term.enum";

export interface IMonth {
  id:      MonthEnum;
  name:    string;
  termId:  TermEnum;
}