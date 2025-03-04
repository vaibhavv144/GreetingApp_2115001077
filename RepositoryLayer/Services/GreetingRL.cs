﻿using System;
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
    }
}