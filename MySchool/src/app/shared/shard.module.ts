import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule,  ReactiveFormsModule } from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import {MatInputModule} from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogModule, MatDialogTitle } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { RouterLink } from '@angular/router';
import { ProgressSpinnerComponent } from './components/progress-spinner/progress-spinner.component';
import { DialogModule } from 'primeng/dialog';
import { MatSelectModule } from '@angular/material/select';

const components=[PageNotFoundComponent,ProgressSpinnerComponent];

const modules=[
  FormsModule,
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogTitle,
    RouterLink,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    ReactiveFormsModule,
    TranslateModule,
    DialogModule,
    MatDialogModule,
    MatSelectModule,
    ProgressSpinnerModule
]

@NgModule({
  declarations:components,
  imports: modules,
  exports:[
   ...components,...modules
  ]
})
export class ShardModule { }
