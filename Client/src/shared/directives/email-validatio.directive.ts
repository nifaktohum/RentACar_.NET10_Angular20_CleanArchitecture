import { Directive, ElementRef, HostListener, Renderer2, inject, input } from '@angular/core';

@Directive({
  selector: '[emailValidationDirective]',
})
export class EmailValidationDirective {
  private el = inject(ElementRef);
  private renderer = inject(Renderer2);

  // HTML'den gelen hata kutusunun referansı (#emailError)
  errorTarget = input.required<HTMLElement>({ alias: 'emailValidationDirective' });

  // 1. Kullanıcı inputtan çıktığı an (blur) e-posta kontrolünü tetikliyoruz
  @HostListener('blur')
  onBlur() {
    this.checkValidation();
  }

  // 2. Kullanıcı içeriye yeni bir karakter girdiğinde kırmızılıkları pürüzsüzce temizle
  @HostListener('input')
  onInput() {
    this.clearError();
  }

  private checkValidation() {
    const value = this.el.nativeElement.value?.trim() || '';

    // Her kontrolden önce ortalığı bir temizleyelim
    this.clearError();

    // Boş bırakılamaz kontrolü
    if (!value) {
      this.applyError('* E-posta adresi boş bırakılamaz.');
      return;
    }

    // RegEx ile format kontrolü
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(value)) {
      this.applyError('* Lütfen geçerli bir e-posta adresi giriniz.');
    }
  }

  private applyError(message: string) {
    const inputElement = this.el.nativeElement;

    // İnput çerçevesini kıpkırmızı yapıyoruz
    this.renderer.setStyle(inputElement, 'border-color', '#ef4444');
    this.renderer.setStyle(inputElement, 'box-shadow', '0 0 0 3px rgba(239, 239, 239, 0.2)');
    this.renderer.setStyle(inputElement, 'transition', 'all 0.3s ease-in-out');

    // Hedef hata div'inin içine mesajı bas ve görünür kıl
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

    // Stilleri sıfırla
    this.renderer.removeStyle(inputElement, 'border-color');
    this.renderer.removeStyle(inputElement, 'box-shadow');

    const target = this.errorTarget();
    if (target) {
      this.renderer.setProperty(target, 'innerText', '');
      this.renderer.setStyle(target, 'display', 'none');
    }
  }
}