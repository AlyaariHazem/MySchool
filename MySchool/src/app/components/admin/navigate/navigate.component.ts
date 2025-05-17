import { Component} from '@angular/core';

import { Store } from '@ngrx/store';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { map } from 'rxjs';

@Component({
  selector: 'app-navigate',
  templateUrl: './navigate.component.html',
  styleUrl: './navigate.component.scss'
})
export class NavigateComponent {
  constructor(private store:Store){}
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
}
