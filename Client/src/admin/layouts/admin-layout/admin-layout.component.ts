import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminNavbarComponent } from './admin-navbar/admin-navbar.component';
import { AdminSidebarComponent } from './admin-sidebar/admin-sidebar.component';

@Component({
  selector: 'app-admin-layout',
  imports: [
    RouterOutlet,
    AdminNavbarComponent,
    AdminSidebarComponent
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent {

  // Menünün varsayılan olarak açık gelmesini sağlıyoruz
  isSidebarCollapsed = signal<boolean>(false);
  // Navbar'daki menü butonuna basıldığında tetiklenecek fonksiyon
  toggleSidebar(): void {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed())
  }



}
