import { Directive, effect, inject, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { PermissionService } from '../../core/services/permission.service';

@Directive({
  selector: '[appHasPermissionDirective]'
})
  // herhangi bir sayfada(örneğin araç ekleme sayfası) yetkisi olmayan butonu şu şekilde gizleyebilirsin:
export class HasPermissionDirective {
  private permissionService = inject(PermissionService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private currentPermission: string = '';

  @Input() set hasPermission(permission: string) {
    this.currentPermission = permission;
    this.updateView();
  }

  constructor() {
    // İzinler değiştiğinde (örn: logout/login) UI'ı güncelle
    effect(() => {
      this.permissionService.userPermissions();
      this.updateView();
    });
  }



  // * PRİVATE ---------------------------->

  private updateView() {
    this.viewContainer.clear();
    if (this.permissionService.hasPermission(this.currentPermission)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }

}
