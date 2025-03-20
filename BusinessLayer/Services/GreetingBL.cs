
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Interface;
using ModelLayer.Model;
using ModelLayer.Entity;
using RepositoryLayer.Interface;
using NLog;

namespace BusinessLayer.Service
{
    /// <summary>
    /// Business Layer class for Greeting functionalities
    /// </summary>
    public class GreetingBL : IGreetingBL
    {
        private readonly IGreetingRL _greetingRL;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor for GreetingBL
        /// </summary>
        /// <param name="greetingRL"></param>
        public GreetingBL(IGreetingRL greetingRL)
        {
            _greetingRL = greetingRL;
        }
        //UC2
        /// <summary>
        /// Returns a simple Hello World message
        /// </summary>
        /// <returns>String</returns>
        public string GetGreetingBL()
        {
            return "Hello World";
        }

        //UC3
        /// <summary>
        /// Generates a greeting message based on provided first and last name
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns>Formatted greeting message</returns>
        public string GetGreeting(string? firstName, string? lastName)
        {
            try
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

                Logger.Info("Generated greeting message: {0}", greetingMessage);
                return greetingMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while generating greeting message.");
                throw;
            }
        }

        //UC4
        public GreetEntity SaveGreetingBL(GreetingModel greetingModel)
        {
            Logger.Info("Saving greeting: {0}", greetingModel.Message);
            var result = _greetingRL.SaveGreetingRL(greetingModel);
            return result;
        }

        //UC5

        public GreetingModel GetGreetingByIdBL(int Id)
        {
            Logger.Info("Fetching greeting by ID: {0}", Id);
            return _greetingRL.GetGreetingByIdRL(Id);
        }





        //UC6
        public List<GreetingModel> GetAllGreetingsBL()
        {
            Logger.Info("Fetching all greetings.");
            var entityList = _greetingRL.GetAllGreetingsRL();
            if (entityList != null)
            {
                return entityList.Select(g => new GreetingModel
                {
                    Id = g.Id,
                    Message = g.Message,
                    Uid = g.UserId // Ensure Uid is included
                }).ToList();
            }
            return null;
        }



        //UC7
        public GreetingModel EditGreetingBL(int id, GreetingModel greetingModel)
        {
            Logger.Info("Editing greeting ID: {0}", id);
            var result = _greetingRL.EditGreetingRL(id, greetingModel);
            if (result != null)
            {
                return new GreetingModel()
                {
                    Id = result.Id,
                    Message = result.Message,
                    Uid = result.UserId // Ensure Uid is returned
                };
            }
            return null;
        }


        //UC8
        public bool DeleteGreetingBL(int id)
        {
            Logger.Info("Deleting greeting ID: {0}", id);
            var result = _greetingRL.DeleteGreetingRL(id);
            return result;
        }
    }
}
