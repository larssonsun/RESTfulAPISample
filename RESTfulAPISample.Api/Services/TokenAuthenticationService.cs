﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPISample.Core.DomainModel;
using RESTfulAPISample.Core.Interface;

namespace RESTfulAPISample.Api.Service
{
    public class TokenAuthenticationService : IAuthenticateService
    {
        private readonly IUserRepository _userService;
        private readonly TokenManagement _tokenManagement;
        public TokenAuthenticationService(IUserRepository userService, IOptions<TokenManagement> tokenManagement)
        {
            _userService = userService;
            _tokenManagement = tokenManagement.Value;
        }
        public (bool IsAuthenticated, string Token) IsAuthenticated(LoginRequest request)
        {

            var validateResult = _userService.IsValid(request);
            if (!validateResult.IsValid)
                return (false, null);

            var claims = validateResult.Payload?.Select(x => new Claim(x.Key, x.Value));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                issuer: _tokenManagement.Issuer,
                audience: _tokenManagement.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration),
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return (true, token);

        }
    }
}