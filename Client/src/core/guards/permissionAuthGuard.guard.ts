import { inject } from '@angular/core'
import { CanActivateFn, Router } from '@angular/router'
import { AuthService } from '../services/auth.service' // // Servisinin gerçek yoluna göre burayı ayarlarsın
import { SnackbarService } from '../services/snackbar.service'
import { PermissionService } from '../../admin/core/services/permission.service'

export const permissionAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService)
  const permissionService = inject(PermissionService)
  const snackbarService = inject(SnackbarService)
  const router = inject(Router)

  console.log('----------------------------');
  console.log('Token:', authService.getToken());
  console.log('CurrentUser:', authService.currentUser());
  console.log('isAuthenticated:', authService.isAuthenticated());
  console.log('isTokenExpired:', authService.isTokenExpired());
  console.log('----------------------------');
  
  // // 1. KONTROL: Kullanıcı giriş yapmış mı VE token süresi hala geçerli mi?
  // // Eğer kullanıcı hiç giriş yapmadıysa VEYA token süresi dolduysa içeri almıyoruz!
  if (!authService.isAuthenticated() || authService.isTokenExpired()) {
    // // Eğer süresi dolmuş bir token varsa temizlik yapıp kullanıcıyı öyle yönlendiriyoruz
    authService.logout()
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  // // 2. ADIM: Gidilmek istenen sayfanın (rotanın) tanımındaki gerekli izin kodunu okuyoruz
  const requiredPermission = route.data['requiredPermission'] as string

  // // Eğer sayfaya özel bir izin kısıtı koymadıysak (boşsa), sadece login ve süre kontrolü yetti; geçiş serbest
  if (!requiredPermission) {
    return true
  }

  // Signal tabanlı kontrol:
  if (permissionService.hasPermission(requiredPermission)) {
    return true;
  }

  // // ❌ Kullanıcı sisteme girmiş ve tokenı sağlam ama o sayfayı görmeye (Örn: Silme/Düzenleme) izni yok!
  snackbarService.info('Bu alana erişim yetkiniz bulunmamaktadır!')
  router.navigate(['/admin/dashboard']);
  return false;
}