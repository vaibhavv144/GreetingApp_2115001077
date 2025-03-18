
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using ModelLayer.Model;
using ModelLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class GreetingBL : IGreetingBL
    {
        private readonly IGreetingRL _greetingRL;

        public GreetingBL(IGreetingRL greetingRL)
        {
            _greetingRL = greetingRL;
        }

        public string GetGreetingBL()

        {
            return "Hello World";
        }
        //UC3
        public string GetGreeting(string? firstName, string? lastName)
        {

            string greetingMessage;

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                greetingMessage = $"Hello {firstName} {lastName}!";
            }
            else if (!string.IsNullOrEmpty(firstName))
            {
                greetingMessage = $"Hello {firstName}!";
            }
            else if (!string.IsNullOrEmpty(lastName))
            {
                greetingMessage = $"Hello {lastName}!";
            }
            else
            {
                greetingMessage = "Hello, World!";
            }

            return greetingMessage;
        }

        public string GetGreeting()
        {
            throw new NotImplementedException();
        }
        //UC4
        public GreetEntity AddGreetingBL(GreetingModel greetingModel)
        {
                var result = _greetingRL.AddGreetingRL(greetingModel);
                return result;
        }

        //UC5
        public GreetingModel GetGreetingByIdBL(int Id)
        {
            return _greetingRL.GetGreetingByIdRL(Id);
        }

        //UC6

        public List<GreetingModel> GetAllGreetingsBL()
        {
            var entityList = _greetingRL.GetAllGreetingsRL();  // Calling Repository Layer
            if (entityList != null)
            {
                return entityList.Select(g => new GreetingModel
                {
                    Id = g.Id,
                    Message = g.Message
                }).ToList();  // Converting List of Entity to List of Model
            }
            return null;
        }

        //UC7
        public GreetingModel EditGreetingBL(int id, GreetingModel greetingModel)
        {
            var result = _greetingRL.EditGreetingRL(id, greetingModel); // Calling Repository Layer
            if (result != null)
            {
                return new GreetingModel()
                {
                    Id = result.Id,
                    Message = result.Message
                };
            }
            return null;
        }

        //UC8
        public bool DeleteGreetingBL(int id)
        {
            var result = _greetingRL.DeleteGreetingRL(id);
            if (result)
            {
                return true; // Successfully Deleted
            }
            return false; // Not Found
        }



    }
}
