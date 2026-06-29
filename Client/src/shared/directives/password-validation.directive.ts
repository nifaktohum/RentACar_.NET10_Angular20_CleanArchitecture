import { Directive, ElementRef, HostListener, Renderer2, inject, input } from '@angular/core';

@Directive({
  selector: '[passwordValidationDirective]',
})
export class PasswordValidationDirective {
  private el = inject(ElementRef);
  private renderer = inject(Renderer2);

  // HTML'den gelen hata kutusunun referansı (#passwordError)
  errorTarget = input.required<HTMLElement>({ alias: 'passwordValidationDirective' });

  // 1. Kullanıcı inputtan çıktığı an (blur) validasyon kurallarını işletiyoruz
  @HostListener('blur')
  onBlur() {
    this.checkValidation();
  }

  // 2. Kullanıcı içeriye yeni bir harf yazdığı an (input) kırmızılıkları anında temizliyoruz
  @HostListener('input')
  onInput() {
    this.clearError();
  }

  private checkValidation() {
    const value = this.el.nativeElement.value || '';

    this.clearError();

    if (!value.trim()) {
      this.applyError('* Şifre alanı boş bırakılamaz.');
      return;
    }
    if (value.length < 6) {
      this.applyError('* Şifre en az 6 karakter olmalıdır.');
      return;
    }
    if (!/[0-9]/.test(value)) {
      this.applyError('* Şifre en az bir rakam içermelidir.');
      return;
    }
    if (!/[A-ZÇĞİÖŞÜ]/.test(value)) {
      this.applyError('* Şifre en az bir büyük harf içermelidir.');
      return;
    }
    if (!/[a-zçğıöşü]/.test(value)) {
      this.applyError('* Şifre en az bir küçük harf içermelidir.');
      return;
    }
  }

  private applyError(message: string) {
    const inputElement = this.el.nativeElement;

    // İnput kutusuna kırmızı sınır çizgilerini çekiyoruz
    this.renderer.setStyle(inputElement, 'border-color', '#ef4444');
    this.renderer.setStyle(inputElement, 'box-shadow', '0 0 0 3px rgba(239, 68, 68, 0.2)');
    this.renderer.setStyle(inputElement, 'transition', 'all 0.3s ease-in-out');

    // Hedef kutunun içeriğini doldur ve görünür yap
    const target = this.errorTarget();
    if (target) {
      this.renderer.setProperty(target, 'innerText', message);
      this.renderer.setStyle(target, 'color', '#ef4444');
      this.renderer.setStyle(target, 'font-size', '0.85rem');
      this.renderer.setStyle(target, 'margin-top', '0.4rem');
      this.renderer.setStyle(target, 'font-weight', '500');
      this.renderer.setStyle(target, 'display', 'block');
    }
  }

  private clearError() {
    const inputElement = this.el.nativeElement;

    this.renderer.removeStyle(inputElement, 'border-color');
    this.renderer.removeStyle(inputElement, 'box-shadow');

    const target = this.errorTarget();
    if (target) {
      this.renderer.setProperty(target, 'innerText', '');
      this.renderer.setStyle(target, 'display', 'none');
    }
  }
}