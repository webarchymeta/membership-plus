//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool from CryptoGateway Software Inc.
//     Tool name: CGW X-Script RDB visual Layer Generator
//
//     Archymeta Information Technologies Co., Ltd.
//
//     Changes to this file, could be overwritten if the code is re-generated.
//     Add (if not yet) a code-manager node to the generator to specify 
//     how existing files are processed.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Json;

namespace CryptoGateway.RDB.Data.MembershipPlus
{
    /// <summary>
    /// A entity in "MemberCallbacks" data set.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///    Properties of the entity are categorized in the following:
    ///  </para>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Primary keys</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>ChannelID</term>
    ///      <description>See <see cref="MemberCallback.ChannelID" />. Primary key; intrinsic id; fixed; not null; max-length = 80 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>HubID</term>
    ///      <description>See <see cref="MemberCallback.HubID" />. Primary key; intrinsic id; fixed; not null; max-length = 50 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="MemberCallback.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserID</term>
    ///      <description>See <see cref="MemberCallback.UserID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Intrinsic Identifiers</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>ChannelID</term>
    ///      <description>See <see cref="MemberCallback.ChannelID" />. Primary key; intrinsic id; fixed; not null; max-length = 80 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>HubID</term>
    ///      <description>See <see cref="MemberCallback.HubID" />. Primary key; intrinsic id; fixed; not null; max-length = 50 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="MemberCallback.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserID</term>
    ///      <description>See <see cref="MemberCallback.UserID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Editable properties</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>IsDisconnected</term>
    ///      <description>See <see cref="MemberCallback.IsDisconnected" />. Editable; not null.</description>
    ///    </item>
    ///    <item>
    ///      <term>LastActiveDate</term>
    ///      <description>See <see cref="MemberCallback.LastActiveDate" />. Editable; not null.</description>
    ///    </item>
    ///    <item>
    ///      <term>ConnectionID</term>
    ///      <description>See <see cref="MemberCallback.ConnectionID" />. Editable; nullable; max-length = 50 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>SupervisorMode</term>
    ///      <description>See <see cref="MemberCallback.SupervisorMode" />. Editable; nullable.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Foreign keys</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="MemberCallback.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserID</term>
    ///      <description>See <see cref="MemberCallback.UserID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>This entity depends on</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>UserAppMemberRef</term>
    ///      <description>See <see cref="MemberCallback.UserAppMemberRef" />, which is a member of the data set "UserAppMembers" for <see cref="UserAppMember" />.</description>
    ///    </item>
    ///  </list>
    /// </remarks>
    [DataContract]
    [Serializable]
    public class MemberCallback : IDbEntity 
    {
        /// <summary>
        /// For internal use only.
        /// </summary>
        public bool IsOperationHandled = false;

        /// <summary>
        /// Used on the server side to return an unique key for caching purposes.
        /// </summary>
        public string CacheKey
        {
            get
            {
                return this.ApplicationID + ":" + this.ChannelID + ":" + this.HubID + ":" + this.UserID;
            }
        }

        /// <summary>
        /// Whether or not the entity was already persisted into to the data source. 
        /// </summary>
        [DataMember]
        public bool IsPersisted
        {
            get { return _isPersisted; }
            set { _isPersisted = value; }
        }
        private bool _isPersisted = false;

        /// <summary>
        /// Used internally.
        /// </summary>
        public bool StartAutoUpdating
        {
            get { return _startAutoUpdating; }
            set { _startAutoUpdating = value; }
        }
        private bool _startAutoUpdating = false;

        /// <summary>
        /// Used to matching entities in input adding or updating entity list and the returned ones, see <see cref="IMemberCallbackService.AddOrUpdateEntities" />.
        /// </summary>
        [DataMember]
        public int UpdateIndex
        {
            get { return _updateIndex; }
            set { _updateIndex = value; }
        }
        private int _updateIndex = -1;

