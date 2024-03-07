using Microsoft.EntityFrameworkCore;
using TwoFactorAuth.Data;
using TwoFactorAuth.Models;

namespace TwoFactorAuth.Services
{
    public interface IUserService
    {
        Task<User> GetUserByUsername(string username);
        Task<bool> Login(User user);
        Task<bool> IsTwoFactorEnabled(string username);
        Task<string> GetSecretKeyFor2FA(string username);
        Task<bool> RegisterUser(User user);
        Task<bool> SaveUserScrectKeyFor2FA(string username, string screctKey);
        Task<bool> DisableTwoFactorAuth(string username);
    }
    public class UserService : IUserService
    {
        private AppDBContext _dbContext;
        public UserService(AppDBContext appDBContext)
        {
            _dbContext = appDBContext;
        }
        public async Task<User> GetUserByUsername(string username)
        {
            try
            {
                var res = await _dbContext.Users.FirstOrDefaultAsync(m => m.Username == username );
                return res;

            }
            catch (Exception ex)
            {
                return new User();
            }
        }

        public async Task<bool> Login(User user)
        {
            try
            {
                var res = await _dbContext.Users.FirstOrDefaultAsync(m => m.Username == user.Username && m.Password == user.Password);
                if (res != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public async Task<bool> IsTwoFactorEnabled(string username)
        {
            var res = await _dbContext.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (res != null)
            {
                return res.IsTwoFactorEnabled;
            }
            else
            {
                return false;
            }
        }
        public async Task<string> GetSecretKeyFor2FA(string username)
        {
            var res = await _dbContext.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (res != null)
            {
                return res.SecretKey;
            }
            else
            {
                return null;
            }
        }
        public async Task<bool> RegisterUser(User user)
        {
            try
            {
                bool isExist = await _dbContext.Users.AnyAsync(u => u.Username == user.Username);
                if (isExist)
                {
                    return false;
                }
                var res = await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public async Task<bool> SaveUserScrectKeyFor2FA(string username, string screctKey)
        {
            try
            {
                var userDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (userDb != null)
                {
                    userDb.SecretKey = screctKey;
                    userDb.IsTwoFactorEnabled = true;
                    var res = _dbContext.Users.Update(userDb);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public async Task<bool> DisableTwoFactorAuth(string username)
        {
            try
            {
                var userDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (userDb != null)
                {
                    userDb.SecretKey = string.Empty;
                    userDb.IsTwoFactorEnabled = false;
                    var res = _dbContext.Users.Update(userDb);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
