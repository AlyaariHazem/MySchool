import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FormValidationService {
  private formValiditySource = new BehaviorSubject<boolean>(false); // Initial form validity is false
  formValidity$ = this.formValiditySource.asObservable(); // Observable for subscribers

  constructor() {}

  /**
   * Updates the validity status of the form.
   * @param isValid - Boolean representing if the form is valid
   */
  updateValidity(isValid: boolean): void {
    this.formValiditySource.next(isValid); // Emit the updated validity status
  }
}
