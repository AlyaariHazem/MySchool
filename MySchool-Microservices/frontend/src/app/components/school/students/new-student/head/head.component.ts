import { Component, EventEmitter, Input, Output, ViewChild, ElementRef, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-head',
  templateUrl: './head.component.html',
  styleUrls: ['./head.component.scss']
})
export class HeadComponent implements AfterViewInit {
  @ViewChild('video') videoRef!: ElementRef<HTMLVideoElement>;

  @Input() student?: { name: string, id: string };
  @Output() studentAdded = new EventEmitter<void>();

  photoUrl: string | null = null; // الصورة المعروضة
  showCaptureBtn = false; // لعرض زر "📸 حفظ الصورة"

  ngAfterViewInit() {
    // يتم تركه فارغ حالياً حتى يتم طلب الكاميرا عند الضغط
  }

  takePhoto() {
    const video = this.videoRef.nativeElement;

    navigator.mediaDevices.getUserMedia({ video: true }).then((stream) => {
      video.srcObject = stream;
      video.style.display = 'block';
      this.showCaptureBtn = true;
    }).catch(err => {
      console.error("حدث خطأ عند فتح الكاميرا:", err);
    });
  }

  capturePhoto() {
    const video = this.videoRef.nativeElement;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    canvas.getContext('2d')?.drawImage(video, 0, 0, canvas.width, canvas.height);
    this.photoUrl = canvas.toDataURL('image/png'); // حفظ الصورة
    if (video.srcObject instanceof MediaStream) {
      (video.srcObject as MediaStream).getTracks().forEach(track => track.stop());
    } else {
      console.error('video.srcObject is not a MediaStream');
    }
    video.style.display = 'none';
    this.showCaptureBtn = false;
  }

  uploadPhoto(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files?.[0]) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        this.photoUrl = e.target?.result as string;
        console.log('تم رفع الصورة:', this.photoUrl);
      };
      reader.readAsDataURL(file);
    }
  }

  addStudent(): void {
    console.log('Student added:', this.student);
    this.studentAdded.emit();
  }

  newStudent(): void {
    console.log('New student form initialized.');
  }

  printGrades(): void {
    console.log('Printing student grades...');
  }
}
