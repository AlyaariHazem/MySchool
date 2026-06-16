import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';

import { ShardModule } from 'app/shared/shard.module';

@Component({
  selector: 'app-recruitment-about',
  standalone: true,
  imports: [ShardModule, TranslateModule, RouterLink, ButtonModule],
  templateUrl: './recruitment-about.component.html',
  styleUrl: './recruitment-about.component.scss',
})
export class RecruitmentAboutComponent {}
