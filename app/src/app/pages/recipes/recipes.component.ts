import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateRecipe, Recipe } from '../../models/recipe.model';
import { Category } from '../../models/category.model';
import { RecipeService } from '../../services/recipe.service';
import { CategoryService } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-recipes',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './recipes.component.html',
  styleUrls: ['./recipes.component.css']
})
export class RecipesComponent implements OnInit {

    recipes: Recipe[] = [];
    categories: Category[] = [];
    editingId: string | null = null;
    success = '';
    error = '';
    searchTerm: string = '';
    selectedCategory: string = '';
    filteredRecipes: Recipe[] = [];
    newRecipe: CreateRecipe = {
    title: '',
    description: '',
    preparationTimeMinutes: 0,
    categoryId: '',
    imageUrl: '' 
    
  };

constructor(
  private recipeService: RecipeService,
  private categoryService: CategoryService,
  public auth: AuthService 
  
) {}

  ngOnInit(): void {
    this.loadRecipes();
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories = data,
      error: () => this.error = 'Failed to load categories'
    });
  }

loadRecipes() {
  this.recipeService.getAll().subscribe({
    next: (data) => {
      this.recipes = data;
      this.applyFilters(); // 🔥 important
    },
    error: () => this.error = 'Failed to load recipes'
  });
}
applyFilters() {
  this.filteredRecipes = this.recipes.filter(r => {

    const matchesSearch =
      r.title.toLowerCase().includes(this.searchTerm.toLowerCase());

    const matchesCategory =
      this.selectedCategory === '' || r.categoryId === this.selectedCategory;

    return matchesSearch && matchesCategory;
  });
}
  createRecipe() {
    if (!this.newRecipe.title || !this.newRecipe.categoryId) {
      this.error = 'Title and category are required';
      return;
    }

    this.recipeService.create(this.newRecipe).subscribe({
      next: () => {
        this.success = 'Recipe created ✔';
        this.error = '';
        this.loadRecipes();
        this.resetForm();
      },
      error: () => {
        this.error = 'Failed to create recipe';
        this.success = '';
      }
    });
  }

  startEdit(recipe: Recipe) { 
    
    this.editingId = recipe.id;

    this.newRecipe = {
      title: recipe.title,
      description: recipe.description,
      preparationTimeMinutes: recipe.preparationTimeMinutes,
      categoryId: recipe.categoryId,
      imageUrl: recipe.imageUrl || '' 
      
    };
  }

  updateRecipe() {
    if (!this.editingId) return;

    this.recipeService.update(this.editingId, this.newRecipe).subscribe({
      next: () => {
        this.success = 'Recipe updated ✔';
        this.error = '';
        this.editingId = null;
        this.loadRecipes();
        this.resetForm();
      },
      error: () => this.error = 'Update failed'
    });
  }

  deleteRecipe(id: string) {
    if (!confirm('Delete this recipe?')) return;

    this.recipeService.delete(id).subscribe({
      next: () => {
        this.success = 'Deleted ✔';
        this.loadRecipes();
      },
      error: () => this.error = 'Delete failed'
    });
  }

  resetForm() {
    this.newRecipe = {
      title: '',
      description: '',
      preparationTimeMinutes: 0,
      categoryId: '',
      imageUrl: '' // ✅ reset properly
    };
    this.editingId = null;
  }
}