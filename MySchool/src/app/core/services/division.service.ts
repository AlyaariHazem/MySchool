import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { map, Observable } from 'rxjs';
import { divisions } from '../models/division.model';

@Injectable({
  providedIn: 'root'
})
export class DivisionService {
  private API = inject(BackendAspService);

  constructor() { }
  
  getAll(): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/Divisions`).pipe(
      map(response=>response)
    );
  }

}
