// camera.component.ts
import { Component, EventEmitter, Output } from '@angular/core';
import { WebcamImage } from 'ngx-webcam';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-camera',
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.scss']
})
export class CameraComponent {
  /** ➜ We emit the file and the preview (base64) */
  @Output() imageCaptured = new EventEmitter<{ file: File; previewUrl: string }>();
  @Output() cancel         = new EventEmitter<void>();

   webcamImage: WebcamImage | null = null;
   trigger     = new Subject<void>();

  /** Video capture options */
  videoOptions: MediaTrackConstraints = { width: { ideal: 640 }, height: { ideal: 480 } };
  get triggerObservable() { return this.trigger.asObservable(); }
  triggerSnapshot()       { this.trigger.next(); }

  handleImage(img: WebcamImage) {
    this.webcamImage = img;
    console.log('📸 Captured image', img);
  }

  /** Convert base64 to File and emit both */
  saveImage(): void {
    if (!this.webcamImage) return;

    const file = this.dataURLToFile(
      this.webcamImage.imageAsDataUrl,
      `webcam_${Date.now()}.png`
    );

    this.imageCaptured.emit({
      file,
      previewUrl: this.webcamImage.imageAsDataUrl
    });
  }

  /** Convert base64 string to a File */
  private dataURLToFile(dataUrl: string, filename: string): File {
    const [header, b64] = dataUrl.split(',');
    const mime = /:(.*?);/.exec(header)![1];
    const bytes = Uint8Array.from(atob(b64), c => c.charCodeAt(0));
    return new File([bytes], filename, { type: mime });
  }
}
