import { ChangeDetectionStrategy, Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { BlankComponent } from "../../components/blank/blank.component";
import { ButtonModule } from 'primeng/button';


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

  ngOnInit(): void {
    this.breadCrumbService.reset();
  }
  
}
