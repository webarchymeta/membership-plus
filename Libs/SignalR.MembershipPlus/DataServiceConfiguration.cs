using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Messaging;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.SignalR
{
    public class DataServiceConfiguration : ScaleoutConfiguration
    {
        public string AppRoot
        { 
            get; set; 
        }

        public Application_ App
        {
            get;
            set;
        }

        public string HostName
        {
            get;
            set;
        }

        public string BackPlaneUrl
        {
            get;
            set;
        }

        public int MaxBacklogMessages
        {
            get { return _maxBacklogMessages; }
            set { _maxBacklogMessages = value; }
        }
        private int _maxBacklogMessages = 200;

        public int TimeWindowInHours
        {
            get { return _timeWindowInHours; }
            set { _timeWindowInHours = value; }
        }
        private int _timeWindowInHours = 2;

        public int HostStateUpdateIntervalInSeconds
        {
            get { return _hostStateUpdateIntervalInSeconds; }
            set { _hostStateUpdateIntervalInSeconds = value; }
        }
        private int _hostStateUpdateIntervalInSeconds = 30;
    }
}
