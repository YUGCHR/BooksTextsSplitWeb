﻿using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BooksTextsSplit.Library.Models;
using BooksTextsSplit.Library.Services;

namespace BooksTextsSplit.Library.Helpers
{   // this.AuthenticationWithToken was changed on AuthenticationWithCoockie
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>        
    {
        private readonly IAuthService _authService;
        private UserData _context;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAuthService authService,
            UserData context)
            : base(options, logger, encoder, clock)
        {            
            _authService = authService;
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // skip authentication if endpoint has [AllowAnonymous] attribute
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();

            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            UserData user = null;
            try
            {
                // var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                // var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                // var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                // var email = credentials[0];
                // var password = credentials[1];
                // user = await _authService.Authenticate(email, password);

                // var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);                
                // var fetchToken = authHeader.Parameter;
                var fetchToken = Request.Headers["Authorization"];
                user = await _authService.AuthByToken(fetchToken);
                _context.Id = user.Id;
                _context.FirstName = user.FirstName;
                _context.LastName = user.LastName;
                _context.Username = user.Username;
                _context.Email = user.Email;
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
