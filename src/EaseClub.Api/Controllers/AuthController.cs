using Azure.Core;
using EaseClub.Api.Common.Filters;
using EaseClub.Application.Features.Auth.Commands.Login;
using EaseClub.Application.Features.Auth.Commands.LogOut;
using EaseClub.Application.Features.Auth.Commands.Register;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/auth")]
    [ApiController]
    public class AuthController(ISender sender) : ApiController
    {

       //-------------------------------------------------------------Login----------------------------------------------------
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

        //-------------------------------------------------------------Logout----------------------------------------------------

        [RequireClientTypeHeader] // Optional: ensures X-Client-Type is present
        [MapToApiVersion("1.0")]

        [ProducesResponseType(StatusCodes.Status204NoContent)] // Success
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // Missing header or token
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointSummary("Logout")]
        [EndpointDescription(
            "Logs out the user.\n\n" +
            "Web clients: refresh token is read from cookie and cleared.\n" +
            "Mobile clients: refresh token must be provided in request body.\n" +
            "Returns 204 No Content on success."
            )]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request = null)
        {
            var clientTypeHeader = HttpContext.Items["ClientType"]?.ToString();
            

            bool isWeb = clientTypeHeader!.Equals("Web", StringComparison.OrdinalIgnoreCase);

            string? refreshToken = null;

            if (isWeb)
            {
                // Web: read refresh token from cookie
                Request.Cookies.TryGetValue("refreshToken", out refreshToken);

                // Clear the cookie
                SetRefreshTokenCookie("", DateTime.UtcNow.AddDays(-1));
            }
            else
            {
                // Mobile: token comes in request body
                if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return Problem(new List<Error>
                    {
                        Error.Validation("RefreshTokenMissing", "Refresh token is required for mobile logout.")
                     });
                }
                refreshToken = request.RefreshToken;
            }

            var command = new LogOutCommand(refreshToken!);
            var result = await sender.Send(command);

            return result.Match(
                Success => NoContent(),
                Problem
            );
        }
        //-------------------------------------------------------------Register----------------------------------------------------
        [RequireClientTypeHeader]
        [EndpointName("Register")]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)] // Web+Mobile
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)] // Email exists
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

        [EndpointSummary("Register user ")]
        [EndpointDescription("Register new user to system.\n\n" +
            "Required Header: X-Client-Type: Web | Mobile\n" +
            "If X-Client-Type is missing or invalid, returns 400 Bad Request.\n\n" +
            "Behavior:\n" +
            "- Web clients: receive AccessToken in response body, RefreshToken set as HttpOnly cookie.\n" +
            "- Mobile clients: receive AccessToken + RefreshToken in response body."
            )]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto request)
        {
            var result = await sender.Send(new RegisterCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.PhoneNumber
            ));

            var clientTypeHeader = HttpContext.Items["ClientType"]?.ToString();

            bool isWeb = clientTypeHeader!.Equals("Web", StringComparison.OrdinalIgnoreCase);

            return result.Match(
                 value =>
                 {
                     if (isWeb)
                     {
                         SetRefreshTokenCookie(value.RefreshToken, value.RefreshTokenExpiry.Value);
                         return Ok(new AuthTokensDto(AccessToken: value.AccessToken, null, null));
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
        //-------------------------------------------------------------SetRefreshToken----------------------------------------------------
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
