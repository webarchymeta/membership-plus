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
    /// A entity in "Applications" data set.
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
    ///      <term>ID</term>
    ///      <description>See <see cref="Application_.ID" />. Primary key; fixed; not null.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Intrinsic Identifiers</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>Name</term>
    ///      <description>See <see cref="Application_.Name" />. Intrinsic id; fixed; not null; max-length = 100 characters.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>Editable properties</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>DisplayName</term>
    ///      <description>See <see cref="Application_.DisplayName" />. Editable; nullable; max-length = 150 characters.</description>
    ///    </item>
    ///  </list>
    ///  <list type="table">
    ///    <listheader>
    ///       <term>The following entity sets depend on this entity</term><description>Description</description>
    ///    </listheader>
    ///    <item>
    ///      <term>Announcements</term>
    ///      <description>See <see cref="Application_.Announcements" />, which is a sub-set of the data set "Announcements" for <see cref="Announcement" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>Communications</term>
    ///      <description>See <see cref="Application_.Communications" />, which is a sub-set of the data set "Communications" for <see cref="Communication" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>EventCalendars</term>
    ///      <description>See <see cref="Application_.EventCalendars" />, which is a sub-set of the data set "EventCalendar" for <see cref="EventCalendar" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>MemberNotifications</term>
    ///      <description>See <see cref="Application_.MemberNotifications" />, which is a sub-set of the data set "MemberNotifications" for <see cref="MemberNotification" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>Roles</term>
    ///      <description>See <see cref="Application_.Roles" />, which is a sub-set of the data set "Roles" for <see cref="Role" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>ShortMessages</term>
    ///      <description>See <see cref="Application_.ShortMessages" />, which is a sub-set of the data set "ShortMessages" for <see cref="ShortMessage" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>SignalRHostStates</term>
    ///      <description>See <see cref="Application_.SignalRHostStates" />, which is a sub-set of the data set "SignalRHostStates" for <see cref="SignalRHostState" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>SignalRMessages</term>
    ///      <description>See <see cref="Application_.SignalRMessages" />, which is a sub-set of the data set "SignalRMessages" for <see cref="SignalRMessage" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserAppMembers</term>
    ///      <description>See <see cref="Application_.UserAppMembers" />, which is a sub-set of the data set "UserAppMembers" for <see cref="UserAppMember" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserDetails</term>
    ///      <description>See <see cref="Application_.UserDetails" />, which is a sub-set of the data set "UserDetails" for <see cref="UserDetail" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserGroups</term>
    ///      <description>See <see cref="Application_.UserGroups" />, which is a sub-set of the data set "UserGroups" for <see cref="UserGroup" />.</description>
    ///    </item>
    ///    <item>
    ///      <term>UserProfiles</term>
    ///      <description>See <see cref="Application_.UserProfiles" />, which is a sub-set of the data set "UserProfiles" for <see cref="UserProfile" />.</description>
    ///    </item>
    ///  </list>
    /// </remarks>
    [DataContract]
    [Serializable]
    public class Application_ : IDbEntity 
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
                return this.ID;
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
        /// Used to matching entities in input adding or updating entity list and the returned ones, see <see cref="IApplication_Service.AddOrUpdateEntities" />.
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
                str += "Name = " + Name + "\r\n";
                if (IsDisplayNameModified)
                    str += "Modified [DisplayName] = " + DisplayName + "\r\n";;
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
            return String.Format(@"{0}", Name.Trim());
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
        public Application_()
        {
        }

        /// <summary>
        /// Constructor for serialization (<see cref="ISerializable" />).
        /// </summary>
        public Application_(SerializationInfo info, StreamingContext context)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Application_));
            var strm = new System.IO.MemoryStream();
            byte[] bf = (byte[])info.GetValue("data", typeof(byte[]));
            strm.Write(bf, 0, bf.Length);
            strm.Position = 0;
            var e = ser.ReadObject(strm) as Application_;
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
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Application_));
            var strm = new System.IO.MemoryStream();
            ser.WriteObject(strm, ShallowCopy());
            info.AddValue("data", strm.ToArray(), typeof(byte[]));
        }

