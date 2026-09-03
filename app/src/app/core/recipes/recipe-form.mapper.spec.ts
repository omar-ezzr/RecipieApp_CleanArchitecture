import { RecipeFormMapper } from './recipe-form.mapper';

describe('RecipeFormMapper', () => {
  it('preserves an empty ingredient quantity while normalizing a valid recipe', () => {
    const form = RecipeFormMapper.empty();
    form.title = '  Couscous  ';
    form.description = '  A family recipe  ';
    form.categoryId = 'category-1';
    form.cuisineId = 'cuisine-1';
    form.ingredients = [{ name: '  Semolina ', quantity: '   ' }];
    form.steps = [{ stepNumber: 9, instruction: '  Steam gently.  ' }];

    const result = RecipeFormMapper.toPayload(form);

    expect(result.error).toBeUndefined();
    expect(result.payload?.ingredients).toEqual([{ name: 'Semolina', quantity: '' }]);
    expect(result.payload?.steps).toEqual([{ stepNumber: 1, instruction: 'Steam gently.' }]);
  });
});
