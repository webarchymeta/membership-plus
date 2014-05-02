//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Tool name: CGW X-Script Linq to SQL Layer Generator
//
//     Archymeta Information Technologies Co., Ltd.
//
//     Changes to this file, especially those bit flags, may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CryptoGateway.RDB.Data.MembershipPlus
{
    /// <summary>
    /// A dummy callback.
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class DummyCallback : IServiceNotificationCallback
    {
        /// <summary>
        /// Change notification callback.
        /// </summary>
        /// <param name="SetType">The type of the changed entity.</param>
        /// <param name="Status">The type of changes of the entity.</param>
        /// <param name="Entity">The changed entity.</param>
        public void EntityChanged(EntitySetType SetType, int Status, string Entity)
        {
        }
    }

    /// <summary>
    /// Proxy for <see cref="IMembershipPlusService2" /> service.
    /// </summary>
    public class MembershipPlusServiceProxy : DuplexClientBase<IMembershipPlusService2>, IMembershipPlusService2
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MembershipPlusServiceProxy() 
            : base(new DummyCallback(), "HTTP")
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="callback">The client callback.</param>
        public MembershipPlusServiceProxy(InstanceContext callback) 
            : base(callback, "HTTP")
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="svcConfig">The name of the configuration node for the end point.</param>
        public MembershipPlusServiceProxy(string svcConfig) 
            : base(new DummyCallback(), svcConfig)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="callback">The client callback.</param>
        /// <param name="svcConfig">The name of the configuration node for the end point.</param>
        public MembershipPlusServiceProxy(InstanceContext callback, string svcConfig) 
            : base(callback, svcConfig)
        {

        }

        /// <summary>
        /// Initializes a new instance using the specified binding and target address. 
        /// </summary>
        /// <param name="callback">The client callback.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        public MembershipPlusServiceProxy(InstanceContext callback, Binding binding, EndpointAddress remoteAddress)
            : base(callback, binding, remoteAddress)
        {

        }

        /// <summary>
        /// Client attached error handler.
        /// </summary>
        public Action<Exception> DelHandleError = null;


        /// <summary>
        ///   Sign in the service for relational database "MembershipPlus" and authenticate the identity of the caller. 
        /// Depending on the end points, the authentication may have been delegated to the host. E.g., the end point serving javascript
        /// requests are delegated to Asp.Net website authentication system. For other end points, the caller must provide correct credentials
        /// in order to have permission to continue the call processing.
        /// </summary>
        /// <remarks>
        ///   Note: The current version of the system does not check for credentials. It also does not validate the returned caller context object. 
        /// Therefore care must be taken to limit the access to the service to trusted nodes or users within a secured network environment.
        /// </remarks>
        /// <param name="cntx">Caller supplied and initialized caller context. If it is null, the service will create an initial one.</param>
        /// <param name="credentials">Caller credential information.</param>
        /// <returns>
        ///   An initialized caller context object used for subsequent API calls. Supplying an invalid caller context will
        /// result in a deny of the service.
        /// </returns>
        public CallContext SignInService(CallContext cntx, CallerCredentials credentials)
        {
            try
            {
                return Channel.SignInService(cntx, credentials);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   Sign in the service for relational database "MembershipPlus" and authenticate the identity of the caller. 
        /// Depending on the end points, the authentication may have been delegated to the host. E.g., the end point serving javascript
        /// requests are delegated to Asp.Net website authentication system. For other end points, the caller must provide correct credentials
        /// in order to have permission to continue the call processing.
        /// </summary>
        /// <remarks>
        ///   Note: The current version of the system does not check for credentials. It also does not validate the returned caller context object. 
        /// Therefore care must be taken to limit the access to the service to trusted nodes or users within a secured network environment.
        /// </remarks>
        /// <param name="cntx">Caller supplied and initialized caller context. If it is null, the service will create an initial one.</param>
        /// <param name="credentials">Caller credential information.</param>
        /// <returns>
        ///   An initialized caller context object used for subsequent API calls. Supplying an invalid caller context will
        /// result in a deny of the service.
        /// </returns>
        public async System.Threading.Tasks.Task<CallContext> SignInServiceAsync(CallContext cntx, CallerCredentials credentials)
        {
            try
            {
                return await Channel.SignInServiceAsync(cntx, credentials);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }
#endif


        /// <summary>
        /// Register a subscription to notification of data source changes.
        /// </summary>
        /// <param name="clientID">An identifier that the client is assigned during signin/initialization stage.</param>
        /// <param name="sets">A list of data sets that the client will receive notifications. If it is set to null, then change notifications 
        /// about all data sets will be sent to the client.</param>
        public void SubscribeToUpdates(string clientID, EntitySetType[] sets)
        {
            try
            {
                Channel.SubscribeToUpdates(clientID, sets);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        /// Register a subscription to notification of data source changes.
        /// </summary>
        /// <param name="clientID">An identifier that the client is assigned during signin/initialization stage.</param>
        /// <param name="sets">A list of data sets that the client will receive notifications. If it is set to null, then change notifications 
        /// about all data sets will be sent to the client.</param>
        public async System.Threading.Tasks.Task SubscribeToUpdatesAsync(string clientID, EntitySetType[] sets)
        {
            try
            {
                await Channel.SubscribeToUpdatesAsync(clientID, sets);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
#endif


        /// <summary>
        /// un-register a subscription to data source change notifications.
        /// </summary>
        /// <param name="clientID">An identifier that the client is assigned during signin/initialization stage.</param>
        public void UnsubscribeToUpdates(string clientID)
        {
            try
            {
                Channel.UnsubscribeToUpdates(clientID);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        /// un-register a subscription to data source change notifications.
        /// </summary>
        /// <param name="clientID">An identifier that the client is assigned during signin/initialization stage.</param>
        public async System.Threading.Tasks.Task UnsubscribeToUpdatesAsync(string clientID)
        {

            try
            {
                await Channel.UnsubscribeToUpdatesAsync(clientID);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
#endif


        /// <summary>
        ///   Initialize or refresh and check the validity of the caller context information of the caller. 
        /// </summary>
        /// <remarks>
        ///   Note: The current version of the system does not validate the returned caller context object. 
        /// Therefore care must be taken to limit the access to the service to trusted nodes or users within a secured network environment.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   An initialized and refreshed caller context object used for subsequent API calls. Supplying an invalid caller context will
        /// result in a deny of the service.
        /// </returns>
        public CallContext InitializeCallContext(CallContext cntx)
        {
            try
            {
                return Channel.InitializeCallContext(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   Initialize or refresh and check the validity of the caller context information of the caller. Awaitable asynchronous version.
        /// </summary>
        /// <remarks>
        ///   Note: The current version of the system does not validate the returned caller context object. 
        /// Therefore care must be taken to limit the access to the service to trusted nodes or users within a secured network environment.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   An initialized and refreshed caller context object used for subsequent API calls. Supplying an invalid caller context will
        /// result in a deny of the service.
        /// </returns>
        public async System.Threading.Tasks.Task<CallContext> InitializeCallContextAsync(CallContext cntx)
        {
            try
            {
                return await Channel.InitializeCallContextAsync(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }
#endif


        /// <summary>
        ///   Retrieve information about the database. 
        /// </summary>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Brief information about current database.
        /// </returns>
        public DBInformation GetDatabaseInfo(CallContext cntx)
        {
            try
            {
                return Channel.GetDatabaseInfo(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   Retrieve information about the database. Awaitable asynchronous version.
        /// </summary>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Brief information about current database.
        /// </returns>
        public async System.Threading.Tasks.Task<DBInformation> GetDatabaseInfoAsync(CallContext cntx)
        {
            try
            {
                return await Channel.GetDatabaseInfoAsync(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }
#endif


        /// <summary>
        ///   If the targeting database does not exist or is an empty one, create the database and/or the tables and other constructs. 
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For real relational database stores, it is safer to create an empty database named "MembershipPlus" inside the targeting database server and
        /// then call this method to create the tables and other constructs.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public bool CreateDatabase(CallContext cntx)
        {
            try
            {
                return Channel.CreateDatabase(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   If the targeting database does not exist or is an empty one, create the database and/or the tables and other constructs. Awaitable asynchronous version.
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For real relational database stores, it is safer to create an empty database named "MembershipPlus" inside the targeting database server and
        /// then call this method to create the tables and other constructs.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public async System.Threading.Tasks.Task<bool> CreateDatabaseAsync(CallContext cntx)
        {
            try
            {
                return await Channel.CreateDatabaseAsync(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }
#endif


        /// <summary>
        ///   Load persisted database information from local storage. 
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For self loading relational data stores, calling this method will not have any effect.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public bool LoadDatabase(CallContext cntx)
        {
            try
            {
                return Channel.LoadDatabase(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   Load persisted database information from local storage. Awaitable asynchronous version.
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For self loading relational data stores, calling this method will not have any effect.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public async System.Threading.Tasks.Task<bool> LoadDatabaseAsync(CallContext cntx)
        {
            try
            {
                return await Channel.LoadDatabaseAsync(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }
#endif


        /// <summary>
        ///   Save database information to local storage. 
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For self loading relational data stores, calling this method will not have any effect.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public bool SaveDatabase(CallContext cntx)
        {
            try
            {
                return Channel.SaveDatabase(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }

#if SUPPORT_ASYNC

        /// <summary>
        ///   Save database information to local storage. Awaitable asynchronous version.
        /// </summary>
        /// <remarks>
        ///   Depending on the type of the relational data source attached, this method may not be relevent. 
        /// For self loading relational data stores, calling this method will not have any effect.
        /// </remarks>
        /// <param name="cntx">Authenticated caller context object. If cannot be null.</param>
        /// <returns>
        ///   Whether or not the call is successful.
        /// </returns>
        public async System.Threading.Tasks.Task<bool> SaveDatabaseAsync(CallContext cntx)
        {
            try
            {
                return await Channel.SaveDatabaseAsync(cntx);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return default(bool);
            }
        }
#endif


        private void HandleError(Exception ex)
        {
            if (DelHandleError != null)
                DelHandleError(ex);
            else
                throw new Exception("server exception", ex);
        }

        /// <summary>
        /// The data service requires that date and time string for a filter expression should be normalized in particular format and precision
        /// so that date and time comparison operation will be consistent.
        /// </summary>
        /// <param name="dt">The data and time object.</param>
        /// <param name="tc">The time coordinate. Available options are "Utc" and "Ltc" with the former referring to the Coordinated Universal Time and the 
        /// later the local time.</param>
        /// <returns>The normalized date and time string.</returns>
        public string FormatRepoDateTime(DateTime dt, string tc = "Utc")
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + " " + tc;
        }
    }
}