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
    feeID!: number; 
    feeName: string = '';
    feeNameAlis?: string;
    classID!: number; 
    className: string = '';
    classYear!: Date;
    amount?: number;
    mandatory: boolean = false;
  }
export class FeeClass{
    classID!:number;
    feeID!:number;
    amount?:number;
    mandatory:boolean=true;
}