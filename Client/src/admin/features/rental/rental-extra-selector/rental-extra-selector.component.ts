import { ChangeDetectionStrategy, Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { ExtraService } from '../../../core/services/extra.service';
import { MessageService } from 'primeng/api';
import { SelectedExtra } from '../../../core/models/rental-extra/selected-extra.model';
import { ExtraSummary } from '../../../core/models/extra/extraSummary';
import { ExtraCategoriesConfig, ExtraCategoryLabels, PriceTypeLabels } from '../../../core/models/extra/enum/extra-enums.model';
import { FormsModule } from '@angular/forms';
import { DividerModule } from 'primeng/divider';
import { CurrencyPipe, NgClass } from '@angular/common';
import { CheckboxModule } from 'primeng/checkbox';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-rental-extra-selector',
  imports: [
    FormsModule,
    DividerModule,
    CheckboxModule,
    InputNumberModule,
    ButtonModule

  ],
  templateUrl: './rental-extra-selector.component.html',
  styleUrl: './rental-extra-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RentalExtraSelectorComponent {
  
}
