import { Directive, ElementRef, HostListener, Renderer2, inject, input } from '@angular/core';

@Directive({
  selector: '[confirmPasswordValidationDirective]',
})
export class ConfirmPasswordValidationDirective {
  private el = inject(ElementRef);
  private renderer = inject(Renderer2);

  // 🔥 1. Hata mesajını basacağımız asil kutu (Signal Input)
  errorTarget = input.required<HTMLElement>({ alias: 'confirmPasswordValidationDirective' });

  // 🔥 2. Karşılaştıracağımız ana şifrenin input elementi (Bunu da dışarıdan signal ile alıyoruz kanka)
  mainPasswordInput = input.required<HTMLInputElement>();

  @HostListener('blur')
  onBlur() {
    this.checkValidation();
  }

  @HostListener('input')
  onInput() {
    this.clearError();
  }

  private checkValidation() {
    const value = this.el.nativeElement.value || '';
    const mainPasswordValue = this.mainPasswordInput().value || '';

    this.clearError();

    // Kural 1: Boş bırakılamaz kuralı
    if (!value.trim()) {
      this.applyError('* Şifre tekrar alanı boş bırakılamaz kanka.');
      return;
    }

    // Kural 2: 🔥 Şifre bir önceki şifre ile aynı olmalı / uyuşmalı kontrolü!
    if (value !== mainPasswordValue) {
      this.applyError('* Girdiğiniz şifreler birbiriyle uyuşmuyor kanka.');
      return;
    }
  }

  private applyError(message: string) {
    const inputElement = this.el.nativeElement;
    this.renderer.setStyle(inputElement, 'border-color', '#ef4444');
    this.renderer.setStyle(inputElement, 'box-shadow', '0 0 0 3px rgba(239, 68, 68, 0.2)');

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