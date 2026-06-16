import { Injectable, ApplicationRef } from '@angular/core';
import {
  Router,
  NavigationStart,
  NavigationEnd,
  NavigationCancel,
  NavigationError,
} from '@angular/router';
import { LoaderService } from './loader.service';

export type RouteLoadPhase = 'idle' | 'navigating' | 'awaiting-data';

/**
 * Full-screen spinner: from route navigation until in-flight HTTP for that navigation finishes.
 * Phase `idle` = in-page actions only (no spinner); use header loading bar for those.
 */
@Injectable({ providedIn: 'root' })
export class RouteLoadCoordinatorService {
  private _epoch = 0;
  private readonly pendingByEpoch = new Map<number, number>();
  private phase: RouteLoadPhase = 'idle';
  private readonly ignoredNavigationIds = new Set<number>();

  get epoch(): number {
    return this._epoch;
  }

  get routeLoadPhase(): RouteLoadPhase {
    return this.phase;
  }

  /** HTTP that should count toward hiding the route spinner (not in-page filter/update). */
  shouldTrackHttpForRouteSpinner(): boolean {
    return this.phase === 'navigating' || this.phase === 'awaiting-data';
  }

  /** Suppress ngx-loading-bar for these requests (spinner covers the same period). */
  shouldSuppressHttpLoadingBar(): boolean {
    return this.phase !== 'idle';
  }

  onTrackedHttpStart(requestEpoch: number): void {
    const n = (this.pendingByEpoch.get(requestEpoch) ?? 0) + 1;
    this.pendingByEpoch.set(requestEpoch, n);
  }

  onTrackedHttpEnd(requestEpoch: number): void {
    const prev = this.pendingByEpoch.get(requestEpoch) ?? 0;
    const n = Math.max(0, prev - 1);
    if (n === 0) {
      this.pendingByEpoch.delete(requestEpoch);
    } else {
      this.pendingByEpoch.set(requestEpoch, n);
    }

    if (
      requestEpoch === this._epoch &&
      this.phase === 'awaiting-data' &&
      n === 0
    ) {
      this.finishRouteLoad();
    }
  }

  private getPending(e: number): number {
    return this.pendingByEpoch.get(e) ?? 0;
  }

  private finishRouteLoad(): void {
    this.phase = 'idle';
    this.loader.hide();
  }

  private scheduleTryCompleteAwaitingData(): void {
    const tryOnce = () => this.tryCompleteAwaitingData();
    this.appRef.whenStable().then(() => {
      setTimeout(tryOnce, 0);
      setTimeout(tryOnce, 50);
      setTimeout(tryOnce, 200);
      setTimeout(tryOnce, 400);
    });
  }

  private tryCompleteAwaitingData(): void {
    if (this.phase !== 'awaiting-data') {
      return;
    }
    if (this.getPending(this._epoch) === 0) {
      this.finishRouteLoad();
    }
  }

  constructor(
    private readonly router: Router,
    private readonly loader: LoaderService,
    private readonly appRef: ApplicationRef
  ) {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        const state = this.router.getCurrentNavigation()?.extras?.state as
          | { ignoreLoadingBar?: boolean }
          | undefined;
        if (state?.ignoreLoadingBar === true) {
          this.ignoredNavigationIds.add(event.id);
          return;
        }
        this._epoch++;
        this.pendingByEpoch.set(this._epoch, 0);
        this.phase = 'navigating';
        this.loader.show();
        return;
      }

      if (event instanceof NavigationEnd) {
        if (this.ignoredNavigationIds.delete(event.id)) {
          return;
        }
        this.phase = 'awaiting-data';
        this.scheduleTryCompleteAwaitingData();
        return;
      }

      if (event instanceof NavigationCancel || event instanceof NavigationError) {
        if (this.ignoredNavigationIds.delete(event.id)) {
          return;
        }
        this.phase = 'idle';
        this.pendingByEpoch.delete(this._epoch);
        this.loader.hide();
      }
    });
  }
}
