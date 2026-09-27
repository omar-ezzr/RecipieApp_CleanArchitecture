using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class Category : BaseEntity
    {
           public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<Recipie> Recipes { get; set; } = [];
    }
}