        /// <summary>
        /// Its value provides a list of value for intrinsic keys and modified properties.
        /// </summary>
        public string SignatureString 
        { 
            get
            {
                string str = "";
                str += "ChannelID = " + ChannelID + "\r\n";
                str += "HubID = " + HubID + "\r\n";
                str += "ApplicationID = " + ApplicationID + "\r\n";
                str += "UserID = " + UserID + "\r\n";
                if (IsIsDisconnectedModified)
                    str += "Modified [IsDisconnected] = " + IsDisconnected + "\r\n";
                if (IsLastActiveDateModified)
                    str += "Modified [LastActiveDate] = " + LastActiveDate + "\r\n";
                if (IsConnectionIDModified)
                    str += "Modified [ConnectionID] = " + ConnectionID + "\r\n";
                if (IsSupervisorModeModified)
                    str += "Modified [SupervisorMode] = " + SupervisorMode + "\r\n";;
                return str.Trim();
            }
        }

        /// <summary>
        /// Configured at system generation step, its value provides a short, but characteristic summary of the entity.
        /// </summary>
        [DataMember]
        public string DistinctString
        {
            get 
            {
                if (_distinctString == null)
                    _distinctString = GetDistinctString(true);
                return _distinctString;
            }
            set
            {
                _distinctString = value;
            }
        }
        private string _distinctString = null;

        private string GetDistinctString(bool ShowPathInfo)
        {
            return String.Format(@"HubID = {0}, ApplicationID = {1}, UserID = {2}, ChannelID = {3}", HubID.Trim(), ApplicationID.Trim(), UserID.Trim(), ChannelID.Trim());
        }

        /// <summary>
        /// Whether or not the entity was edited.
        /// </summary>
        [DataMember]
        public bool IsEntityChanged
        {
            get { return _isEntityChanged; }
            set { _isEntityChanged = value; }
        }
        private bool _isEntityChanged = true;

        /// <summary>
        /// Whether or not the entity was to be deleted.
        /// </summary>
        [DataMember]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }
        private bool _isDeleted = false;

#region constructors and serialization

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MemberCallback()
        {
        }

        /// <summary>
        /// Constructor for serialization (<see cref="ISerializable" />).
        /// </summary>
        public MemberCallback(SerializationInfo info, StreamingContext context)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MemberCallback));
            var strm = new System.IO.MemoryStream();
            byte[] bf = (byte[])info.GetValue("data", typeof(byte[]));
            strm.Write(bf, 0, bf.Length);
            strm.Position = 0;
            var e = ser.ReadObject(strm) as MemberCallback;
            IsPersisted = false;
            StartAutoUpdating = false;
            MergeChanges(e, this);
            StartAutoUpdating = true;
        }

        /// <summary>
        /// Implementation of the <see cref="ISerializable" /> interface
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MemberCallback));
            var strm = new System.IO.MemoryStream();
            ser.WriteObject(strm, ShallowCopy());
            info.AddValue("data", strm.ToArray(), typeof(byte[]));
        }

#endregion

