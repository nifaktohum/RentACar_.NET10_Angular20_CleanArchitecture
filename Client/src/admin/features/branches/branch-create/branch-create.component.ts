import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { BlankComponent } from '../../../components/blank/blank.component';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BranchModel } from '../../../core/models/branch.model';
import { BranchService } from '../../../../core/services/branch.service';
import { MessageService } from 'primeng/api';


@Component({
  selector: 'app-branch-create',
  imports: [
    BlankComponent,
    FormsModule,
  ],
  templateUrl: './branch-create.component.html',
  styleUrl: './branch-create.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BranchCreateComponent {
  readonly breadcrumbService = inject(BreadcrumbService);
  readonly branchService = inject(BranchService);
  readonly messageServiceToast = inject(MessageService);
  readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  readonly id = signal<string | null>(null);
  readonly isEditMode = computed(() => !!this.id());
  readonly pageTitle = computed(() => this.id() ? 'Şube Bilgilerini Güncelle' : 'Yeni Şube Tanımla');
  readonly pageIcon = computed(() => this.id() ? 'ri-edit-box-line' : 'ri-add-circle-line');


  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Şubeler',
      icon: 'ri-map-pin-user-line',
      url: '/admin/branches',
      isActive: false
    }
  ]);

  // Form verilerini tutan dinamik signal
  // Form verilerini tutan dinamik signal yapısı (EntityModel alanları dahil)
  readonly branchForm = signal<BranchModel>({
    id: '',
    name: '',
    address: {
      city: '',
      district: '',
      fullAddress: '',
      phone1: '',
      phone2: null,
      email: ''
    },
    isActive: true,
    // EntityModel'den gelen ve TS'in zorunlu kıldığı alanlar:
    createdAt: new Date().toISOString(), // veya null/undefined durumunuza göre
    createdBy: '',
    createdFullname: '',
    isDeleted: false
  });

  //==================================>
  constructor() {
    this.route.params.subscribe(res => {
      if (res['id']) {
        this.id.set(res['id']);
        this.loadBranchData(res['id']); // Gerçek veriyi API'den çeken metot
        this.breadcrumbs.update(prev => [...prev, {
          title: 'Güncelle',
          icon: 'ri-edit-box-line',
          url: '/admin/branches/branch-cread',
          isActive: true
        }])
      } else {
        this.breadcrumbs.update(prev => [...prev, {
          title: 'Ekle',
          icon: 'ri-add-circle-line',
          url: '/admin/branches/branch-cread',
          isActive: true
        }])
      }
      this.breadcrumbService.reset(this.breadcrumbs())
    })

  }

  // Form Kaydetme (Ekleme / Güncelleme ortak metodu)
  onSubmit() {
    const payload = this.branchForm();
    // Objeyi konsolda düzgün görmek için virgül (,) kullan kanka:
    if (this.isEditMode()) {
      // 1. GÜNCELLEME SENARYOSU
      const currentId = this.id();
      if (currentId) {
        this.branchService.updateBranch(currentId, payload).subscribe({
          next: () => {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: 'Şube başarı ile güncellendi.',
            })
            // this.snackbarService.success('Şube başarı ile güncellendi. :)');
            this.router.navigateByUrl('/admin/branches');
          },
          error: (err) => {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata :(',
              detail: 'Şube güncellemede hata.'
            })
            // this.snackbarService.error('Güncelleme hatası. :(');
            console.error(err);
          }
        });
      }
    } else {
      // 2. EKLEME SENARYOSU (Artık if bloğunun dışındaki doğru else yerinde!)
      this.branchService.createBranch(payload).subscribe({
        next: () => {
          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Şube başarı ile eklendi. :)'
          })
          // this.snackbarService.success('Şube başarı ile eklendi. :)');
          this.router.navigateByUrl('/admin/branches');
        },
        error: (err) => {
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Şube ekleme hatası. :('
          })
          // this.snackbarService.error('Şube ekleme hatası. :('); 
          console.error(err);
        }
      });
    }
  }


  onCancel() {
    this.router.navigate(['/admin/branches']);
  }

  //====================================>
  // HTTP İsteği ile Gerçek Veriyi Forma Entegre Eden Metot
  private loadBranchData(id: string) {
    this.branchService.getBranchById(id).subscribe({
      next: (data: BranchModel) => {
        this.branchForm.set(data); // Gelen veri doğrudan signal içerisine gömülür ve HTML tetiklenir
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata :)',
          detail: 'Şube verisi yüklenirken hata oluştu:'
        })
        // this.snackbarService.error('Şube verisi yüklenirken hata oluştu:');
        console.error(err);
      }
    });
  }


}
