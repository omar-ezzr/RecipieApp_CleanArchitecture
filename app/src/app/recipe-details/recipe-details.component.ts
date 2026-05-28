import { Component, OnInit } from '@angular/core';

import { CommonModule } from '@angular/common';

import { ActivatedRoute } from '@angular/router';

import { RecipeService } from '../services/recipe.service';

import { Recipe } from '../models/recipe.model';


@Component({
  selector: 'app-recipe-details',

  standalone: true,

  imports: [CommonModule],

  templateUrl: './recipe-details.component.html',

  styleUrl: './recipe-details.component.css'
})

export class RecipeDetailsComponent
implements OnInit {

  recipe?: Recipe;

  isLoading = true;


  constructor(

    private route: ActivatedRoute,

    private recipeService: RecipeService

  ) {}


  ngOnInit(): void {

    const id =
      this.route.snapshot.paramMap.get('id');

    if (!id) return;

    this.recipeService.getById(id)
      .subscribe({

        next: (data) => {

          this.recipe = data;

          this.isLoading = false;
        },

        error: () => {

          this.isLoading = false;
        }
      });
  }
}