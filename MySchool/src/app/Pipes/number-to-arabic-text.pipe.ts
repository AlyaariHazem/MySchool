import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'numberToArabicText',
  standalone: true
})
export class NumberToArabicTextPipe implements PipeTransform {

  private units: string[] = ['', 'واحد', 'اثنان', 'ثلاثة', 'أربعة', 'خمسة', 'ستة', 'سبعة', 'ثمانية', 'تسعة'];
  private tens: string[] = ['', 'عشرة', 'عشرون', 'ثلاثون', 'أربعون', 'خمسون', 'ستون', 'سبعون', 'ثمانون', 'تسعون'];
  private hundreds: string[] = ['', 'مائة', 'مائتان', 'ثلاثمائة', 'أربعمائة', 'خمسمائة', 'ستمائة', 'سبعمائة', 'ثمانمائة', 'تسعمائة'];

  transform(value: number): string {
    if (value === 0) return 'صفر ريال يمني لا غير';
  
    const num = Math.floor(value);
    const parts: string[] = [];
  
    const thousands = Math.floor(num / 1000);
    const belowThousand = num % 1000;
  
    // آلاف
    if (thousands > 0) {
      if (thousands === 1) parts.push('ألف');
      else if (thousands === 2) parts.push('ألفان');
      else if (thousands >= 3 && thousands <= 10) {
        parts.push(`${this.units[thousands]} آلاف`);
      } else {
        parts.push(`${this.convertBelowThousand(thousands)} ألف`);
      }
    }
  
    if (belowThousand > 0) {
      if (parts.length > 0) parts.push('و');
      parts.push(this.convertBelowThousand(belowThousand));
    }
  
    return parts.join(' ') + ' ريال يمني لا غير';
  }
  
  private convertBelowThousand(value: number): string {
    const parts: string[] = [];
    const hundreds = Math.floor(value / 100);
    const tensUnits = value % 100;
  
    if (hundreds > 0) {
      parts.push(this.hundreds[hundreds]);
    }
  
    if (tensUnits > 0) {
      if (parts.length > 0) parts.push('و');
  
      if (tensUnits < 10) {
        parts.push(this.units[tensUnits]);
      } else if (tensUnits >= 10 && tensUnits < 20) {
        switch (tensUnits) {
          case 10: parts.push('عشرة'); break;
          case 11: parts.push('أحد عشر'); break;
          case 12: parts.push('اثنا عشر'); break;
          default: parts.push(`${this.units[tensUnits % 10]} عشر`);
        }
      } else {
        const unit = tensUnits % 10;
        const ten = Math.floor(tensUnits / 10);
        if (unit > 0) {
          parts.push(`${this.units[unit]} و ${this.tens[ten]}`);
        } else {
          parts.push(this.tens[ten]);
        }
      }
    }
  
    return parts.join(' ');
  }
  

  
}
