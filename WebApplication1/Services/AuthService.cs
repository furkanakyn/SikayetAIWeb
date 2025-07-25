using SikayetAIWeb.Models;
using System.Security.Cryptography;
using System.Text;

namespace SikayetAIWeb.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
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

            return user;
        }

        private void CreatePasswordHash(string password, out string passwordHash)
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

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var combinedHash = Convert.FromBase64String(storedHash);

            // Salt ve hash'i ayır (salt ilk 128 byte)
            var passwordSalt = new byte[128];
            var passwordHash = new byte[combinedHash.Length - 128];
            Buffer.BlockCopy(combinedHash, 0, passwordSalt, 0, 128);
            Buffer.BlockCopy(combinedHash, 128, passwordHash, 0, combinedHash.Length - 128);

            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return computedHash.SequenceEqual(passwordHash);
        }
    }
}