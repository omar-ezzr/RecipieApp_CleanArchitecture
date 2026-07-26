import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-recipe-card-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article class="recipe-card-skeleton" *ngFor="let item of items" aria-hidden="true">
      <div class="sk-image"></div>
      <div class="sk-body">
        <span></span>
        <strong></strong>
        <p></p>
        <p class="short"></p>
      </div>
    </article>
  `,
  styleUrl: './recipe-card-skeleton.component.css'
})
export class RecipeCardSkeletonComponent {
  @Input() count = 6;

  get items(): number[] {
    return Array.from({ length: this.count }, (_, index) => index);
  }
}
