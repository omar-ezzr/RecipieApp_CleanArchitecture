import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  isMenuOpen = false;
  unreadCount = 0;

  constructor(
    private router: Router,
    private auth: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadUnreadCount();
  }

  @HostListener('document:keydown.escape')
  closeOnEscape(): void {
    this.closeMenu();
  }

  isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  displayName(): string {
    return this.auth.getCurrentDisplayName() ?? 'Your kitchen';
  }

  profileLink(): string[] {
    const userId = this.auth.getCurrentUserId();
    return userId ? ['/users', userId] : ['/recipes'];
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  logout() {
    this.auth.logout();
    this.unreadCount = 0;
    this.closeMenu();
    this.router.navigate(['/login']);
  }

  private loadUnreadCount(): void {
    if (!this.auth.isLoggedIn()) {
      this.unreadCount = 0;
      return;
    }

    this.notificationService.unreadCount().subscribe({
      next: result => this.unreadCount = result.count,
      error: () => this.unreadCount = 0
    });
  }
}