#region Properties of the current entity

        /// <summary>
        /// Meta-info: primary key; intrinsic id; fixed; not null; max-length = 80 characters.
        /// </summary>
        [Key]
        [Required]
        [Editable(false)]
        [StringLength(80)]
        [DataMember(IsRequired = true)]
        public string ChannelID
        { 
            get
            {
                return _ChannelID;
            }
            set
            {
                if (_ChannelID != value)
                {
                    _ChannelID = value;
                }
            }
        }
        private string _ChannelID = default(string);

        /// <summary>
        /// Meta-info: primary key; intrinsic id; fixed; not null; max-length = 50 characters.
        /// </summary>
        [Key]
        [Required]
        [Editable(false)]
        [StringLength(50)]
        [DataMember(IsRequired = true)]
        public string HubID
        { 
            get
            {
                return _HubID;
            }
            set
            {
                if (_HubID != value)
                {
                    _HubID = value;
                }
            }
        }
        private string _HubID = default(string);

        /// <summary>
        /// Meta-info: primary key; intrinsic id; fixed; not null; foreign key.
        /// </summary>
        [Key]
        [Editable(false)]
        [DataMember(IsRequired = true)]
        public string ApplicationID
        { 
            get
            {
                return _ApplicationID;
            }
            set
            {
                if (_ApplicationID != value)
                {
                    _ApplicationID = value;
                }
            }
        }
        private string _ApplicationID = default(string);

        /// <summary>
        /// Meta-info: primary key; intrinsic id; fixed; not null; foreign key.
        /// </summary>
        [Key]
        [Editable(false)]
        [DataMember(IsRequired = true)]
        public string UserID
        { 
            get
            {
                return _UserID;
            }
            set
            {
                if (_UserID != value)
                {
                    _UserID = value;
                }
            }
        }
        private string _UserID = default(string);

        /// <summary>
        /// Meta-info: editable; not null.
        /// </summary>
        [Required]
        [Editable(true)]
        [DataMember(IsRequired = true)]
        public bool IsDisconnected
        { 
            get
            {
                return _IsDisconnected;
            }
            set
            {
                if (_IsDisconnected != value)
                {
                    _IsDisconnected = value;
                    if (StartAutoUpdating)
                        IsIsDisconnectedModified = true;
                }
            }
        }
        private bool _IsDisconnected = default(bool);

        /// <summary>
        /// Wether or not the value of <see cref="IsDisconnected" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="IsDisconnected" /> only if this is set to true no matter what
        /// the actual value of <see cref="IsDisconnected" /> is.
        /// </summary>
        [DataMember]
        public bool IsIsDisconnectedModified
        { 
            get
            {
                return _isIsDisconnectedModified;
            }
            set
            {
                _isIsDisconnectedModified = value;
            }
        }
        private bool _isIsDisconnectedModified = false;

        /// <summary>
        /// Meta-info: editable; not null.
        /// </summary>
        [Required]
        [Editable(true)]
        [DataMember(IsRequired = true)]
        public DateTime LastActiveDate
        { 
            get
            {
                return _LastActiveDate;
            }
            set
            {
                if (_LastActiveDate != value)
                {
                    _LastActiveDate = value;
                    if (StartAutoUpdating)
                        IsLastActiveDateModified = true;
                }
            }
        }
        private DateTime _LastActiveDate = default(DateTime);

        /// <summary>
        /// Wether or not the value of <see cref="LastActiveDate" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="LastActiveDate" /> only if this is set to true no matter what
        /// the actual value of <see cref="LastActiveDate" /> is.
        /// </summary>
        [DataMember]
        public bool IsLastActiveDateModified
        { 
            get
            {
                return _isLastActiveDateModified;
            }
            set
            {
                _isLastActiveDateModified = value;
            }
        }
        private bool _isLastActiveDateModified = false;

        /// <summary>
        /// Meta-info: editable; nullable; max-length = 50 characters.
        /// </summary>
        [Editable(true)]
        [StringLength(50)]
        [DataMember(IsRequired = false)]
        public string ConnectionID
        { 
            get
            {
                return _ConnectionID;
            }
            set
            {
                if (_ConnectionID != value)
                {
                    _ConnectionID = value;
                    if (StartAutoUpdating)
                        IsConnectionIDModified = true;
                }
            }
        }
        private string _ConnectionID = default(string);

        /// <summary>
        /// Wether or not the value of <see cref="ConnectionID" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="ConnectionID" /> only if this is set to true no matter what
        /// the actual value of <see cref="ConnectionID" /> is.
        /// </summary>
        [DataMember]
        public bool IsConnectionIDModified
        { 
            get
            {
                return _isConnectionIDModified;
            }
            set
            {
                _isConnectionIDModified = value;
            }
        }
        private bool _isConnectionIDModified = false;

        /// <summary>
        /// Meta-info: editable; nullable.
        /// </summary>
        [Editable(true)]
        [DataMember(IsRequired = false)]
        public System.Nullable<bool> SupervisorMode
        { 
            get
            {
                return _SupervisorMode;
            }
            set
            {
                if (_SupervisorMode != value)
                {
                    _SupervisorMode = value;
                    if (StartAutoUpdating)
                        IsSupervisorModeModified = true;
                }
            }
        }
        private System.Nullable<bool> _SupervisorMode = default(System.Nullable<bool>);

        /// <summary>
        /// Wether or not the value of <see cref="SupervisorMode" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="SupervisorMode" /> only if this is set to true no matter what
        /// the actual value of <see cref="SupervisorMode" /> is.
        /// </summary>
        [DataMember]
        public bool IsSupervisorModeModified
        { 
            get
            {
                return _isSupervisorModeModified;
            }
            set
            {
                _isSupervisorModeModified = value;
            }
        }
        private bool _isSupervisorModeModified = false;

