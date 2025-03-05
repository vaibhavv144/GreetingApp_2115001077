using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IGreetingRL
    {
        GreetEntity AddGreetingRL(GreetingModel greetingModel);
        public GreetingModel GetGreetingByIdRL(int Id);
    }
}
