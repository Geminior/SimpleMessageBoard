namespace SimpleMessageBoard.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Services;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IUserService _authService;

        public AuthController(IUserService authService)
        {
            _authService = authService;
        }

        /// <summary>  Authenticates a user and returns an authentication token for use in further requests.</summary>
        /// <remarks>
        /// Sample:
        ///
        ///     POST /Auth
        ///     {
        ///        "username": "yourusername",
        ///        "password": "yourpassword"
        ///     }
        ///
        /// </remarks>
        /// <param name="creds">The credentials to authenticate.</param>
        /// <returns>A bearer token which can be used to authenticate requests to other services.</returns>
        /// <response code="200">Authentication succeeded and a token was returned.</response>
        /// <response code="401">If authentication failed for whatever reason.</response>
        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(401)]
        public async Task<ActionResult<AuthToken>> Post(Credentials creds)
        {
            var token = await _authService.AuthenticateForToken(creds.UserName, creds.Password);
            if (token == null)
            {
                return Unauthorized();
            }

            return token;
        }

        public class Credentials
        {
            public string UserName { get; set; }

            public string Password { get; set; }
        }
    }
}
