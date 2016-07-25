﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using GoogleApi.Entities.Common;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Shared;

namespace DocDbUtils
{
    class Program
    {
        private const string EndpointUri = "https://friendsgodocdb.documents.azure.com:443/";
        private const string PrimaryKey = "uiEW7oMq9HaaLGkyyoolxm2MIdtbUsi3YBzUb97nAdss2OPlGDU24nsbrxzfeGy2Oze3ve0CuFKS3yeqjLEjwA==";
        private DocumentClient client;

        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.GetStartedDemo().Wait();
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
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
        private async Task GetStartedDemo()
        {
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
            //await this.CreateDatabaseIfNotExists("Users");
            //await this.CreateDocumentCollectionIfNotExistsAsync("FriendsGo", "Users");
            //await this.CreateDocumentCollectionIfNotExistsAsync("FriendsGo", "Groups");

            //var user = new BotUser("testUser");
            var group = new Group(new Location(0,0));
            //await this.CreateBotUserDocumentIfNotExistsAsync("FriendsGo", "Users", user);
            await this.CreateBotGroupDocumentIfNotExistsAsync("FriendsGo", "Groups", group);

        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", databaseName);
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                    this.WriteToConsoleAndPromptToContinue("Created {0}", databaseName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExistsAsync(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", collectionName);
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
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });

                    this.WriteToConsoleAndPromptToContinue("Created {0}", collectionName);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateBotUserDocumentIfNotExistsAsync(string databaseName, string collectionName, BotUser user)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, user.Id.ToString()));
                this.WriteToConsoleAndPromptToContinue("Found {0}", user.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), user);
                    this.WriteToConsoleAndPromptToContinue("Created User {0}", user.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateBotGroupDocumentIfNotExistsAsync(string databaseName, string collectionName, Group group)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, group.Id.ToString()));
                this.WriteToConsoleAndPromptToContinue("Found {0}", group.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), group);
                    this.WriteToConsoleAndPromptToContinue("Created User {0}", group.Id);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}