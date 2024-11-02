import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule,  ReactiveFormsModule } from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
// import { TranslateModule } from '@ngx-translate/core';

// const components=[];

const modules=[
  FormsModule,
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    ReactiveFormsModule,
    CommonModule
]

@NgModule({
  // declarations:components,
  imports: modules,
  exports:[
    ...modules
  //  ...components,...modules
  ]
})
export class ShardModule { }
