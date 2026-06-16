import { MonthEnum } from "../enums/month.enum";
import { TermEnum } from "../enums/term.enum";
import { IMonth } from "../models/month.model";

export const MONTHS: IMonth[] = [
  { id: MonthEnum.May, name: 'مايو', termId: TermEnum.First },
  { id: MonthEnum.June, name: 'يونيو', termId: TermEnum.First },
  { id: MonthEnum.July, name: 'يوليو', termId: TermEnum.First },
  { id: MonthEnum.August, name: 'أغسطس', termId: TermEnum.First },
  { id: MonthEnum.September, name: 'سبتمبر', termId: TermEnum.Second },
  { id: MonthEnum.October, name: 'أكتوبر', termId: TermEnum.Second },
  { id: MonthEnum.November, name: 'نوفمبر', termId: TermEnum.Second },
  { id: MonthEnum.December, name: 'ديسمبر', termId: TermEnum.Second }
];