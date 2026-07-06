import { ChangeDetectionStrategy, Component, inject, model, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { Category } from '../../core/models/category.model';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { CategoriesService } from '../../core/services/categories.service';
import { MessageService, TreeNode } from 'primeng/api';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { TreeSelectModule } from 'primeng/treeselect';
import { SelectModule } from 'primeng/select'
import { FormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { CategoryFormDialogComponent } from "./category-form-dialog/category-form-dialog.component";
import { CardModule } from 'primeng/card';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { TreeTableModule } from 'primeng/treetable';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-categories',
  imports: [
    FormsModule,
    TreeTableModule,
    ButtonModule,
    TableModule,
    CardModule,
    TagModule,
    TreeSelectModule, SelectModule, MatExpansionModule,
    CategoryFormDialogComponent,
    IconFieldModule,
    InputIconModule,
  ],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated

})
export class CategoriesComponent implements OnInit {
  private categoryService = inject(CategoriesService);
  private breadcrumbService = inject(BreadcrumbService);
  private messageServiceToast = inject(MessageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private route = inject(ActivatedRoute);
  
  readonly isLoading = signal<boolean>(false);
  readonly mode = signal<'dialog' | 'page'>('dialog');

  // Signals
  categories = signal<Category[]>([]);
  treeCategories = signal<TreeNode<Category>[]>([]);
  parentCategories = signal<Category[]>([]);
  isDialogVisible = model<boolean>(false);
  dialogTitle = signal<"create" | "edit">('create');
  editData = signal<Category | null>(null);
  isPageMode = signal<boolean>(false);


  categoryForm = signal<any>({
    name: '',
    slug: '',
    description: '',
    displayOrder: 0,
    parentCategoryId: null,
    isActive: true
  });
  // Breadcrumb
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Kategoriler',
      url: '/admin/categories',
      icon: 'ri-node-tree',
      isActive: true
    }]);
  // ==================================================>

  ngOnInit(): void {
    const mode = this.route.snapshot.data['mode'];
    this.mode.set(mode);

    this.route.data.subscribe(data => {
      // Eğer mod 'page' ise hiyerarşik veriyi çek
      if (data['mode'] === 'page') {
        this.categoryService.getHierarchy().subscribe({
          next: (res) => {
           
            // 'res.data' senin parent listeni oluşturacak
            this.parentCategories.set(res.data ?? []);
          }
        });
      }
    });
 
    
    this.route.data.subscribe(data => {
      this.isPageMode.set(data['mode'] === 'page');
    });
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadCategories();
  }

  loadCategories() {
    this.isLoading.set(true);

    this.categoryService.getAll().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.categories.set(this.flattenCategories(res.data));
          this.treeCategories.set(
            this.convertToTreeNodes(res.data)
          );
          // Parent dropdown için sadece ana kategorileri al
          this.parentCategories.set(res.data);
          this.isLoading.set(false);
          return;
        }
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: res.errorMessages?.join(', ') || 'Kategoriler yüklenirken hata oluştu.'
        });

        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.message || 'Kategori yüklemede sistem hatası. :('
        });
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }


  getParentName(category: Category): string {
    return category.parentCategory?.name || '-';
  }

  openNewCategoryDialog() {
    this.dialogTitle.set('create');
    this.categoryForm.set({
      name: '',
      slug: '',
      description: '',
      displayOrder: 0,
      parentCategoryId: null,
      isActive: true
    });
    this.isDialogVisible.set(true);
  }

  // 3. Dialog Kapatma
  closeDialog() {
    this.isDialogVisible.set(false);
  }

  navigateToHierarchy() {
    throw new Error('Method not implemented.');
  }

  deleteCategory(id: string, name: string) {
    this.customConfirmDialogService.showDeleteConfirm( name, () => {
      this.isLoading.set(true);
      this.categoryService.delete(id).subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res.isSuccessful) {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Başarılı 🗑️',
              detail: `"${name}" kategorisi başarıyla silindi.`
            });
            this.loadCategories();
            this.isLoading.set(false)
            return;
          }
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: `${res.errorMessages}` || 'Kategori silinemedi.'
          });
          this.isLoading.set(false)
        }, 
        error: (err) => {
          this.messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: `${err.error.errorMessages}` || 'Kullanıcı silinirken hata oluştu!'
          });
          this.isLoading.set(false)
          console.error(err);
        }
      })
    });
  }


  openEditCategoryDialog(category: Category): void {
    this.dialogTitle.set('edit');
    this.editData.set(category);
    this.isDialogVisible.set(true);
  }

  onSaveSuccess(isSuccess: boolean): void {
    if (isSuccess) {
      this.loadCategories(); 
    }
  }


  refreshData() {
    this.loadCategories();
  }

  // =================== PRİVARTE ===========================> 

  // categories.component.ts içine ekle
  private flattenCategories(data: Category[]): Category[] {
    let list: any[] = [];

    data.forEach(cat => {
      // 1. Ana kategoriyi ekle (level 0)
      list.push({ ...cat, level: 0 });

      // 2. Varsa alt kategorileri ekle (level 1)
      if (cat.subCategories && cat.subCategories.length > 0) {
        cat.subCategories.forEach((sub: any) => {
          // Alt kategoriye 'parentName' bilgisini manuel taşıyoruz
          list.push({
            ...sub,
            level: 1,
            parentName: cat.name
          });
        });
      }
    });

    return list;
  }


  // ✅ Tree verisini oluştur
  private convertToTreeNodes(categories: Category[]): TreeNode<Category>[] {
    var res = categories.map((c, index) => ({
      data: {
        ...c,
        index: index, // ✅ 0'dan başlayan index
        displayNumber: index + 1 // ✅ 1'den başlayan görünen numara,
        
      },
      children: c.subCategories?.length
      ? this.convertToTreeNodes(c.subCategories)
      : undefined,
      leaf: !c.subCategories?.length
    }));

    return res;
  }


}
