import { Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appCount]',
  standalone: true
})
export class CountDirective implements OnInit {
  @Input('appCount') count!: number;

  constructor(private elementRef: ElementRef, private render: Renderer2) { }

  ngOnInit(): void {
    const isCountExist = this.count > 0 ? this.count : 'Invalid';
    this.render.setProperty(this.elementRef.nativeElement, 'innerText', isCountExist);
    
  }

}