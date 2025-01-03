// student-month-result.component.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-student-month-result',
  template: ``,
})
export class StudentMonthResultComponent {
  private title = 'Student Month Result';
  private content = `
    <div>
      <h3>Student Month Result</h3>
      <p><strong>Default text</strong> for Student Month Result report...</p>
    </div>
  `;

  getTitle(): string {
    return this.title;
  }

  getContent(): string {
    return this.content;
  }
}
