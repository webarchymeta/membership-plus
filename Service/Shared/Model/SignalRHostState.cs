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
    /// A entity in "SignalRHostStates" data set.
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
    ///      <term>HostName</term>
    ///      <description>See <see cref="SignalRHostState.HostName" />. Primary key; intrinsic id; fixed; not null; max-length = 100 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="SignalRHostState.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Intrinsic Identifiers</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>HostName</term>
    ///      <description>See <see cref="SignalRHostState.HostName" />. Primary key; intrinsic id; fixed; not null; max-length = 100 characters.</description>
    ///    </item>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="SignalRHostState.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Editable properties</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>LastMsgId</term>
    ///      <description>See <see cref="SignalRHostState.LastMsgId" />. Editable; not null.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Foreign keys</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>ApplicationID</term>
    ///      <description>See <see cref="SignalRHostState.ApplicationID" />. Primary key; intrinsic id; fixed; not null; foreign key.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>This entity depends on</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>Application_Ref</term>
    ///      <description>See <see cref="SignalRHostState.Application_Ref" />, which is a member of the data set "Applications" for <see cref="Application_" />.</description>
    ///    </item>
    ///  </list>
    /// </remarks>
    [DataContract]
    [Serializable]
    public class SignalRHostState : IDbEntity 
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
                return this.ApplicationID + ":" + this.HostName;
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
        /// Used to matching entities in input adding or updating entity list and the returned ones, see <see cref="ISignalRHostStateService.AddOrUpdateEntities" />.
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
                str += "HostName = " + HostName + "\r\n";
                str += "ApplicationID = " + ApplicationID + "\r\n";
                if (IsLastMsgIdModified)
                    str += "Modified [LastMsgId] = " + LastMsgId + "\r\n";;
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
            return String.Format(@"{0} at {1}", HostName.Trim(), LastMsgId);
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
        public SignalRHostState()
        {
        }

        /// <summary>
        /// Constructor for serialization (<see cref="ISerializable" />).
        /// </summary>
        public SignalRHostState(SerializationInfo info, StreamingContext context)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SignalRHostState));
            var strm = new System.IO.MemoryStream();
            byte[] bf = (byte[])info.GetValue("data", typeof(byte[]));
            strm.Write(bf, 0, bf.Length);
            strm.Position = 0;
            var e = ser.ReadObject(strm) as SignalRHostState;
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
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SignalRHostState));
            var strm = new System.IO.MemoryStream();
            ser.WriteObject(strm, ShallowCopy());
            info.AddValue("data", strm.ToArray(), typeof(byte[]));
        }

#endregion

#region Properties of the current entity

        /// <summary>
        /// Meta-info: primary key; intrinsic id; fixed; not null; max-length = 100 characters.
        /// </summary>
        [Key]
        [Required]
        [Editable(false)]
        [StringLength(100)]
        [DataMember(IsRequired = true)]
        public string HostName
        { 
            get
            {
                return _HostName;
            }
            set
            {
                if (_HostName != value)
                {
                    _HostName = value;
                }
            }
        }
        private string _HostName = default(string);

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
        /// Meta-info: editable; not null.
        /// </summary>
        [Required]
        [Editable(true)]
        [DataMember(IsRequired = true)]
        public Int64 LastMsgId
        { 
            get
            {
                return _LastMsgId;
            }
            set
            {
                if (_LastMsgId != value)
                {
                    _LastMsgId = value;
                    if (StartAutoUpdating)
                        IsLastMsgIdModified = true;
                }
            }
        }
        private Int64 _LastMsgId = default(Int64);

        /// <summary>
        /// Wether or not the value of <see cref="LastMsgId" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="LastMsgId" /> only if this is set to true no matter what
        /// the actual value of <see cref="LastMsgId" /> is.
        /// </summary>
        [DataMember]
        public bool IsLastMsgIdModified
        { 
            get
            {
                return _isLastMsgIdModified;
            }
            set
            {
                _isLastMsgIdModified = value;
            }
        }
        private bool _isLastMsgIdModified = false;

#endregion

