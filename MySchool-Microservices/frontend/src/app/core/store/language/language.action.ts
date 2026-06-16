import { createAction, props } from "@ngrx/store";

export const languageAction=createAction("changeLanguage",props<{lang:string}>());