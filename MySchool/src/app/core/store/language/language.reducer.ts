import {  createReducer, on } from "@ngrx/store";
import { languageAction } from "./language.action";

const intialStage =localStorage.getItem('lang')?localStorage.getItem('lang'):'en';
export const languageReduser=createReducer(
    intialStage,
    on(languageAction,(state,action)=>action.lang)
);