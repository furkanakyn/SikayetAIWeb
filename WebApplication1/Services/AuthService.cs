using SikayetAIWeb.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SikayetAIWeb.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public User Register(User user, string password)
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Şifre gereklidir");

            if (_context.Users.Any(u => u.Username == user.Username))
                throw new Exception("Bu kullanıcı adı zaten alınmış");

            if (_context.Users.Any(u => u.Email == user.Email))
                throw new Exception("Bu email adresi zaten kayıtlı");

            // Şifre hashleme
            CreatePasswordHash(password, out string passwordHash);

            var newUser = new User
            {
                Username = user.Username,
                PasswordHash = passwordHash,
                Email = user.Email,
                FullName = user.FullName,
                UserType = user.UserType,
                DepartmentId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return newUser;
        }

        public User Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
                throw new Exception("Kullanıcı adı veya şifre hatalı");

            // Admin kullanıcıları normal giriş yapamaz
            if (user.UserType == UserType.admin)
            {
                throw new Exception("Admin kullanıcılar buradan giriş yapamaz");
            }

            return user;
        }

        public bool VerifyAdminPassword(string username, string password)
        {
            try
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == username && u.UserType == UserType.admin);

                if (user == null)
                {
                    _logger.LogWarning($"Admin kullanıcı bulunamadı: {username}");
                    return false;
                }

                return VerifyPasswordHash(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Admin şifre doğrulama hatası: {username}");
                return false;
            }
        }

        public void CreatePasswordHash(string password, out string passwordHash)
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = hmac.Key;
            var passwordHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Hash ve salt'ı birleştirip base64 string olarak sakla
            var combinedHash = new byte[passwordSalt.Length + passwordHashBytes.Length];
            Buffer.BlockCopy(passwordSalt, 0, combinedHash, 0, passwordSalt.Length);
            Buffer.BlockCopy(passwordHashBytes, 0, combinedHash, passwordSalt.Length, passwordHashBytes.Length);

            passwordHash = Convert.ToBase64String(combinedHash);
        }

        public bool VerifyPasswordHash(string password, string storedHash)
        {
            try
            {
                // Base64 geçerlilik kontrolü
                if (string.IsNullOrEmpty(storedHash))
                {
                    _logger.LogWarning("Boş hash değeri");
                    return false;
                }

                // Gerçek dönüşüm
                var combinedHash = Convert.FromBase64String(storedHash);

                // Salt boyutu (HMACSHA512 için 128 byte)
                int saltSize = 128;

                // Minimum uzunluk kontrolü
                if (combinedHash.Length <= saltSize)
                {
                    _logger.LogError("Hash uzunluğu geçersiz");
                    return false;
                }

                // Salt ve hash'i ayır
                var passwordSalt = new byte[saltSize];
                var passwordHash = new byte[combinedHash.Length - saltSize];

                Buffer.BlockCopy(combinedHash, 0, passwordSalt, 0, saltSize);
                Buffer.BlockCopy(combinedHash, saltSize, passwordHash, 0, passwordHash.Length);

                using var hmac = new HMACSHA512(passwordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(passwordHash);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Base64 format hatası");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre doğrulama hatası");
                return false;
            }
        }
    }
}