#endregion

#region Entities that the current one depends upon.

        /// <summary>
        /// Entity in data set "UserAppMembers" for <see cref="UserAppMember" /> that this entity depend upon through .
        /// The corresponding foreign key set is { <see cref="MemberCallback.ApplicationID" />, <see cref="MemberCallback.UserID" /> }.
        /// </summary>
        [DataMember]
        public UserAppMember UserAppMemberRef
        {
            get 
            {
                if (_UserAppMemberRef == null && AutoLoadUserAppMemberRef != null)
                    _UserAppMemberRef = AutoLoadUserAppMemberRef();
                return _UserAppMemberRef; 
            }
            set 
            { 
                _UserAppMemberRef = value; 
            }
        }
        private UserAppMember _UserAppMemberRef = null;

        /// <summary>
        /// <see cref="MemberCallback.UserAppMemberRef" /> is not initialized when the entity is created. Clients could call this method to load it provided a proper delegate <see cref="MemberCallback.DelLoadUserAppMemberRef" /> was setup
        /// before calling it.
        /// </summary>
        public void LoadUserAppMemberRef()
        {
            if (_UserAppMemberRef != null)
                return;
            if (DelLoadUserAppMemberRef != null)
                _UserAppMemberRef = DelLoadUserAppMemberRef();
        }

        /// <summary>
        /// A delegate to load <see cref="MemberCallback.UserAppMemberRef" />.
        /// </summary>
        public Func<UserAppMember> DelLoadUserAppMemberRef = null;

        /// <summary>
        /// A delegate to load <see cref="MemberCallback.UserAppMemberRef" /> automatically when it is referred to at the first time.
        /// </summary>
        public Func<UserAppMember> AutoLoadUserAppMemberRef = null;

#endregion

#region Entities that depend on the current one.

