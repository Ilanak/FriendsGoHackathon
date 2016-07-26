﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Security.Policy;
using GameManager;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Shared;

namespace DocDbUtils
{
    public static class DocDbApi
    {
        private const string EndpointUri = "https://friendsgodocdb.documents.azure.com:443/";
        private const string PrimaryKey = "uiEW7oMq9HaaLGkyyoolxm2MIdtbUsi3YBzUb97nAdss2OPlGDU24nsbrxzfeGy2Oze3ve0CuFKS3yeqjLEjwA==";

        private const string DatabaseName = "FriendsGo";
        private const string UsersCollectionName = "Users";
        private const string GroupsCollectionName = "Groups";

        private static DocumentClient client;

        /// <summary>
        ///  creates group in docDB groups collection
        /// </summary>
        /// <param name="group"> new group </param>
        /// <returns>Task</returns>
        public static async Task CreateGroup(Group group)
        {
            await CreateGroupDocumentIfNotExistsAsync(DatabaseName, GroupsCollectionName, group);
        }

        /// <summary>
        ///  creates user in docDB users collection
        /// </summary>
        /// <param name="user"> new user </param>
        /// <returns>Task</returns>
        public static async Task CreateUser(BotUser user)
        {
            await CreateBotUserDocumentIfNotExistsAsync(DatabaseName, UsersCollectionName, user);
        }

        /// <summary>
        ///  returns all groups with same id , or null if no group exisits if this Id
        /// </summary>
        /// <param name="id">groupd id </param>
        /// <returns>selected group if exists</returns>
        public static Group GetGroupById(string id)
        {
            return GetEntityById<Group>(DatabaseName, GroupsCollectionName, id).FirstOrDefault();
        }

        /// <summary>
        ///  returns user with same id , or null if no users exisits if this Id
        /// </summary>
        /// <param name="id">user id </param>
        /// <returns>selected user if exists</returns>
        public static BotUser GetUserById(string id)
        {
            return GetEntityById<BotUser>(DatabaseName, UsersCollectionName, id).FirstOrDefault();
        }

        //public static void UpdateGroup(string telegramId, Group newGroup)
        //{
        //    ReplaceEntity(DatabaseName, GroupsCollectionName, telegramId, newGroup).Wait();
        //}
        //private static async Task ReplaceEntity<T>(string databaseName, string collectionName, string telegramId, T updatedEntity)
        //{
        //    try
        //    {
        //        await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, telegramId), updatedEntity);
        //    }
        //    catch (DocumentClientException de)
        //    {
        //        throw;
        //    }
        //}

        private static List<T> GetEntityById<T>(string databaseName, string collectionName, string entityTelegramId) where T:DocDbEntityBase
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IQueryable<T> query = client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(entity => entity.TelegramId == entityTelegramId);

            return query.ToList();
        }

        public static void InitDocDbConnection()
        {
            try
            {
                //DocDbApi p = new DocDbApi();
                client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }
        private static async Task GetStartedDemo()
        {

            //await this.CreateDatabaseIfNotExists("Users");
            //await this.CreateDocumentCollectionIfNotExistsAsync("FriendsGo", "Users");
            //await this.CreateDocumentCollectionIfNotExistsAsync("FriendsGo", "Groups");

            //var user = new BotUser("testUser");
            //var group = new Group(new Location(0,0));
            //await this.CreateBotUserDocumentIfNotExistsAsync("FriendsGo", "Users", user);
            //await this.CreateGroupDocumentIfNotExistsAsync("FriendsGo", "Groups", group);

        }

        private static void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private static async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseName });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateDocumentCollectionIfNotExistsAsync(string databaseName, string collectionName)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });

                    WriteToConsoleAndPromptToContinue("Created {0}", collectionName);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateBotUserDocumentIfNotExistsAsync(string databaseName, string collectionName, BotUser user)
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, user.TelegramId));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), user);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateGroupDocumentIfNotExistsAsync(string databaseName, string collectionName, Group group)
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, group.TelegramId));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), group);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
