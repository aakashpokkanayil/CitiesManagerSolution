using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CitiesManager.Core.DTO;
using Microsoft.AspNetCore.Identity;
using CitiesManager.Core.Domain.Identity;
using CitiesManager.Core.ServiceContracts;
using System.Security.Claims;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDto register)
        {
            if (!ModelState.IsValid)
            {
                string errorMsg=string.Join("|",ModelState.Values.SelectMany(x=>x.Errors).Select(x=>x.ErrorMessage));
                return Problem(errorMsg);
            }
            ApplicationUser applicationUser = new()
            {
                Email=register.Email,
                PhoneNumber=register.Phone,
                UserName=register.Email,
                PersonName=register.PersonName,
            };

            IdentityResult result = await _userManager.CreateAsync(applicationUser,register.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                AuthResponseDto authResponse= _jwtService.CreateJwtToken(applicationUser);
                applicationUser.RefreshToken=authResponse.RefreshToken;
                applicationUser.RefreshTokenExpiration=authResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(applicationUser);
                return Ok(authResponse);
            }
            else
            {
                string errorMsg=string.Join("|",result.Errors.Select(x=>x.Description));
                return Problem(errorMsg);
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser applicationUser= await _userManager.FindByEmailAsync(email);
            if (applicationUser == null)
            {
                return Ok(false);
            }
            return Ok(true);
            
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApplicationUser>> Login(LoginDto login)
        {

            if (!ModelState.IsValid)
            {
                string errorMsg = string.Join("|", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Problem(errorMsg);
            }

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(login.Email);
                if (user == null)
                {
                    return NoContent();
                }
                AuthResponseDto authResponse = _jwtService.CreateJwtToken(user);
                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpiration = authResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(user);
                return Ok(authResponse);
            }
            else
            {
                return Problem("Invalid Email or Password.");
            }

            
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost("generate-new-token")]
        public async Task<IActionResult> GenerateNewToken(TokenModelDto token)
        {
            if (token == null)
            {
                return BadRequest();
            }
            ClaimsPrincipal? claimsPrincipal = _jwtService.GetClaimsPrincipalFromToken(token.JwtToken);
            if (claimsPrincipal == null)
            {
                return BadRequest("Invalid Token");
            }
            string? email=claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            //ClaimTypes.NameIdentifier is email which we set while creating token.
            ApplicationUser? user= await _userManager.FindByNameAsync(email);
            if (user == null || user.RefreshToken!=token.RefreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                return BadRequest("Invalid Refresh Token");
            }

            AuthResponseDto authResponse= _jwtService.CreateJwtToken(user);

            user.RefreshToken=authResponse.RefreshToken;
            user.RefreshTokenExpiration=authResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return Ok(authResponse);
        }

    }
}
