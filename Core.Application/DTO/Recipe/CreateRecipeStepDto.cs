namespace Core.Application.DTO.Recipe;

public sealed class CreateRecipeStepDto
{
    public int StepNumber { get; set; }
    public required string Instruction { get; set; }
}
