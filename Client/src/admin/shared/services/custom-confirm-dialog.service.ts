import { inject, Injectable } from '@angular/core';
import { ConfirmationService } from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class CustomConfirmDialogService {
  private confirmationServiceDialog = inject(ConfirmationService);


  showDeleteConfirm(targetName: string, onAccept: () => void, onReject?: () => void): void {

    const confirmationMessage = `
      <div class="dialog-delete-container">
        <div class="target-card">
          <div class="card-value">
            <i class="ri-delete-bin-5-line"></i>
            <span class="branch-name-highlight">${targetName}</span>
          </div>
        </div>
      </div>
    `;

    this.confirmationServiceDialog.confirm({
      message: confirmationMessage,
      header: 'Silme Onayı',
      icon: 'none',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-secondary p-button-text',

      accept: () => { onAccept() },
      reject: () => { if(onReject) onReject() }
    });
  }
}
