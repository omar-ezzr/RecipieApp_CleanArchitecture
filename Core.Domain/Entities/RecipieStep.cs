using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class RecipieStep : BaseEntity
    {
public int StepNumber { get; set; }
    public string Instruction { get; set; } = default!;

    public Guid RecipeId { get; set; }
    }
}