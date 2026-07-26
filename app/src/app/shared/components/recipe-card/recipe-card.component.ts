import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { API_BASE_URL } from '../../../app-api.config';
import { DifficultyLevel, Recipe } from '../../../models/recipe.model';
import { resolveAssetUrl } from '../../../core/utils/asset-url.util';

@Component({
  selector: 'app-recipe-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './recipe-card.component.html',
  styleUrl: './recipe-card.component.css'
})
export class RecipeCardComponent {
  @Input({ required: true }) recipe!: Recipe;
  @Input() isFavorite = false;
  @Input() showFavorite = true;
  @Input() canManage = false;
  @Input() showManageActions = true;

  @Output() favoriteToggled = new EventEmitter<Recipe>();
  @Output() edit = new EventEmitter<Recipe>();
  @Output() delete = new EventEmitter<Recipe>();

  difficultyLabel(value: DifficultyLevel): string {
    return value === DifficultyLevel.Easy
      ? 'Easy'
      : value === DifficultyLevel.Medium
        ? 'Medium'
        : 'Hard';
  }

  imageUrl(): string {
    return resolveAssetUrl(this.recipe.imageUrl, API_BASE_URL);
  }
}
