using System.ComponentModel.DataAnnotations;
namespace Core.Application.DTO.Categories;
public sealed class UpdateCategoryDto { [Required, StringLength(100)] public string Name { get; init; } = string.Empty; }
