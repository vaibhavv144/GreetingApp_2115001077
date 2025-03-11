using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.DTO;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        UserEntity RegistrationBL(RegisterDTO registerDTO);
        UserEntity LoginnUserBL(LoginDTO loginDTO);
    }
}