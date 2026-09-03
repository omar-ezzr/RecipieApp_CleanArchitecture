import { CreateRecipe, DifficultyLevel, Recipe } from '../../models/recipe.model';

export interface RecipeFormResult {
  payload?: CreateRecipe;
  error?: string;
}

/** Maps recipe form state only; components retain HTTP, navigation, and messaging. */
export class RecipeFormMapper {
  static empty(): CreateRecipe {
    return {
      title: '', description: '', preparationTimeMinutes: 30, categoryId: '', cuisineId: '', regionId: null,
      difficulty: DifficultyLevel.Easy, imageUrl: null, traditionalName: null, originDescription: null,
      isTraditional: false, servingOccasion: null, ingredients: [{ name: '', quantity: '' }],
      steps: [{ stepNumber: 1, instruction: '' }]
    };
  }

  static fromRecipe(recipe: Recipe): CreateRecipe {
    return {
      title: recipe.title, description: recipe.description, preparationTimeMinutes: recipe.preparationTimeMinutes,
      categoryId: recipe.categoryId, cuisineId: recipe.cuisineId, regionId: recipe.regionId || null,
      difficulty: recipe.difficulty, imageUrl: recipe.imageUrl || null, traditionalName: recipe.traditionalName || null,
      originDescription: recipe.originDescription || null, isTraditional: recipe.isTraditional,
      servingOccasion: recipe.servingOccasion || null,
      ingredients: recipe.ingredients.length ? recipe.ingredients.map(({ name, quantity }) => ({ name, quantity })) : [{ name: '', quantity: '' }],
      steps: recipe.steps.length ? recipe.steps.map(({ stepNumber, instruction }) => ({ stepNumber, instruction })) : [{ stepNumber: 1, instruction: '' }]
    };
  }

  static toPayload(form: CreateRecipe): RecipeFormResult {
    const title = form.title.trim();
    if (!title) return { error: 'Please enter a recipe title.' };
    const description = form.description.trim();
    if (!description) return { error: 'Please add a description.' };
    if (!form.categoryId) return { error: 'Please select a category.' };
    if (!form.cuisineId) return { error: 'Please select a cuisine.' };
    const preparationTimeMinutes = Number(form.preparationTimeMinutes);
    if (!Number.isFinite(preparationTimeMinutes) || preparationTimeMinutes <= 0) return { error: 'Preparation time must be greater than 0 minutes.' };
    const ingredients = form.ingredients.map(({ name, quantity }) => ({ name: name.trim(), quantity: quantity?.trim() ?? '' }));
    if (!ingredients.length || ingredients.some(ingredient => !ingredient.name)) return { error: 'Please enter a name for every ingredient.' };
    const steps = form.steps.map((step, index) => ({ stepNumber: index + 1, instruction: step.instruction.trim() }));
    if (!steps.length || steps.some(step => !step.instruction)) return { error: 'Please add an instruction for every preparation step.' };
    return { payload: {
      title, description, preparationTimeMinutes, categoryId: form.categoryId, cuisineId: form.cuisineId,
      regionId: form.regionId || null, difficulty: form.difficulty, imageUrl: this.optional(form.imageUrl),
      traditionalName: this.optional(form.traditionalName), originDescription: this.optional(form.originDescription),
      isTraditional: form.isTraditional, servingOccasion: this.optional(form.servingOccasion), ingredients, steps
    }};
  }

  static renumberSteps(form: CreateRecipe): void {
    form.steps = form.steps.map((step, index) => ({ ...step, stepNumber: index + 1 }));
  }

  private static optional(value: string | null | undefined): string | null {
    return value?.trim() || null;
  }
}
