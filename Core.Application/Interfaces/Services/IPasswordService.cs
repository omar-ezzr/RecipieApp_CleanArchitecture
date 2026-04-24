using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Services
{
    public interface IPasswordService
    {
        string Hash(string password);
    bool Verify(string password, string hash);
    }
}