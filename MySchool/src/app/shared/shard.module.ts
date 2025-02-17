import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule,  ReactiveFormsModule } from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { RouterLink } from '@angular/router';

const components=[PageNotFoundComponent];

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
    TranslateModule
]

@NgModule({
  declarations:components,
  imports: modules,
  exports:[
   ...components,...modules
  ]
})
export class ShardModule { }
