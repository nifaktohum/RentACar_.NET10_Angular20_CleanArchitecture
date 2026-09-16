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

  // * Vitrin resmini degiştir Onay 
  showIsMainImageConfirm(targetName: string, onAccept: () => void, onReject?: () => void): void {

    const confirmationMessage = `
      <div class="dialog-IsMain-container">
        <div class="target-card">
          <div class="card-value">
            <i class="ri-information-line"></i>
            <span class="ismain-highlight">${targetName}</span>
          </div>
        </div>
      </div>
    `;

    this.confirmationServiceDialog.confirm({
      message: confirmationMessage,
      header: 'Vitrin Resmi Onay',
      icon: 'none',
      acceptLabel: 'Evet',
      rejectLabel: 'Hayır',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-secondary p-button-text',

      accept: () => { onAccept() },
      reject: () => { if(onReject) onReject() }
    });
  }

 // * Durum değişikliği onay dialog'u
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


  /**
   * ✅ YENİ METOD: Genel Onay Dialog'u
   * @param title - Dialog başlığı
   * @param message - Dialog mesajı
   * @param onAccept - Kabul edildiğinde çalışacak fonksiyon
   * @param onReject - Reddedildiğinde çalışacak fonksiyon (opsiyonel)
   * @param acceptLabel - Kabul butonu metni (opsiyonel, varsayılan: 'Evet')
   * @param rejectLabel - Reddet butonu metni (opsiyonel, varsayılan: 'Hayır')
   * @param acceptButtonStyle - Kabul butonu stili (opsiyonel, varsayılan: 'p-button-primary')
   */
  showConfirm(
    title: string = 'Onay',
    message: string = 'Bu işlemi onaylıyor musunuz?',
    onAccept: () => void,
    onReject?: () => void,
    acceptLabel: string = 'Evet',
    rejectLabel: string = 'Hayır',
    acceptButtonStyle: string = 'p-button-primary'
  ): void {
    this.confirmationServiceDialog.confirm({
      message: message,
      header: title,
      icon: 'ri-question-line',
      acceptLabel: acceptLabel,
      rejectLabel: rejectLabel,
      acceptButtonStyleClass: `${acceptButtonStyle} p-button-text`,
      rejectButtonStyleClass: 'p-button-secondary p-button-text',
      accept: () => { onAccept(); },
      reject: () => { if (onReject) onReject(); }
    });
  }

  /**
  * ✅ YENİ METOD: Çıkış Onayı Dialog'u (Özel)
  * Kaydedilmemiş değişiklikler için kullanılır
  */
  showExitConfirm(
    onAccept: () => void,
    onReject?: () => void
  ): void {
    this.confirmationServiceDialog.confirm({
      message: 'Bu sayfadan ayrılmak istediğinize emin misiniz? Yapılan değişiklikler kaybolacak.',
      header: 'Değişiklikler Kaydedilmedi',
      icon: 'ri-error-warning-line',
      acceptLabel: 'Evet, Çık',
      rejectLabel: 'Hayır, Kal',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-secondary p-button-text',
      accept: () => { onAccept(); },
      reject: () => { if (onReject) onReject(); }
    });
  }

}
