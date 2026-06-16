import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HandleErrorService {
  logErrorResponse(errorResopnse: HttpErrorResponse): Observable<any> {
    if (errorResopnse.status === 0) {
      //this is error come form client
      console.log(`a client side error accurred:${errorResopnse.status} --- ${errorResopnse.error}`);
    } else {
      //this is error come form backend
      console.log(`a backend error accurred:${errorResopnse.status} --- ${errorResopnse.error}`);
    }
    return throwError(() => new Error('something bad happend,please try again later '));
  }
  constructor() { }
}
