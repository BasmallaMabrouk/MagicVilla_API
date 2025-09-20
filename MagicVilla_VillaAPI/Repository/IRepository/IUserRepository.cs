using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.Identity.Data;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDTO>Login(LoginRequestDTO loginRequest);
        Task<LocalUser>Register(RegisterationRequestDTO registerationRequest);  
    }
}
