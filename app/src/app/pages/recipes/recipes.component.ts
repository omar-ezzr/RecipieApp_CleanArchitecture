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

  // ========================
  //  DATA
  // ========================
  recipes: Recipe[] = [];
  filteredRecipes: Recipe[] = [];
  categories: Category[] = [];

  // ========================
  //  FILTER STATE
  // ========================
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedDifficulty: string = '';

  // ========================
  //  FORM STATE
  // ========================
  editingId: string | null = null;
  success = '';
  error = '';
  // ========================
  // pagination
  // ========================

  currentPage: number = 1;
pageSize: number = 12;
totalItems: number = 0;
totalPages: number = 0;

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

  // ========================
  // INIT
  // ========================
  ngOnInit() {
    this.loadRecipes();
    this.loadCategories();
  }

loadRecipes() {
  const params = {
    page: this.currentPage,
    pageSize: this.pageSize,
    search: this.searchTerm,
    difficulty: this.selectedDifficulty,
    categoryId: this.selectedCategory // careful: must be ID if backend expects it
  };

  this.recipeService.getPaged(params).subscribe({
    next: (res) => {
      this.recipes = res.items;
      this.filteredRecipes = res.items;

      this.totalItems = res.total;
      this.totalPages = Math.ceil(this.totalItems / this.pageSize);
    },
    error: () => this.error = 'Failed to load recipes'
  });
}

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories = data,
      error: () => this.error = 'Failed to load categories'
    });
  }

  // ========================
  //  FILTER LOGIC (FIXED)
  // ========================
applyFilters() {
  this.currentPage = 1;
  this.loadRecipes();
}
goToPage(page: number) {
  if (page < 1 || page > this.totalPages) return;

  this.currentPage = page;
  this.loadRecipes();
}
  // ========================
  // CREATE
  // ========================
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

  // ========================
  // EDIT
  // ========================
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

  // ========================
  //  DELETE
  // ========================
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

  // ========================
  // RESET
  // ========================
  resetForm() {
    this.newRecipe = {
      title: '',
      description: '',
      preparationTimeMinutes: 0,
      categoryId: '',
      imageUrl: ''
    };
    this.editingId = null;
  }
}