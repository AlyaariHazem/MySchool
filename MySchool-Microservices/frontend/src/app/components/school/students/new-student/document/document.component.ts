import { Component, OnInit, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { StudentFormStoreService } from '../../../core/store/student-form-store.service';
import { FileStoreService } from '../../../core/store/file-store.service';

@Component({
  selector: 'app-document',
  templateUrl: './document.component.html',
  styleUrls: ['./document.component.scss'],
})
export class DocumentComponent implements OnInit {
  documentsFormGroup!: FormGroup;
  selectedFiles: File[] = [];
  filePreviews: { name: string; url: string }[] = [];

  private formStore = inject(StudentFormStoreService);
  private fileStore =inject(FileStoreService);

  ngOnInit(): void {
    const fullForm = this.formStore.getForm();
    console.log('the full form is', fullForm.value);
    const docs = fullForm.get('documents');
    if (docs instanceof FormGroup) {
      this.documentsFormGroup = docs;

      const existingNames = this.documentsFormGroup.get('attachments')?.value;
      const existingURLs = this.documentsFormGroup.get('attachmentsURLs')?.value;
      if (Array.isArray(existingNames)) {
        // Optional: populate restored names for UI consistency
        this.filePreviews = existingNames.map((name, index) => ({ name, url: existingURLs[index] }));
      }
    } else {
      throw new Error('documents is not a FormGroup');
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const selectedFile = input.files[0];
      const exists = this.selectedFiles.some(file => file.name === selectedFile.name);

      if (!exists) {
        this.selectedFiles.push(selectedFile);

        const fileURL = URL.createObjectURL(selectedFile);
        this.filePreviews.push({ name: selectedFile.name, url: fileURL });
        this.fileStore.setFiles(this.selectedFiles);
        
        // Save file names to form state
        const fileNames = this.filePreviews.map(f => f.name);
        this.documentsFormGroup.get('attachments')?.setValue(fileNames);
        this.documentsFormGroup.get('attachmentsURLs')?.setValue(this.filePreviews.map(f => f.url));
      } else {
        alert('This file has already been selected.');
      }
    }
  }
}
