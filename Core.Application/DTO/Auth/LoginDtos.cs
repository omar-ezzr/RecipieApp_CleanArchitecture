using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DTO.Auth
{
    public class LoginDto
    {
        public string Email {get; set;} = default!;
        public string Password {get; set;} = default!;

    }
}