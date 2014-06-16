using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using log4net;
using log4net.Core;
using log4net.Appender;
#if MEMBERSHIPPLUS
using CryptoGateway.RDB.Data.MembershipPlus;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif

namespace Archymeta.Web.Logging
{
    internal class LoggingEventWrap
    {
        public LoggingEvent Evt;
        public string webUser;
        public string pageUrl;
        public string referUrl;
        public string requestId;
    }

    /// <summary>
    /// "Lossless" custom appender for log4net used to send log items to remote remote data services.
    /// </summary>
    public class DataServiceAppender : AppenderSkeleton
    {
        /// <summary>
        /// The application entity for the data service.
        /// </summary>
        /// <remarks>
        /// It should be initialized before any logging activity
        /// </remarks>
        public static Application_ App
        {
            get;
            set;
        }

        /// <summary>
        /// A global client representation of the application to the logging data service.
        /// </summary>
        /// <remarks>
        /// It should be initialized before any logging activity
        /// </remarks>
        public static CallContext ClientContext
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum number of waiting log items to be sent to the data service. The default is 5000.
        /// </summary>
        /// <remarks>
        /// The response of the system to a new log item when number of waiting log items has reached this value depends on the 
        /// value of <see cref="DataServiceAppender.Lossy"/>. When <see cref="DataServiceAppender.Lossy"/> is true, then the new item
        /// will be discarded otherwise the addition statement will block until there are number of waiting log items drops below 
        /// this value.
        /// </remarks>
        public int MaxQueueLength
        {
            get { return _maxQueueLength; }
            set { _maxQueueLength = value; }
        }
        private static int _maxQueueLength = 5000;

        /// <summary>
        /// The maximum number of log items to accumulate locally before they are been sent to the data service in one block. The default is 10.
        /// </summary>
        /// <remarks>
        /// If the appender is not busy, it will send what is left in the event queue periodically. The client code does not need to
        /// "flush" them.
        /// </remarks>
        public int MaxUpdateBlockSize
        {
            get { return _maxUpdateBlockSize; }
            set { _maxUpdateBlockSize = value; }
        }
        private static int _maxUpdateBlockSize = 10;

        /// <summary>
        /// Whether or not the record method invoking stack frames at the log position. The default is true.
        /// </summary>
        public bool RecordStackFrames
        {
            get { return _recordStackFrames; }
            set { _recordStackFrames = value; }
        }
        private static bool _recordStackFrames = true;

        /// <summary>
        /// Whether or not to include stack frames whose code file is known. This is what "User" mean here. The default is true.
        /// </summary>
        /// <remarks>
        /// <para>If the corresponding .pdb file of an assembly is not available at the deployment site, then that assembly and the 
        /// stackframes that reference it are not a "User" one.</para>
        /// </remarks>
        public bool UserStackFramesOnly
        {
            get { return _userStackFramesOnly; }
            set { _userStackFramesOnly = value; }
        }
        private static bool _userStackFramesOnly = true;

        /// <summary>
        /// Maximum number of stackframes to include. The default is -1, which means all.
        /// </summary>
        public int MaxStackFramesUp
        {
            get { return _maxStackFramesUp; }
            set { _maxStackFramesUp = value; }
        }
        private static int _maxStackFramesUp = -1;

        /// <summary>
        /// Whether or not to drop a new log item when the waiting log items has reached <see cref="DataServiceAppender.MaxQueueLength" />. The default is false.
        /// </summary>
        public bool Lossy
        {
            get { return _lossy; }
            set { _lossy = value; }
        }
        private bool _lossy = false;

