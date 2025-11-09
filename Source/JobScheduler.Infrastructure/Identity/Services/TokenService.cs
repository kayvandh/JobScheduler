using Framework.Cache.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JobScheduler.Application.Common.Interfaces.Identity;
using JobScheduler.Application.Common.Interfaces.Persistence;
using JobScheduler.Domain.Entities;
using JobScheduler.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobScheduler.Infrastructure.Identity.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ICacheService _cacheService;
        private readonly IJobSchedulerUnitOfWork _unitOfWork;

        private const string CachePrefix = "user:token:";

        public TokenService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ICacheService cacheService,
            IJobSchedulerUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _config = config;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new InvalidOperationException("User not found.");

            var userRoles = await _userManager.GetRolesAsync(user);

            var jti = Guid.NewGuid().ToString("N");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim("display_name", user.DisplayName ?? "")
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15"));

            var jwtToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var cacheKey = $"{CachePrefix}{user.Id}:{jti}";
            await _cacheService.SetAsync(cacheKey, "valid", expires - DateTime.UtcNow);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7"))
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return (accessToken, refreshToken.Token);
        }

        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwt = validatedToken as JwtSecurityToken;
                var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

                if (userId == null || jti == null)
                    return null;

                var cacheKey = $"{CachePrefix}{userId}:{jti}";
                var exists = _cacheService.TryGetValueAsync<string>(cacheKey).Result;
                if (!exists.Found)
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            return await _unitOfWork.RefreshTokens.GetOneAsync(r => r.Token == refreshToken && r.IsActive);
        }

        public async Task<RefreshToken> RotateRefreshTokenAsync(Guid userId, string oldToken)
        {
            var existing = await _unitOfWork.RefreshTokens.GetOneAsync(r => r.Token == oldToken);
            if (existing == null || !existing.IsActive)
                throw new InvalidOperationException("Invalid refresh token.");

            existing.IsRevoked = true;

            var newToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7"))
            };

            await _unitOfWork.RefreshTokens.AddAsync(newToken);
            await _unitOfWork.SaveChangesAsync();

            return newToken;
        }

        public async Task RevokeAccessTokensAsync(Guid userId)
        {
            await _cacheService.RemoveByPrefixAsync($"{CachePrefix}{userId}:");
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _unitOfWork.RefreshTokens.GetOneAsync(r => r.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}