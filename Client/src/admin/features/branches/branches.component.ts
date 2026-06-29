import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal, TemplateRef, ViewChild, ViewEncapsulation } from '@angular/core';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { BranchService } from '../../../core/services/branch.service';
import { BranchModel, GetBranchesResponse } from '../../core/models/branch.model';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext'
import { TableLazyLoadEvent } from 'primeng/table';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from "primeng/tooltip";
import { Ripple } from "primeng/ripple";
import { DatePipe, NgClass } from '@angular/common';
import { RouterLink } from "@angular/router";
import { PhoneFormatPipe } from "../../../shared/pipes/phone-format.pipe";
import { ConfirmationService, MessageService } from 'primeng/api';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { Result } from '../../../core/models/result.model';

@Component({
  selector: 'app-branches',
  imports: [
    RouterLink,
    NgClass,
    TableModule,
    TagModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    DatePipe,
    TooltipModule,
    Ripple,
    PhoneFormatPipe,
  ],
  templateUrl: './branches.component.html',
  styleUrl: './branches.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class BranchesComponent implements OnInit {

  readonly breadCrumbService = inject(BreadcrumbService);
  readonly branchService = inject(BranchService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private messageServiceToast = inject(MessageService);
  // Verileri ve Count değerini Signal olarak tutuyoruz. UI anında tepki verecek.
  readonly branches = signal<BranchModel[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);
  loading: boolean = false;


  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Şubeler',
      url: '/admin/branches',
      icon: 'ri-map-pin-user-line',
      isActive: true
    }
  ])

  //==============================================>

  refreshData() {
    this.loadBranches();
  }

  ngOnInit(): void {
    this.breadCrumbService.reset(this.breadcrumbs());
    this.loadBranches();
  }

  loadBranchess() {
    this.isLoading.set(true);

    this.branchService.getBranches().subscribe({
      next: (response) => {
        console.log(response);
        
        if (response.isSuccessful && response.data) {
          this.branches.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
        this.isLoading.set(false);
      }, 
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Şubeler yüklenirken bir hata oluştu'
        })
        console.error(err?.error?.message || err?.message);        
        this.isLoading.set(false)
      }
    })
  }

  // Tablo her sayfa değiştirdiğinde burası tetiklenir
  onLazyLoad(event: TableLazyLoadEvent) {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;

    // .NET backend pagination mimarisi genellikle Page Number (Sayfa No) ve Page Size (Sayfa Boyutu) bekler.
    const pageIndex = (first / rows) + 1;

    this.loadBranches(pageIndex, rows);
  }

  // Metodunu parametre alacak şekilde güncelledik
  loadBranches(page: number = 1, pageSize: number = 10) {
    this.isLoading.set(true);

    // Servis metoduna parametreleri paslıyoruz: getBranches(page, pageSize) gibi.
    this.branchService.getBranches(page, pageSize).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.branches.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Şubeler yüklenirken bir hata oluştu'
        })
        // this.snackbarService.error('Şubeler yüklenirken bir hata oluştu');
        console.error('Hata: ', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }


  deleteBranch(id: string, name: string) {

    this.customConfirmDialogService.showDeleteConfirm(name,
      () => {
        this.branchService.deleteBranchById(id).subscribe({
          next: () => {
            // Ekranda bildirim gösterelim (PrimeNG Toast kullanıyorsan)
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Silme başarılı',
              detail: `${name}`,
              life: 3000
            });

            // 🎯 KRİTİK: Silme işleminden sonra tablo listesini veritabanından güncelliyoruz!
            this.loadBranches(); // Tabloyu yenile
          },
          error: (err) => {
            console.error('Silme işlemi esnasında hata oluştu:', err);
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: 'Şube silinirken bir hata meydana geldi.',
              life: 3000
            });
          }
        });
       },
      () => { 
        this.messageServiceToast.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Silme işlemi iptal edildi: \nŞube: ${name}`,
          life: 3000
        });
      });
    
  }









}
