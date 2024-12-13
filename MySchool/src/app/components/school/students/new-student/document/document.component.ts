import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-document',
  templateUrl: './document.component.html',
  styleUrls: ['./document.component.scss'],
})
export class DocumentComponent {
  @Input() formGroup!: FormGroup;
  @Output() filesChanged = new EventEmitter<string[]>(); // Emit file names to parent

  selectedFiles: File[] = [];
  attachments: string[] = [];

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const selectedFile = input.files[0];

      // Avoid duplicate files
      const fileExists = this.selectedFiles.some(file => file.name === selectedFile.name);
      if (!fileExists) {
        this.selectedFiles.push(selectedFile);
        this.attachments.push(selectedFile.name);

        // Emit updated attachments to parent
        this.filesChanged.emit(this.attachments);
      } else {
        alert('This file has already been selected.');
      }
    }
  }
}
