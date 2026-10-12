import { Directive, ElementRef, AfterViewInit, inject } from '@angular/core';

@Directive({
  selector: '[autofocusDirective]',
})
export class AutofocusDirective implements AfterViewInit {
  private el = inject(ElementRef);

  ngAfterViewInit() {
    setTimeout(() => {
      this.el.nativeElement.focus();
    }, 50);
  }
}

/*
    imleci kutunun içinde yakıp söndürmeye başlar. 
    Kullanıcıya tıkla-yaz hamlesi yapmaz, direkt yazmaya başlasın diye arkada yaparım.
*/