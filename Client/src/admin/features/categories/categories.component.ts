import { ChangeDetectionStrategy, Component, inject, model, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { Category } from '../../core/models/category.model';
import { TagModule } from 'primeng/tag';
import { NgClass } from '@angular/common';
import { CategoriesService } from '../../core/services/categories.service';
import { MessageService, TreeNode } from 'primeng/api';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { TreeSelectModule } from 'primeng/treeselect';
import { SelectModule } from 'primeng/select'
import { FormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { CategoryFormDialogComponent } from "./category-form-dialog/category-form-dialog.component";


@Component({
  selector: 'app-categories',
  imports: [
    FormsModule,
    //PrimeNG
    ButtonModule,
    TableModule,
    TagModule,
    NgClass,
    TreeSelectModule, SelectModule, MatExpansionModule,
    CategoryFormDialogComponent
],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated

})
export class CategoriesComponent implements OnInit{
  private categoryService = inject(CategoriesService);
  private breadcrumbService = inject(BreadcrumbService);
  private messageServiceToast = inject(MessageService);
  
  readonly isLoading = signal<boolean>(false);

  // Signals
  categories = signal<Category[]>([]);
  parentCategories = signal<Category[]>([]);
  isDialogVisible = model<boolean>(false);
  dialogTitle = signal<string>('Yeni Kategori Ekle');

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
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadCategories();
  }
  
  loadCategories() {
    this.isLoading.set(true);

    this.categoryService.getAll().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          const flatData = this.flattenCategories(res.data);
          this.categories.set(flatData);
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
    console.log(category.parentCategory);
    
    return category.parentCategory?.name || '-';
  }
  openNewCategoryDialog() {
    this.dialogTitle.set('Yeni Kategori Ekle');
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
  deleteCategory(arg0: any, arg1: any) {
    throw new Error('Method not implemented.');
  }  
  openEditCategoryDialog(_t39: any) {
    throw new Error('Method not implemented.');
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


  // Veriyi TreeNode'a dönüştür
  private convertToTreeNodes(categories: Category[]): TreeNode<Category>[] {
    return categories.map(c => ({
      data: c,
      children: c.subCategories?.length
        ? this.convertToTreeNodes(c.subCategories)
        : undefined,
      leaf: !c.subCategories?.length
    }));
  }


}
