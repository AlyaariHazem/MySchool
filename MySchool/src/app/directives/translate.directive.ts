import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appTranslate]',
  standalone: true
})
export class TranslateDirective {
  
  @Input() Color:string="blue";
  @Input('appTranslate') direct:string='';

  constructor(private ele: ElementRef) {
    this.ele.nativeElement.style.backgroundColor=this.Color;
  }

   @HostListener("mouseover") over(){
    this.ele.nativeElement.style.backgroundColor=this.Color;
    this.ele.nativeElement.style.direction=this.direct==='rtl'?'rtl':'ltr';
  }
   @HostListener("mouseout") out(){
    this.ele.nativeElement.style.direction='ltr';
    this.ele.nativeElement.style.backgroundColor='pink';
  }

}
