using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using GameManager;

namespace FriendsGoBot
{
    

    [BotAuthentication]
    public class MessagesController : ApiController
    {
       
        static GameCommands manager = new GameCommands();
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            Trace.TraceInformation("post activity");
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                if (activity.Text[0] == '/')
                {
                    //this is a command
                    if (activity.Text.Equals("/join"))
                    {
                        manager.JoinGame(activity.ChannelId, activity.From.Id);
                    }
                    //this is a command
                    if (activity.Text.Equals("/challenge"))
                    {
                        manager.GetMission(activity.ChannelId);
                    }
                    //this is a command
                    if (activity.Text.Equals("/checkin"))
                    {
                        manager.CheckIn(activity.ChannelId, activity.From.Id, 23.6754, 25.45345);
                    }
                    //this is a command
                    if (activity.Text.Equals("/stat"))
                    {
                        manager.Stat(activity.ChannelId);
                    }
                }
            }
           
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                message.CreateReply("delete user data");
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}