import { Component } from '@angular/core';

@Component({
  selector: 'app-head',
  templateUrl: './head.component.html',
  styleUrl: './head.component.scss'
})
export class HeadComponent {
  student = {
    id: '12345',
    name: 'محمد علي',
    photoUrl: ''
  };
  activeTab = 'DataStudent';

  takePhoto() {
    // Implement logic to open camera and take a photo
  }

  uploadPhoto(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.student.photoUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  addStudent() {
    // Implement logic to add a new student
  }

  newStudent() {
    // Implement logic to reset the form for a new student
  }

  printGrades() {
    // Implement logic to print student grades
  }

}
