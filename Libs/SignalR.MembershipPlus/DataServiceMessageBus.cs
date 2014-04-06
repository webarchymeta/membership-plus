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
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Tracing;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.SignalR
{
    public class DataServiceMessageBus : ScaleoutMessageBus
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

        private void Initialize()
        {
            Initialized = false;
            var cntx = Cntx;
            var msgsvc = new SignalRMessageServiceProxy();
            ProcOldMessages(cntx, msgsvc);
            string url = config.BackPlaneUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = msgsvc.Endpoint.Address.Uri.ToString();
                url = url.Substring(0, url.IndexOf("/Services"));
            }
            var hubConn = new HubConnection(url);
            var hubProxy = hubConn.CreateHubProxy("NotificationHub");
            hubConn.Start().Wait();
            hubProxy.Invoke("JoinGroup", EntitySetType.SignalRMessage.ToString()).Wait();
            hubProxy.On<dynamic>("entityChanged", (dmsg) => ProcMessage(cntx, dmsg));
            MessageQueue = new BlockingCollection<IList<Message>>(new ConcurrentQueue<IList<Message>>(), config.MaxQueueLength);
            Task.Factory.StartNew(SendMessageThread);
            Initialized = true;
        }

        private void ProcOldMessages(CallContext cntx, SignalRMessageServiceProxy msgsvc)
        {
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
            DateTime dt = DateTime.UtcNow.AddHours(-config.TimeWindowInHours);
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ID" },
                new QToken { TkName = "desc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ApplicationID == \"" + config.App.ID + "\" && ID > " + host.LastMsgId + " && TimeStamp > " + dt.Ticks }
            });
            var msgs = msgsvc.QueryDatabaseLimited(cntx, new SignalRMessageSet(), qexpr, config.MaxBacklogMessages).ToArray();
            if (msgs.Length > 0)
            {
                host.LastMsgId = msgs[0].ID;
                LastMessageId = (ulong)host.LastMsgId;
                IsLastMessageIdChanged = true;
                foreach (var e in from d in msgs orderby d.ID ascending select d)
                {
                    OnReceived(0, (ulong)e.ID, ScaleoutMessage.FromBytes(e.MesssageData));
                }
            }
        }

        private void ProcMessage(CallContext cntx, dynamic dmsg)
        {
            ulong msgId;
            var message = GetJsonMessage(dmsg, out msgId);
            if (message != null)
            {
                OnReceived(0, msgId, message);
                LastMessageId = msgId;
                IsLastMessageIdChanged = true;
            }
        }

        private static void TrySetupUpdate()
        {
            lock (SyncRoot)
            {
                if (LastMessageIdUpdateThread == null || !LastMessageIdUpdateThread.IsAlive)
                {
                    LastMessageIdUpdateThread = new Thread(LastIdUpdateThread);
                    LastMessageIdUpdateThread.IsBackground = true;
                    LastMessageIdUpdateThread.Priority = ThreadPriority.BelowNormal;
                    LastMessageIdUpdateThread.Start();
                    evt.WaitOne();
                }
            }
        }

        private static void LastIdUpdateThread()
        {
            var hsvc = new SignalRHostStateServiceProxy();
            SignalRHostStateSet set = new SignalRHostStateSet();
            try
            {
                evt.Set();
                while (true)
                {
                    Thread.Sleep(1000 * config.HostStateUpdateIntervalInSeconds);
                    if (IsLastMessageIdChanged)
                    {
                        SignalRHostState host = new SignalRHostState
                        {
                            IsPersisted = true,
                            HostName = config.HostName,
                            ApplicationID = config.App.ID,
                            LastMsgId = (long)LastMessageId,
                            IsLastMsgIdModified = true
                        };
                        try
                        {
                            hsvc.AddOrUpdateEntities(Cntx, set, new SignalRHostState[] { host });
                            IsLastMessageIdChanged = false;
                        }
                        catch
                        {

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
                    }
                    catch
                    {

                    }
                }
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
    }
}
