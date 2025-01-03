import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-navigate',
  templateUrl: './navigate.component.html',
  styleUrl: './navigate.component.scss'
})
export class NavigateComponent {
  langDir!:string;
  languageStore=inject(Store);
  dir:string="ltr";
  currentLanguage():void{
    this.languageStore.select("language").subscribe((res)=>{
      this.langDir=res;
      console.log("the language is",this.langDir);
      this.dir=(res=="en")?"ltr":"rtl";
    });
  }
}
