import { Component } from '@angular/core';

@Component({
  selector: 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrls:['./study-year.component.scss',
            './../../../../shared/styles/style-input.scss'
  ] 
})
export class StudyYearComponent {
  constructor(){}
  visible: boolean = false;

  showDialog() {
      this.visible = true;
  }
  
}
