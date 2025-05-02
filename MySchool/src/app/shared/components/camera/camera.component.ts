import { Component, EventEmitter, Output } from '@angular/core';
import { WebcamImage } from 'ngx-webcam';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-camera',
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.scss']
})
export class CameraComponent {
  @Output() imageCaptured = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  webcamImage: WebcamImage | null = null;
  trigger: Subject<void> = new Subject<void>();

  videoOptions: MediaTrackConstraints = {
    width: { ideal: 640 },
    height: { ideal: 480 }
  };

  get triggerObservable() {
    return this.trigger.asObservable();
  }

  triggerSnapshot(): void {
    this.trigger.next();
  }

  handleImage(webcamImage: WebcamImage): void {
    this.webcamImage = webcamImage;
    console.log('📷 Captured image:', webcamImage);
  }

  saveImage(): void {
    if (this.webcamImage) {
      this.imageCaptured.emit(this.webcamImage.imageAsDataUrl);
    }
  }

}
