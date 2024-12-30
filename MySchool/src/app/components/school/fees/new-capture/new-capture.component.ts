import { Component } from '@angular/core';
interface City {
  name: string;
  code: string;
}
@Component({
  selector: 'app-new-capture',
  templateUrl: './new-capture.component.html',
  styleUrls: ['./new-capture.component.scss',
              './../../../../shared/styles/style-select.scss']
})
export class NewCaptureComponent {
  showDiv2 = false;  // Initially Div 2 is hidden
  changeView: boolean = true;  // Toggle the button icon direction

  toggleDiv() {
    this.showDiv2 = !this.showDiv2;  // Toggle the visibility of Div 2
    this.changeView = !this.changeView;  // Toggle the button icon direction
  }
  cities: City[] | undefined;

  selectedCity: City | undefined;

  ngOnInit() {
      this.cities = [
          { name: 'حازم عبدالله اليعري', code: 'NY' },
          { name: 'علي عبدالله اليعري', code: 'RM' },
          { name: 'محمد عبدالله اليعري', code: 'LDN' },
          { name: 'فاهم عبدالله اليعري', code: 'IST' },
          { name: 'أحمد فتحي مسفر', code: 'PRS' }
      ];
  }
}
