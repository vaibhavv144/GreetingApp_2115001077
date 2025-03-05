using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Interface;
using RepositoryLayer.Contexts;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Service
{
    public class GreetingRL : IGreetingRL

    {
        private readonly GreetingAppContext _dbContext;

        public GreetingRL(GreetingAppContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        }
        public GreetEntity AddGreetingRL(GreetingModel greetingModel)
        {
            var existingMessage = _dbContext.Greet.FirstOrDefault<GreetEntity>(e => e.Id == greetingModel.Id);
            if (existingMessage == null)
            {
                var newMessage = new GreetEntity
                {
                    

                    Message = greetingModel.Message,
                };
                _dbContext.Greet.Add(newMessage);
                _dbContext.SaveChanges();

                return newMessage;
            }


            return existingMessage;
        }

        //UC5
        public GreetingModel GetGreetingByIdRL(int Id)
        {
            var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == Id);

            if (entity != null)
            {
                return new GreetingModel()
                {
                    Id = entity.Id,
                    Message = entity.Message
                };
            }
            return null;
        }
        
        //UC6
        public List<GreetEntity> GetAllGreetingsRL()
        {
            return _dbContext.Greet.ToList();  // Fetching All Data from Database
        }


        //UC7
        public GreetEntity EditGreetingRL(int id, GreetingModel greetingModel)
        {
            var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
            if (entity != null)
            {
                entity.Message = greetingModel.Message;
                _dbContext.Greet.Update(entity);
                _dbContext.SaveChanges();
                return entity; // Returning the updated Entity
            }
            return null; // If not found
        }

        
        //UC8
        public bool DeleteGreetingRL(int id)
        {
            var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
            if (entity != null)
            {
                _dbContext.Greet.Remove(entity);
                _dbContext.SaveChanges();
                return true; // Successfully Deleted
            }
            return false; // Not Found
        }
    }
}