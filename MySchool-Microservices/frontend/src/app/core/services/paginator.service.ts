// paginator.service.ts
import { Injectable, signal } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';

@Injectable({ providedIn: 'root' })
export class PaginatorService {
  first = signal(0);
  rows  = signal(4);

  pageSlice<T>(data: T[]): T[] {
    const start = this.first();
    return data.slice(start, start + this.rows());
  }

  onPageChange(evt: PaginatorState): void {
    this.first.set(evt.first ?? 0);
    this.rows.set(evt.rows  ?? 4);
  }
}
