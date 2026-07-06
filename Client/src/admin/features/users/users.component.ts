import { ChangeDetectionStrategy, Component, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { UserModel } from '../../core/models/user.model';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { BranchService } from '../../../core/services/branch.service';
import { RoleService } from '../../core/services/role.service';
import { MessageService } from 'primeng/api';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { RoleModel } from '../../core/models/Role.model';
import { BranchModel } from '../../core/models/branch.model';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { UserService } from '../../core/services/user.service';
import { CreateUserRequest } from '../../core/models/createUser.moder';
import { UpdateUserRequest } from '../../core/models/user.model';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { PhoneFormatPipe } from "../../../shared/pipes/phone-format.pipe";
import { AuthService } from '../../../core/services/auth.service';
import { DatePipe } from '@angular/common';
import { UserDetail } from '../../core/models/detailUser.model';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-users',
  imports: [
    TableModule,
    ButtonModule,
    FloatLabelModule,
    DialogModule,
    FormsModule, SelectModule, ToggleSwitchModule,
    PhoneFormatPipe, DatePipe, 
    CardModule
],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class UsersComponent implements OnInit {
  private breadcrumbService = inject(BreadcrumbService);
  private userService = inject(UserService);
  private branchService = inject(BranchService);
  private roleService = inject(RoleService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);
  readonly authService = inject(AuthService);

  users = signal<UserModel[]>([]);
  branches = signal<BranchModel[]>([]);
  roles = signal<RoleModel[]>([]);
  isLoading = signal<boolean>(false);
  isUserDialogVisible = false;
  isDetailDialogVisible = false;
  dialogTitle = signal<string>('Yeni Kullanıcı');

  selectedUser = signal<UserDetail | null>(null); 

  userForm: any = {
    id: '',
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    branchId: '',
    roleId: '',
    isActive: true
  };

  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Kullanıcılar',
      url: '/admin/users',
      icon: 'ri-user-3-line',
      isActive: true
    }
  ]);

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadUsers();
    this.loadBranches();
    this.loadRoles();
    console.log(this.branches().forEach(b => b.name));
    
  }

  loadUsers() {
    this.isLoading.set(true);
    this.userService.getUsers().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.users.set(res.data);
          this.isLoading.set(false)
          return;
        }
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: `${res.errorMessages}` || 'Kullanıcılar yüklenirken hata oluştu.'
        });
        
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: `${err.error.message}` || 'Sistem taradından bir hata oluştu!'
        });
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  loadBranches() {
    this.branchService.getBranches().subscribe({
      next: (res: any) => {        
          this.branches.set(res.data.items);        
      },
      error: (err) => console.error('Şubeler yüklenemedi:', err)
    });
  }

  loadRoles() {
    this.roleService.getRoles().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.roles.set(res.data);
        }
      },
      error: (err) => console.error('Roller yüklenemedi:', err)
    });
  }

  refreshData() {
    this.loadUsers();
  }

  openUserDialog() {
    this.dialogTitle.set('Yeni Kullanıcı');
    this.userForm = {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      password: '',
      branchId: '',
      roleId: '',
      isActive: true
    };
    this.isUserDialogVisible = true;
  }
  
  openEditUserDialog(user: any) {
    this.dialogTitle.set(user.fullName + ' Düzenle');
    // 1. Rol ismini al (Örn: 'Customer')
    const roleName = user.roleNames && user.roleNames.length > 0 ? user.roleNames[0] : '';
    // 2. Roles listesinden bu isme sahip olan objeyi bul
    const matchedRole = this.roles().find(r => r.name === roleName);
  
    const matchedBranch = this.branches().find(b =>
      b.name.trim().toLowerCase() === user.branchName.trim().toLowerCase()
    );

    
    this.userForm = {
      id: user.id,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      phoneNumber: user.phoneNumber,
      password: '',
      branchId: matchedBranch?.id || '',
      roleId: matchedRole ? matchedRole.id : '',
      isActive: user.isActive
    };

    this.isUserDialogVisible = true;
  }

  viewUser(user: UserModel) {
    this.selectedUser.set(user as any);
    this.isDetailDialogVisible = true;
  }

  closeUserDialog() {
    this.isUserDialogVisible = false;
  }

  saveUser() {
    if (!this.userForm.firstName || !this.userForm.lastName || !this.userForm.email) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Ad, Soyad ve Email zorunludur!'
      });
      return;
    }

    this.isLoading.set(true);

    if (this.userForm.id) {
      // UPDATE
      const updateData: UpdateUserRequest = {
        id: this.userForm.id,
        firstName: this.userForm.firstName,
        lastName: this.userForm.lastName,
        email: this.userForm.email,
        phoneNumber: this.userForm.phoneNumber,
        branchId: this.userForm.branchId,
        roleId: this.userForm.roleId,
        isActive: this.userForm.isActive
      };
      this.userService.updateUser(updateData).subscribe({
        
        next: (res) => {
          console.log(updateData)
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: 'Kullanıcı başarıyla güncellendi.'
            });
            this.closeUserDialog();
            this.loadUsers();
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: 'Kullanıcı güncellenemedi.'
            });
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err.error?.detail || 'Sistem hatası oluştu.'
          });
          this.isLoading.set(false);
        }
      });
    } else {
      // CREATE
      const createData: CreateUserRequest = {
        firstName: this.userForm.firstName,
        lastName: this.userForm.lastName,
        email: this.userForm.email,
        phoneNumber: this.userForm.phoneNumber,
        password: this.userForm.password || 'Customer123!',
        branchId: this.userForm.branchId,
        roleId: this.userForm.roleId
      };
      console.log(createData);
      
      this.userService.createUser(createData).subscribe({
        next: (res) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: 'Kullanıcı başarıyla oluşturuldu.'
            });
            this.closeUserDialog();
            this.loadUsers();
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: 'Kullanıcı oluşturulamadı.'
            });
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err.error?.detail || 'Sistem hatası oluştu.'
          });
          this.isLoading.set(false);
        }
      });
    }
  }

  deleteUser(id: string, name: string) {
    this.isLoading.set(true);
    this.confirmDialog.showDeleteConfirm(name, () => {
      this.userService.deleteUser(id).subscribe({
        next: (res) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: `'${name}' kullanıcısı silindi.`,
            });
            this.loadUsers();
            this.isLoading.set(false)
          }
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: `${res.errorMessages}` || 'Kullanıcı silinemedi.'
          });
          this.isLoading.set(false)

        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: `${err.error.errorMessages}` || 'Kullanıcı silinirken hata oluştu!'
          });
          this.isLoading.set(false)
          console.error(err);
        }
      });
    });
  }

  // ✅ KULLANICI DETAYINI API'DEN ÇEK!
  openUserDetail(user: any) {
    this.isLoading.set(true);

    this.userService.getUserById(user.id).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.selectedUser.set(res.data);
          this.isDetailDialogVisible = true;
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: res.errorMessages?.[0] || 'Kullanıcı detayı yüklenemedi.'
          });
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.message || 'Sistem hatası oluştu!'
        });
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }







}
