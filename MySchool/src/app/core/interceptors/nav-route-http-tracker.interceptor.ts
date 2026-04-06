import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { NGX_LOADING_BAR_IGNORED } from '@ngx-loading-bar/http-client';
import { RouteLoadCoordinatorService } from '../services/route-load-coordinator.service';

/**
 * Counts HTTP during route navigation/load for the full-screen spinner, and hides the header
 * loading bar for those requests (spinner only). In-page requests (idle) keep the header bar.
 */
@Injectable()
export class NavRouteHttpTrackerInterceptor implements HttpInterceptor {
  constructor(private readonly coordinator: RouteLoadCoordinatorService) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const trackSpinner = this.coordinator.shouldTrackHttpForRouteSpinner();
    const requestEpoch = this.coordinator.epoch;
    const suppressBar = this.coordinator.shouldSuppressHttpLoadingBar();

    let r = req;
    if (suppressBar) {
      r = r.clone({
        context: r.context.set(NGX_LOADING_BAR_IGNORED, true),
      });
    }

    if (trackSpinner) {
      this.coordinator.onTrackedHttpStart(requestEpoch);
    }

    return next.handle(r).pipe(
      finalize(() => {
        if (trackSpinner) {
          this.coordinator.onTrackedHttpEnd(requestEpoch);
        }
      })
    );
  }
}
