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

  /**
/**
 * Durum değişikliği onay dialog'u
 */
  showStatusChangeConfirm(
    targetName: string,
    newStatus: boolean,
    onAccept: () => void,
    onReject?: () => void
  ): void {
    const statusText = newStatus ? 'aktifleştirilecek' : 'pasifleştirilecek';
    const statusLabel = newStatus ? 'Aktif' : 'Pasif';
    const icon = newStatus ? 'ri-checkbox-circle-line' : 'ri-indeterminate-circle-line';

    const confirmationMessage = `
    <div class="dialog-delete-container">
      <div class="target-card" style="border-left-color: ${newStatus ? '#22c55e' : '#ef4444'}">
        <div class="card-value">
          <i class="${icon}" style="color: ${newStatus ? '#22c55e' : '#ef4444'}; background-color: ${newStatus ? '#f0fdf4' : '#fef2f2'};"></i>
          <span class="branch-name-highlight">"${targetName}" <br> paketi ${statusText}</span>
        </div>
      </div>
    </div>
  `;

    this.confirmationServiceDialog.confirm({
      message: confirmationMessage,
      header: 'Durum Değişikliği',
      icon: 'none',
      acceptLabel: `Evet, ${statusLabel}leştir`,
      rejectLabel: 'Vazgeç',
      acceptButtonStyleClass: newStatus ? 'p-button-success p-button-text' : 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-secondary p-button-text',
      accept: () => { onAccept(); },
      reject: () => { if (onReject) onReject(); }
    });
  }
}
