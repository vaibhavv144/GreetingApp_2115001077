
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.DTO;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        //UC10
        UserEntity Registration(RegisterDTO registerDTO);
        UserEntity LoginnUserRL(LoginDTO loginDTO);
    }
}
