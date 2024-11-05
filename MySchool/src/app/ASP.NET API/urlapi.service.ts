import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class URLAPIService {
  public baseUrl = 'http://localhost:5180/api';

  constructor(public http: HttpClient, public router: Router) { }
}
