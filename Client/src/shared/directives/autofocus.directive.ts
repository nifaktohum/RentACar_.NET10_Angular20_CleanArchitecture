import { Directive, ElementRef, AfterViewInit, inject } from '@angular/core';

@Directive({
  selector: '[autofocusDirective]',
})
export class AutofocusDirective implements AfterViewInit {
  private el = inject(ElementRef);

  ngAfterViewInit() {
    // // 🔥 Sihirli dokunuş: DOM tamamen yüklendiği an elementi yakalayıp odağı çakıyoruz kanka!
    // // setTimeout kullanmamızın sebebi Angular'ın render döngüsünü (ExpressionChangedAfterItHasBeenCheckedError) riske atmamak.
    setTimeout(() => {
      this.el.nativeElement.focus();
    }, 50);
  }
}