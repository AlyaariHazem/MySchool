import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { selectLanguage } from '../../../core/store/language/language.selectors';
import { PagePermission, PermissionService } from '../../../core/services/permission.service';

@Component({
  selector: 'app-navigate',
  templateUrl: './navigate.component.html',
  styleUrl: './navigate.component.scss'
})
export class NavigateComponent {
  private readonly permissions = inject(PermissionService);

  constructor(private store: Store){}
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  get canUseAiChat(): boolean {
    return this.permissions.hasPermission(PagePermission.AiChat.View);
  }
}
