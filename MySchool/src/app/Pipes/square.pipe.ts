import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'square',
  standalone: true
})
export class SquarePipe implements PipeTransform {

  transform(value:number,pow:number=2){
    return Math.pow(value,pow);
  }

}
//to call it <p> 4 |square:3 </p>