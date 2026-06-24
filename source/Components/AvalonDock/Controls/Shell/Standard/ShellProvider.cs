/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Text;

	using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

	/// <summary>Specifies how shell item attributes are combined.</summary>
	internal enum SIATTRIBFLAGS
	{
		/// <summary>The A N D value.</summary>
		AND = 0x00000001,

		/// <summary>The O R value.</summary>
		OR = 0x00000002,

		/// <summary>The A P P C O M P A T value.</summary>
		APPCOMPAT = 0x00000003,
	}

	/// <summary>Specifies the type of application document list.</summary>
	internal enum APPDOCLISTTYPE
	{
		/// <summary>The recently used documents list.</summary>
		ADLT_RECENT = 0,   // The recently used documents list

		/// <summary>The frequently used documents list.</summary>
		ADLT_FREQUENT,     // The frequently used documents list
	}

	/// <summary>Specifies taskbar tab properties.</summary>
	[Flags]
	internal enum STPF
	{
		/// <summary>The N O N E value.</summary>
		NONE = 0x00000000,

		/// <summary>The U S E A P P T H U M B N A I L A L W A Y S value.</summary>
		USEAPPTHUMBNAILALWAYS = 0x00000001,

		/// <summary>The U S E A P P T H U M B N A I L W H E N A C T I V E value.</summary>
		USEAPPTHUMBNAILWHENACTIVE = 0x00000002,

		/// <summary>The U S E A P P P E E K A L W A Y S value.</summary>
		USEAPPPEEKALWAYS = 0x00000004,

		/// <summary>The U S E A P P P E E K W H E N A C T I V E value.</summary>
		USEAPPPEEKWHENACTIVE = 0x00000008,
	}

	/// <summary>Specifies the taskbar progress state.</summary>
	internal enum TBPF
	{
		/// <summary>The N O P R O G R E S S value.</summary>
		NOPROGRESS = 0x00000000,

		/// <summary>The I N D E T E R M I N A T E value.</summary>
		INDETERMINATE = 0x00000001,

		/// <summary>The N O R M A L value.</summary>
		NORMAL = 0x00000002,

		/// <summary>The E R R O R value.</summary>
		ERROR = 0x00000004,

		/// <summary>The P A U S E D value.</summary>
		PAUSED = 0x00000008,
	}

	/// <summary>Specifies thumbnail toolbar button fields.</summary>
	[Flags]
	internal enum THB : uint
	{
		/// <summary>The B I T M A P value.</summary>
		BITMAP = 0x0001,

		/// <summary>The I C O N value.</summary>
		ICON = 0x0002,

		/// <summary>The T O O L T I P value.</summary>
		TOOLTIP = 0x0004,

		/// <summary>The F L A G S value.</summary>
		FLAGS = 0x0008,
	}

	/// <summary>Specifies thumbnail toolbar button options.</summary>
	[Flags]
	internal enum THBF : uint
	{
		/// <summary>The E N A B L E D value.</summary>
		ENABLED = 0x0000,

		/// <summary>The D I S A B L E D value.</summary>
		DISABLED = 0x0001,

		/// <summary>The D I S M I S S O N C L I C K value.</summary>
		DISMISSONCLICK = 0x0002,

		/// <summary>The N O B A C K G R O U N D value.</summary>
		NOBACKGROUND = 0x0004,

		/// <summary>The H I D D E N value.</summary>
		HIDDEN = 0x0008,

		// Added post-beta

		/// <summary>The N O N I N T E R A C T I V E value.</summary>
		NONINTERACTIVE = 0x0010,
	}

	/// <summary>Specifies property store retrieval options.</summary>
	internal enum GPS
	{
		// If no flags are specified (GPS_DEFAULT), a read-only property store is returned that includes properties for the file or item.
		// In the case that the shell item is a file, the property store contains:
		//     1. properties about the file from the file system
		//     2. properties from the file itself provided by the file's property handler, unless that file is offline,
		//         see GPS_OPENSLOWITEM
		//     3. if requested by the file's property handler and supported by the file system, properties stored in the
		//     alternate property store.
		//
		// Non-file shell items should return a similar read-only store
		//
		// Specifying other GPS_ flags modifies the store that is returned

		/// <summary>The D E F A U L T value.</summary>
		DEFAULT = 0x00000000,

		/// <summary>Only include properties directly from the file's property handler.</summary>
		HANDLERPROPERTIESONLY = 0x00000001,   // only include properties directly from the file's property handler

		/// <summary>Writable stores will only include handler properties.</summary>
		READWRITE = 0x00000002,   // Writable stores will only include handler properties

		/// <summary>A read/write store that only holds properties for the lifetime of the IShellItem object.</summary>
		TEMPORARY = 0x00000004,   // A read/write store that only holds properties for the lifetime of the IShellItem object

		/// <summary>Do not include any properties from the file's property handler (because the file's property handler will hit the disk).</summary>
		FASTPROPERTIESONLY = 0x00000008,   // do not include any properties from the file's property handler (because the file's property handler will hit the disk)

		/// <summary>Include properties from a file's property handler, even if it means retrieving the file from offline storage.</summary>
		OPENSLOWITEM = 0x00000010,   // include properties from a file's property handler, even if it means retrieving the file from offline storage.

		/// <summary>Delay the creation of the file's property handler until those properties are read, written, or enumerated.</summary>
		DELAYCREATION = 0x00000020,   // delay the creation of the file's property handler until those properties are read, written, or enumerated

		/// <summary>For readonly stores, succeed and return all available properties, even if one or more sources of properties fails. Not valid with GPS_READWRITE.</summary>
		BESTEFFORT = 0x00000040,   // For readonly stores, succeed and return all available properties, even if one or more sources of properties fails. Not valid with GPS_READWRITE.

		/// <summary>Some data sources protect the read property store with an oplock, this disables that.</summary>
		NO_OPLOCK = 0x00000080,   // some data sources protect the read property store with an oplock, this disables that

		/// <summary>The MASK VALID value.</summary>
		MASK_VALID = 0x000000FF,
	}

	/// <summary>Specifies a known destination category.</summary>
	internal enum KDC
	{
		/// <summary>The F R E Q U E N T value.</summary>
		FREQUENT = 1,

		/// <summary>The R E C E N T value.</summary>
		RECENT,
	}

	// IShellFolder::GetAttributesOf flags

	/// <summary>Specifies shell folder attributes.</summary>
	[Flags]
	internal enum SFGAO : uint
	{
		/// <summary>The C A N C O P Y value.</summary>
		CANCOPY = 0x1,

		/// <summary>The C A N M O V E value.</summary>
		CANMOVE = 0x2,

		/// <summary>The C A N L I N K value.</summary>
		CANLINK = 0x4,

		/// <summary>The S T O R A G E value.</summary>
		STORAGE = 0x00000008,

		/// <summary>The C A N R E N A M E value.</summary>
		CANRENAME = 0x00000010,

		/// <summary>The C A N D E L E T E value.</summary>
		CANDELETE = 0x00000020,

		/// <summary>The H A S P R O P S H E E T value.</summary>
		HASPROPSHEET = 0x00000040,

		// unused = 0x00000080,

		/// <summary>The D R O P T A R G E T value.</summary>
		DROPTARGET = 0x00000100,

		/// <summary>The C A P A B I L I T Y M A S K value.</summary>
		CAPABILITYMASK = 0x00000177,

		// unused = 0x00000200,
		// unused = 0x00000400,
		// unused = 0x00000800,
		// unused = 0x00001000,

		/// <summary>The E N C R Y P T E D value.</summary>
		ENCRYPTED = 0x00002000,

		/// <summary>The I S S L O W value.</summary>
		ISSLOW = 0x00004000,

		/// <summary>The G H O S T E D value.</summary>
		GHOSTED = 0x00008000,

		/// <summary>The L I N K value.</summary>
		LINK = 0x00010000,

		/// <summary>The S H A R E value.</summary>
		SHARE = 0x00020000,

		/// <summary>The R E A D O N L Y value.</summary>
		READONLY = 0x00040000,

		/// <summary>The H I D D E N value.</summary>
		HIDDEN = 0x00080000,

		/// <summary>The D I S P L A Y A T T R M A S K value.</summary>
		DISPLAYATTRMASK = 0x000FC000,

		/// <summary>The F I L E S Y S A N C E S T O R value.</summary>
		FILESYSANCESTOR = 0x10000000,

		/// <summary>The F O L D E R value.</summary>
		FOLDER = 0x20000000,

		/// <summary>The F I L E S Y S T E M value.</summary>
		FILESYSTEM = 0x40000000,

		/// <summary>The H A S S U B F O L D E R value.</summary>
		HASSUBFOLDER = 0x80000000,

		/// <summary>The C O N T E N T S M A S K value.</summary>
		CONTENTSMASK = 0x80000000,

		/// <summary>The V A L I D A T E value.</summary>
		VALIDATE = 0x01000000,

		/// <summary>The R E M O V A B L E value.</summary>
		REMOVABLE = 0x02000000,

		/// <summary>The C O M P R E S S E D value.</summary>
		COMPRESSED = 0x04000000,

		/// <summary>The B R O W S A B L E value.</summary>
		BROWSABLE = 0x08000000,

		/// <summary>The N O N E N U M E R A T E D value.</summary>
		NONENUMERATED = 0x00100000,

		/// <summary>The N E W C O N T E N T value.</summary>
		NEWCONTENT = 0x00200000,

		/// <summary>The C A N M O N I K E R value.</summary>
		CANMONIKER = 0x00400000,

		/// <summary>The H A S S T O R A G E value.</summary>
		HASSTORAGE = 0x00400000,

		/// <summary>The S T R E A M value.</summary>
		STREAM = 0x00400000,

		/// <summary>The S T O R A G E A N C E S T O R value.</summary>
		STORAGEANCESTOR = 0x00800000,

		/// <summary>The S T O R A G E C A P M A S K value.</summary>
		STORAGECAPMASK = 0x70C50008,

		/// <summary>The P K E Y S F G A O M A S K value.</summary>
		PKEYSFGAOMASK = 0x81044000,
	}

	/// <summary>Specifies shell folder enumeration options.</summary>
	internal enum SHCONTF
	{
		/// <summary>Hint that client is checking if (what) child items the folder contains - not all details (e.g. short file name) are needed.</summary>
		CHECKING_FOR_CHILDREN = 0x0010,   // hint that client is checking if (what) child items the folder contains - not all details (e.g. short file name) are needed

		/// <summary>Only want folders enumerated (SFGAO_FOLDER).</summary>
		FOLDERS = 0x0020,   // only want folders enumerated (SFGAO_FOLDER)

		/// <summary>Include non folders (items without SFGAO_FOLDER).</summary>
		NONFOLDERS = 0x0040,   // include non folders (items without SFGAO_FOLDER)

		/// <summary>Show items normally hidden (items with SFGAO_HIDDEN).</summary>
		INCLUDEHIDDEN = 0x0080,   // show items normally hidden (items with SFGAO_HIDDEN)

		/// <summary>DEFUNCT - this is always assumed.</summary>
		INIT_ON_FIRST_NEXT = 0x0100,   // DEFUNCT - this is always assumed

		/// <summary>Hint that client is looking for printers.</summary>
		NETPRINTERSRCH = 0x0200,   // hint that client is looking for printers

		/// <summary>Hint that client is looking sharable resources (local drives or hidden root shares).</summary>
		SHAREABLE = 0x0400,   // hint that client is looking sharable resources (local drives or hidden root shares)

		/// <summary>Include all items with accessible storage and their ancestors.</summary>
		STORAGE = 0x0800,   // include all items with accessible storage and their ancestors

		/// <summary>Mark child folders to indicate that they should provide a "navigation" enumeration by default.</summary>
		NAVIGATION_ENUM = 0x1000,   // mark child folders to indicate that they should provide a "navigation" enumeration by default

		/// <summary>Hint that client is only interested in items that can be enumerated quickly.</summary>
		FASTITEMS = 0x2000,   // hint that client is only interested in items that can be enumerated quickly

		/// <summary>Enumerate items as flat list even if folder is stacked.</summary>
		FLATLIST = 0x4000,   // enumerate items as flat list even if folder is stacked

		/// <summary>Inform enumerator that client is listening for change notifications so enumerator does not need to be complete, items can be reported via change notifications.</summary>
		ENABLE_ASYNC = 0x8000,   // inform enumerator that client is listening for change notifications so enumerator does not need to be complete, items can be reported via change notifications
	}

	/// <summary>Specifies shell display name options.</summary>
	[Flags]
	internal enum SHGDN
	{
		/// <summary>Default (display purpose).</summary>
		SHGDN_NORMAL = 0x0000,  // default (display purpose)

		/// <summary>Displayed under a folder (relative).</summary>
		SHGDN_INFOLDER = 0x0001,  // displayed under a folder (relative)

		/// <summary>For in-place editing.</summary>
		SHGDN_FOREDITING = 0x1000,  // for in-place editing

		/// <summary>UI friendly parsing name (remove ugly stuff).</summary>
		SHGDN_FORADDRESSBAR = 0x4000,  // UI friendly parsing name (remove ugly stuff)

		/// <summary>Parsing name for ParseDisplayName().</summary>
		SHGDN_FORPARSING = 0x8000,  // parsing name for ParseDisplayName()
	}

	/// <summary>Specifies shell item comparison hints.</summary>
	internal enum SICHINT : uint
	{
		/// <summary>The D I S P L A Y value.</summary>
		DISPLAY = 0x00000000,

		/// <summary>The A L L F I E L D S value.</summary>
		ALLFIELDS = 0x80000000,

		/// <summary>The C A N O N I C A L value.</summary>
		CANONICAL = 0x10000000,

		/// <summary>The TEST FILESYSPATH IF NOT EQUAL value.</summary>
		TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
	}

	/// <summary>Specifies shell item display name formats.</summary>
	internal enum SIGDN : uint
	{
		// lower word (& with 0xFFFF)

		/// <summary>SHGDN_NORMAL.</summary>
		NORMALDISPLAY = 0x00000000, // SHGDN_NORMAL

		/// <summary>SHGDN_INFOLDER | SHGDN_FORPARSING.</summary>
		PARENTRELATIVEPARSING = 0x80018001, // SHGDN_INFOLDER | SHGDN_FORPARSING

		/// <summary>SHGDN_FORPARSING.</summary>
		DESKTOPABSOLUTEPARSING = 0x80028000, // SHGDN_FORPARSING

		/// <summary>SHGDN_INFOLDER | SHGDN_FOREDITING.</summary>
		PARENTRELATIVEEDITING = 0x80031001, // SHGDN_INFOLDER | SHGDN_FOREDITING

		/// <summary>SHGDN_FORPARSING | SHGDN_FORADDRESSBAR.</summary>
		DESKTOPABSOLUTEEDITING = 0x8004c000, // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR

		/// <summary>SHGDN_FORPARSING.</summary>
		FILESYSPATH = 0x80058000, // SHGDN_FORPARSING

		/// <summary>SHGDN_FORPARSING.</summary>
		URL = 0x80068000, // SHGDN_FORPARSING

		/// <summary>SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR.</summary>
		PARENTRELATIVEFORADDRESSBAR = 0x8007c001, // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR

		/// <summary>SHGDN_INFOLDER.</summary>
		PARENTRELATIVE = 0x80080001, // SHGDN_INFOLDER
	}

	/// <summary>Provides string constants for property store options.</summary>
	internal static class STR_GPS
	{
		/// <summary>String constant for handler-only properties.</summary>
		public const string HANDLERPROPERTIESONLY = "GPS_HANDLERPROPERTIESONLY";

		/// <summary>String constant for fast properties only.</summary>
		public const string FASTPROPERTIESONLY = "GPS_FASTPROPERTIESONLY";

		/// <summary>String constant for opening slow items.</summary>
		public const string OPENSLOWITEM = "GPS_OPENSLOWITEM";

		/// <summary>String constant for delayed property handler creation.</summary>
		public const string DELAYCREATION = "GPS_DELAYCREATION";

		/// <summary>String constant for best-effort property retrieval.</summary>
		public const string BESTEFFORT = "GPS_BESTEFFORT";

		/// <summary>String constant for disabling oplocks.</summary>
		public const string NO_OPLOCK = "GPS_NO_OPLOCK";
	}

	/// <summary>Thumbnail toolbar button structure.</summary>
	[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
	internal struct THUMBBUTTON
	{
		/// <summary>Message identifier for a clicked thumbnail button.</summary>
		public const int THBN_CLICKED = 0x1800;

		/// <summary>Specifies which members contain valid data.</summary>
		public THB dwMask;

		/// <summary>The button identifier.</summary>
		public uint iId;

		/// <summary>The bitmap index.</summary>
		public uint iBitmap;

		/// <summary>The icon handle.</summary>
		public IntPtr hIcon;

		/// <summary>The tooltip text.</summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szTip;

		/// <summary>The button state flags.</summary>
		public THBF dwFlags;
	}

	/// <summary>Property key structure.</summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct PKEY
	{
		/// <summary>The format identifier.</summary>
		private readonly Guid _fmtid;

		/// <summary>The property identifier.</summary>
		private readonly uint _pid;

		/// <summary>Initializes a new instance of the <see cref="PKEY"/> struct.</summary>
		/// <param name="fmtid">The format identifier.</param>
		/// <param name="pid">The property identifier.</param>
		public PKEY(Guid fmtid, uint pid)
		{
			_fmtid = fmtid;
			_pid = pid;
		}

		/// <summary>Property key for the title.</summary>
		public static readonly PKEY Title = new PKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);

		/// <summary>Property key for the AppUserModelID.</summary>
		public static readonly PKEY AppUserModel_ID = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

		/// <summary>Property key for destination list separators.</summary>
		public static readonly PKEY AppUserModel_IsDestListSeparator = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);

		/// <summary>Property key for the relaunch command.</summary>
		public static readonly PKEY AppUserModel_RelaunchCommand = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);

		/// <summary>Property key for the relaunch display name resource.</summary>
		public static readonly PKEY AppUserModel_RelaunchDisplayNameResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);

		/// <summary>Property key for the relaunch icon resource.</summary>
		public static readonly PKEY AppUserModel_RelaunchIconResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
	}

	/// <summary>COM interface for enumerating item identifier lists.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.EnumIdList)]
	internal interface IEnumIDList
	{
		/// <summary>Retrieves the next set of items.</summary>
		/// <param name="celt">The number of elements.</param>
		/// <param name="rgelt">The retrieved elements.</param>
		/// <param name="pceltFetched">The number of fetched elements.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig()]
		HRESULT Next(uint celt, out IntPtr rgelt, out int pceltFetched);

		/// <summary>Skips the specified number of items.</summary>
		/// <param name="celt">The number of elements.</param>
		/// <returns>Returns the skip result.</returns>
		[PreserveSig()]
		HRESULT Skip(uint celt);

		/// <summary>Resets the enumeration sequence.</summary>
		void Reset();

		/// <summary>Creates a copy of the enumerator.</summary>
		/// <param name="ppenum">The cloned enumerator.</param>
		void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
	}

	/// <summary>COM interface for enumerating shell objects.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.EnumObjects)]
	internal interface IEnumObjects
	{
		// [local]
		// This signature might not work... Hopefully don't need this interface though.

		/// <summary>Retrieves the next set of items.</summary>
		/// <param name="celt">The number of elements.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <param name="rgelt">The retrieved elements.</param>
		/// <param name="pceltFetched">The number of fetched elements.</param>
		void Next(uint celt, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, IidParameterIndex = 1, SizeParamIndex = 0)] object[] rgelt, [Out] out uint pceltFetched);

		/*
		[call_as(Next)] HRESULT RemoteNext(
			[in] ULONG celt,
			[in] REFIID riid,
			[out, size_is(celt), length_is(*pceltFetched), iid_is(riid)] void **rgelt,
			[out] ULONG *pceltFetched);
		 */

		/// <summary>Skips the specified number of items.</summary>
		/// <param name="celt">The number of elements.</param>
		void Skip(uint celt);

		/// <summary>Resets the enumeration sequence.</summary>
		void Reset();

		/// <summary>Creates a copy of the enumerator.</summary>
		/// <returns>Returns the cloned enumerator.</returns>
		IEnumObjects Clone();
	}

	/// <summary>COM interface for accessing an object array.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ObjectArray)]
	internal interface IObjectArray
	{
		/// <summary>Gets the number of items.</summary>
		/// <returns>Returns the number of items.</returns>
		uint GetCount();

		/// <summary>Gets the object at the specified index.</summary>
		/// <param name="uiIndex">The zero-based index.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested object.</returns>
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetAt([In] uint uiIndex, [In] ref Guid riid);
	}

	/// <summary>COM interface for managing an object collection.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ObjectArray)]
	internal interface IObjectCollection : IObjectArray
	{
		/// <summary>Gets the number of items.</summary>
		/// <returns>Returns the number of items.</returns>
		new uint GetCount();

		/// <summary>Gets the object at the specified index.</summary>
		/// <param name="uiIndex">The zero-based index.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested object.</returns>
		[return: MarshalAs(UnmanagedType.IUnknown)]
		new object GetAt([In] uint uiIndex, [In] ref Guid riid);

		/// <summary>Adds an object to the collection.</summary>
		/// <param name="punk">The destination object.</param>
		void AddObject([MarshalAs(UnmanagedType.IUnknown)] object punk);

		/// <summary>Adds the objects from the specified array.</summary>
		/// <param name="poaSource">The source object array.</param>
		void AddFromArray(IObjectArray poaSource);

		/// <summary>Removes the object at the specified index.</summary>
		/// <param name="uiIndex">The zero-based index.</param>
		void RemoveObjectAt(uint uiIndex);

		/// <summary>Removes all objects from the collection.</summary>
		void Clear();
	}

	/// <summary>COM interface for reading and writing shell properties.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.PropertyStore)]
	internal interface IPropertyStore
	{
		/// <summary>Gets the number of items.</summary>
		/// <returns>Returns the number of items.</returns>
		uint GetCount();

		/// <summary>Gets the object at the specified index.</summary>
		/// <param name="iProp">The property index.</param>
		/// <returns>Returns the requested object.</returns>
		PKEY GetAt(uint iProp);

		/// <summary>Performs the get value operation.</summary>
		/// <param name="pkey">The property key.</param>
		/// <param name="pv">The property value.</param>
		void GetValue([In] ref PKEY pkey, [In, Out] PROPVARIANT pv);

		/// <summary>Performs the set value operation.</summary>
		/// <param name="pkey">The property key.</param>
		/// <param name="pv">The property value.</param>
		void SetValue([In] ref PKEY pkey, PROPVARIANT pv);

		/// <summary>Performs the commit operation.</summary>
		void Commit();
	}

	/// <summary>COM interface for shell folder operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ShellFolder)]
	internal interface IShellFolder
	{
		/// <summary>Parses a display name into an item identifier list.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="pbc">The bind context.</param>
		/// <param name="pszDisplayName">The display name.</param>
		/// <param name="pchEaten">The number of characters consumed.</param>
		/// <param name="ppidl">The item identifier list.</param>
		/// <param name="pdwAttributes">The attributes.</param>
		void ParseDisplayName(
			[In] IntPtr hwnd,
			[In] IBindCtx pbc,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
			[In, Out] ref int pchEaten,
			[Out] out IntPtr ppidl,
			[In, Out] ref uint pdwAttributes);

		/// <summary>Enumerates the objects in the folder.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="grfFlags">The enumeration flags.</param>
		/// <returns>Returns the object enumerator.</returns>
		IEnumIDList EnumObjects(
			[In] IntPtr hwnd,
			[In] SHCONTF grfFlags);

		// returns an instance of a sub-folder which is specified by the IDList (pidl).
		// IShellFolder or derived interfaces

		/// <summary>Binds to the specified object.</summary>
		/// <param name="pidl">The item identifier list.</param>
		/// <param name="pbc">The bind context.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested COM interface.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object BindToObject(
			[In] IntPtr pidl,
			[In] IBindCtx pbc,
			[In] ref Guid riid);

		// produces the same result as BindToObject()

		/// <summary>Binds to the specified storage object.</summary>
		/// <param name="pidl">The item identifier list.</param>
		/// <param name="pbc">The bind context.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested storage interface.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object BindToStorage([In] IntPtr pidl, [In] IBindCtx pbc, [In] ref Guid riid);

		// compares two IDLists and returns the result. The shell
		// explorer always passes 0 as lParam, which indicates 'sort by name'.
		// It should return 0 (as CODE of the scode), if two id indicates the
		// same object; negative value if pidl1 should be placed before pidl2;
		// positive value if pidl2 should be placed before pidl1.
		// use the macro ResultFromShort() to extract the result comparison
		// it deals with the casting and type conversion issues for you

		/// <summary>Compares two item identifier lists.</summary>
		/// <param name="lParam">The comparison parameter.</param>
		/// <param name="pidl1">The first item identifier list.</param>
		/// <param name="pidl2">The second item identifier list.</param>
		/// <returns>Returns the comparison result.</returns>
		[PreserveSig]
		HRESULT CompareIDs([In] IntPtr lParam, [In] IntPtr pidl1, [In] IntPtr pidl2);

		// creates a view object of the folder itself. The view
		// object is a difference instance from the shell folder object.
		// 'hwndOwner' can be used  as the owner window of its dialog box or
		// menu during the lifetime of the view object.
		// This member function should always create a new
		// instance which has only one reference count. The explorer may create
		// more than one instances of view object from one shell folder object
		// and treat them as separate instances.
		// returns IShellView derived interface

		/// <summary>Creates a view object for the folder.</summary>
		/// <param name="hwndOwner">The owner window handle.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the created view object.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid);

		// returns the attributes of specified objects in that
		// folder. 'cidl' and 'apidl' specifies objects. 'apidl' contains only
		// simple IDLists. The explorer initializes *prgfInOut with a set of
		// flags to be evaluated. The shell folder may optimize the operation
		// by not returning unspecified flags.

		/// <summary>Gets the attributes of the specified items.</summary>
		/// <param name="cidl">The number of item identifiers.</param>
		/// <param name="apidl">The item identifier list array.</param>
		/// <param name="rgfInOut">The attributes to query and receive.</param>
		void GetAttributesOf(
			[In] uint cidl,
			[In] IntPtr apidl,
			[In, Out] ref SFGAO rgfInOut);

		// creates a UI object to be used for specified objects.
		// The shell explorer passes either IID_IDataObject (for transfer operation)
		// or IID_IContextMenu (for context menu operation) as riid
		// and many other interfaces

		/// <summary>Gets a UI object for the specified items.</summary>
		/// <param name="hwndOwner">The owner window handle.</param>
		/// <param name="cidl">The number of item identifiers.</param>
		/// <param name="apidl">The item identifier list array.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <param name="rgfReserved">Reserved flags.</param>
		/// <returns>Returns the requested UI object.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetUIObjectOf(
			[In] IntPtr hwndOwner,
			[In] uint cidl,
			[In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 2)] IntPtr apidl,
			[In] ref Guid riid,
			[In, Out] ref uint rgfReserved);

		// returns the display name of the specified object.
		// If the ID contains the display name (in the locale character set),
		// it returns the offset to the name. Otherwise, it returns a pointer
		// to the display name string (UNICODE), which is allocated by the
		// task allocator, or fills in a buffer.
		// use the helper APIS StrRetToStr() or StrRetToBuf() to deal with the different
		// forms of the STRRET structure

		/// <summary>Gets the display name of the specified item.</summary>
		/// <param name="pidl">The item identifier list.</param>
		/// <param name="uFlags">The display name flags.</param>
		/// <param name="pName">The display name result.</param>
		void GetDisplayNameOf([In] IntPtr pidl, [In] SHGDN uFlags, [Out] out IntPtr pName);

		// sets the display name of the specified object.
		// If it changes the ID as well, it returns the new ID which is
		// alocated by the task allocator.

		/// <summary>Sets the display name of the specified item.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="pidl">The item identifier list.</param>
		/// <param name="pszName">The name value.</param>
		/// <param name="uFlags">The display name flags.</param>
		/// <param name="ppidlOut">The renamed item identifier list.</param>
		void SetNameOf(
			[In] IntPtr hwnd,
			[In] IntPtr pidl,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
			[In] SHGDN uFlags,
			[Out] out IntPtr ppidlOut);
	}

	/// <summary>COM interface for shell item operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ShellItem)]
	internal interface IShellItem
	{
		/// <summary>Binds to a handler for the shell item.</summary>
		/// <param name="pbc">The bind context.</param>
		/// <param name="bhid">The handler identifier.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested handler interface.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object BindToHandler(IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);

		/// <summary>Gets the parent shell item.</summary>
		/// <returns>Returns the parent shell item.</returns>
		IShellItem GetParent();

		/// <summary>Gets the display name.</summary>
		/// <param name="sigdnName">The display name format.</param>
		/// <returns>Returns the display name.</returns>
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetDisplayName(SIGDN sigdnName);

		/// <summary>Gets the specified attributes.</summary>
		/// <param name="sfgaoMask">The attribute mask.</param>
		/// <returns>Returns the requested attributes.</returns>
		SFGAO GetAttributes(SFGAO sfgaoMask);

		/// <summary>Compares this item with another shell item.</summary>
		/// <param name="psi">The shell item to compare.</param>
		/// <param name="hint">The comparison hint.</param>
		/// <returns>Returns the comparison result.</returns>
		int Compare(IShellItem psi, SICHINT hint);
	}

	/// <summary>COM interface for shell item array operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ShellItemArray)]
	internal interface IShellItemArray
	{
		/// <summary>Binds to a handler for the shell item.</summary>
		/// <param name="pbc">The bind context.</param>
		/// <param name="rbhid">The handler identifier.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested handler interface.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object BindToHandler(IBindCtx pbc, [In] ref Guid rbhid, [In] ref Guid riid);

		/// <summary>Gets the property store.</summary>
		/// <param name="flags">The property store flags.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property store.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyStore(int flags, [In] ref Guid riid);

		/// <summary>Gets the property description list.</summary>
		/// <param name="keyType">The property key type.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property description list.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyDescriptionList([In] ref PKEY keyType, [In] ref Guid riid);

		/// <summary>Gets the specified attributes.</summary>
		/// <param name="dwAttribFlags">The attribute flags.</param>
		/// <param name="sfgaoMask">The attribute mask.</param>
		/// <returns>Returns the requested attributes.</returns>
		uint GetAttributes(SIATTRIBFLAGS dwAttribFlags, uint sfgaoMask);

		/// <summary>Gets the number of items.</summary>
		/// <returns>Returns the number of items.</returns>
		uint GetCount();

		/// <summary>Performs the get item at operation.</summary>
		/// <param name="dwIndex">The item index.</param>
		/// <returns>Returns the shell item at the specified index.</returns>
		IShellItem GetItemAt(uint dwIndex);

		/// <summary>Performs the enum items operation.</summary>
		/// <returns>Returns the item enumerator.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object EnumItems();
	}

	/// <summary>COM interface for extended shell item operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ShellItem2)]
	internal interface IShellItem2 : IShellItem
	{
		/// <summary>Binds to a handler for the shell item.</summary>
		/// <param name="pbc">The bind context.</param>
		/// <param name="bhid">The handler identifier.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested handler interface.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		new object BindToHandler([In] IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);

		/// <summary>Gets the parent shell item.</summary>
		/// <returns>Returns the parent shell item.</returns>
		new IShellItem GetParent();

		/// <summary>Gets the display name.</summary>
		/// <param name="sigdnName">The display name format.</param>
		/// <returns>Returns the display name.</returns>
		[return: MarshalAs(UnmanagedType.LPWStr)]
		new string GetDisplayName(SIGDN sigdnName);

		/// <summary>Gets the specified attributes.</summary>
		/// <param name="sfgaoMask">The attribute mask.</param>
		/// <returns>Returns the requested attributes.</returns>
		new SFGAO GetAttributes(SFGAO sfgaoMask);

		/// <summary>Compares this item with another shell item.</summary>
		/// <param name="psi">The shell item to compare.</param>
		/// <param name="hint">The comparison hint.</param>
		/// <returns>Returns the comparison result.</returns>
		new int Compare(IShellItem psi, SICHINT hint);

		/// <summary>Gets the property store.</summary>
		/// <param name="flags">The property store flags.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property store.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyStore(
			GPS flags,
			[In] ref Guid riid);

		/// <summary>Gets the property store using a creation object.</summary>
		/// <param name="flags">The property store flags.</param>
		/// <param name="punkCreateObject">The creation object.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property store.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyStoreWithCreateObject(
			GPS flags,
			[MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject,   // factory for low-rights creation of type ICreateObject
			[In] ref Guid riid);

		/// <summary>Gets the property store for the specified keys.</summary>
		/// <param name="rgKeys">The property key array.</param>
		/// <param name="cKeys">The number of property keys.</param>
		/// <param name="flags">The property store flags.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property store.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyStoreForKeys(
			IntPtr rgKeys,
			uint cKeys,
			GPS flags,
			[In] ref Guid riid);

		/// <summary>Gets the property description list.</summary>
		/// <param name="keyType">The property key type.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the property description list.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetPropertyDescriptionList(
			IntPtr keyType,
			[In] ref Guid riid);

		// Ensures any cached information in this item is up to date, or returns __HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) if the item does not exist.

		/// <summary>Updates cached information for the item.</summary>
		/// <param name="pbc">The bind context.</param>
		void Update(IBindCtx pbc);

		/// <summary>Gets the property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the property value.</returns>
		PROPVARIANT GetProperty(IntPtr key);

		/// <summary>Gets the CLSID property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the CLSID value.</returns>
		Guid GetCLSID(IntPtr key);

		/// <summary>Gets the file time property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the file time value.</returns>
		FILETIME GetFileTime(IntPtr key);

		/// <summary>Gets the 32-bit integer property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the 32-bit integer value.</returns>
		int GetInt32(IntPtr key);

		/// <summary>Gets the string property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the string value.</returns>
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetString(IntPtr key);

		/// <summary>Gets the unsigned 32-bit integer property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the unsigned 32-bit integer value.</returns>
		uint GetUInt32(IntPtr key);

		/// <summary>Gets the unsigned 64-bit integer property value.</summary>
		/// <param name="key">The key.</param>
		/// <returns>Returns the unsigned 64-bit integer value.</returns>
		ulong GetUInt64(IntPtr key);

		/// <summary>Gets the Boolean property value.</summary>
		/// <param name="key">The key.</param>
		[return: MarshalAs(UnmanagedType.Bool)]
		void GetBool(IntPtr key);
	}

	/// <summary>COM interface for Windows shell links.</summary>
	[ComImport]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ShellLink)]
	internal interface IShellLinkW
	{
		/// <summary>Gets the shell link path.</summary>
		/// <param name="pszFile">The path text or buffer.</param>
		/// <param name="cchMaxPath">The buffer length.</param>
		/// <param name="pfd">The file data buffer.</param>
		/// <param name="fFlags">The option flags.</param>
		void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, [In, Out] WIN32_FIND_DATAW pfd, SLGP fFlags);

		/// <summary>Gets the item identifier list.</summary>
		/// <param name="ppidl">The item identifier list.</param>
		void GetIDList(out IntPtr ppidl);

		/// <summary>Sets the item identifier list.</summary>
		/// <param name="pidl">The item identifier list.</param>
		void SetIDList(IntPtr pidl);

		/// <summary>Gets the shell link description.</summary>
		/// <param name="pszFile">The path text or buffer.</param>
		/// <param name="cchMaxName">The buffer length.</param>
		void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);

		/// <summary>Sets the shell link description.</summary>
		/// <param name="pszName">The name value.</param>
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

		/// <summary>Gets the working directory.</summary>
		/// <param name="pszDir">The working directory.</param>
		/// <param name="cchMaxPath">The buffer length.</param>
		void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

		/// <summary>Sets the working directory.</summary>
		/// <param name="pszDir">The working directory.</param>
		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

		/// <summary>Gets the shell link arguments.</summary>
		/// <param name="pszArgs">The argument text or buffer.</param>
		/// <param name="cchMaxPath">The buffer length.</param>
		void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

		/// <summary>Sets the shell link arguments.</summary>
		/// <param name="pszArgs">The argument text or buffer.</param>
		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

		/// <summary>Gets the hot key.</summary>
		/// <returns>Returns the hot key.</returns>
		short GetHotKey();

		/// <summary>Sets the hot key.</summary>
		/// <param name="wHotKey">The hot key value.</param>
		void SetHotKey(short wHotKey);

		/// <summary>Gets the show command.</summary>
		/// <returns>Returns the show command.</returns>
		uint GetShowCmd();

		/// <summary>Sets the show command.</summary>
		/// <param name="iShowCmd">The show command.</param>
		void SetShowCmd(uint iShowCmd);

		/// <summary>Gets the icon location.</summary>
		/// <param name="pszIconPath">The icon path or buffer.</param>
		/// <param name="cchIconPath">The buffer length.</param>
		/// <param name="piIcon">The icon index.</param>
		void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

		/// <summary>Sets the icon location.</summary>
		/// <param name="pszIconPath">The icon path or buffer.</param>
		/// <param name="iIcon">The icon index.</param>
		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

		/// <summary>Sets the relative path.</summary>
		/// <param name="pszPathRel">The relative path.</param>
		/// <param name="dwReserved">The reserved value.</param>
		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

		/// <summary>Resolves the shell link.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="fFlags">The option flags.</param>
		void Resolve(IntPtr hwnd, uint fFlags);

		/// <summary>Sets the shell link path.</summary>
		/// <param name="pszFile">The path text or buffer.</param>
		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}

	/// <summary>COM interface for taskbar list operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.TaskbarList)]
	internal interface ITaskbarList
	{
		/// <summary>Initializes the taskbar list object.</summary>
		void HrInit();

		/// <summary>Adds a tab to the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		void AddTab(IntPtr hwnd);

		/// <summary>Deletes a tab from the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		void DeleteTab(IntPtr hwnd);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwnd">The window handle.</param>
		void ActivateTab(IntPtr hwnd);

		/// <summary>Marks a taskbar tab as active.</summary>
		/// <param name="hwnd">The window handle.</param>
		void SetActiveAlt(IntPtr hwnd);
	}

	/// <summary>COM interface for extended taskbar list operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.TaskbarList2)]
	internal interface ITaskbarList2 : ITaskbarList
	{
		/// <summary>Initializes the taskbar list object.</summary>
		new void HrInit();

		/// <summary>Adds a tab to the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void AddTab(IntPtr hwnd);

		/// <summary>Deletes a tab from the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void DeleteTab(IntPtr hwnd);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void ActivateTab(IntPtr hwnd);

		/// <summary>Marks a taskbar tab as active.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void SetActiveAlt(IntPtr hwnd);

		/// <summary>Marks a window as full-screen.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="fFullscreen">The f fullscreen.</param>
		void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
	}

	// Used to remove items from the automatic destination lists created when apps or the system call SHAddToRecentDocs to report usage of a document.

	/// <summary>COM interface for managing application destinations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ApplicationDestinations)]
	internal interface IApplicationDestinations
	{
		// Set the App User Model ID for the application removing destinations from its list.  If an AppID is not provided
		// via this method, the system will use a heuristically determined ID.  This method must be called before
		// RemoveDestination or RemoveAllDestinations.

		/// <summary>Sets the AppUserModelID.</summary>
		/// <param name="pszAppID">The AppUserModelID.</param>
		void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

		// Remove an IShellItem or an IShellLink from the automatic destination list

		/// <summary>Removes the specified destination.</summary>
		/// <param name="punk">The destination object.</param>
		void RemoveDestination([MarshalAs(UnmanagedType.IUnknown)] object punk);

		// Clear the frequent and recent destination lists for this application.

		/// <summary>Removes all destinations.</summary>
		void RemoveAllDestinations();
	}

	/// <summary>COM interface for retrieving application document lists.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ApplicationDocumentLists)]
	internal interface IApplicationDocumentLists
	{
		/// <summary>Sets the AppUserModelID.</summary>
		/// <param name="pszAppID">The AppUserModelID.</param>
		void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

		/// <summary>Gets a document list.</summary>
		/// <param name="listtype">The document list type.</param>
		/// <param name="cItemsDesired">The requested item count.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the requested document list.</returns>
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetList([In] APPDOCLISTTYPE listtype, [In] uint cItemsDesired, [In] ref Guid riid);
	}

	// Custom Destination List

	/// <summary>COM interface for managing custom destination lists.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.CustomDestinationList)]
	internal interface ICustomDestinationList
	{
		/// <summary>Sets the AppUserModelID.</summary>
		/// <param name="pszAppID">The AppUserModelID.</param>
		void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

		// Retrieve IObjectArray of IShellItems or IShellLinks that represent removed destinations

		/// <summary>Begins a custom destination list.</summary>
		/// <param name="pcMaxSlots">The maximum number of slots.</param>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the removed destination collection.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object BeginList(out uint pcMaxSlots, [In] ref Guid riid);

		// PreserveSig because this will return custom errors when attempting to add unregistered ShellItems.
		// Can't readily detect that case without just trying to append it.

		/// <summary>Appends a custom category.</summary>
		/// <param name="pszCategory">The category name.</param>
		/// <param name="poa">The object array.</param>
		/// <returns>Returns the append category result.</returns>
		[PreserveSig]
		HRESULT AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string pszCategory, IObjectArray poa);

		/// <summary>Appends a known category.</summary>
		/// <param name="category">The known category.</param>
		void AppendKnownCategory(KDC category);

		/// <summary>Adds user tasks.</summary>
		/// <param name="poa">The object array.</param>
		/// <returns>Returns the add user tasks result.</returns>
		[PreserveSig]
		HRESULT AddUserTasks(IObjectArray poa);

		/// <summary>Commits the destination list.</summary>
		void CommitList();

		// Retrieve IObjectCollection of IShellItems

		/// <summary>Gets the removed destinations.</summary>
		/// <param name="riid">The interface identifier.</param>
		/// <returns>Returns the get removed destinations result.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object GetRemovedDestinations([In] ref Guid riid);

		/// <summary>Deletes the destination list.</summary>
		/// <param name="pszAppID">The AppUserModelID.</param>
		void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

		/// <summary>Aborts the current destination list.</summary>
		void AbortList();
	}

	/// <summary>COM interface for accessing the AppUserModelID.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ObjectWithAppUserModelId)]
	internal interface IObjectWithAppUserModelId
	{
		/// <summary>Sets the AppUserModelID.</summary>
		/// <param name="pszAppID">The AppUserModelID.</param>
		void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

		/// <summary>Gets the AppUserModelID.</summary>
		/// <returns>Returns the AppUserModelID.</returns>
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetAppID();
	}

	/// <summary>COM interface for accessing the ProgID.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.ObjectWithProgId)]
	internal interface IObjectWithProgId
	{
		/// <summary>Sets the ProgID.</summary>
		/// <param name="pszProgID">The ProgID.</param>
		void SetProgID([MarshalAs(UnmanagedType.LPWStr)] string pszProgID);

		/// <summary>Gets the ProgID.</summary>
		/// <returns>Returns the ProgID.</returns>
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetProgID();
	}

	/// <summary>COM interface for advanced taskbar list operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.TaskbarList3)]
	internal interface ITaskbarList3 : ITaskbarList2
	{
		/// <summary>Initializes the taskbar list object.</summary>
		new void HrInit();

		/// <summary>Adds a tab to the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void AddTab(IntPtr hwnd);

		/// <summary>Deletes a tab from the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void DeleteTab(IntPtr hwnd);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void ActivateTab(IntPtr hwnd);

		/// <summary>Marks a taskbar tab as active.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void SetActiveAlt(IntPtr hwnd);

		/// <summary>Marks a window as full-screen.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="fFullscreen">The f fullscreen.</param>
		new void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		/// <summary>Sets the taskbar progress value.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="ullCompleted">The completed portion.</param>
		/// <param name="ullTotal">The total portion.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

		/// <summary>Sets the taskbar progress state.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="tbpFlags">The progress state flags.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetProgressState(IntPtr hwnd, TBPF tbpFlags);

		/// <summary>Registers a tab with the taskbar.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndMDI">The MDI window handle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

		/// <summary>Unregisters a tab from the taskbar.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT UnregisterTab(IntPtr hwndTab);

		/// <summary>Sets the taskbar tab order.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndInsertBefore">The tab that should precede the specified tab.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndMDI">The MDI window handle.</param>
		/// <param name="dwReserved">The reserved value.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);

		/// <summary>Adds thumbnail toolbar buttons.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="cButtons">The number of buttons.</param>
		/// <param name="pButtons">The thumbnail buttons.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

		/// <summary>Updates thumbnail toolbar buttons.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="cButtons">The number of buttons.</param>
		/// <param name="pButtons">The thumbnail buttons.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

		/// <summary>Sets the thumbnail toolbar image list.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="himl">The image list.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT ThumbBarSetImageList(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);

		/// <summary>Sets the taskbar overlay icon.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="hIcon">The icon handle.</param>
		/// <param name="pszDescription">The icon description.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

		/// <summary>Sets the thumbnail tooltip.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="pszTip">The tooltip text.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

		// Using RefRECT to making passing NULL possible.  Removes clipping from the HWND.

		/// <summary>Sets the thumbnail clip region.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="prcClip">The clip rectangle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		HRESULT SetThumbnailClip(IntPtr hwnd, RefRECT prcClip);
	}

	/// <summary>COM interface for taskbar tab operations.</summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(IID.TaskbarList3)]
	internal interface ITaskbarList4 : ITaskbarList3
	{
		/// <summary>Initializes the taskbar list object.</summary>
		new void HrInit();

		/// <summary>Adds a tab to the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void AddTab(IntPtr hwnd);

		/// <summary>Deletes a tab from the taskbar.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void DeleteTab(IntPtr hwnd);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void ActivateTab(IntPtr hwnd);

		/// <summary>Marks a taskbar tab as active.</summary>
		/// <param name="hwnd">The window handle.</param>
		new void SetActiveAlt(IntPtr hwnd);

		/// <summary>Marks a window as full-screen.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="fFullscreen">The f fullscreen.</param>
		new void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		/// <summary>Sets the taskbar progress value.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="ullCompleted">The completed portion.</param>
		/// <param name="ullTotal">The total portion.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

		/// <summary>Sets the taskbar progress state.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="tbpFlags">The progress state flags.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetProgressState(IntPtr hwnd, TBPF tbpFlags);

		/// <summary>Registers a tab with the taskbar.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndMDI">The MDI window handle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

		/// <summary>Unregisters a tab from the taskbar.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT UnregisterTab(IntPtr hwndTab);

		/// <summary>Sets the taskbar tab order.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndInsertBefore">The tab that should precede the specified tab.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);

		/// <summary>Activates a taskbar tab.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="hwndMDI">The MDI window handle.</param>
		/// <param name="dwReserved">The reserved value.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);

		/// <summary>Adds thumbnail toolbar buttons.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="cButtons">The number of buttons.</param>
		/// <param name="pButtons">The thumbnail buttons.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

		/// <summary>Updates thumbnail toolbar buttons.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="cButtons">The number of buttons.</param>
		/// <param name="pButtons">The thumbnail buttons.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

		/// <summary>Sets the thumbnail toolbar image list.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="himl">The image list.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT ThumbBarSetImageList(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);

		/// <summary>Sets the taskbar overlay icon.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="hIcon">The icon handle.</param>
		/// <param name="pszDescription">The icon description.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

		/// <summary>Sets the thumbnail tooltip.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="pszTip">The tooltip text.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

		// Using RefRECT to making passing NULL possible.  Removes clipping from the HWND.

		/// <summary>Sets the thumbnail clip region.</summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="prcClip">The clip rectangle.</param>
		/// <returns>Returns the HRESULT from the operation.</returns>
		[PreserveSig]
		new HRESULT SetThumbnailClip(IntPtr hwnd, RefRECT prcClip);

		/// <summary>Sets the taskbar tab properties.</summary>
		/// <param name="hwndTab">The tab window handle.</param>
		/// <param name="stpFlags">The tab properties flags.</param>
		void SetTabProperties(IntPtr hwndTab, STPF stpFlags);
	}
}