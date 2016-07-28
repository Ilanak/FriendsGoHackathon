using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Security.Policy;
using System.Text;
using GameManager;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
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
        private const string UserGroupCollectionName = "UsersGroups";

        private static DocumentClient client;
        public static Exception UserOrGroupNotFoundException { get; set; }

        #region user API

        /// <summary>
        ///  creates user in docDB users collection
        /// </summary>
        /// <param name="user"> new user </param>
        /// <returns>Task</returns>
        public static async Task CreateUser(BotUser user)
        {
            await CreateBotUserDocumentIfNotExistsAsync(DatabaseName, UsersCollectionName, user);
        }

        public static async Task DeleteUser(string telemgramId)
        {
            await DeleteDocument(DatabaseName, UsersCollectionName, telemgramId);
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

        /// <summary>
        ///  update user by Id
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="user"></param>
        public static void UpdateUser(string telegramId, BotUser user)
        {
            ReplaceEntity(DatabaseName, GroupsCollectionName, telegramId, user);
        }

        public static List<BotUser> GetAllUsers()
        {
            return GetEntityById<BotUser>(DatabaseName, UserGroupCollectionName);
        }
        #endregion

        #region groups API

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
        ///  returns all groups with same id , or null if no group exisits if this Id
        /// </summary>
        /// <param name="id">groupd id </param>
        /// <returns>selected group if exists</returns>
        public static Group GetGroupById(string id)
        {
            return GetEntityById<Group>(DatabaseName, GroupsCollectionName, id).FirstOrDefault();
        }

        public static List<Group> GetAllGroups()
        {
            return GetEntityById<Group>(DatabaseName, GroupsCollectionName);
        }

        public static async Task UpdateGroup(string telegramId, Group newGroup)
        {
            await ReplaceEntity(DatabaseName, GroupsCollectionName, telegramId, newGroup);
        }

        public static async Task  DeleteGroup(string telemgramId)
        {
            await DeleteDocument(DatabaseName, GroupsCollectionName, telemgramId);
        }

        #endregion

        #region UserGroupsApi

        public static async Task AddUserGroups(string userId, string groupId)
        {
            var user = GetUserById(userId);
            var group = GetGroupById(groupId);
            if (user == null || group == null)
            {
                throw UserOrGroupNotFoundException;
            }

            var userGroup = new UserGroup(userId: userId, groupId: groupId);
            await CreateuserGroupDocumentIfNotExistsAsync(DatabaseName, UserGroupCollectionName, userGroup);
        }

        /// <summary>
        ///  returns all groups with same id , or null if no group exisits if this Id
        /// </summary>
        /// <param name="userId">groupd id </param>
        /// /// <param name="groupId">groupd id </param>
        /// <returns>selected group if exists</returns>
        public static UserGroup GetUserGroupById(string userId, string groupId)
        {
            return GetEntityById<UserGroup>(DatabaseName, UserGroupCollectionName, GetUserGroupId(userId, groupId)).FirstOrDefault();
        }

        public static void DeleteUserGroup(string userId, string groupId)
        {
            DeleteDocument(DatabaseName, UserGroupCollectionName, GetUserGroupId(userId, groupId));
        }

        public static List<UserGroup> GetUsersGroups(string userId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IQueryable<UserGroup> query = client.CreateDocumentQuery<UserGroup>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseName, UserGroupCollectionName), queryOptions)
                    .Where(entity => (entity.TelegramId.Contains(userId)));

            var groups = query.ToList();

            return groups.Where(grp => grp.UserId == userId).ToList();
        }

        public static List<UserGroup> GetGroupsUsers(string groupId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IQueryable<UserGroup> query = client.CreateDocumentQuery<UserGroup>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseName, UserGroupCollectionName), queryOptions)
                    .Where(entity => (entity.TelegramId.Contains(groupId)));

            var groups = query.ToList();

            return groups.Where(grp => grp.GroupId == groupId).ToList();
        }

        #endregion

        /// <summary>
        /// Get list of top 10 groups (from all around the world!!!)
        /// </summary>
        /// <returns></returns>
        public static string GetTopGroups()
        {
            var groups = GetAllGroups();
            var statList = groups.OrderByDescending(grp => grp.Level);
            var topGroups = new StringBuilder();
            var place = 1;

            topGroups.AppendFormat("Top Groups:");
            topGroups.AppendLine();

            foreach (var group in statList)
            {
                topGroups.AppendFormat("#{0} {1} - level :{2}", place++, group.TelegramId, group.Level);
                topGroups.AppendLine();
            }

            return topGroups.ToString();
        }

        private static string GetUserGroupId(string userId, string groupId)
        {
            return string.Format("{0}_{1}", userId, groupId);
        }
        private static async Task ReplaceEntity<T>(string databaseName, string collectionName, string telegramId, T updatedEntity)
        {
            var uri = UriFactory.CreateDocumentUri(databaseName, collectionName, telegramId);
            await client.ReplaceDocumentAsync(uri, updatedEntity);
        }

        private static List<T> GetEntityById<T>(string databaseName, string collectionName, string entityTelegramId = null) where T : DocDbEntityBase
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IQueryable<T> query;
            if (entityTelegramId != null)
            {
                query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                .Where(entity => entity.TelegramId == entityTelegramId);
            }
            else
            {
                query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions); ;
            }
            return query.ToList();
        }

        private static async Task DeleteDocument(string databaseName, string collectionName, string documentName)
        {
            try
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentName));
                Console.WriteLine("Deleted {0}", documentName);
            }
            catch (DocumentClientException de)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
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

        public static void CreateCollection(string name)
        {
            CreateDocumentCollectionIfNotExists(DatabaseName, name);
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

        private static void CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                var uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
                client.ReadDocumentCollectionAsync(uri).Wait();

            }
            catch (Exception ex)
            {
                DocumentCollection collectionInfo = new DocumentCollection();
                collectionInfo.Id = collectionName;

                // Configure collections for maximum query flexibility including string range queries.
                collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                // Here we create a collection with 400 RU/s.
                client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    collectionInfo,
                    new RequestOptions { OfferThroughput = 400 }).Wait();
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

        private static async Task CreateuserGroupDocumentIfNotExistsAsync(string databaseName, string collectionName, UserGroup userGroup)
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, userGroup.GroupId));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), userGroup);
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
            catch (Exception de)
            {
                if (true)
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
