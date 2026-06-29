import { ChangeDetectionStrategy, Component, EventEmitter, input, model, output, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../core/models/category.model';

@Component({
  selector: 'app-category-form-dialog',
  imports: [
    DialogModule,
    FormsModule
  ],
  templateUrl: './category-form-dialog.component.html',
  styleUrl: './category-form-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class CategoryFormDialogComponent {
  isDialogVisible(): any {
    throw new Error('Method not implemented.');
  }
  readonly title = input<string>("Kategori Ekle")
  readonly visible = model(false);
  readonly mode = input<"create" | "edit">("create");
  readonly editData = input<Category | null>(null);
  readonly parentCategories = input<Category[]>([]);
  readonly saving = input<boolean>(false);
  readonly save = output<CreateCategoryRequest | UpdateCategoryRequest>();

  // Form model
  formData: CreateCategoryRequest = {
    name: '',
    slug: '',
    description: null,
    displayOrder: null,
    parentCategoryId: null
  };

  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Kategori Ekle' : 'Kategori Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }

  // Lifecycle
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editData'] && this.editData() && this.mode() === 'edit') {
      this.populateForm(this.editData()!);
    }

    if (changes['visible'] && !this.visible) {
      this.resetForm();
    }

    if (changes['mode'] && this.mode() === 'create') {
      this.resetForm();
    }
  }

  onSubmit(): void {
    // Validasyon
    if (!this.formData.name?.trim()) {
      // Toast veya error mesajı göster
      return;
    }

    if (!this.formData.slug?.trim()) {
      return;
    }

    // Slug'ı formatla
    this.formData.slug = this.formData.slug
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^a-z0-9-]/g, '');

    // Save event'ini fırlat
    if (this.isEditMode && this.editData()) {
      const updateData: UpdateCategoryRequest = {
        id: this.editData()!.id,
        ...this.formData
      };
      this.save.emit(updateData);
    } else {
      this.save.emit(this.formData);
    }
  }

  //  * Slug'ı otomatik formatlar
  formatSlug(): void {
    if (this.formData.name) {
      this.formData.slug = this.formData.name
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }
  }

  //  * Form'u doldurur (edit modu için)
  private populateForm(category: Category): void {
    this.formData = {
      name: category.name,
      slug: category.slug,
      description: category.description ?? null,
      displayOrder: category.displayOrder ?? null,
      parentCategoryId: category.parentCategoryId ?? null
    };
  }

  //  * Form'u resetler
  private resetForm(): void {
    this.formData = {
      name: '',
      slug: '',
      description: null,
      displayOrder: null,
      parentCategoryId: null
    };
  }


  close(): void {
    this.visible.set(false);
    this.resetForm();
  }

}
