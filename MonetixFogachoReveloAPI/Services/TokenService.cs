﻿using Microsoft.IdentityModel.Tokens;
using MonetixFogachoReveloAPI.Data.Models;
using MonetixFogachoReveloAPI.Interfaz;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MonetixFogachoReveloAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration) => _configuration = configuration;

        public string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
           new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
           new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
       };
            return GenerateJwtToken(claims);
        }

        public string GenerateJwtToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
