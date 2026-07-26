import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="empty-state" aria-live="polite">
      <p class="ui-label empty-kicker" *ngIf="kicker">{{ kicker }}</p>
      <h2>{{ title }}</h2>
      <p>{{ message }}</p>
      <a *ngIf="actionLink && actionLabel" class="btn btn-primary-app" [routerLink]="actionLink">
        {{ actionLabel }}
      </a>
    </section>
  `,
  styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {
  @Input() kicker = '';
  @Input({ required: true }) title = '';
  @Input({ required: true }) message = '';
  @Input() actionLabel = '';
  @Input() actionLink: string | unknown[] | null = null;
}
