import { Component, Input, ChangeDetectionStrategy } from '@angular/core';


@Component({
    selector: 'app-recipe-card-skeleton',
    imports: [],
    template: `
    @for (item of items; track item) {
      <article class="recipe-card-skeleton" aria-hidden="true">
        <div class="sk-image"></div>
        <div class="sk-body">
          <span></span>
          <strong></strong>
          <p></p>
          <p class="short"></p>
        </div>
      </article>
    }
    `,
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './recipe-card-skeleton.component.css'
})
export class RecipeCardSkeletonComponent {
  @Input() count = 6;

  get items(): number[] {
    return Array.from({ length: this.count }, (_, index) => index);
  }
}
