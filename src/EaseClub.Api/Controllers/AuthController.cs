using Azure.Core;

using EaseClub.Api.Common.Filters;
using EaseClub.Application.Features.Auth.Commands.ForgotPassword;
using EaseClub.Application.Features.Auth.Commands.Login;
using EaseClub.Application.Features.Auth.Commands.LogOut;
using EaseClub.Application.Features.Auth.Commands.RefreshToken;
using EaseClub.Application.Features.Auth.Commands.Register;
using EaseClub.Application.Features.Auth.Commands.ResetPassword;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Domain.Common;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/auth")]
    [AllowAnonymous]
    public class AuthController(ISender sender) : ApiController
    {
        private const string RefreshTokenCookieName = "Refresh-Token";

        //-------------------------------------------------------------Login----------------------------------------------------
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [RequireClientType]
        [RequireClientTypeHeader]

        [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("Login")]
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
        
        [HttpPost("logout")]
        [MapToApiVersion("1.0")]
        [RequireClientType]
        [RequireClientTypeHeader] 

        [ProducesResponseType(StatusCodes.Status204NoContent)] // Success
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // Missing header or token
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("Logout")]
        [EndpointSummary("Logout")]
        [EndpointDescription(
            "Logs out the user.\n\n" +
            "Required Header: X-Client-Type: Web | Mobile\n" +
            "If X-Client-Type is missing or invalid, returns 400 Bad Request.\n\n" +
            "Behavior:\n" +
            "- Web clients: refresh token is read from cookie and cleared.\n\n" +
            "- Mobile clients: refresh token must be provided in request body.\n\n" +
            "- Returns 204 No Content on success."
            )]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request = null)
        {
            var clientTypeHeader = HttpContext.Items["ClientType"]?.ToString();
            

            bool isWeb = clientTypeHeader!.Equals("Web", StringComparison.OrdinalIgnoreCase);

            string? refreshToken = null;

            if (isWeb)
            {
                // Web: read refresh token from cookie
                Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshToken);

                // Clear the cookie
                DeleteRefreshTokenCookie();
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

        [HttpPost("register")]
        [MapToApiVersion("1.0")]
        [RequireClientType]
        [RequireClientTypeHeader]

        [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)] // Web+Mobile
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)] // Email exists
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

        [EndpointName("Register")]
        [EndpointSummary("Register user ")]
        [EndpointDescription("Register new user to system.\n\n" +
            "Required Header: X-Client-Type: Web | Mobile\n" +
            "If X-Client-Type is missing or invalid, returns 400 Bad Request.\n\n" +
            "Behavior:\n" +
            "- Web clients: receive AccessToken in response body, RefreshToken set as HttpOnly cookie.\n" +
            "- Mobile clients: receive AccessToken + RefreshToken in response body.\n\n" +
            "Password requirements:\n" +
            "- Minimum length is 10 characters\n" +
            "- At least one uppercase letter\n" +
            "- At least one lowercase letter\n" +
            "- At least one number\n\n" +
            "If password does not meet these requirements, a 400 Bad Request is returned."
            )]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
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

        //-------------------------------------------------------------Refresh------------------------------------------------------

        [HttpPost("refresh")]
        [MapToApiVersion("1.0")]
        [RequireClientType]
        [RequireClientTypeHeader]

        [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

        [EndpointName("Refresh Token")]
        [EndpointSummary("Refresh your old token.")]
        [EndpointDescription("Refresh you tokens.\n\n" +
            "Required Header: X-Client-Type: Web | Mobile\n" +
            "If X-Client-Type is missing or invalid, returns 400 Bad Request.\n\n" +
            "Behavior:\n" +
            "- Web clients: receive AccessToken in response body, RefreshToken set as HttpOnly cookie.\n" +
            "- Mobile clients: receive AccessToken + RefreshToken in response body."
            )]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto? request=null)
        {
            var clientTypeHeader = HttpContext.Items["ClientType"]?.ToString();

            bool isWeb = clientTypeHeader!.Equals("Web", StringComparison.OrdinalIgnoreCase);
            string? refreshToken;

            if (isWeb)
            {
                // Web → read from cookie
                 Request.Cookies.TryGetValue(RefreshTokenCookieName, out refreshToken);
            }
            else
            {
                // Mobile → read from body
                refreshToken = request?.RefreshToken;
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Problem(new List<Error>
                    {
                    Error.Validation("RefreshTokenMissing", "Refresh token is required.")
                    });
            }

            var result = await sender.Send(new RefreshTokenCommand(refreshToken));


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
                 errors =>
                 {
                     DeleteRefreshTokenCookie();
                     return Problem(errors);
                 }
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

            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
        }

        //-------------------------------------------------------------DeleteRefreshToken----------------------------------------------------
        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,               // JS cannot access the cookie,
                Expires = DateTime.UnixEpoch,
                SameSite = SameSiteMode.Strict,// strict to Protect from CSRF
                Secure = true,                 // Only over HTTPS
                Path = "/"
            };

            Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
        }

        //-------------------------------------------------------------ForgotPassword----------------------------------------------------

        [HttpPost("forgot-password")] // Client-type agnostic endpoint (Web & Mobile)
        [MapToApiVersion("1.0")]

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("Forgot-Password")]
        [EndpointSummary("Forgot Password")]
        [EndpointDescription("send request to reset password by email.")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
        {

            var result = await sender.Send(new ForgotPasswordCommand(model.Email));
            return result.Match(
                Success => NoContent(),
                Problem);
        }

        //-------------------------------------------------------------ResetPassword----------------------------------------------------
      
        [HttpPost("reset-password")] // Client-type agnostic endpoint (Web & Mobile)
        [MapToApiVersion("1.0")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("reset-password")]
        [EndpointSummary("reset your password")]
        [EndpointDescription("reset your password by sending new one with token ")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await sender.Send(new ResetPasswordCommand(model));

            return result.Match(
                 Success => Ok(),
                 Problem
                 );
        }
    }
}
