import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SocialNotification } from '../../models/notification.model';
import { NotificationService } from '../../services/notification.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, RouterModule, EmptyStateComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit {
  notifications: SocialNotification[] = [];
  isLoading = false;

  constructor(
    private notificationService: NotificationService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.notificationService.getNotifications().subscribe({
      next: result => {
        this.notifications = result.items;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Failed to load notifications');
        this.isLoading = false;
      }
    });
  }

  message(notification: SocialNotification): string {
    if (notification.type === 'Follow') {
      return `${notification.actor.displayName} followed you`;
    }

    if (notification.type === 'RecipeLike') {
      return `${notification.actor.displayName} liked ${notification.recipeTitle || 'your recipe'}`;
    }

    return `${notification.actor.displayName} commented on ${notification.recipeTitle || 'your recipe'}`;
  }

  link(notification: SocialNotification): string[] {
    return notification.recipeId
      ? ['/recipes', notification.recipeId]
      : ['/users', notification.actor.id];
  }

  markRead(notification: SocialNotification): void {
    if (notification.isRead) {
      return;
    }

    this.notificationService.markRead(notification.id).subscribe({
      next: () => notification.isRead = true
    });
  }

  markAllRead(): void {
    this.notificationService.markAllRead().subscribe({
      next: () => this.notifications = this.notifications.map(item => ({ ...item, isRead: true })),
      error: () => this.toastr.error('Failed to mark notifications read')
    });
  }

  delete(notification: SocialNotification): void {
    this.notificationService.delete(notification.id).subscribe({
      next: () => this.notifications = this.notifications.filter(item => item.id !== notification.id),
      error: () => this.toastr.error('Failed to delete notification')
    });
  }

  trackNotification(_index: number, notification: SocialNotification): string {
    return notification.id;
  }
}
