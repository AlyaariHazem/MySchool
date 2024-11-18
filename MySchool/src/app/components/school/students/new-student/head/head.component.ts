import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-head',
  templateUrl: './head.component.html',
  styleUrl: './head.component.scss'
})
export class HeadComponent {
  activeTab = 'DataStudent';

  takePhoto() {
    // Implement logic to open camera and take a photo
  }

  // uploadPhoto(event: any) {
  //   const file = event.target.files[0];
  //   if (file) {
  //     const reader = new FileReader();
  //     reader.onload = (e: any) => {
  //       this.student.photoUrl = e.target.result;
  //     };
  //     reader.readAsDataURL(file);
  //   }
  // }

  @Input() student?: { name: string, id: string }; // Input for student data
  @Output() studentAdded = new EventEmitter<void>(); // Output event for adding student

  // Method to handle the photo upload (file selection)
  uploadPhoto(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files?.[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = (e: ProgressEvent<FileReader>) => {
        const photoUrl = e.target?.result as string;
        // Logic to update student photo
        console.log('Photo uploaded:', photoUrl);
      };

      reader.readAsDataURL(file);
    }
  }

  // Method to handle adding a student
  addStudent(): void {
    console.log('Student added:', this.student);
    // Add logic to save or send student data to the backend
    this.studentAdded.emit(); // Emit the event to notify the parent component
  }

  // Method to handle creating a new student (reset form, etc.)
  newStudent(): void {
    console.log('New student form initialized.');
    // Logic for resetting form or setting up a new student
  }

  // Method to handle printing the grades
  printGrades(): void {
    console.log('Printing student grades...');
    // Logic to handle printing or generating the student's grade certificate
  }

}
