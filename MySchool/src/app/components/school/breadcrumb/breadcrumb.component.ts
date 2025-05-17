import { Component, OnInit} from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map } from 'rxjs/operators';
import { MenuItem } from 'primeng/api';

import { Store } from '@ngrx/store';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit {


  // PrimeNG breadcrumb items
  items: MenuItem[] = [];
  home: MenuItem = { icon: 'pi pi-home', routerLink: ['/school/dashboard'] };

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(private router: Router, private route: ActivatedRoute,private store:Store) {
    // Listen to NavigationEnd events to update breadcrumbs dynamically
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.items = this.buildBreadCrumb(this.route.root);
    });
  }

  ngOnInit(): void {}

  buildBreadCrumb(route: ActivatedRoute, url: string = '', breadcrumbs: MenuItem[] = []): MenuItem[] {
    // Get the child routes of the current route
    const children: ActivatedRoute[] = route.children;

    // Return if there are no more child routes
    if (children.length === 0) {
      return breadcrumbs;
    }

    // Iterate over each child route
    for (const child of children) {
      // Get the route's URL segment
      const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
      if (routeURL) {
        url += `/${routeURL}`;
      }

      // Check if the route has a 'breadcrumb' data property
      const label = child.snapshot.data['breadcrumb'];
      if (label) {
        breadcrumbs.push({ label, routerLink: [url] });
      }

      // Recurse on the child route
      return this.buildBreadCrumb(child, url, breadcrumbs);
    }

    return breadcrumbs;
  }
}