        /// <summary>
        /// An optional base url of an independent log data service Specify it only when the targeting data service is different from the main data service of the application. If not 
        /// specified, the main data service of the application is assumed.
        /// </summary>
        /// <remarks>
        /// Properties: <see cref="DataServiceAppender.MaxReceivedMessageSize"/>, <see cref="DataServiceAppender.MaxBufferPoolSize"/>, <see cref="DataServiceAppender.MaxBufferSize"/>, 
        /// <see cref="DataServiceAppender.MaxArrayLength"/>, <see cref="DataServiceAppender.MaxBytesPerRead"/>, <see cref="DataServiceAppender.MaxDepth"/>, 
        /// <see cref="DataServiceAppender.MaxNameTableCharCount"/> and <see cref="DataServiceAppender.MaxStringContentLength"/> are only relevant when this property is set to
        /// a valid data service url and their values should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public string LoggerServiceUrl
        {
            get { return _loggerServiceUrl; }
            set { _loggerServiceUrl = value; }
        }
        private static string _loggerServiceUrl = null;

        /// <summary>
        /// The maximum size, in bytes, for a message that can be received on any service invokation channel involved.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 65536000.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public long MaxReceivedMessageSize
        {
            get { return _maxReceivedMessageSize; }
            set { _maxReceivedMessageSize = value; }
        }
        private static long _maxReceivedMessageSize = 65536000;

        /// <summary>
        /// The maximum amount of memory, in bytes, that is allocated for use by the manager of the message buffers that receive messages from the any service invokation channel involved.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 65536000.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public long MaxBufferPoolSize
        {
            get { return _maxBufferPoolSize; }
            set { _maxBufferPoolSize = value; }
        }
        private static long _maxBufferPoolSize = 65536000;

        /// <summary>
        /// The maximum amount of memory, in bytes, that is allocated for use by the manager of the message buffers that receive messages from any service invokation channel involved. 
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 65536000.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxBufferSize
        {
            get { return _maxBufferSize; }
            set { _maxBufferSize = value; }
        }
        private static int _maxBufferSize = 65536000;

        /// <summary>
        /// The maximum size, in bytes, for a buffer that receives messages from any service invokation channel involved.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding.ReaderQuotas" /> member of <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 104857600.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxArrayLength
        {
            get { return _maxArrayLength; }
            set { _maxArrayLength = value; }
        }
        private static int _maxArrayLength = 104857600;

        /// <summary>
        /// The maximum allowed bytes returned for each read.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding.ReaderQuotas" /> member of <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 4096.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxBytesPerRead
        {
            get { return _maxBytesPerRead; }
            set { _maxBytesPerRead = value; }
        }
        private static int _maxBytesPerRead = 4096;

        /// <summary>
        /// The maximum nested node depth.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding.ReaderQuotas" /> member of <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 64.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxDepth
        {
            get { return _maxDepth; }
            set { _maxDepth = value; }
        }
        private static int _maxDepth = 64;

        /// <summary>
        /// The maximum characters allowed in a table name.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding.ReaderQuotas" /> member of <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 16384.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxNameTableCharCount
        {
            get { return _maxNameTableCharCount; }
            set { _maxNameTableCharCount = value; }
        }
        private static int _maxNameTableCharCount = 16384;

        /// <summary>
        /// The maximum string length returned by the reader.
        /// </summary>
        /// <remarks>
        /// It is used to set the corresponding http service binding (see <see cref="System.ServiceModel.BasicHttpBinding.ReaderQuotas" /> member of <see cref="System.ServiceModel.BasicHttpBinding" />), the current default value is 81920.
        /// Its value should match the corresponding ones of the targeting log data service.
        /// </remarks>
        public int MaxStringContentLength
        {
            get { return _maxStringContentLength; }
            set { _maxStringContentLength = value; }
        }
        private static int _maxStringContentLength = 181920;

        /// <summary>
        /// 
        /// </summary>
        public DataServiceAppender()
            : base()
        {
        }

