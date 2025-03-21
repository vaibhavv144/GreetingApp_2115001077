﻿
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.EntityFrameworkCore;
//using RepositoryLayer.Interface;
//using RepositoryLayer.Contexts;
//using ModelLayer.Model;
//using ModelLayer.Entity;
//using NLog;

//namespace RepositoryLayer.Service
//{
//    /// <summary>
//    /// GreetingRL Class inheriting IGreetingRL
//    /// </summary>
//    public class GreetingRL : IGreetingRL
//    {
//        /// <summary>
//        /// Creating DBContext of GreetingApp
//        /// </summary>
//        private readonly GreetingAppContext _dbContext;
//        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        /// <summary>
//        /// Creating GreetingRL constructor
//        /// </summary>
//        /// <param name="dbContext"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public GreetingRL(GreetingAppContext dbContext)
//        {
//            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//        }

//        // UC4 - Save Greeting
//        public GreetEntity SaveGreetingRL(GreetingModel greetingModel)
//        {
//            try
//            {
//                bool userExists = _dbContext.Users.Any(u => u.UserId == greetingModel.Uid);
//                if (!userExists)
//                {
//                    Logger.Warn("User with ID: {0} not found. Cannot save greeting.", greetingModel.Uid);
//                    return null;
//                }

//                var newMessage = new GreetEntity
//                {
//                    Message = greetingModel.Message,
//                    UserId = greetingModel.Uid // Ensure UserId is assigned
//                };

//                _dbContext.Greet.Add(newMessage);
//                _dbContext.SaveChanges();

//                Logger.Info("Greeting saved successfully with ID: {0}", newMessage.Id);
//                return newMessage;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex, "Error while saving greeting.");
//                throw;
//            }
//        }

//        // UC5 - Get Greeting by ID
//        public GreetingModel GetGreetingByIdRL(int Id)
//        {
//            try
//            {
//                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == Id);
//                if (entity != null)
//                {
//                    Logger.Info("Greeting fetched successfully for ID: {0}", Id);
//                    return new GreetingModel
//                    {
//                        Id = entity.Id,
//                        Message = entity.Message,
//                        Uid = entity.UserId // Ensure Uid is included
//                    };
//                }
//                Logger.Warn("Greeting not found for ID: {0}", Id);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex, "Error while fetching greeting by ID: {0}", Id);
//                throw;
//            }
//        }

//        // UC6 - Get All Greetings
//        public List<GreetEntity> GetAllGreetingsRL()
//        {
//            try
//            {
//                var greetings = _dbContext.Greet.ToList();
//                Logger.Info("Fetched all greetings successfully. Total Count: {0}", greetings.Count);
//                return greetings;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex, "Error while fetching all greetings.");
//                throw;
//            }
//        }

//        // UC7 - Edit Greeting
//        public GreetEntity EditGreetingRL(int id, GreetingModel greetingModel)
//        {
//            try
//            {
//                bool userExists = _dbContext.Users.Any(u => u.UserId == greetingModel.Uid);
//                if (!userExists)
//                {
//                    Logger.Warn("User with ID: {0} not found. Cannot update greeting.", greetingModel.Uid);
//                    return null;
//                }

//                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
//                if (entity != null)
//                {
//                    entity.Message = greetingModel.Message;
//                    entity.UserId = greetingModel.Uid; // Ensure UserId is updated

//                    _dbContext.Greet.Update(entity);
//                    _dbContext.SaveChanges();

//                    Logger.Info("Greeting updated successfully for ID: {0}", id);
//                    return entity;
//                }

//                Logger.Warn("Greeting not found for ID: {0} to update", id);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex, "Error while updating greeting with ID: {0}", id);
//                throw;
//            }
//        }

