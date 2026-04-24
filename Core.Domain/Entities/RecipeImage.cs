using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class RecipeImage : BaseEntity
    {
    public string Url { get; set; } = default!;
    public bool IsMain { get; set; }

    public Guid RecipeId { get; set; }
    }
}