import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { HandleErrorService } from '../services/handle-error.service';

@Injectable({
  providedIn: 'root'
})
export class IntercepterService implements HttpInterceptor {
   handleError=inject(HandleErrorService);
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>{
    return next.handle(req).pipe(
      catchError(error => this.handleError.logErrorResponse(error))
    );
  }
  constructor() { }
}
