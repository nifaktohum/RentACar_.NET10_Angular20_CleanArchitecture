import { Directive, effect, ElementRef, inject, Input, Renderer2, signal } from '@angular/core';

@Directive({
  selector: '[togglePasswordDirective]',
  exportAs: 'toggleDir'
})
export class TogglePasswordDirective {
  private el = inject(ElementRef);
  private renderer = inject(Renderer2);

  // // 🔥 Angular'daki Remix Icon veya tıkladığın göz elementini buraya paslayacağız kanka
  @Input('togglePasswordDirective') targetIcon!: HTMLElement;

  // // 🔥 İŞTE SİGNAL'İMİZ: Varsayılan olarak şifre gizli (false)
  private isPasswordVisible = signal<boolean>(false);

  constructor() {
    // // 🔥 EFFECT SİHRİ: isPasswordVisible signal'i her değiştiğinde 
    // // Angular bu bloğu otomatik tetikler ve DOM'u günceller!
    effect(() => {
      const isVisible = this.isPasswordVisible();
      const inputType = isVisible ? 'text' : 'password';

      // 1. Input'un tipini güvenle değiştiriyoruz kanka
      this.renderer.setAttribute(this.el.nativeElement, 'type', inputType);

      // 2. Remix Icon sınıflarını signal durumuna göre güncelliyoruz
      if (this.targetIcon) {
        if (isVisible) {
          this.renderer.removeClass(this.targetIcon, 'ri-eye-off-line');
          this.renderer.addClass(this.targetIcon, 'ri-eye-line');
          // Renk dokunuşu (İstersen 'crimson' veya tam '#ff4d4d' gibi şık bir kırmızı hex kodu da yazabilirsin kanka)
          this.renderer.setStyle(this.targetIcon, 'color', '#008000');
        } else {
          this.renderer.removeClass(this.targetIcon, 'ri-eye-line');
          this.renderer.addClass(this.targetIcon, 'ri-eye-off-line');
          // Renk dokunuşu (Çok cırtlak olmayan, şık bir gold/sarı tonu '#f59e0b')
          this.renderer.setStyle(this.targetIcon, 'color', '#ff0000');
        }

        // İkonun geçişini yumuşatmak için küçük bir CSS transition efekti kanka
        // this.renderer.setStyle(this.targetIcon, 'transition', '.4s ease-in-out');
      }
    });
  }



  toggle() {
    // // 🔥 Signal değerini tersine çeviriyoruz (true ise false, false ise true)
    this.isPasswordVisible.update(value => !value);
  }
}