import { Routes } from '@angular/router';

import { LoginComponent } from './auth/login/login.component';
import { adminGuardGuard } from './core/guards/admin-guard.guard';
import { PageNotFoundComponent } from './shared/components/page-not-found/page-not-found.component';

export const routes: Routes = [
    {
        path:'',
        component:LoginComponent
    },
    {
        path:'school',
        loadChildren: ()=> import('./components/school/school.module').then(m=> m.SchoolModule),
        canMatch:[adminGuardGuard]
    },
    {
        path:'admin',
        loadChildren: ()=> import('./components/admin/admin.module').then(m=> m.AdminModule),
        canMatch:[adminGuardGuard]
    },
    {path:'not-found',component:PageNotFoundComponent},
    { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
    
];
