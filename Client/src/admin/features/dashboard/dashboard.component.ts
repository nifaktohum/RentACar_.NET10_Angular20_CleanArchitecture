import { ChangeDetectionStrategy, Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { BlankComponent } from "../../components/blank/blank.component";
import { ButtonModule } from 'primeng/button';
import { ProtectionPackageService } from '../../core/services/protection-packages/protection-package.service';
import { ProtectionBenefitService } from '../../core/services/protection-packages/protection-benefit.service';


@Component({
  selector: 'app-dashboard',
  imports: [
    BlankComponent,
    //PrimeNG
    ButtonModule,
],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class DashboardComponent implements OnInit {
  readonly breadCrumbService = inject(BreadcrumbService);
  // Sidebar'ın görünürlük durumunu tutan değişken
  sidebarVisible: boolean = window.innerWidth >= 1024;

  private packageService = inject(ProtectionBenefitService)

  ngOnInit(): void {
    this.breadCrumbService.reset();    
  }
  
}
