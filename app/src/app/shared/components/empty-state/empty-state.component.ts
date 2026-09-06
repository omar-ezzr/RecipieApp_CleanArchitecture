import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';


@Component({
    selector: 'app-empty-state',
    imports: [RouterLink],
    template: `
    <section class="empty-state" aria-live="polite">
      @if (kicker) {
        <p class="ui-label empty-kicker">{{ kicker }}</p>
      }
      <h2>{{ title }}</h2>
      <p>{{ message }}</p>
      @if (actionLink && actionLabel) {
        <a class="btn btn-primary-app" [routerLink]="actionLink">
          {{ actionLabel }}
        </a>
      }
    </section>
    `,
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {
  @Input() kicker = '';
  @Input({ required: true }) title = '';
  @Input({ required: true }) message = '';
  @Input() actionLabel = '';
  @Input() actionLink: string | unknown[] | null = null;
}
