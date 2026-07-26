import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="page-header">
      <div>
        <p class="ui-label page-kicker" *ngIf="kicker">{{ kicker }}</p>
        <h1 class="page-title">{{ title }}</h1>
        <p class="body-copy" *ngIf="description">{{ description }}</p>
      </div>
      <a *ngIf="actionLink && actionLabel" class="btn btn-primary-app" [routerLink]="actionLink">
        {{ actionLabel }}
      </a>
    </header>
  `,
  styleUrl: './page-header.component.css'
})
export class PageHeaderComponent {
  @Input() kicker = '';
  @Input({ required: true }) title = '';
  @Input() description = '';
  @Input() actionLabel = '';
  @Input() actionLink: string | unknown[] | null = null;
}
