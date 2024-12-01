using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTO
{
    public class TokenModelDto
    {
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }

    }
}
