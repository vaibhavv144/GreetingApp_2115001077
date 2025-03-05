using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IGreetingBL
    {
        string GetGreetingBL();
        string GetGreeting(string? firstName, string? lastName);

        GreetEntity AddGreetingBL(GreetingModel greetingModel);

        GreetingModel GetGreetingByIdBL(int Id);

        List<GreetingModel> GetAllGreetingsBL();

        GreetingModel EditGreetingBL(int id, GreetingModel greetingModel);

        bool DeleteGreetingBL(int id);


    }
}