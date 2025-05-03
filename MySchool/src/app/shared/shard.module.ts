import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule,  ReactiveFormsModule } from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import {MatInputModule} from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogModule, MatDialogTitle } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { MatSelectModule } from '@angular/material/select';
import { ConfirmDialog } from 'primeng/confirmdialog';

import { ProgressSpinnerComponent } from './components/progress-spinner/progress-spinner.component';
import { CameraComponent } from './components/camera/camera.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { WebcamModule } from 'ngx-webcam';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { DialogService, DynamicDialog } from 'primeng/dynamicdialog';



const components=[
  PageNotFoundComponent,
  ProgressSpinnerComponent,
  CameraComponent,
  ConfirmDialogComponent
];

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
    ProgressSpinnerModule,
    ConfirmDialog,
    DynamicDialog,
    WebcamModule 
]

@NgModule({
  declarations:components,
  imports: modules,
  exports:[
   ...components,...modules
  ],
  providers: [DialogService]
})
export class ShardModule { }