        protected override void Append(LoggingEvent evt)
        {
            SetupThread();
            string webUserName = GlobalContext.Properties["user"].ToString();
            string pageUrl = GlobalContext.Properties["pageUrl"].ToString();
            string referUrl = GlobalContext.Properties["referUrl"].ToString();
            string requestId = GlobalContext.Properties["requestId"].ToString();
            var evtw = new LoggingEventWrap { Evt = evt, webUser = webUserName, pageUrl = pageUrl, referUrl = referUrl, requestId = requestId };
            if (!_lossy)
                EventQueue.Add(getEntity(evtw));
            else
                EventQueue.TryAdd(getEntity(evtw));
        }

        protected override void Append(LoggingEvent[] evts)
        {
            SetupThread();
            foreach (var evt in evts)
            {
                string webUserName = GlobalContext.Properties["user"].ToString();
                string pageUrl = GlobalContext.Properties["pageUrl"].ToString();
                string referUrl = GlobalContext.Properties["referUrl"].ToString();
                string requestId = GlobalContext.Properties["requestId"].ToString();
                var evtw = new LoggingEventWrap { Evt = evt, webUser = webUserName, pageUrl = pageUrl, referUrl = referUrl, requestId = requestId };
                if (!_lossy)
                    EventQueue.Add(getEntity(evtw));
                else
                    EventQueue.TryAdd(getEntity(evtw));
            } 
        }

        private static EventLogServiceProxy GetService()
        {
            EventLogServiceProxy svc;
            if (!string.IsNullOrEmpty(_loggerServiceUrl))
            {
                BasicHttpBinding b = new BasicHttpBinding();
                b.MaxReceivedMessageSize = _maxReceivedMessageSize;
                b.MaxBufferPoolSize = _maxBufferPoolSize;
                b.MaxBufferSize = _maxBufferSize;
                b.ReaderQuotas = new XmlDictionaryReaderQuotas();
                b.ReaderQuotas.MaxDepth = _maxDepth;
                b.ReaderQuotas.MaxArrayLength = _maxArrayLength;
                b.ReaderQuotas.MaxBytesPerRead = _maxBytesPerRead;
                b.ReaderQuotas.MaxNameTableCharCount = _maxNameTableCharCount;
                b.ReaderQuotas.MaxStringContentLength = _maxStringContentLength;
                EndpointAddress addr = new EndpointAddress(_loggerServiceUrl);
                svc = new EventLogServiceProxy(b, addr);
            }
            else
                svc = new EventLogServiceProxy();
            return svc;
        }

