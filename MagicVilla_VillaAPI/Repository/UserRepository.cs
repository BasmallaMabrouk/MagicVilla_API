using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private string SecretKey;
        public UserRepository(ApplicationDbContext db , IConfiguration configuration)
        {
            _db = db;
            SecretKey = configuration.GetValue<string>("ApiSetting:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.LocalUsers.FirstOrDefault(u => u.username == username);
            if(user == null)
            {
                return true;
            }
            return false;   
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequest)
        {
            var user =await _db.LocalUsers.FirstOrDefaultAsync(u => u.username.ToLower() == loginRequest.userName.ToLower() && u.Password == loginRequest.Password);
            if (user == null) {
                return new LoginResponseDTO() { 
                    Token="",
                    User=null 
                };
                    }
            //genrate token
            var TokenHandeler = new JwtSecurityTokenHandler();
            var Key = Encoding.ASCII.GetBytes(SecretKey);
            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim (ClaimTypes.Name ,user.Id.ToString()),
                    new Claim(ClaimTypes.Role ,user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
               SigningCredentials =new(new SymmetricSecurityKey(Key),SecurityAlgorithms.HmacSha256Signature)
            };
            var Token = TokenHandeler.CreateToken(TokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                Token = TokenHandeler.WriteToken(Token),
                User = user
            };
            return loginResponseDTO;
        }

        public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequest)
        {
            LocalUser user = new ()
            {
                Name = registerationRequest.Name,
                username = registerationRequest.UserName,
                Password = registerationRequest.Password,
                Role = registerationRequest.Role 
            };
            _db.LocalUsers.Add(user);
           await _db.SaveChangesAsync();
            user.Password = "";
            return user;
        }
    }
}
