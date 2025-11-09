using JobScheduler.Domain.Entities;
using System.Security.Claims;

namespace JobScheduler.Application.Common.Interfaces.Identity
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(Guid userId);

        ClaimsPrincipal? ValidateAccessToken(string token);

        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);

        Task<RefreshToken> RotateRefreshTokenAsync(Guid userId, string oldToken);

        Task RevokeRefreshTokenAsync(string refreshToken);

        Task RevokeAccessTokensAsync(Guid userId);
    }
}