        private static void SetupThread()
        {
            if (_thread == null || !_thread.IsAlive)
            {
                _thread = new Thread(EventProcessThread);
                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private static BlockingCollection<EventLog> EventQueue
        {
            get
            {
                return _eventQueue ?? (_eventQueue = new BlockingCollection<EventLog>(_maxQueueLength));
            }
        }
        private static BlockingCollection<EventLog> _eventQueue = null;

        private static Thread _thread = null;
        private static bool stopProcessing = false;

        private static void EventProcessThread()
        {
            List<EventLog> block = new List<EventLog>();
            while (!stopProcessing)
            {
                EventLog e;
                while (!EventQueue.TryTake(out e, 300))
                {
                    if (block.Count > 0)
                        sendBlock(block);
                    if (stopProcessing)
                        break;
                }
                block.Add(e);
                if (block.Count >= _maxUpdateBlockSize)
                    sendBlock(block);
            }
            _thread = null;
        }

        private static EventLog getEntity(LoggingEventWrap evtw)
        {
            EventLog log = new EventLog();
            log.ID = Guid.NewGuid().ToString();
            log.AppAgent = evtw.Evt.UserName;
            log.AppDomain = evtw.Evt.Domain;
            log.AppID = App != null ? App.ID : null;
            log.EventLevel = evtw.Evt.Level.Name;
            if (evtw.Evt.ExceptionObject != null)
            {
                log.ExceptionInfo = excpetionToString(evtw.Evt.ExceptionObject);
                //it's important to turn this on for delay loaded properties
                log.IsExceptionInfoLoaded = true;
            }
            log.LoggerName = evtw.Evt.LoggerName;
            TracedLogMessage tmsg = null;
            if (evtw.Evt.MessageObject is TracedLogMessage)
            {
                tmsg = evtw.Evt.MessageObject as TracedLogMessage;
                log.Message_ = tmsg.Msg;
                log.CallTrackID = tmsg.ID;
            }
            else if (evtw.Evt.MessageObject is string)
                log.Message_ = evtw.Evt.MessageObject as string;
            else
                log.Message_ = evtw.Evt.RenderedMessage;
            log.ThreadName_ = evtw.Evt.ThreadName;
            log.ThreadPrincipal = evtw.Evt.Identity;
            log.TimeStamp_ = evtw.Evt.TimeStamp.Ticks;
            log.Username = evtw.webUser == null ? evtw.Evt.UserName : evtw.webUser;
            log.PageUrl = evtw.pageUrl;
            log.ReferringUrl = evtw.referUrl;
            if (tmsg == null)
                log.CallTrackID = evtw.requestId;
            if (evtw.Evt.Level >= Level.Debug &&  evtw.Evt.LocationInformation != null && _recordStackFrames)
                log.ChangedEventLocations = new EventLocation[] { getLocation(log.ID, evtw.Evt.LocationInformation) };
            return log;
        }

        private static EventLocation getLocation(string id, LocationInfo loc)
        {
            EventLocation eloc = new EventLocation();
            eloc.EventID = id;
            eloc.ClassName_ = loc.ClassName;
            // 220 is the current FileName_ size.
            eloc.FileName_ = loc.FileName != null && loc.FileName.Length > 220 ? "..." + loc.FileName.Substring(loc.FileName.Length - 220 - 3) : loc.FileName;
            eloc.MethodName_ = loc.MethodName;
            eloc.LineNumber = loc.LineNumber;
            if (loc.StackFrames != null && loc.StackFrames.Length > 0)
            {
                List<EventStackFrame> frames = new List<EventStackFrame>();
                int frmId = 1;
                foreach (var frm in loc.StackFrames)
                {
                    if (_maxStackFramesUp >= 0 && frmId > _maxStackFramesUp)
                        break;
                    else if (_userStackFramesOnly && string.IsNullOrEmpty(frm.FileName))
                        continue;
                    EventStackFrame efrm = new EventStackFrame();
                    efrm.EventID = id;
                    efrm.ID = frmId++;
                    efrm.ClassName_ = frm.ClassName;
                    // 220 is the current FileName_ size.
                    efrm.FileName_ = frm.FileName != null && frm.FileName.Length > 220 ? "..." + frm.FileName.Substring(frm.FileName.Length - 220 - 3) : frm.FileName;
                    efrm.LineNumber = frm.LineNumber;
                    string callinfo = frm.Method.Name + "(";
                    foreach (var p in frm.Method.Parameters)
                        callinfo += p + ", ";
                    callinfo = callinfo.TrimEnd(", ".ToCharArray()) + ")";
                    efrm.MethodInfo = callinfo;
                    frames.Add(efrm);
                }
                eloc.ChangedEventStackFrames = frames.ToArray();
            }
            return eloc;
        }

        private static void sendBlock(List<EventLog> block)
        {
            try
            {
                var svc = GetService();
                svc.AddOrUpdateEntities(ClientContext.CreateCopy(), new EventLogSet(), block.ToArray());
            }
            catch (Exception ex)
            {
                log4net.Util.LogLog.Warn(typeof(DataServiceAppender), excpetionToString(ex));
            }
            finally
            {
                block.Clear();
            }
        }

        private static string excpetionToString(Exception ex)
        {
            string err = "";
            while (ex != null)
            {
                string exstr = ex.GetType() + "\r\n";
                exstr += ex.Message + "\r\n";
#if DEBUG
                exstr += ex.Source + "\r\n";
                exstr += ex.StackTrace + "\r\n";
#endif
                err = exstr + err;
                ex = ex.InnerException;
            }
            return err;
        }
    }
}