#endregion

#region Properties of the current entity

        /// <summary>
        /// Meta-info: primary key; fixed; not null.
        /// </summary>
        [Key]
        [Editable(false)]
        [DataMember(IsRequired = true)]
        public string ID
        { 
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                }
            }
        }
        private string _ID = default(string);

        /// <summary>
        /// Meta-info: intrinsic id; fixed; not null; max-length = 100 characters.
        /// </summary>
        [Key]
        [Required]
        [Editable(false)]
        [StringLength(100)]
        [DataMember(IsRequired = true)]
        public string Name
        { 
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                }
            }
        }
        private string _Name = default(string);

        /// <summary>
        /// Meta-info: editable; nullable; max-length = 150 characters.
        /// </summary>
        [Editable(true)]
        [StringLength(150)]
        [DataMember(IsRequired = false)]
        public string DisplayName
        { 
            get
            {
                return _DisplayName;
            }
            set
            {
                if (_DisplayName != value)
                {
                    _DisplayName = value;
                    if (StartAutoUpdating)
                        IsDisplayNameModified = true;
                }
            }
        }
        private string _DisplayName = default(string);

        /// <summary>
        /// Wether or not the value of <see cref="DisplayName" /> was changed compared to what it was loaded last time. 
        /// Note: the backend data source updates the changed <see cref="DisplayName" /> only if this is set to true no matter what
        /// the actual value of <see cref="DisplayName" /> is.
        /// </summary>
        [DataMember]
        public bool IsDisplayNameModified
        { 
            get
            {
                return _isDisplayNameModified;
            }
            set
            {
                _isDisplayNameModified = value;
            }
        }
        private bool _isDisplayNameModified = false;

#endregion

#region Entities that the current one depends upon.

#endregion