//        // UC8 - Delete Greeting
//        public bool DeleteGreetingRL(int id)
//        {
//            try
//            {
//                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
//                if (entity != null)
//                {
//                    _dbContext.Greet.Remove(entity);
//                    _dbContext.SaveChanges();
//                    Logger.Info("Greeting deleted successfully for ID: {0}", id);
//                    return true;
//                }
//                Logger.Warn("Greeting not found for ID: {0} to delete", id);
//                return false;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex, "Error while deleting greeting with ID: {0}", id);
//                throw;
//            }
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Interface;
using RepositoryLayer.Contexts;
using ModelLayer.Model;
using ModelLayer.Entity;
using NLog;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace RepositoryLayer.Service
{
    public class GreetingRL : IGreetingRL
    {
        private readonly GreetingAppContext _dbContext;
        private readonly IDatabase _redisDb;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GreetingRL(GreetingAppContext dbContext, IConnectionMultiplexer redisConnection)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _redisDb = redisConnection.GetDatabase();
        }

        public GreetEntity SaveGreetingRL(GreetingModel greetingModel)
        {
            try
            {
                bool userExists = _dbContext.Users.Any(u => u.UserId == greetingModel.Uid);
                if (!userExists)
                {
                    Logger.Warn("User with ID: {0} not found. Cannot save greeting.", greetingModel.Uid);
                    return null;
                }

                var newMessage = new GreetEntity
                {
                    Message = greetingModel.Message,
                    UserId = greetingModel.Uid
                };

                _dbContext.Greet.Add(newMessage);
                _dbContext.SaveChanges();

                // Invalidate cache for GetAllGreetingsRL
                _redisDb.KeyDelete("all_greetings");

                Logger.Info("Greeting saved successfully with ID: {0}", newMessage.Id);
                return newMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while saving greeting.");
                throw;
            }
        }

        public GreetingModel GetGreetingByIdRL(int Id)
        {
            try
            {
                string cacheKey = $"greeting:{Id}";
                var cachedGreeting = _redisDb.StringGet(cacheKey);

                if (!cachedGreeting.IsNullOrEmpty)
                {
                    Logger.Info("Greeting fetched from cache for ID: {0}", Id);
                    return JsonConvert.DeserializeObject<GreetingModel>(cachedGreeting);
                }

                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == Id);
                if (entity != null)
                {
                    var greetingModel = new GreetingModel
                    {
                        Id = entity.Id,
                        Message = entity.Message,
                        Uid = entity.UserId
                    };

                    // Store data in Redis cache
                    _redisDb.StringSet(cacheKey, JsonConvert.SerializeObject(greetingModel), TimeSpan.FromMinutes(10));

                    Logger.Info("Greeting fetched successfully for ID: {0}", Id);
                    return greetingModel;
                }

                Logger.Warn("Greeting not found for ID: {0}", Id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching greeting by ID: {0}", Id);
                throw;
            }
        }

        public List<GreetEntity> GetAllGreetingsRL()
        {
            try
            {
                var cachedGreetings = _redisDb.StringGet("all_greetings");

                if (!cachedGreetings.IsNullOrEmpty)
                {
                    Logger.Info("Fetched greetings from cache.");
                    return JsonConvert.DeserializeObject<List<GreetEntity>>(cachedGreetings);
                }

                var greetings = _dbContext.Greet.ToList();

                // Store data in Redis cache
                _redisDb.StringSet("all_greetings", JsonConvert.SerializeObject(greetings), TimeSpan.FromMinutes(10));

                Logger.Info("Fetched all greetings successfully. Total Count: {0}", greetings.Count);
                return greetings;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching all greetings.");
                throw;
            }
        }

        public GreetEntity EditGreetingRL(int id, GreetingModel greetingModel)
        {
            try
            {
                bool userExists = _dbContext.Users.Any(u => u.UserId == greetingModel.Uid);
                if (!userExists)
                {
                    Logger.Warn("User with ID: {0} not found. Cannot update greeting.", greetingModel.Uid);
                    return null;
                }

                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
                if (entity != null)
                {
                    entity.Message = greetingModel.Message;
                    entity.UserId = greetingModel.Uid;

                    _dbContext.Greet.Update(entity);
                    _dbContext.SaveChanges();

                    // Invalidate cache
                    _redisDb.KeyDelete($"greeting:{id}");
                    _redisDb.KeyDelete("all_greetings");

                    Logger.Info("Greeting updated successfully for ID: {0}", id);
                    return entity;
                }

                Logger.Warn("Greeting not found for ID: {0} to update", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while updating greeting with ID: {0}", id);
                throw;
            }
        }

        public bool DeleteGreetingRL(int id)
        {
            try
            {
                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
                if (entity != null)
                {
                    _dbContext.Greet.Remove(entity);
                    _dbContext.SaveChanges();

                    // Invalidate cache
                    _redisDb.KeyDelete($"greeting:{id}");
                    _redisDb.KeyDelete("all_greetings");

                    Logger.Info("Greeting deleted successfully for ID: {0}", id);
                    return true;
                }

                Logger.Warn("Greeting not found for ID: {0} to delete", id);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while deleting greeting with ID: {0}", id);
                throw;
            }
        }
    }
}
