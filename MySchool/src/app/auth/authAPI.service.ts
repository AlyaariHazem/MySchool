import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';

import { User } from '../core/models/user.model';
import { BackendAspService } from '../ASP.NET/backend-asp.service';


@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private API = inject(BackendAspService);

  constructor(public router: Router) { }

  /** Reads JWT payload (no signature verification). */
  private readJwtPayload(token: string): Record<string, unknown> | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const pad = base64.length % 4;
      if (pad) base64 += '='.repeat(4 - pad);
      return JSON.parse(atob(base64)) as Record<string, unknown>;
    } catch {
      return null;
    }
  }

  /** True if the access token carries a TenantId claim (required by TenantResolutionMiddleware for /api/*). */
  tokenHasTenantId(token: string): boolean {
    const p = this.readJwtPayload(token);
    if (!p) return false;
    const v = p['TenantId'] ?? p['tenantId'];
    if (v === undefined || v === null) return false;
    const s = String(v).trim();
    return s.length > 0 && s !== '0';
  }

  /**
   * When the JWT has no TenantId (stale session or server could not embed it), call my-tenants + select-tenant.
   * /api/auth/* skips tenant middleware, so this works with a bearer token that lacks TenantId.
   */
  ensureTenantContext(): Observable<boolean> {
    const token =
      typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null;
    if (!token) return of(false);
    if (this.tokenHasTenantId(token)) return of(true);

    return this.API.http
      .get<Array<{ tenantId: number }>>(`${this.API.baseUrl}/auth/my-tenants`)
      .pipe(
        switchMap((tenants) => {
          if (!Array.isArray(tenants) || tenants.length === 0) return of(false);
          const tenantId = tenants[0].tenantId;
          return this.API.http
            .post<{ token: string; tenantId?: number }>(
              `${this.API.baseUrl}/auth/select-tenant`,
              { tenantId },
              { withCredentials: true },
            )
            .pipe(
              tap((st) => {
                if (st?.token) {
                  localStorage.setItem('token', st.token);
                  localStorage.setItem('tenantId', String(st.tenantId ?? tenantId));
                }
              }),
              map(() => true),
            );
        }),
        catchError(() => of(false)),
      );
  }

  private mergeTokenAfterTenantResolution(response: any): Observable<any> {
    const token =
      response?.token ??
      (typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null);
    if (!token || this.tokenHasTenantId(token)) return of(response);
    return this.ensureTenantContext().pipe(
      map((ok) => {
        if (!ok) return response;
        const t = localStorage.getItem('token') ?? response.token;
        return { ...response, token: t };
      }),
    );
  }

  /**
   * If login returns school choices but no embedded tenant in the JWT yet, exchange the token via POST /auth/select-tenant
   * so the next requests include TenantId (avoids 403 TenantRequired from TenantResolutionMiddleware).
   */
  login(user: User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/auth/login`, user).pipe(
      switchMap((response: any) => {
        if (response?.token) {
          localStorage.setItem('token', response.token);
        }
        const tenants = response?.tenants as Array<{ tenantId?: number }> | undefined;
        const needsSelect =
          Array.isArray(tenants) &&
          tenants.length > 0 &&
          (response.tenantId === null || response.tenantId === undefined);
        if (needsSelect) {
          const tenantId = tenants[0].tenantId;
          if (tenantId != null) {
            return this.API.http
              .post(`${this.API.baseUrl}/auth/select-tenant`, { tenantId }, { withCredentials: true })
              .pipe(
                map((st: any) => ({
                  ...response,
                  token: st.token ?? response.token,
                  tenantId: st.tenantId ?? tenantId,
                  expiration: st.expiration ?? response.expiration,
                })),
              );
          }
        }
        return of(response);
      }),
      switchMap((response: any) => this.mergeTokenAfterTenantResolution(response)),
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
        if (response.tenantId != null && response.tenantId !== undefined) {
          localStorage.setItem('tenantId', String(response.tenantId));
        }
        if (response.managerName) {
          localStorage.setItem('managerName', response.managerName);
        }
        if (response.yearId) {
          localStorage.setItem('yearId', response.yearId);
        }
        if (response.managerName === " ") {
          localStorage.setItem('managerName', "Admin");
        }
        if (response.schoolName) {
          localStorage.setItem('schoolName', response.schoolName);
        }
        if (response.userName) {
          localStorage.setItem('userName', response.userName);
        }
        if (response.schoolId) {
          localStorage.setItem('schoolId', response.schoolId);
        }
        if (user.userType) {
          localStorage.setItem('userType', user.userType);
        }
      })
    );
  }

  register(user: User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/auth/register`, user, {
      responseType: 'json' // ✅ Ensures response is treated as JSON
    });
  }


  logout(): Observable<void> {
     return this.API.http.post<void>(`${this.API.baseUrl}/auth/logout`, {},
      { withCredentials: true })               // what this will do?
      .pipe(
        tap(() => {
          ['token', 'tenantId', 'managerName', 'yearId', 'schoolName', 'userName', 'schoolId', 'userType'].forEach(item => localStorage.removeItem(item));
          this.router.navigate(['/']);
        })
      );
  }
}
