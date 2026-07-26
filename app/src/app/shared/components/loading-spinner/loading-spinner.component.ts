import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `
    <div class="loading-state" role="status" [attr.aria-label]="label">
      <span class="loading-spinner" aria-hidden="true"></span>
      <span>{{ label }}</span>
    </div>
  `,
  styleUrl: './loading-spinner.component.css'
})
export class LoadingSpinnerComponent {
  @Input() label = 'Loading';
}
