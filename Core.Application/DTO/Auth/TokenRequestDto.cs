using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DTO.Auth
{
    public class TokenRequestDto
    {
            public string RefreshToken { get; set; } = default!;

    }
}