using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace MemberAdminMvc5.SignalRHubs
{
    public class ChatHub : HubBase
    {
        #region Data Members

        //static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        //static List<MessageDetail> CurrentMessage = new List<MessageDetail>();

        #endregion

        #region Methods

        public void Connect(string userName)
        {
            //var id = Context.ConnectionId;


            //if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            //{
            //    ConnectedUsers.Add(new UserDetail { ConnectionId = id, UserName = userName });

            //    // send to caller
            //    Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessage);

            //    // send to all except caller client
            //    Clients.AllExcept(id).onNewUserConnected(id, userName);

            //}

        }

        public void SendMessageToAll(string userName, string message)
        {
            // store last 100 messages in cache
            AddMessageinCache(userName, message);

            // Broad cast message
            Clients.All.messageReceived(userName, message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {
            //string fromUserId = Context.ConnectionId;

            //var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            //var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            //if (toUser != null && fromUser != null)
            //{
            //    // send to 
            //    Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message);

            //    // send to caller user
            //    Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
            //}

        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            //var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //if (item != null)
            //{
            //    ConnectedUsers.Remove(item);

            //    var id = Context.ConnectionId;
            //    Clients.All.onUserDisconnected(id, item.UserName);

            //}

            return base.OnDisconnected();
        }


        #endregion

        #region private Messages

        private void AddMessageinCache(string userName, string message)
        {
            //CurrentMessage.Add(new MessageDetail { UserName = userName, Message = message });

            //if (CurrentMessage.Count > 100)
            //    CurrentMessage.RemoveAt(0);
        }

        #endregion
    }
}