import { Component, inject, OnInit } from '@angular/core';

import { GuardianService } from '../../../../core/services/guardian.service';
import { GuardianInfo } from '../../../../core/models/guardian.model';

@Component({
  selector: 'app-all-parents',
  templateUrl: './all-parents.component.html',
  styleUrl: './all-parents.component.scss'
})
export class AllParentsComponent implements OnInit {

  guardianSerivce = inject(GuardianService);
  
  guardians:GuardianInfo[]=[];
  now:Date=new Date();
  ngOnInit(): void {
    this.guardianSerivce.getGuardiansInfo().subscribe({
      next: (res) => {
        this.guardians = res;
        console.log('Guardians fetched successfully:', this.guardians);
      },
      error: (err) => console.error('Error occurred while fetching guardians:', err)
    });
  }

  cards: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

}