#endregion

        /// <summary>
        /// Whether or not the present entity is identitical to <paramref name="other" />, in the sense that they have the same (set of) primary key(s).
        /// </summary>
        /// <param name="other">The entity to be compared to.</param>
        /// <returns>
        ///   The result of comparison.
        /// </returns>
        public bool IsEntityIdentical(MemberCallback other)
        {
            if (other == null)
                return false;
            if (ChannelID != other.ChannelID)
                return false;
            if (HubID != other.HubID)
                return false;
            if (ApplicationID != other.ApplicationID)
                return false;
            if (UserID != other.UserID)
                return false;
            return true;
        }              

        /// <summary>
        /// Whether or not the present entity is identitical to <paramref name="other" />, in the sense that they have the same (set of) intrinsic identifiers.
        /// </summary>
        /// <param name="other">The entity to be compared to.</param>
        /// <returns>
        ///   The result of comparison.
        /// </returns>
        public bool IsEntityTheSame(MemberCallback other)
        {
            if (other == null)
                return false;
            else
                return ChannelID == other.ChannelID &&  HubID == other.HubID &&  ApplicationID == other.ApplicationID &&  UserID == other.UserID;
        }              

        /// <summary>
        /// Merge changes inside entity <paramref name="from" /> to the entity <paramref name="to" />. Any changes in <paramref name="from" /> that is not changed in <paramref name="to" /> is updated inside <paramref name="to" />.
        /// </summary>
        /// <param name="from">The "old" entity acting as merging source.</param>
        /// <param name="to">The "new" entity which inherits changes made in <paramref name="from" />.</param>
        /// <returns>
        /// </returns>
        public static void MergeChanges(MemberCallback from, MemberCallback to)
        {
            if (to.IsPersisted)
            {
                if (from.IsIsDisconnectedModified && !to.IsIsDisconnectedModified)
                {
                    to.IsDisconnected = from.IsDisconnected;
                    to.IsIsDisconnectedModified = true;
                }
                if (from.IsLastActiveDateModified && !to.IsLastActiveDateModified)
                {
                    to.LastActiveDate = from.LastActiveDate;
                    to.IsLastActiveDateModified = true;
                }
                if (from.IsConnectionIDModified && !to.IsConnectionIDModified)
                {
                    to.ConnectionID = from.ConnectionID;
                    to.IsConnectionIDModified = true;
                }
                if (from.IsSupervisorModeModified && !to.IsSupervisorModeModified)
                {
                    to.SupervisorMode = from.SupervisorMode;
                    to.IsSupervisorModeModified = true;
                }
            }
            else
            {
                to.IsPersisted = from.IsPersisted;
                to.ChannelID = from.ChannelID;
                to.HubID = from.HubID;
                to.ApplicationID = from.ApplicationID;
                to.UserID = from.UserID;
                to.IsDisconnected = from.IsDisconnected;
                to.IsIsDisconnectedModified = from.IsIsDisconnectedModified;
                to.LastActiveDate = from.LastActiveDate;
                to.IsLastActiveDateModified = from.IsLastActiveDateModified;
                to.ConnectionID = from.ConnectionID;
                to.IsConnectionIDModified = from.IsConnectionIDModified;
                to.SupervisorMode = from.SupervisorMode;
                to.IsSupervisorModeModified = from.IsSupervisorModeModified;
            }
        }

        /// <summary>
        /// Update changes to the current entity compared to an input <paramref name="newdata" /> and set the entity to a proper state for updating.
        /// </summary>
        /// <param name="newdata">The "new" entity acting as the source of the changes, if any.</param>
        /// <returns>
        /// </returns>
        public void UpdateChanges(MemberCallback newdata)
        {
            int cnt = 0;
            if (IsDisconnected != newdata.IsDisconnected)
            {
                IsDisconnected = newdata.IsDisconnected;
                IsIsDisconnectedModified = true;
                cnt++;
            }
            if (LastActiveDate != newdata.LastActiveDate)
            {
                LastActiveDate = newdata.LastActiveDate;
                IsLastActiveDateModified = true;
                cnt++;
            }
            if (ConnectionID != newdata.ConnectionID)
            {
                ConnectionID = newdata.ConnectionID;
                IsConnectionIDModified = true;
                cnt++;
            }
            if (SupervisorMode != newdata.SupervisorMode)
            {
                SupervisorMode = newdata.SupervisorMode;
                IsSupervisorModeModified = true;
                cnt++;
            }
            IsEntityChanged = cnt > 0;
        }

        /// <summary>
        /// Internal use
        /// </summary>
        public void NormalizeValues()
        {
            StartAutoUpdating = false;
            if (!IsEntityChanged)
                IsEntityChanged = IsIsDisconnectedModified || IsLastActiveDateModified || IsConnectionIDModified || IsSupervisorModeModified;
            StartAutoUpdating = true;
        }

        /// <summary>
        /// Make a shallow copy of the entity.
        /// </summary>
        IDbEntity IDbEntity.ShallowCopy(bool preserveState)
        {
            return ShallowCopy(false, preserveState);
        }

        /// <summary>
        /// Internal use
        /// </summary>
        public MemberCallback ShallowCopy(bool allData = false, bool preserveState = false)
        {
            MemberCallback e = new MemberCallback();
            e.StartAutoUpdating = false;
            e.ChannelID = ChannelID;
            e.HubID = HubID;
            e.ApplicationID = ApplicationID;
            e.UserID = UserID;
            e.IsDisconnected = IsDisconnected;
            if (preserveState)
                e.IsIsDisconnectedModified = IsIsDisconnectedModified;
            else
                e.IsIsDisconnectedModified = false;
            e.LastActiveDate = LastActiveDate;
            if (preserveState)
                e.IsLastActiveDateModified = IsLastActiveDateModified;
            else
                e.IsLastActiveDateModified = false;
            e.ConnectionID = ConnectionID;
            if (preserveState)
                e.IsConnectionIDModified = IsConnectionIDModified;
            else
                e.IsConnectionIDModified = false;
            e.SupervisorMode = SupervisorMode;
            if (preserveState)
                e.IsSupervisorModeModified = IsSupervisorModeModified;
            else
                e.IsSupervisorModeModified = false;
            e.DistinctString = GetDistinctString(true);
            e.IsPersisted = IsPersisted;
            if (preserveState)
                e.IsEntityChanged = IsEntityChanged;
            else
                e.IsEntityChanged = false;
            e.StartAutoUpdating = true;
            return e;
        }

        /// <summary>
        /// A textual representation of the entity.
        /// </summary>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(@"
