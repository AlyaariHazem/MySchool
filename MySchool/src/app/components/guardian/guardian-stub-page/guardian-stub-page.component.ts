import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

/**
 * Placeholder content for guardian routes that will be wired to child/student APIs (grades, reports, etc.).
 */
@Component({
  selector: 'app-guardian-stub-page',
  templateUrl: './guardian-stub-page.component.html',
  styleUrl: './guardian-stub-page.component.scss',
})
export class GuardianStubPageComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private sub?: Subscription;

  title = '';
  message = '';

  ngOnInit(): void {
    const apply = (d: Record<string, unknown>) => {
      this.title = (d['stubTitle'] as string) || '';
      this.message =
        (d['stubMessage'] as string) ||
        'هذه الصفحة قيد الإعداد لمتابعة بيانات الأبناء.';
    };
    apply(this.route.snapshot.data as Record<string, unknown>);
    this.sub = this.route.data.subscribe((d) => apply(d as Record<string, unknown>));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
