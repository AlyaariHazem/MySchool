import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule } from '@ngx-translate/core';
import { map } from 'rxjs';

import { selectLanguage } from 'app/core/store/language/language.selectors';

@Component({
  selector: 'app-public-recruitment-topbar',
  standalone: true,
  imports: [AsyncPipe, TranslateModule, RouterLink, RouterLinkActive],
  templateUrl: './public-recruitment-topbar.component.html',
  styleUrl: './public-recruitment-topbar.component.scss',
})
export class PublicRecruitmentTopbarComponent {
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
}
