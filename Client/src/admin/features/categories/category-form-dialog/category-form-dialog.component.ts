import { ChangeDetectionStrategy, Component, inject, input, model, output, signal, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { Category, CategoryRequest } from '../../../core/models/category.model';
import { FloatLabel } from 'primeng/floatlabel';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { CategoriesService } from '../../../core/services/categories.service';
import { finalize } from 'rxjs';
import { NgTemplateOutlet } from '@angular/common';
import { ToggleSwitch } from 'primeng/toggleswitch';

@Component({
  selector: 'app-category-form-dialog',
  imports: [
    NgTemplateOutlet,
    DialogModule,
    FormsModule,
    FloatLabel,
    SelectModule,
    ButtonModule,
    ToggleSwitch
  ],
  templateUrl: './category-form-dialog.component.html',
  styleUrl: './category-form-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class CategoryFormDialogComponent {
  private messageServiceToast = inject(MessageService);
  private categoriesService = inject(CategoriesService);

  // ==================== INPUTS ====================
  readonly mode = input<'dialog' | 'page'>('dialog');
  readonly title = input<string>("Kategori Ekle");
  readonly visible = model(false);
  readonly editmode = input<"create" | "edit">("create");
  readonly editData = input<Category | null>(null);
  readonly parentCategories = model<Category[]>([]);
  readonly isLoading = model<boolean>(false);

  // ==================== OUTPUTS ====================
  readonly save = output<CategoryRequest>();
  readonly saveSuccess = output<boolean>();

  // ==================== SIGNALS ====================
  readonly isSubCategoryMode = signal<boolean>(false);

  // ==================== FORM MODEL ====================
  categoryForm: CategoryRequest = {
    id: '',
    name: '',
    slug: '',
    description: null,
    displayOrder: null,
    parentCategoryId: null,
    isActive: true
  };

  // ==================== COMPUTED ====================
  get dialogTitle(): string {
    return this.editmode() === 'create' ? 'Yeni Kategori Ekle' : 'Kategori Düzenle';
  }

  get isEditMode(): boolean {
    return this.editmode() === 'edit';
  }

  // ✅ Update için veriyi hazırla (her seferinde dinamik oluştur)
  private getUpdateData(): CategoryRequest {
    return {
      id: this.editData()?.id || '',
      name: this.categoryForm.name,
      slug: this.categoryForm.slug,
      description: this.categoryForm.description,
      displayOrder: this.categoryForm.displayOrder,
      parentCategoryId: this.categoryForm.parentCategoryId,
      isActive: this.categoryForm.isActive
    };
  }

  // ==================== LIFECYCLE ====================
  ngOnChanges(changes: SimpleChanges): void {
    // ✅ Edit modunda formu doldur
    if (changes['editData'] && this.editData() && this.editmode() === 'edit') {
      this.populateForm(this.editData()!);
    }

    // ✅ Dialog kapatıldığında formu resetle
    if (changes['visible'] && !this.visible()) {
      this.resetForm();
    }

    // ✅ Mode değiştiğinde formu resetle
    if (changes['mode'] && this.editmode() === 'create') {
      this.resetForm();
    }
  }

  // ==================== PUBLIC METHODS ====================

  /**
   * Kategoriyi kaydeder (Create veya Update)
   */
  saveCategory(): void {
    // Validasyonlar
    if (!this.categoryForm.name?.trim() || !this.categoryForm.slug?.trim()) {
      this.messageServiceToast.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Kategori adı ve Slug zorunludur.'
      });
      return;
    }

    // Slug'ı formatla
    this.categoryForm.slug = this.categoryForm.slug
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^a-z0-9-]/g, '');

    this.isLoading.set(true);

    // ✅ Create veya Update işlemi
    const request$ = this.editmode() === 'create'
      ? this.categoriesService.create(this.categoryForm)
      : this.categoriesService.update(this.getUpdateData());  // ✅ Dinamik update data

    request$.pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          const action = this.editmode() === 'create' ? 'kaydedildi' : 'güncellendi';
          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Başarılı 🎉',
            detail: `"${this.categoryForm.name}" başarıyla ${action}.`
          });

          this.resetForm();

          // ✅ Page modunda formu temizle, sayfada kal
          if (this.mode() === 'page') {
            // Form zaten resetlendi, devam et
            this.saveSuccess.emit(true);
            // Odağı name alanına ver
            setTimeout(() => {
              const nameInput = document.getElementById('name') as HTMLInputElement;
              if (nameInput) nameInput.focus();
            }, 100);
          } else {
            // Dialog modunda kapat
            this.closeDialog();
            this.saveSuccess.emit(true);
          }
          return;
        }

        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: res.errorMessages?.join(', ') || 'İşlem başarısız.'
        });
        this.saveSuccess.emit(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Sistem Hatası',
          detail: err.error?.errorMessages?.join(', ') || 'Sunucuya ulaşılamadı, lütfen tekrar deneyin.'
        });
        this.saveSuccess.emit(false);
        console.error(err);
      }
    });
  }

  /**
   * Slug'ı otomatik formatlar
   */
  formatSlug(): void {
    if (this.categoryForm.name) {
      this.categoryForm.slug = this.categoryForm.name
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }
  }

  /**
   * İsim değiştiğinde slug'ı güncelle
   */
  onNameSlugChange(): void {
    this.categoryForm.slug = this.categoryForm.name
      .toLowerCase()
      .trim()
      .replace(/\s+/g, '-')
      .replace(/[^a-z0-9-]/g, '');
  }

  /**
   * Dialog'u kapat
   */
  closeDialog(): void {
    this.visible.set(false);
    this.resetForm();
  }

  /**
   * Kapat (alias)
   */
  close(): void {
    this.visible.set(false);
    this.resetForm();
  }

  /**
   * Ana kategori moduna geç
   */
  setParentCategoryMode(): void {
    this.isSubCategoryMode.set(false);
    this.categoryForm.parentCategoryId = null;
  }

  /**
   * Alt kategori moduna geç
   */
  setSubCategoryMode(): void {
    this.isSubCategoryMode.set(true);
  }

  // ==================== PRIVATE METHODS ====================

  /**
   * Form'u doldurur (edit modu için)
   */
  private populateForm(category: Category): void {
    this.categoryForm = {
      id: category.id,
      name: category.name,
      slug: category.slug,
      description: category.description ?? null,
      displayOrder: category.displayOrder ?? null,
      parentCategoryId: category.parentCategoryId ?? null,
      isActive: category.isActive
    };

    // ✅ Alt kategori ise isSubCategoryMode'u true yap
    this.isSubCategoryMode.set(category.parentCategoryId !== null);
  }

  /**
   * Form'u resetler
   */
  private resetForm(): void {
    this.categoryForm = {
      id: '',
      name: '',
      slug: '',
      description: null,
      displayOrder: null,
      parentCategoryId: null,
      isActive: true
    };

    // ✅ Create modunda ise SubCategory modunu sıfırla
    if (this.editmode() === 'create') {
      this.isSubCategoryMode.set(false);
    }
  }
}