import { Component, inject, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AuthAPIService } from '../../../auth/authAPI.service';
import { languageAction } from '../../../core/store/language/language.action';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit {
  
  toggle:boolean=false;
  open(){
    this.toggle=!this.toggle;
  }
  private auth = inject(AuthAPIService);
  
  langDir!:string;
  langName!:string;
  languageimage!:string;
  languageStore=inject(Store);
  dir:string="ltr";
  currentLanguage():void{
    this.languageStore.select("language").subscribe((res)=>{
      this.langDir=res;
      if(this.langDir=='en'){
        this.languageimage='Amarica.jpg';
      }else{
        this.languageimage='SudiAribia.jpg';
      }
      this.dir=(res=="en")?"ltr":"rtl";
      this.langName=(res=="en")?"English":"العربية";
    });
  }
  changeLang(changeLang:string):void{
    this.languageStore.dispatch(languageAction({lang:changeLang}));
  }
  
  ngOnInit(): void {
    this.currentLanguage();
  }
  Logout(){
    // console.log('logout');
    this.auth.logout();
  }
}
