import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FileStoreService {

   //this is for Files and images
   private selectedFiles: File[] = [];
   private studentImage: File | null = null;
 
   setFiles(files: File[]): void {
     this.selectedFiles = files;
   }
 
   getFiles(): File[] {
     return this.selectedFiles;
   }
 
   setStudentImage(file: File): void {
     this.studentImage = file;
   }
 
   getStudentImage(): File | null {
     return this.studentImage;
   }
}
