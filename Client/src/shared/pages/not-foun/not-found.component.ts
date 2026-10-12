import { Location } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal, ViewEncapsulation } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { interval } from 'rxjs';

@Component({
  selector: 'app-not-foun',
  imports: [
    RouterLink
  ],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class NotFoundComponent {
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  private readonly title = inject(Title);
  private readonly destroyRef = inject(DestroyRef);

  /** Geri sayım süresi (saniye) */
  private readonly redirectDelay = 10;

  /** Kalan süre — signal ile reaktif */
  readonly countdown = signal(this.redirectDelay);

  /** Kullanıcı beklemeyi iptal etti mi? */
  readonly cancelled = signal(false);

  ngOnInit(): void {
    this.title.setTitle('404 — Sayfa Bulunamadı');

    // interval(1000)
    //   .pipe(takeUntilDestroyed(this.destroyRef))
    //   .subscribe(() => {
    //     if (this.cancelled()) return;

    //     const next = this.countdown() - 1;
    //     this.countdown.set(next);

    //     if (next <= 0) {
    //       this.router.navigateByUrl('/');
    //     }
    //   });
  }

  goBack(): void {
    this.location.back();
  }

  cancelRedirect(): void {
    this.cancelled.set(true);
  }
}
