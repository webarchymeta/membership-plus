using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.SignalR
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseDataService(this IDependencyResolver resolver, DataServiceConfiguration config, CallContext clientContext)
        {
            var bus = new Lazy<DataServiceMessageBus>(() => new DataServiceMessageBus(resolver, config, clientContext));
            resolver.Register(typeof(IMessageBus), () => bus.Value);
            return resolver;
        }
    }
}