----===== [[MemberCallback]] =====----
  ChannelID = '" + ChannelID + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  HubID = '" + HubID + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  ApplicationID = '" + ApplicationID + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  UserID = '" + UserID + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  IsDisconnected = " + IsDisconnected + @"");
            if (IsIsDisconnectedModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
  LastActiveDate = " + LastActiveDate + @"");
            if (IsLastActiveDateModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
  ConnectionID = '" + (ConnectionID != null ? ConnectionID : "null") + @"'");
            if (IsConnectionIDModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
  SupervisorMode = " + (SupervisorMode.HasValue ? SupervisorMode.Value.ToString() : "null") + @"");
            if (IsSupervisorModeModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
");
            return sb.ToString();
        }

    }

    ///<summary>
    ///The result of an add or update of type <see cref="MemberCallback" />.
    ///</summary>
    [DataContract]
    [Serializable]
    public class MemberCallbackUpdateResult : IUpdateResult
    {
        /// <summary>
        /// Status of add, update or delete operation
        /// </summary>
        [DataMember]
        public int OpStatus
        {
            get { return _opStatus; }
            set { _opStatus = value; }
        }
        private int _opStatus = (int)EntityOpStatus.Unknown;

        /// <summary>
        /// Parents or child operation status
        /// </summary>
        [DataMember]
        public int RelatedOpStatus
        {
            get { return _relatedOpStatus; }
            set { _relatedOpStatus = value; }
        }
        private int _relatedOpStatus = (int)EntityOpStatus.Unknown;

        /// <summary>
        /// The updated entity.
        /// </summary>
        [DataMember]
        public MemberCallback UpdatedItem
        {
            get;
            set;
        }

        /// <summary>
        /// If the relational data source has a way of detecting concurrent update conflicts, this is the item inside the
        /// data source that had been updated by other agents in between the load and update time interval of the present
        /// agent. The client software should resolve the conflict.
        /// </summary>
        [DataMember]
        public MemberCallback ConflictItem
        {
            get;
            set;
        }

        /// <summary>
        /// String representation of the entity.
        /// </summary>
        public string EntityInfo 
        { 
            get { return UpdatedItem == null ? "NULL" : UpdatedItem.ToString(); }
        }
    }

}