#region Entities that depend on the current one.

        /// <summary>
        /// Entitity set <see cref="AnnouncementSet" /> for data set "Announcements" of <see cref="Announcement" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="AnnouncementSet" /> set is { <see cref="Announcement.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public AnnouncementSet Announcements
		{
			get
			{
                if (_Announcements == null)
                    _Announcements = new AnnouncementSet();
				return _Announcements;
			}
            set
            {
                _Announcements = value;
            }
		}
		private AnnouncementSet _Announcements = null;

        /// <summary>
        /// Entitites enumeration expression for data set "Announcements" of <see cref="Announcement" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="AnnouncementSet" /> set is { <see cref="Announcement.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<Announcement> AnnouncementEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="Announcement" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="AnnouncementSet" /> set is { <see cref="Announcement.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public Announcement[] ChangedAnnouncements
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="CommunicationSet" /> for data set "Communications" of <see cref="Communication" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="CommunicationSet" /> set is { <see cref="Communication.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public CommunicationSet Communications
		{
			get
			{
                if (_Communications == null)
                    _Communications = new CommunicationSet();
				return _Communications;
			}
            set
            {
                _Communications = value;
            }
		}
		private CommunicationSet _Communications = null;

        /// <summary>
        /// Entitites enumeration expression for data set "Communications" of <see cref="Communication" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="CommunicationSet" /> set is { <see cref="Communication.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<Communication> CommunicationEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="Communication" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="CommunicationSet" /> set is { <see cref="Communication.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public Communication[] ChangedCommunications
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="EventCalendarSet" /> for data set "EventCalendar" of <see cref="EventCalendar" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="EventCalendarSet" /> set is { <see cref="EventCalendar.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public EventCalendarSet EventCalendars
		{
			get
			{
                if (_EventCalendars == null)
                    _EventCalendars = new EventCalendarSet();
				return _EventCalendars;
			}
            set
            {
                _EventCalendars = value;
            }
		}
		private EventCalendarSet _EventCalendars = null;

        /// <summary>
        /// Entitites enumeration expression for data set "EventCalendar" of <see cref="EventCalendar" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="EventCalendarSet" /> set is { <see cref="EventCalendar.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<EventCalendar> EventCalendarEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="EventCalendar" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="EventCalendarSet" /> set is { <see cref="EventCalendar.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public EventCalendar[] ChangedEventCalendars
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="MemberNotificationSet" /> for data set "MemberNotifications" of <see cref="MemberNotification" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="MemberNotificationSet" /> set is { <see cref="MemberNotification.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public MemberNotificationSet MemberNotifications
		{
			get
			{
                if (_MemberNotifications == null)
                    _MemberNotifications = new MemberNotificationSet();
				return _MemberNotifications;
			}
            set
            {
                _MemberNotifications = value;
            }
		}
		private MemberNotificationSet _MemberNotifications = null;

        /// <summary>
        /// Entitites enumeration expression for data set "MemberNotifications" of <see cref="MemberNotification" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="MemberNotificationSet" /> set is { <see cref="MemberNotification.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<MemberNotification> MemberNotificationEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="MemberNotification" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="MemberNotificationSet" /> set is { <see cref="MemberNotification.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public MemberNotification[] ChangedMemberNotifications
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="RoleSet" /> for data set "Roles" of <see cref="Role" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="RoleSet" /> set is { <see cref="Role.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public RoleSet Roles
		{
			get
			{
                if (_Roles == null)
                    _Roles = new RoleSet();
				return _Roles;
			}
            set
            {
                _Roles = value;
            }
		}
		private RoleSet _Roles = null;

        /// <summary>
        /// Entitites enumeration expression for data set "Roles" of <see cref="Role" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="RoleSet" /> set is { <see cref="Role.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<Role> RoleEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="Role" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="RoleSet" /> set is { <see cref="Role.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public Role[] ChangedRoles
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="ShortMessageSet" /> for data set "ShortMessages" of <see cref="ShortMessage" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="ShortMessageSet" /> set is { <see cref="ShortMessage.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public ShortMessageSet ShortMessages
		{
			get
			{
                if (_ShortMessages == null)
                    _ShortMessages = new ShortMessageSet();
				return _ShortMessages;
			}
            set
            {
                _ShortMessages = value;
            }
		}
		private ShortMessageSet _ShortMessages = null;

        /// <summary>
        /// Entitites enumeration expression for data set "ShortMessages" of <see cref="ShortMessage" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="ShortMessageSet" /> set is { <see cref="ShortMessage.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<ShortMessage> ShortMessageEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="ShortMessage" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="ShortMessageSet" /> set is { <see cref="ShortMessage.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public ShortMessage[] ChangedShortMessages
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="SignalRHostStateSet" /> for data set "SignalRHostStates" of <see cref="SignalRHostState" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="SignalRHostStateSet" /> set is { <see cref="SignalRHostState.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public SignalRHostStateSet SignalRHostStates
		{
			get
			{
                if (_SignalRHostStates == null)
                    _SignalRHostStates = new SignalRHostStateSet();
				return _SignalRHostStates;
			}
            set
            {
                _SignalRHostStates = value;
            }
		}
		private SignalRHostStateSet _SignalRHostStates = null;

        /// <summary>
        /// Entitites enumeration expression for data set "SignalRHostStates" of <see cref="SignalRHostState" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="SignalRHostStateSet" /> set is { <see cref="SignalRHostState.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<SignalRHostState> SignalRHostStateEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="SignalRHostState" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="SignalRHostStateSet" /> set is { <see cref="SignalRHostState.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public SignalRHostState[] ChangedSignalRHostStates
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="SignalRMessageSet" /> for data set "SignalRMessages" of <see cref="SignalRMessage" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="SignalRMessageSet" /> set is { <see cref="SignalRMessage.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public SignalRMessageSet SignalRMessages
		{
			get
			{
                if (_SignalRMessages == null)
                    _SignalRMessages = new SignalRMessageSet();
				return _SignalRMessages;
			}
            set
            {
                _SignalRMessages = value;
            }
		}
		private SignalRMessageSet _SignalRMessages = null;

        /// <summary>
        /// Entitites enumeration expression for data set "SignalRMessages" of <see cref="SignalRMessage" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="SignalRMessageSet" /> set is { <see cref="SignalRMessage.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<SignalRMessage> SignalRMessageEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="SignalRMessage" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="SignalRMessageSet" /> set is { <see cref="SignalRMessage.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public SignalRMessage[] ChangedSignalRMessages
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="UserAppMemberSet" /> for data set "UserAppMembers" of <see cref="UserAppMember" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserAppMemberSet" /> set is { <see cref="UserAppMember.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserAppMemberSet UserAppMembers
		{
			get
			{
                if (_UserAppMembers == null)
                    _UserAppMembers = new UserAppMemberSet();
				return _UserAppMembers;
			}
            set
            {
                _UserAppMembers = value;
            }
		}
		private UserAppMemberSet _UserAppMembers = null;

        /// <summary>
        /// Entitites enumeration expression for data set "UserAppMembers" of <see cref="UserAppMember" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserAppMemberSet" /> set is { <see cref="UserAppMember.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<UserAppMember> UserAppMemberEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="UserAppMember" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="UserAppMemberSet" /> set is { <see cref="UserAppMember.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserAppMember[] ChangedUserAppMembers
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="UserDetailSet" /> for data set "UserDetails" of <see cref="UserDetail" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserDetailSet" /> set is { <see cref="UserDetail.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserDetailSet UserDetails
		{
			get
			{
                if (_UserDetails == null)
                    _UserDetails = new UserDetailSet();
				return _UserDetails;
			}
            set
            {
                _UserDetails = value;
            }
		}
		private UserDetailSet _UserDetails = null;

        /// <summary>
        /// Entitites enumeration expression for data set "UserDetails" of <see cref="UserDetail" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserDetailSet" /> set is { <see cref="UserDetail.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<UserDetail> UserDetailEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="UserDetail" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="UserDetailSet" /> set is { <see cref="UserDetail.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserDetail[] ChangedUserDetails
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="UserGroupSet" /> for data set "UserGroups" of <see cref="UserGroup" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserGroupSet" /> set is { <see cref="UserGroup.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserGroupSet UserGroups
		{
			get
			{
                if (_UserGroups == null)
                    _UserGroups = new UserGroupSet();
				return _UserGroups;
			}
            set
            {
                _UserGroups = value;
            }
		}
		private UserGroupSet _UserGroups = null;

        /// <summary>
        /// Entitites enumeration expression for data set "UserGroups" of <see cref="UserGroup" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserGroupSet" /> set is { <see cref="UserGroup.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<UserGroup> UserGroupEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="UserGroup" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="UserGroupSet" /> set is { <see cref="UserGroup.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserGroup[] ChangedUserGroups
		{
			get;
            set;
		}

        /// <summary>
        /// Entitity set <see cref="UserProfileSet" /> for data set "UserProfiles" of <see cref="UserProfile" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserProfileSet" /> set is { <see cref="UserProfile.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserProfileSet UserProfiles
		{
			get
			{
                if (_UserProfiles == null)
                    _UserProfiles = new UserProfileSet();
				return _UserProfiles;
			}
            set
            {
                _UserProfiles = value;
            }
		}
		private UserProfileSet _UserProfiles = null;

        /// <summary>
        /// Entitites enumeration expression for data set "UserProfiles" of <see cref="UserProfile" /> that depend on the current entity.
        /// The corresponding foreign key in <see cref="UserProfileSet" /> set is { <see cref="UserProfile.ApplicationID" /> }.
        /// </summary>
		public IEnumerable<UserProfile> UserProfileEnum
		{
			get;
            set;
		}

        /// <summary>
        /// A list of <see cref="UserProfile" /> that is to be added or updated to the data source, together with the current entity.
        /// The corresponding foreign key in <see cref="UserProfileSet" /> set is { <see cref="UserProfile.ApplicationID" /> }.
        /// </summary>
        [DataMember]
		public UserProfile[] ChangedUserProfiles
		{
			get;
            set;
		}

#endregion

        /// <summary>
        /// Whether or not the present entity is identitical to <paramref name="other" />, in the sense that they have the same (set of) primary key(s).
        /// </summary>
        /// <param name="other">The entity to be compared to.</param>
        /// <returns>
        ///   The result of comparison.
        /// </returns>
        public bool IsEntityIdentical(Application_ other)
        {
            if (other == null)
                return false;
            if (ID != other.ID)
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
        public bool IsEntityTheSame(Application_ other)
        {
            if (other == null)
                return false;
            else
                return Name == other.Name;
        }              

        /// <summary>
        /// Merge changes inside entity <paramref name="from" /> to the entity <paramref name="to" />. Any changes in <paramref name="from" /> that is not changed in <paramref name="to" /> is updated inside <paramref name="to" />.
        /// </summary>
        /// <param name="from">The "old" entity acting as merging source.</param>
        /// <param name="to">The "new" entity which inherits changes made in <paramref name="from" />.</param>
        /// <returns>
        /// </returns>
        public static void MergeChanges(Application_ from, Application_ to)
        {
            if (to.IsPersisted)
            {
                if (from.IsDisplayNameModified && !to.IsDisplayNameModified)
                {
                    to.DisplayName = from.DisplayName;
                    to.IsDisplayNameModified = true;
                }
            }
            else
            {
                to.IsPersisted = from.IsPersisted;
                to.ID = from.ID;
                to.Name = from.Name;
                to.DisplayName = from.DisplayName;
                to.IsDisplayNameModified = from.IsDisplayNameModified;
            }
        }

        /// <summary>
        /// Update changes to the current entity compared to an input <paramref name="newdata" /> and set the entity to a proper state for updating.
        /// </summary>
        /// <param name="newdata">The "new" entity acting as the source of the changes, if any.</param>
        /// <returns>
        /// </returns>
        public void UpdateChanges(Application_ newdata)
        {
            int cnt = 0;
            if (DisplayName != newdata.DisplayName)
            {
                DisplayName = newdata.DisplayName;
                IsDisplayNameModified = true;
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
            if (Name == null)
                Name = "";
            if (!IsEntityChanged)
                IsEntityChanged = IsDisplayNameModified;
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
        public Application_ ShallowCopy(bool allData = false, bool preserveState = false)
        {
            Application_ e = new Application_();
            e.StartAutoUpdating = false;
            e.ID = ID;
            e.Name = Name;
            e.DisplayName = DisplayName;
            if (preserveState)
                e.IsDisplayNameModified = IsDisplayNameModified;
            else
                e.IsDisplayNameModified = false;
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
----===== [[Application_]] =====----
  ID = '" + ID + @"'
  Name = '" + Name + @"'");
            sb.Append(@" (natural id)");
            sb.Append(@"
  DisplayName = '" + (DisplayName != null ? DisplayName : "null") + @"'");
            if (IsDisplayNameModified)
                sb.Append(@" (modified)");
            else
                sb.Append(@" (unchanged)");
            sb.Append(@"
");
            return sb.ToString();
        }

    }

    ///<summary>
    ///The result of an add or update of type <see cref="Application_" />.
    ///</summary>
    [DataContract]
    [Serializable]
    public class Application_UpdateResult : IUpdateResult
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
        public Application_ UpdatedItem
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
        public Application_ ConflictItem
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
