using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class Ingredient : BaseEntity
    {
            public string Name { get; set; } = default!;
    public string Quantity { get; set; } = default!;

    public Guid RecipeId { get; set; }

}
}