import { Component } from '@angular/core';

@Component({
  selector: 'app-add-parents',
  templateUrl: './add-parents.component.html',
  styleUrl: './add-parents.component.scss'
})
export class AddParentsComponent {
  visible: boolean = false;

  showDialog() {
      this.visible = true;
  }
}
