import { Actions, createEffect, ofType } from "@ngrx/effects";
import { languageAction } from "./language.action";
import { tap } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable()
export class LanguageEffect {

    saveLanguage=createEffect(()=>this.actions.pipe(
        ofType(languageAction),
        tap((action)=>{
            localStorage.setItem('lang',action.lang)
        })
    ),{dispatch:false})
    constructor(private actions:Actions){}
    
}