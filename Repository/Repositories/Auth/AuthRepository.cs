using Core.Entities.Auth;
using Core.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly CarFinderDbContext _context;

        public AuthRepository(CarFinderDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByPendingEmailAsync(string pendingEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PendingEmail != null && u.PendingEmail.ToLower() == pendingEmail.ToLower());
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? revokedByIp = null, string? reason = null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = revokedByIp;
            refreshToken.ReasonRevoked = reason;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId, string? revokedByIp = null, string? reason = null)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = revokedByIp;
                token.ReasonRevoked = reason;
            }

            if (activeTokens.Any())
            {
                _context.RefreshTokens.UpdateRange(activeTokens);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EmailOtpToken> SaveEmailOtpAsync(EmailOtpToken emailOtp)
        {
            _context.EmailOtpTokens.Add(emailOtp);
            await _context.SaveChangesAsync();
            return emailOtp;
        }

        public async Task<EmailOtpToken?> GetEmailOtpAsync(string email, string otp)
        {
            return await _context.EmailOtpTokens
                .FirstOrDefaultAsync(eo => eo.Email.ToLower() == email.ToLower() &&
                                          eo.OtpCode == otp &&
                                          !eo.IsUsed &&
                                          eo.ExpiresAt > DateTime.UtcNow);
        }

        public async Task MarkEmailOtpAsUsedAsync(EmailOtpToken emailOtp)
        {
            emailOtp.IsUsed = true;
            emailOtp.UsedAt = DateTime.UtcNow;
            _context.EmailOtpTokens.Update(emailOtp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmailOtpAsync(EmailOtpToken emailOtp)
        {
            _context.EmailOtpTokens.Remove(emailOtp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}