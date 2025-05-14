import { Component } from '@angular/core';
import { LoaderService } from '../../../core/services/loader.service';

@Component({
  selector: 'app-progress-spinner',
  templateUrl: './progress-spinner.component.html',
  styleUrls: ['./progress-spinner.component.scss']
})
export class ProgressSpinnerComponent {
  visible = false;

  constructor(private loaderService: LoaderService) {
    this.loaderService.loading$.subscribe(status => {
      this.visible = status;
    });
  }
}