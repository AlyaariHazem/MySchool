// token.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor() {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');

    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    } else {
      const tid = environment.publicTenantId;
      if (typeof tid === 'number' && tid > 0 && req.url.includes(environment.baseUrl)) {
        req = req.clone({
          setHeaders: { 'X-Tenant-Id': String(tid) },
        });
      }
    }

    return next.handle(req);
  }
}
