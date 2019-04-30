namespace SimpleMessageBoard.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Model;
    using SimpleMessageBoard.Services;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IUserService _authService;

        public AuthController(IUserService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(401)]
        public async Task<ActionResult<AuthToken>> Post(BoardUser user)
        {
            var token = await _authService.AuthenticateForToken(user.UserName, user.Password);
            if (token == null)
            {
                return Unauthorized();
            }

            return token;
        }
    }
}
