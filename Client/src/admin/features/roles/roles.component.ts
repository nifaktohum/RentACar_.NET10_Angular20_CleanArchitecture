import { ChangeDetectionStrategy, Component, computed, effect, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { RoleService } from '../../core/services/role.service';
import { CreateRoleModel, RoleModel, UpdateRoleModel } from '../../core/models/Role.model';
import { MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { PermissionService } from '../../core/services/permission.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { PermissionModel } from '../../core/models/permission.molel';
import { AuthService } from '../../../core/services/auth.service';

export const ADMIN_ROLE_ID = 'A1B2C3D4-E5F6-7A8B-9C0D-1E2F3A4B5C6D';

@Component({
  selector: 'app-roles',
  imports: [
    //PrimeNG
    TableModule,
    ButtonModule,
    FormsModule,
    ConfirmDialogModule,
    DialogModule,
    FloatLabelModule,
    ToggleSwitchModule 
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None
})
export class RolesComponent implements OnInit {
  
  private braadCrumbService = inject(BreadcrumbService);
  private roleService = inject(RoleService);
  private messageServiceToast = inject(MessageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private permissionService = inject(PermissionService);

  adminId = signal<string>(ADMIN_ROLE_ID);
  roles = signal<RoleModel[]>([]);
  readonly isLoading = signal<boolean>(false);
  isRoleDialogVisible: boolean = false;
  isPermissionDialogVisible: boolean = false;
  dialogHeader = signal<string>('Rol Yetki Yönetimi');

  selectedRole: any | null = null;
  // Arama filtresi için kanka
  globalFilter: string = '';
  newRole = {
    id: undefined as string | undefined,
    name: '',
    description: ''
  };

  
  permissions = signal<PermissionModel[]>([]);
  rolePermissions = signal<any[]>([]); // Seçilen rolün izin listesi tutulacak
  // Computed - otomatik güncellenir
  computedPermissions = computed(() => {
    return this.rolePermissions().map(perm => ({
      ...perm,
      displayName: `${perm.name} (${perm.isActive ? '✅' : '❌'})`
    }));
  });


  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Roller',
      url: '/admin/roles',
      icon: 'ri-key-2-line',
      isActive: true
    }
  ])

  //===========================================>
  ngOnInit(): void {
    this.braadCrumbService.reset(this.breadcrumbs())
    this.loadRoles();
  };

  loadRoles() {
    this.isLoading.set(true);

    this.roleService.getRoles().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.roles.set(res.data);
          this.isLoading.set(false)
          return;
        }
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Roller yüklenirken hata oluştu. :('
        }),
          this.isLoading.set(false)
        console.error(res.errorMessages);

      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Roller yüklenirken hata oluştu. :(' + err.status
        }),
          this.isLoading.set(false)
        console.error(err);
      }
    })
  }

  refreshData() {
    this.loadRoles();
  }

  openEditRoleDialog(role: RoleModel) {
    this.newRole = {
      id: role.id ?? '',
      name: role.name,
      description: role.description
    };
    this.isRoleDialogVisible = true;
  }

  saveRole() {
    if (!this.newRole.name.trim()) {
      this.messageServiceToast.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Rol adı boş geçilemez!'
      });
      return;
    }
    this.isLoading.set(true);

    // Güncelleme ===> 
    if (this.newRole.id) {
      const updateData: UpdateRoleModel = {
        id: this.newRole.id,
        name: this.newRole.name,
        description: this.newRole.description
      };

      this.roleService.updateRole(updateData).subscribe({
        next: (res) => {
          if (res.isSuccessful) {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: 'Rol başarıyla güncellendi.'
            });
            this.closeRoleDialog();
            this.loadRoles();
          } else {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: 'Role güncelleme başarısız.'
            });
            console.error(res);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: err.error.detail || 'Sistem hatası oluştu'
          });
          this.isLoading.set(false);
          console.error(err);
        }
      });

    }

    // Ekleme ===> 
    if (!this.newRole.id) {
      
      const createData: CreateRoleModel = {
        name: this.newRole.name,
        description: this.newRole.description
      };
      this.roleService.createRole(createData).subscribe({
        next: () => {
          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Yeni rol sisteme eklendi.'
          });
          this.isRoleDialogVisible = false; // Dialog'u kapat
          this.loadRoles(); // Rol listesini yenile
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: err.error.detail || 'Rol eklenirken bir hata oluştu.'
          });
          this.isLoading.set(false);
        }
      });
    }
  }

  deleteRole(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(name,
      () => {
        const command = { id: id };
        this.roleService.deleteRole(command.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageServiceToast.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `'${name}' rolü başarıyla silindi!`
              });
              this.loadRoles(); 
            } else {
              // Backend'den iş kuralı hatası dönerse (örn: bu role bağlı kullanıcılar varsa) yakala kanka
              this.messageServiceToast.add({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Silme işlemi tamamlanamadı.'
              });
              console.error(response.errorMessages);
            }
          },
          error: (err) => {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: 'Rol silinirken sistemsel bir hata oluştu! :(',
              life: 3000
            });
            console.error(err);
          }
        });
      },
      () => {
        this.messageServiceToast.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Silme işlemi iptal edildi: ${name}`,
          life: 3000
        });
      });
  }

  openRoleDialog() {
    this.newRole = { id: '', name: '', description: '' }; // Formu sıfırla
    this.isRoleDialogVisible = true;
  }

  closeRoleDialog() {
    this.isRoleDialogVisible = false;
  }

  openPermissionDialog(role: RoleModel) {
    this.selectedRole = role;
    this.dialogHeader.set(`${role.name} - Yetki Yönetimi`);
    // Backend'den bu role ait izin havuzunu (id, name, description, isActive) çekiyoruz
    this.permissionService.getPermissionsByRoleId(role.id).subscribe({
      next: (response) => {
        
        if (response.isSuccessful && response.data) {
          this.rolePermissions.set(response.data);
          this.isPermissionDialogVisible = true;
        } else {
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'İzin listesi yüklenemedi.'
          });
          console.error(response.errorMessages);
          
        }
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'İzinler getirilirken sistemsel bir hata oluştu! :(',
        });
        console.error(err);
      }
    });
  }

  onToggleClick(perm: any, event: Event) {
    const originalPermissions = this.rolePermissions();
    const newState = !perm.isActive;

    // ============================================================>
    // --- ADMIN işlemleri  ---
    if (this.selectedRole?.id?.toUpperCase() === this.adminId().toUpperCase()) {
      event.preventDefault();
      event.stopPropagation();

      this.messageServiceToast.add({
        severity: 'warn',
        summary: 'Erişim Engellendi',
        detail: 'Admin yetkileri değiştirilemez!',
        life: 3000
      });

      // PrimeNG switch'inin pasifte takılı kalmasını önleyen çift tetikleme taklası
      this.rolePermissions.update(perms =>
        perms.map(p => p.id === perm.id ? { ...p, isActive: false } : p)
      );

      setTimeout(() => {
        this.rolePermissions.update(perms =>
          perms.map(p => p.id === perm.id ? { ...p, isActive: true } : p)
        );
      }, 200);
      return;
    }

    // ============================================================> 
    // --- Normal kullanıcı işlemleri  ---
    this.rolePermissions.update(perms =>
      perms.map(p => p.id === perm.id ? { ...p, isActive: newState } : p)
    );

    if (newState) {
      this.permissionService.activatePermission(this.selectedRole.id, perm.id).subscribe({
        next: () => {
          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Başarılı',
            detail:  perm.name + ' yetkisi aktif edildi',
            life: 3000
          });
          this.rolePermissions.update(perms => [...perms]);
        },
        error: () => {
          this.rolePermissions.set(originalPermissions);
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Yetki değiştirilemedi!',
            life: 3000
          });
          this.rolePermissions.update(perms => [...perms]);
        }
      });
    } else {
      this.permissionService.deactivatePermission(this.selectedRole.id, perm.id).subscribe({
        next: () => {
          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Yetki devre dışı bırakıldı',
            life: 3000
          });
          this.rolePermissions.update(perms => [...perms]);
        },
        error: () => {
          this.rolePermissions.set(originalPermissions);
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Yetki değiştirilemedi!',
            life: 3000
          });
          this.rolePermissions.update(perms => [...perms]);
        }
      });
    }
  }

}