#region Entities that the current one depends upon.

        /// <summary>
        /// Entity in data set "Applications" for <see cref="Application_" /> that this entity depend upon through .
        /// The corresponding foreign key set is { <see cref="SignalRHostState.ApplicationID" /> }.
        /// </summary>
        [DataMember]
        public Application_ Application_Ref
        {
            get 
            {
                if (_Application_Ref == null && AutoLoadApplication_Ref != null)
                    _Application_Ref = AutoLoadApplication_Ref();
                return _Application_Ref; 
            }
            set 
            { 
                _Application_Ref = value; 
            }
        }
        private Application_ _Application_Ref = null;

        /// <summary>
        /// <see cref="SignalRHostState.Application_Ref" /> is not initialized when the entity is created. Clients could call this method to load it provided a proper delegate <see cref="SignalRHostState.DelLoadApplication_Ref" /> was setup
        /// before calling it.
        /// </summary>
        public void LoadApplication_Ref()
        {
            if (_Application_Ref != null)
                return;
            if (DelLoadApplication_Ref != null)
                _Application_Ref = DelLoadApplication_Ref();
        }

        /// <summary>
        /// A delegate to load <see cref="SignalRHostState.Application_Ref" />.
        /// </summary>
        public Func<Application_> DelLoadApplication_Ref = null;

        /// <summary>
        /// A delegate to load <see cref="SignalRHostState.Application_Ref" /> automatically when it is referred to at the first time.
        /// </summary>
        public Func<Application_> AutoLoadApplication_Ref = null;

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
        public bool IsEntityIdentical(SignalRHostState other)
        {
            if (other == null)
                return false;
            if (HostName != other.HostName)
                return false;
            if (ApplicationID != other.ApplicationID)
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
        public bool IsEntityTheSame(SignalRHostState other)
        {
            if (other == null)
                return false;
            else
                return HostName == other.HostName &&  ApplicationID == other.ApplicationID;
        }              

        /// <summary>
        /// Merge changes inside entity <paramref name="from" /> to the entity <paramref name="to" />. Any changes in <paramref name="from" /> that is not changed in <paramref name="to" /> is updated inside <paramref name="to" />.
        /// </summary>
        /// <param name="from">The "old" entity acting as merging source.</param>
        /// <param name="to">The "new" entity which inherits changes made in <paramref name="from" />.</param>
        /// <returns>
        /// </returns>
        public static void MergeChanges(SignalRHostState from, SignalRHostState to)
        {
            if (to.IsPersisted)
            {
                if (from.IsLastMsgIdModified && !to.IsLastMsgIdModified)
                {
                    to.LastMsgId = from.LastMsgId;
                    to.IsLastMsgIdModified = true;
                }
            }
            else
            {
                to.IsPersisted = from.IsPersisted;
                to.HostName = from.HostName;
                to.ApplicationID = from.ApplicationID;
                to.LastMsgId = from.LastMsgId;
                to.IsLastMsgIdModified = from.IsLastMsgIdModified;
            }
        }

        /// <summary>
        /// Update changes to the current entity compared to an input <paramref name="newdata" /> and set the entity to a proper state for updating.
        /// </summary>
        /// <param name="newdata">The "new" entity acting as the source of the changes, if any.</param>
        /// <returns>
        /// </returns>
        public void UpdateChanges(SignalRHostState newdata)
        {
            int cnt = 0;
            if (LastMsgId != newdata.LastMsgId)
            {
                LastMsgId = newdata.LastMsgId;
                IsLastMsgIdModified = true;
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
                IsEntityChanged = IsLastMsgIdModified;
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
        public SignalRHostState ShallowCopy(bool allData = false, bool preserveState = false, bool checkLoadState = false)
        {
            SignalRHostState e = new SignalRHostState();
            e.StartAutoUpdating = false;
            e.HostName = HostName;
            e.ApplicationID = ApplicationID;
            e.LastMsgId = LastMsgId;
            if (preserveState)
                e.IsLastMsgIdModified = IsLastMsgIdModified;
            else
                e.IsLastMsgIdModified = false;
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
----===== [[SignalRHostState]] =====----
  HostName = '" + HostName + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  ApplicationID = '" + ApplicationID + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  LastMsgId = " + LastMsgId + @"");
            if (IsLastMsgIdModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
");
            return sb.ToString();
        }

    }

    ///<summary>
    ///The result of an add or update of type <see cref="SignalRHostState" />.
    ///</summary>
    [DataContract]
    [Serializable]
    public class SignalRHostStateUpdateResult : IUpdateResult
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
        public SignalRHostState UpdatedItem
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
        public SignalRHostState ConflictItem
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
