export class Fees{
    feeID!:number;
    feeName:string='';
    feeNameAlis:string='';
    note:string='';
    hireDate:string=new Date().toDateString().split('T')[0];
    state:boolean=true;
}

export class Fee{
    feeID!: number;
    feeName: string='';
    feeNameAlis: string='';
    note: string='';     
}

export class FeeClasses {
    feeClassID!:number;
    feeID!: number; 
    feeName: string = '';
    feeNameAlis?: string;
    classID!: number; 
    className: string = '';
    classYear!: Date;
    amount?: number;
    noteDiscount?: string;
    amountDiscount?: number;
    mandatory: boolean = false;
  }
export class FeeClass{
    feeClassID!:number;
    classID!:number;
    feeID!:number;
    amount?:number;
    mandatory:boolean=true;
}

export interface Discount {
    amount: number;
    note: string;
    feeClassID: number;
}
  