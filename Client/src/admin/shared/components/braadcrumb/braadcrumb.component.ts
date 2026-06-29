import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';

@Component({
  selector: 'app-braadcrumb',
  imports: [
    RouterLink,
    NgClass
],
  templateUrl: './braadcrumb.component.html',
  styleUrl: './braadcrumb.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BraadcrumbComponent {
  readonly breadCrumbService = inject(BreadcrumbService);
}
