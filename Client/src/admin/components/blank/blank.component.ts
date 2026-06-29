import { DatePipe, Location, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EntityModel } from '../../core/models/entity.model';



@Component({
  selector: 'app-blank-nav',
  imports: [
    NgClass,
    RouterLink,
    DatePipe,
],
  templateUrl: './blank.component.html',
  styleUrl: './blank.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BlankComponent {
  private location = inject(Location);

  readonly pageTitle = input.required<string>();
  readonly pageIcon = input.required<string>();
  readonly pageDescription = input<string>();
  readonly statusActive = input<boolean>(false);
  readonly isActive = input<boolean>(true);
  readonly backBtnActive = input<boolean>(true);
  readonly editBtnActive = input<boolean>(false);
  readonly editBtnUrl = input<string>("");
  readonly auditEntity = input<EntityModel>();
  readonly auditActive = input<boolean>(false);
  readonly reportBtnDown = input<boolean>(false);

  back() {
    this.location.back();
  }

}
