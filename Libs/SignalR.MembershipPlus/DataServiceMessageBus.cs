using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Tracing;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.SignalR
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class DataServiceMessageBus : ScaleoutMessageBus, IServiceNotificationCallback
    {
        private static CallContext Cntx
        {
            get { return _cntx.CreateCopy(); }
        }
        private static CallContext _cntx;

        private static DataServiceConfiguration config = null;

        //private SignalRMessageServiceProxy msgsvc = new SignalRMessageServiceProxy();

        private static Thread LastMessageIdUpdateThread = null;
        private static ulong LastMessageId = 0;
        private static bool IsLastMessageIdChanged
        {
            get { return _isLastMessageIdChanged; }
            set
            {
                _isLastMessageIdChanged = value;
                if (value)
                    TrySetupUpdate();
            }
        }
        private static bool _isLastMessageIdChanged = false;
        private static AutoResetEvent evt = new AutoResetEvent(false);
        private static object SyncRoot = new object();

        private readonly TraceSource _trace;
        private bool Initialized = false;
        private bool CallbackFailed = false;

        private BlockingCollection<IList<Message>> MessageQueue = null;
        
        public CancellationToken CancelSendOperation = new CancellationToken();

        public DataServiceMessageBus(IDependencyResolver resolver, DataServiceConfiguration configuration, CallContext clientContext)
            : base(resolver, configuration)
        {
            config = configuration;
            _cntx = clientContext;
            var traceManager = resolver.Resolve<ITraceManager>();
            _trace = traceManager["SignalR." + typeof(DataServiceMessageBus).Name];
            Initialize();
        }

        private bool IsDisposed = false;

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                //msgsvc.Close();
            }
            IsDisposed = true;
            base.Dispose(disposing);
        }

        //private IHubProxy hubProxy = null;
        //private HubConnection hubConn = null;
        private static MembershipPlusServiceProxy svc = null;
        private static InstanceContext _notifier = null;
        private static EventHandler _delClosed = null;
        private static DataServiceMessageBus Current = null;

        private DateTime lastReceived = DateTime.MinValue;
        private DateTime lastSent = DateTime.MinValue;

        private void Initialize()
        {
            Initialized = false;
            /*
            var msgsvc = new SignalRMessageServiceProxy();
            //ProcOldMessages(cntx, msgsvc);
            string url = config.BackPlaneUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = msgsvc.Endpoint.Address.Uri.ToString();
                url = url.Substring(0, url.IndexOf("/Services"));
            }
            hubConn = new HubConnection(url);
            hubProxy = hubConn.CreateHubProxy("NotificationHub");
            hubConn.Start().Wait();
            hubProxy.Invoke("JoinGroup", EntitySetType.SignalRMessage.ToString()).Wait();
            hubProxy.On<dynamic>("entityChanged", (dmsg) => ProcMessage(cntx, dmsg));
            */
            _delClosed = new EventHandler(_notifier_Closed);
            Subscribe();
            MessageQueue = new BlockingCollection<IList<Message>>(new ConcurrentQueue<IList<Message>>(), config.MaxQueueLength);
            Task.Factory.StartNew(SendMessageThread);
            Initialized = true;
            Current = this;
        }
        
        private void Subscribe()
        {
            if (_notifier != null)
            {
                _trace.TraceWarning("Callback channel is broken. Try to re-establish ... ");
                _notifier.Closed -= _delClosed;
                _notifier.Faulted -= _delClosed;
            }
            _notifier = new InstanceContext(this);
            _notifier.Closed += _delClosed;
            _notifier.Faulted += _delClosed;
            try
            {
                svc = new MembershipPlusServiceProxy(_notifier);
                svc.SubscribeToUpdates(Cntx.CallerID, new EntitySetType[] { EntitySetType.SignalRMessage });
                _trace.TraceWarning("Subscription done.");
                CallbackFailed = false;
            }
            catch
            {
                _trace.TraceWarning("Subscription failed.");
                CallbackFailed = true;
                // the data service is down ... wait and try again ...
            }
        }

        private void _notifier_Closed(object sender, EventArgs e)
        {
            Subscribe();
        }

        private bool PollMessages(CallContext cntx, ulong lastId)
        {
            var msgsvc = new SignalRMessageServiceProxy();
            DateTime dt = DateTime.UtcNow.AddHours(-config.TimeWindowInHours);
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ID" },
                new QToken { TkName = "desc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ApplicationID == \"" + config.App.ID + "\" && ID > " + lastId + " && TimeStamp > " + dt.Ticks }
            });
            var msgs = msgsvc.QueryDatabaseLimited(cntx, new SignalRMessageSet(), qexpr, config.MaxBacklogMessages).ToArray();
            if (msgs.Length > 0)
            {
                LastMessageId = (ulong)msgs[0].ID;
                IsLastMessageIdChanged = true;
                foreach (var e in from d in msgs orderby d.ID ascending select d)
                {
                    var smsg = ScaleoutMessage.FromBytes(e.MesssageData);
                    forwardMessage((ulong)e.ID, smsg);
                }
            }
            return msgs.Length > 0;
        }

        // it is used to handle SignalR notifications
        private void ProcMessage(CallContext cntx, dynamic dmsg)
        {
            ulong msgId;
            var message = GetJsonMessage(dmsg, out msgId);
            if (message != null)
            {
                try
                {
                    forwardMessage(msgId, message);
                }
                catch
                {

                }
                LastMessageId = msgId;
                IsLastMessageIdChanged = true;
            }
        }

        private ScaleoutMessage GetJsonMessage(dynamic msg, out ulong msgId)
        {
            msgId = 0;
            if ((string)msg["status"] == "added")
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SignalRMessage));
                byte[] bf = Encoding.UTF8.GetBytes((string)msg["data"]);
                MemoryStream strm = new MemoryStream(bf);
                strm.Position = 0;
                var e = ser.ReadObject(strm) as SignalRMessage;
                if (e.ApplicationID == config.App.ID)
                {
                    msgId = (ulong)e.ID;
                    return ScaleoutMessage.FromBytes(e.MesssageData);
                }
                else
                    return null;
            }
            return null;
        }

        public void EntityChanged(EntitySetType SetType, int Status, string Entity)
        {
            if ((Status & (int)EntityOpStatus.Added) != 0)
            {
                switch (SetType)
                {
                    case EntitySetType.SignalRMessage:
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SignalRMessage));
                            byte[] bf = Encoding.UTF8.GetBytes(Entity);
                            MemoryStream strm = new MemoryStream(bf);
                            strm.Position = 0;
                            var e = ser.ReadObject(strm) as SignalRMessage;
                            if (e.ApplicationID == config.App.ID)
                            {
                                ulong msgId = (ulong)e.ID;
                                try
                                {
                                    forwardMessage(msgId, ScaleoutMessage.FromBytes(e.MesssageData));
                                }
                                catch
                                {

                                }
                                LastMessageId = msgId;
                                IsLastMessageIdChanged = true;
                            }
                        }
                        break;
                }
            }
        }

        private object _recvLock = new object();

        private void forwardMessage(ulong id, ScaleoutMessage smsg)
        {
            //lock (_recvLock)
            {
                OnReceived(0, id, smsg);
                lastReceived = DateTime.Now;
            }
        }

        private static void TrySetupUpdate()
        {
            lock (SyncRoot)
            {
                if (LastMessageIdUpdateThread == null || !LastMessageIdUpdateThread.IsAlive)
                {
                    LastMessageIdUpdateThread = new Thread(KeepAliveThread);
                    LastMessageIdUpdateThread.IsBackground = true;
                    LastMessageIdUpdateThread.Priority = ThreadPriority.BelowNormal;
                    LastMessageIdUpdateThread.Start();
                    evt.WaitOne();
                }
            }
        }

        private static void KeepAliveThread()
        {
            SignalRHostStateSet set = new SignalRHostStateSet();
            try
            {
                evt.Set();
                int failCount = 0;
                var cntx = Cntx;
                var hsvc = new SignalRHostStateServiceProxy();
                var host = hsvc.LoadEntityByNature(cntx, config.HostName, config.App.ID).SingleOrDefault();
                if (host == null)
                {
                    host = new SignalRHostState
                    {
                        HostName = config.HostName,
                        ApplicationID = config.App.ID,
                        LastMsgId = 0
                    };
                    var x = hsvc.AddOrUpdateEntities(cntx, new SignalRHostStateSet(), new SignalRHostState[] { host });
                    host = x.ChangedEntities[0].UpdatedItem;
                }
                while (true)
                {
                    Thread.Sleep(1000 * config.HostStateUpdateIntervalInSeconds);
                    hsvc = new SignalRHostStateServiceProxy();
                    if (IsLastMessageIdChanged)
                    {
                        host = new SignalRHostState
                        {
                            IsPersisted = true,
                            HostName = config.HostName,
                            ApplicationID = config.App.ID,
                            LastMsgId = (long)LastMessageId,
                            IsLastMsgIdModified = true
                        };
                        try
                        {
                            hsvc.AddOrUpdateEntities(cntx, set, new SignalRHostState[] { host });
                            IsLastMessageIdChanged = false;
                            if (failCount > 0)
                            {
                                Current.Subscribe();
                                failCount = 0;
                            }
                        }
                        catch
                        {
                            failCount++;
                        }
                    }
                    else
                    {
                        try
                        {
                            bool b = Current.PollMessages(cntx, LastMessageId);
                            if (b || (Current.lastSent - Current.lastReceived) > TimeSpan.FromSeconds(20) || failCount > 0)
                            {
                                Current.Subscribe();
                                failCount = 0;
                            }
                        }
                        catch
                        {
                            failCount++;
                        }
                    }
                    if (Current.CallbackFailed)
                    {
                        try
                        {
                            Current.Subscribe();
                            failCount = 0;
                        }
                        catch
                        {
                            failCount++;
                        }
                    }
                }
            }
            finally
            {
                LastMessageIdUpdateThread = null;
            }
        }

        protected override async Task Send(IList<Message> messages)
        {
            MessageQueue.Add(messages);
        }

        private void SendMessageThread()
        {
            IList<Message> msgs;
            var cntx = Cntx;
            var set = new SignalRMessageSet();
            var msgsvc = new SignalRMessageServiceProxy();
            while (!CancelSendOperation.IsCancellationRequested)
            {
                msgs = MessageQueue.Take(CancelSendOperation);
                if (msgs != null && !CancelSendOperation.IsCancellationRequested)
                {
                    SignalRMessage entity = new SignalRMessage
                    {
                        ApplicationID = config.App.ID,
                        TimeStamp = DateTime.UtcNow.Ticks,
                        MesssageData = (new ScaleoutMessage(msgs)).ToBytes()
                    };
                    try
                    {
                        msgsvc.AddOrUpdateEntities(cntx, set, new SignalRMessage[] { entity });
                        lastSent = DateTime.Now;
                    }
                    catch
                    {

                    }
                }
            }
        }


    }
}
