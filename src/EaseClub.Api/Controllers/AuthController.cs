using Azure.Core;
using EaseClub.Api.Common.Filters;
using EaseClub.Application.Features.Auth.Commands.Login;
using EaseClub.Application.Features.Auth.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/auth")]
    [ApiController]
    public class AuthController(ISender sender) : ApiController
    {


        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [RequireClientTypeHeader]

        [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointSummary("Login")]
        [EndpointDescription("Authenticates a user.\n\n" +
            "Required Header: X-Client-Type: Web | Mobile\n" +
            "If X-Client-Type is missing or invalid, returns 400 Bad Request.\n\n" +
            "Behavior:\n" +
            "- Web clients: receive AccessToken in response body, RefreshToken set as HttpOnly cookie.\n" +
            "- Mobile clients: receive AccessToken + RefreshToken in response body."
            )]
        public async Task<IActionResult> Login([FromBody] LoginUserDto request)
        {
            // Get the client type set by the filter
            var clientTypeHeader = HttpContext.Items["ClientType"]?.ToString()!;
            bool isWeb = clientTypeHeader.Equals("Web", StringComparison.OrdinalIgnoreCase);

            var result = await sender.Send(new LoginCommand(request.Email, request.Password));

            return result.Match(
                value =>
                {
                    if (isWeb)
                    {
                        SetRefreshTokenCookie(value.RefreshToken, value.RefreshTokenExpiry.Value);
                        return Ok(new AuthTokensDto(AccessToken: value.AccessToken,null,null));
                    }

                    return Ok(new AuthTokensDto(
                        AccessToken: value.AccessToken,
                        RefreshToken: value.RefreshToken,
                        RefreshTokenExpiry: value.RefreshTokenExpiry
                    ));
                },
                Problem
            );
        }


        private void SetRefreshTokenCookie(string refreshToken, DateTime expiry)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,               // JS cannot access the cookie
                Expires = expiry,              // Expiration matches token
                SameSite = SameSiteMode.Strict,// strict to Protect from CSRF
                Secure = true,                 // Only over HTTPS
                Path = "/"
            };

            Response.Cookies.Append("Refresh-Token", refreshToken, cookieOptions);
        }

    }
}
