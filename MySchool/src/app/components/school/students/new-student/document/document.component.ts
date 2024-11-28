import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-document',
  templateUrl: './document.component.html',
  styleUrl: './document.component.scss'
})
export class DocumentComponent {
  @Input() formGroup!: FormGroup;
}
