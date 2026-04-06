// token.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { LoaderService } from '../services/loader.service';
import { Router } from '@angular/router';

/** Paths that should not toggle the global progress spinner (background polling, etc.). */
function shouldSkipGlobalLoader(url: string): boolean {
  const withoutQuery = url.split('?')[0];
  let path: string;
  try {
    path = new URL(withoutQuery, 'http://localhost').pathname;
  } catch {
    path = withoutQuery;
  }
  return (
    /\/api\/Notifications\/unread-count\/?$/.test(path) ||
    /\/api\/Notifications\/inbox\/?$/.test(path)
  );
}

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(private loaderService: LoaderService, private router: Router) { }
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');

    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    } else {
      this.router.navigateByUrl('#');
    }

    const skipLoader = shouldSkipGlobalLoader(req.url);
    if (!skipLoader) {
      this.loaderService.show();
    }

    return next.handle(req).pipe(
      finalize(() => {
        if (!skipLoader) {
          this.loaderService.hide();
        }
      })
    );
  }
}
