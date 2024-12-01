using CitiesManager.Core.Domain.Identity;
using CitiesManager.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.ServiceContracts
{
    public interface IJwtService
    {
        AuthResponseDto CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetClaimsPrincipalFromToken(string token);
    }
}
