import { AfterViewInit, Directive, ElementRef, HostListener, OnDestroy, Optional, Self } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[PrimeDateMaskDirective]'
})
export class PrimeDateMaskDirective implements AfterViewInit, OnDestroy {
  private inputEl: HTMLInputElement | null = null;
  private observer?: MutationObserver;

  // Event listener referanslarını cleanup için saklıyoruz
  private handleKeyDown = this.onKeyDown.bind(this);
  private handleInput = this.onInput.bind(this);
  private handlePaste = this.onPaste.bind(this);

  constructor(
    private el: ElementRef<HTMLElement>,
    @Optional() @Self() private ngControl: NgControl
  ) { }

  ngAfterViewInit() {
    this.findAndInitInput();
  }

  private findAndInitInput() {
    // p-datepicker ise içindeki input'u bulur, standart input ise kendisini alır
    const target = this.el.nativeElement;
    this.inputEl = target.tagName === 'INPUT'
      ? (target as HTMLInputElement)
      : target.querySelector('input');

    if (this.inputEl) {
      this.attachListeners();
    } else {
      this.observer = new MutationObserver(() => {
        this.inputEl = target.querySelector('input');
        if (this.inputEl) {
          this.attachListeners();
          this.observer?.disconnect();
        }
      });
      this.observer.observe(target, { childList: true, subtree: true });
    }
  }

  private attachListeners() {
    if (!this.inputEl) return;
    this.inputEl.addEventListener('keydown', this.handleKeyDown);
    this.inputEl.addEventListener('input', this.handleInput);
    this.inputEl.addEventListener('paste', this.handlePaste);
  }

  private detachListeners() {
    if (!this.inputEl) return;
    this.inputEl.removeEventListener('keydown', this.handleKeyDown);
    this.inputEl.removeEventListener('input', this.handleInput);
    this.inputEl.removeEventListener('paste', this.handlePaste);
  }

  ngOnDestroy() {
    this.observer?.disconnect();
    this.detachListeners();
  }

  private onKeyDown(event: KeyboardEvent) {
    const allowedKeys = [
      'Backspace', 'Tab', 'Delete', 'Escape', 'Enter',
      'ArrowLeft', 'ArrowRight', 'Home', 'End'
    ];

    if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) {
      return;
    }

    if (!/^[0-9]$/.test(event.key)) {
      event.preventDefault();
    }
  }

  private onInput(event: Event) {
    if (!this.inputEl) return;

    const rawValue = this.inputEl.value;
    const digits = rawValue.replace(/\D/g, '').substring(0, 8);

    let formatted = '';
    if (digits.length > 0) formatted = digits.substring(0, 2);
    if (digits.length > 2) formatted += '/' + digits.substring(2, 4);
    if (digits.length > 4) formatted += '/' + digits.substring(4, 8);

    if (rawValue !== formatted) {
      this.inputEl.value = formatted;

      // Eğer Angular NgControl varsa model değerini doğrudan güncelle
      if (this.ngControl && this.ngControl.control) {
        this.ngControl.control.setValue(formatted, { emitEvent: true });
      } else {
        this.inputEl.dispatchEvent(new Event('input', { bubbles: true }));
      }
    }
  }

  private onPaste(event: ClipboardEvent) {
    if (!this.inputEl) return;
    event.preventDefault();

    const pasted = event.clipboardData?.getData('text') ?? '';
    const digits = pasted.replace(/\D/g, '').substring(0, 8);

    if (digits) {
      this.inputEl.value = digits;
      this.onInput(event);
    }
  }
}