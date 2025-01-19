import { Component, inject, OnInit } from '@angular/core';

import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-navigate',
  templateUrl: './navigate.component.html',
  styleUrl: './navigate.component.scss'
})
export class NavigateComponent implements OnInit {
  languageService=inject(LanguageService);
   ngOnInit() {
    this.languageService.currentLanguage();
  }
}
