import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FavoriteRecipe, FavoriteService } from '../../services/favorite.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-saved-recipes',
  standalone: true,
  imports: [CommonModule, RouterModule, EmptyStateComponent],
  templateUrl: './saved-recipes.component.html',
  styleUrl: './saved-recipes.component.css'
})
export class SavedRecipesComponent implements OnInit {
  favorites: FavoriteRecipe[] = [];

  constructor(private favoriteService: FavoriteService) {}

  ngOnInit(): void {
    this.favoriteService.getMine().subscribe({
      next: favorites => this.favorites = favorites
    });
  }

  trackFavorite(_index: number, favorite: FavoriteRecipe): string {
    return favorite.id;
  }
}
