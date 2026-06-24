/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

#pragma warning disable CS0169 // Field is never used
#pragma warning disable SYSLIB0004
namespace Standard
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.ConstrainedExecution;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Security.Permissions;
	using System.Text;
	using Microsoft.Win32.SafeHandles;

	// Some COM interfaces and Win32 structures are already declared in the framework.
	// Interesting ones to remember in System.Runtime.InteropServices.ComTypes are:
	using IStream = System.Runtime.InteropServices.ComTypes.IStream;

	/// <summary>
	/// Native Win32Value class.
	/// </summary>
	internal static class Win32Value
	{
		/// <summary>
		/// The MAX_PATH field.
		/// </summary>
		public const uint MAX_PATH = 260;

		/// <summary>
		/// The INFOTIPSIZE field.
		/// </summary>
		public const uint INFOTIPSIZE = 1024;

		/// <summary>
		/// The TRUE field.
		/// </summary>
		public const uint TRUE = 1;

		/// <summary>
		/// The FALSE field.
		/// </summary>
		public const uint FALSE = 0;

		/// <summary>
		/// The sizeof_WCHAR field.
		/// </summary>
		public const uint sizeof_WCHAR = 2;

		/// <summary>
		/// The sizeof_CHAR field.
		/// </summary>
		public const uint sizeof_CHAR = 1;

		/// <summary>
		/// The sizeof_BOOL field.
		/// </summary>
		public const uint sizeof_BOOL = 4;
	}

	/// <summary>
	/// HIGHCONTRAST flags
	/// </summary>
	[Flags]
	internal enum HCF
	{
		/// <summary>
		/// The HIGHCONTRASTON value.
		/// </summary>
		HIGHCONTRASTON = 0x00000001,

		/// <summary>
		/// The AVAILABLE value.
		/// </summary>
		AVAILABLE = 0x00000002,

		/// <summary>
		/// The HOTKEYACTIVE value.
		/// </summary>
		HOTKEYACTIVE = 0x00000004,

		/// <summary>
		/// The CONFIRMHOTKEY value.
		/// </summary>
		CONFIRMHOTKEY = 0x00000008,

		/// <summary>
		/// The HOTKEYSOUND value.
		/// </summary>
		HOTKEYSOUND = 0x00000010,

		/// <summary>
		/// The INDICATOR value.
		/// </summary>
		INDICATOR = 0x00000020,

		/// <summary>
		/// The HOTKEYAVAILABLE value.
		/// </summary>
		HOTKEYAVAILABLE = 0x00000040,
	}

	/// <summary>
	/// BITMAPINFOHEADER Compression type.  BI_*.
	/// </summary>
	internal enum BI
	{
		/// <summary>
		/// The RGB value.
		/// </summary>
		RGB = 0,
	}

	/// <summary>
	/// CombingRgn flags.  RGN_*
	/// </summary>
	internal enum RGN
	{
		/// <summary>
		/// Creates the intersection of the two combined regions.
		/// </summary>
		AND = 1,

		/// <summary>
		/// Creates the union of two combined regions.
		/// </summary>
		OR = 2,

		/// <summary>
		/// Creates the union of two combined regions except for any overlapping areas.
		/// </summary>
		XOR = 3,

		/// <summary>
		/// Combines the parts of hrgnSrc1 that are not part of hrgnSrc2.
		/// </summary>
		DIFF = 4,

		/// <summary>
		/// Creates a copy of the region identified by hrgnSrc1.
		/// </summary>
		COPY = 5,
	}

	/// <summary>
	/// Native CombineRgnResult enumeration.
	/// </summary>
	internal enum CombineRgnResult
	{
		/// <summary>
		/// The ERROR value.
		/// </summary>
		ERROR = 0,

		/// <summary>
		/// The NULLREGION value.
		/// </summary>
		NULLREGION = 1,

		/// <summary>
		/// The SIMPLEREGION value.
		/// </summary>
		SIMPLEREGION = 2,

		/// <summary>
		/// The COMPLEXREGION value.
		/// </summary>
		COMPLEXREGION = 3,
	}

	/// <summary>
	/// For IWebBrowser2.  OLECMDEXECOPT_*
	/// </summary>
	internal enum OLECMDEXECOPT
	{
		/// <summary>
		/// The DODEFAULT value.
		/// </summary>
		DODEFAULT = 0,

		/// <summary>
		/// The PROMPTUSER value.
		/// </summary>
		PROMPTUSER = 1,

		/// <summary>
		/// The DONTPROMPTUSER value.
		/// </summary>
		DONTPROMPTUSER = 2,

		/// <summary>
		/// The SHOWHELP value.
		/// </summary>
		SHOWHELP = 3
	}

	/// <summary>
	/// For IWebBrowser2.  OLECMDF_*
	/// </summary>
	internal enum OLECMDF
	{
		/// <summary>
		/// The SUPPORTED value.
		/// </summary>
		SUPPORTED = 1,

		/// <summary>
		/// The ENABLED value.
		/// </summary>
		ENABLED = 2,

		/// <summary>
		/// The LATCHED value.
		/// </summary>
		LATCHED = 4,

		/// <summary>
		/// The NINCHED value.
		/// </summary>
		NINCHED = 8,

		/// <summary>
		/// The INVISIBLE value.
		/// </summary>
		INVISIBLE = 16,

		/// <summary>
		/// The DEFHIDEONCTXTMENU value.
		/// </summary>
		DEFHIDEONCTXTMENU = 32
	}

	/// <summary>
	/// For IWebBrowser2.  OLECMDID_*
	/// </summary>
	internal enum OLECMDID
	{
		/// <summary>
		/// The OPEN value.
		/// </summary>
		OPEN = 1,

		/// <summary>
		/// The NEW value.
		/// </summary>
		NEW = 2,

		/// <summary>
		/// The SAVE value.
		/// </summary>
		SAVE = 3,

		/// <summary>
		/// The SAVEAS value.
		/// </summary>
		SAVEAS = 4,

		/// <summary>
		/// The SAVECOPYAS value.
		/// </summary>
		SAVECOPYAS = 5,

		/// <summary>
		/// The PRINT value.
		/// </summary>
		PRINT = 6,

		/// <summary>
		/// The PRINTPREVIEW value.
		/// </summary>
		PRINTPREVIEW = 7,

		/// <summary>
		/// The PAGESETUP value.
		/// </summary>
		PAGESETUP = 8,

		/// <summary>
		/// The SPELL value.
		/// </summary>
		SPELL = 9,

		/// <summary>
		/// The PROPERTIES value.
		/// </summary>
		PROPERTIES = 10,

		/// <summary>
		/// The CUT value.
		/// </summary>
		CUT = 11,

		/// <summary>
		/// The COPY value.
		/// </summary>
		COPY = 12,

		/// <summary>
		/// The PASTE value.
		/// </summary>
		PASTE = 13,

		/// <summary>
		/// The PASTESPECIAL value.
		/// </summary>
		PASTESPECIAL = 14,

		/// <summary>
		/// The UNDO value.
		/// </summary>
		UNDO = 15,

		/// <summary>
		/// The REDO value.
		/// </summary>
		REDO = 16,

		/// <summary>
		/// The SELECTALL value.
		/// </summary>
		SELECTALL = 17,

		/// <summary>
		/// The CLEARSELECTION value.
		/// </summary>
		CLEARSELECTION = 18,

		/// <summary>
		/// The ZOOM value.
		/// </summary>
		ZOOM = 19,

		/// <summary>
		/// The GETZOOMRANGE value.
		/// </summary>
		GETZOOMRANGE = 20,

		/// <summary>
		/// The UPDATECOMMANDS value.
		/// </summary>
		UPDATECOMMANDS = 21,

		/// <summary>
		/// The REFRESH value.
		/// </summary>
		REFRESH = 22,

		/// <summary>
		/// The STOP value.
		/// </summary>
		STOP = 23,

		/// <summary>
		/// The HIDETOOLBARS value.
		/// </summary>
		HIDETOOLBARS = 24,

		/// <summary>
		/// The SETPROGRESSMAX value.
		/// </summary>
		SETPROGRESSMAX = 25,

		/// <summary>
		/// The SETPROGRESSPOS value.
		/// </summary>
		SETPROGRESSPOS = 26,

		/// <summary>
		/// The SETPROGRESSTEXT value.
		/// </summary>
		SETPROGRESSTEXT = 27,

		/// <summary>
		/// The SETTITLE value.
		/// </summary>
		SETTITLE = 28,

		/// <summary>
		/// The SETDOWNLOADSTATE value.
		/// </summary>
		SETDOWNLOADSTATE = 29,

		/// <summary>
		/// The STOPDOWNLOAD value.
		/// </summary>
		STOPDOWNLOAD = 30,

		/// <summary>
		/// The ONTOOLBARACTIVATED value.
		/// </summary>
		ONTOOLBARACTIVATED = 31,

		/// <summary>
		/// The FIND value.
		/// </summary>
		FIND = 32,

		/// <summary>
		/// The DELETE value.
		/// </summary>
		DELETE = 33,

		/// <summary>
		/// The HTTPEQUIV value.
		/// </summary>
		HTTPEQUIV = 34,

		/// <summary>
		/// The HTTPEQUIV_DONE value.
		/// </summary>
		HTTPEQUIV_DONE = 35,

		/// <summary>
		/// The ENABLE_INTERACTION value.
		/// </summary>
		ENABLE_INTERACTION = 36,

		/// <summary>
		/// The ONUNLOAD value.
		/// </summary>
		ONUNLOAD = 37,

		/// <summary>
		/// The PROPERTYBAG2 value.
		/// </summary>
		PROPERTYBAG2 = 38,

		/// <summary>
		/// The PREREFRESH value.
		/// </summary>
		PREREFRESH = 39,

		/// <summary>
		/// The SHOWSCRIPTERROR value.
		/// </summary>
		SHOWSCRIPTERROR = 40,

		/// <summary>
		/// The SHOWMESSAGE value.
		/// </summary>
		SHOWMESSAGE = 41,

		/// <summary>
		/// The SHOWFIND value.
		/// </summary>
		SHOWFIND = 42,

		/// <summary>
		/// The SHOWPAGESETUP value.
		/// </summary>
		SHOWPAGESETUP = 43,

		/// <summary>
		/// The SHOWPRINT value.
		/// </summary>
		SHOWPRINT = 44,

		/// <summary>
		/// The CLOSE value.
		/// </summary>
		CLOSE = 45,

		/// <summary>
		/// The ALLOWUILESSSAVEAS value.
		/// </summary>
		ALLOWUILESSSAVEAS = 46,

		/// <summary>
		/// The DONTDOWNLOADCSS value.
		/// </summary>
		DONTDOWNLOADCSS = 47,

		/// <summary>
		/// The UPDATEPAGESTATUS value.
		/// </summary>
		UPDATEPAGESTATUS = 48,

		/// <summary>
		/// The PRINT2 value.
		/// </summary>
		PRINT2 = 49,

		/// <summary>
		/// The PRINTPREVIEW2 value.
		/// </summary>
		PRINTPREVIEW2 = 50,

		/// <summary>
		/// The SETPRINTTEMPLATE value.
		/// </summary>
		SETPRINTTEMPLATE = 51,

		/// <summary>
		/// The GETPRINTTEMPLATE value.
		/// </summary>
		GETPRINTTEMPLATE = 52,

		/// <summary>
		/// The PAGEACTIONBLOCKED value.
		/// </summary>
		PAGEACTIONBLOCKED = 55,

		/// <summary>
		/// The PAGEACTIONUIQUERY value.
		/// </summary>
		PAGEACTIONUIQUERY = 56,

		/// <summary>
		/// The FOCUSVIEWCONTROLS value.
		/// </summary>
		FOCUSVIEWCONTROLS = 57,

		/// <summary>
		/// The FOCUSVIEWCONTROLSQUERY value.
		/// </summary>
		FOCUSVIEWCONTROLSQUERY = 58,

		/// <summary>
		/// The SHOWPAGEACTIONMENU value.
		/// </summary>
		SHOWPAGEACTIONMENU = 59
	}

	/// <summary>
	/// For IWebBrowser2.  READYSTATE_*
	/// </summary>
	internal enum READYSTATE
	{
		/// <summary>
		/// The UNINITIALIZED value.
		/// </summary>
		UNINITIALIZED = 0,

		/// <summary>
		/// The LOADING value.
		/// </summary>
		LOADING = 1,

		/// <summary>
		/// The LOADED value.
		/// </summary>
		LOADED = 2,

		/// <summary>
		/// The INTERACTIVE value.
		/// </summary>
		INTERACTIVE = 3,

		/// <summary>
		/// The COMPLETE value.
		/// </summary>
		COMPLETE = 4
	}

	/// <summary>
	/// DATAOBJ_GET_ITEM_FLAGS.  DOGIF_*.
	/// </summary>
	internal enum DOGIF
	{
		/// <summary>
		/// The DEFAULT value.
		/// </summary>
		DEFAULT = 0x0000,

		/// <summary>
		/// The TRAVERSE_LINK value.
		/// </summary>
		TRAVERSE_LINK = 0x0001,    // if the item is a link get the target

		/// <summary>
		/// The NO_HDROP value.
		/// </summary>
		NO_HDROP = 0x0002,    // don't fallback and use CF_HDROP clipboard format

		/// <summary>
		/// The NO_URL value.
		/// </summary>
		NO_URL = 0x0004,    // don't fallback and use URL clipboard format

		/// <summary>
		/// The ONLY_IF_ONE value.
		/// </summary>
		ONLY_IF_ONE = 0x0008,    // only return the item if there is one item in the array
	}

	/// <summary>
	/// Native DWM_SIT enumeration.
	/// </summary>
	internal enum DWM_SIT
	{
		/// <summary>
		/// The None value.
		/// </summary>
		None,

		/// <summary>
		/// The DISPLAYFRAME value.
		/// </summary>
		DISPLAYFRAME = 1,
	}

	/// <summary>
	/// Native ErrorModes enumeration.
	/// </summary>
	[Flags]
	internal enum ErrorModes
	{
		/// <summary>Use the system default, which is to display all error dialog boxes.</summary>
		Default = 0x0,

		/// <summary>
		/// The system does not display the critical-error-handler message box.
		/// Instead, the system sends the error to the calling process.
		/// </summary>
		FailCriticalErrors = 0x1,

		/// <summary>
		/// 64-bit Windows:  The system automatically fixes memory alignment faults and makes them
		/// invisible to the application. It does this for the calling process and any descendant processes.
		/// After this value is set for a process, subsequent attempts to clear the value are ignored.
		/// </summary>
		NoGpFaultErrorBox = 0x2,

		/// <summary>
		/// The system does not display the general-protection-fault message box.
		/// This flag should only be set by debugging applications that handle general
		/// protection (GP) faults themselves with an exception handler.
		/// </summary>
		NoAlignmentFaultExcept = 0x4,

		/// <summary>
		/// The system does not display a message box when it fails to find a file.
		/// Instead, the error is returned to the calling process.
		/// </summary>
		NoOpenFileErrorBox = 0x8000
	}

	/// <summary>
	/// Non-client hit test values, HT*
	/// </summary>
	internal enum HT
	{
		/// <summary>
		/// The ERROR value.
		/// </summary>
		ERROR = -2,

		/// <summary>
		/// The TRANSPARENT value.
		/// </summary>
		TRANSPARENT = -1,

		/// <summary>
		/// The NOWHERE value.
		/// </summary>
		NOWHERE = 0,

		/// <summary>
		/// The CLIENT value.
		/// </summary>
		CLIENT = 1,

		/// <summary>
		/// The CAPTION value.
		/// </summary>
		CAPTION = 2,

		/// <summary>
		/// The SYSMENU value.
		/// </summary>
		SYSMENU = 3,

		/// <summary>
		/// The GROWBOX value.
		/// </summary>
		GROWBOX = 4,

		/// <summary>
		/// The SIZE value.
		/// </summary>
		SIZE = GROWBOX,

		/// <summary>
		/// The MENU value.
		/// </summary>
		MENU = 5,

		/// <summary>
		/// The HSCROLL value.
		/// </summary>
		HSCROLL = 6,

		/// <summary>
		/// The VSCROLL value.
		/// </summary>
		VSCROLL = 7,

		/// <summary>
		/// The MINBUTTON value.
		/// </summary>
		MINBUTTON = 8,

		/// <summary>
		/// The MAXBUTTON value.
		/// </summary>
		MAXBUTTON = 9,

		/// <summary>
		/// The LEFT value.
		/// </summary>
		LEFT = 10,

		/// <summary>
		/// The RIGHT value.
		/// </summary>
		RIGHT = 11,

		/// <summary>
		/// The TOP value.
		/// </summary>
		TOP = 12,

		/// <summary>
		/// The TOPLEFT value.
		/// </summary>
		TOPLEFT = 13,

		/// <summary>
		/// The TOPRIGHT value.
		/// </summary>
		TOPRIGHT = 14,

		/// <summary>
		/// The BOTTOM value.
		/// </summary>
		BOTTOM = 15,

		/// <summary>
		/// The BOTTOMLEFT value.
		/// </summary>
		BOTTOMLEFT = 16,

		/// <summary>
		/// The BOTTOMRIGHT value.
		/// </summary>
		BOTTOMRIGHT = 17,

		/// <summary>
		/// The BORDER value.
		/// </summary>
		BORDER = 18,

		/// <summary>
		/// The REDUCE value.
		/// </summary>
		REDUCE = MINBUTTON,

		/// <summary>
		/// The ZOOM value.
		/// </summary>
		ZOOM = MAXBUTTON,

		/// <summary>
		/// The SIZEFIRST value.
		/// </summary>
		SIZEFIRST = LEFT,

		/// <summary>
		/// The SIZELAST value.
		/// </summary>
		SIZELAST = BOTTOMRIGHT,

		/// <summary>
		/// The OBJECT value.
		/// </summary>
		OBJECT = 19,

		/// <summary>
		/// The CLOSE value.
		/// </summary>
		CLOSE = 20,

		/// <summary>
		/// The HELP value.
		/// </summary>
		HELP = 21
	}

	/// <summary>
	/// GetClassLongPtr values, GCLP_*
	/// </summary>
	internal enum GCLP
	{
		/// <summary>
		/// The HBRBACKGROUND value.
		/// </summary>
		HBRBACKGROUND = -10,
	}

	/// <summary>
	/// GetWindowLongPtr values, GWL_*
	/// </summary>
	internal enum GWL
	{
		/// <summary>
		/// The WNDPROC value.
		/// </summary>
		WNDPROC = -4,

		/// <summary>
		/// The HINSTANCE value.
		/// </summary>
		HINSTANCE = -6,

		/// <summary>
		/// The HWNDPARENT value.
		/// </summary>
		HWNDPARENT = -8,

		/// <summary>
		/// The STYLE value.
		/// </summary>
		STYLE = -16,

		/// <summary>
		/// The EXSTYLE value.
		/// </summary>
		EXSTYLE = -20,

		/// <summary>
		/// The USERDATA value.
		/// </summary>
		USERDATA = -21,

		/// <summary>
		/// The ID value.
		/// </summary>
		ID = -12
	}

	/// <summary>
	/// SystemMetrics.  SM_*
	/// </summary>
	internal enum SM
	{
		/// <summary>
		/// The CXSCREEN value.
		/// </summary>
		CXSCREEN = 0,

		/// <summary>
		/// The CYSCREEN value.
		/// </summary>
		CYSCREEN = 1,

		/// <summary>
		/// The CXVSCROLL value.
		/// </summary>
		CXVSCROLL = 2,

		/// <summary>
		/// The CYHSCROLL value.
		/// </summary>
		CYHSCROLL = 3,

		/// <summary>
		/// The CYCAPTION value.
		/// </summary>
		CYCAPTION = 4,

		/// <summary>
		/// The CXBORDER value.
		/// </summary>
		CXBORDER = 5,

		/// <summary>
		/// The CYBORDER value.
		/// </summary>
		CYBORDER = 6,

		/// <summary>
		/// The CXFIXEDFRAME value.
		/// </summary>
		CXFIXEDFRAME = 7,

		/// <summary>
		/// The CYFIXEDFRAME value.
		/// </summary>
		CYFIXEDFRAME = 8,

		/// <summary>
		/// The CYVTHUMB value.
		/// </summary>
		CYVTHUMB = 9,

		/// <summary>
		/// The CXHTHUMB value.
		/// </summary>
		CXHTHUMB = 10,

		/// <summary>
		/// The CXICON value.
		/// </summary>
		CXICON = 11,

		/// <summary>
		/// The CYICON value.
		/// </summary>
		CYICON = 12,

		/// <summary>
		/// The CXCURSOR value.
		/// </summary>
		CXCURSOR = 13,

		/// <summary>
		/// The CYCURSOR value.
		/// </summary>
		CYCURSOR = 14,

		/// <summary>
		/// The CYMENU value.
		/// </summary>
		CYMENU = 15,

		/// <summary>
		/// The CXFULLSCREEN value.
		/// </summary>
		CXFULLSCREEN = 16,

		/// <summary>
		/// The CYFULLSCREEN value.
		/// </summary>
		CYFULLSCREEN = 17,

		/// <summary>
		/// The CYKANJIWINDOW value.
		/// </summary>
		CYKANJIWINDOW = 18,

		/// <summary>
		/// The MOUSEPRESENT value.
		/// </summary>
		MOUSEPRESENT = 19,

		/// <summary>
		/// The CYVSCROLL value.
		/// </summary>
		CYVSCROLL = 20,

		/// <summary>
		/// The CXHSCROLL value.
		/// </summary>
		CXHSCROLL = 21,

		/// <summary>
		/// The DEBUG value.
		/// </summary>
		DEBUG = 22,

		/// <summary>
		/// The SWAPBUTTON value.
		/// </summary>
		SWAPBUTTON = 23,

		/// <summary>
		/// The CXMIN value.
		/// </summary>
		CXMIN = 28,

		/// <summary>
		/// The CYMIN value.
		/// </summary>
		CYMIN = 29,

		/// <summary>
		/// The CXSIZE value.
		/// </summary>
		CXSIZE = 30,

		/// <summary>
		/// The CYSIZE value.
		/// </summary>
		CYSIZE = 31,

		/// <summary>
		/// The CXFRAME value.
		/// </summary>
		CXFRAME = 32,

		/// <summary>
		/// The CXSIZEFRAME value.
		/// </summary>
		CXSIZEFRAME = CXFRAME,

		/// <summary>
		/// The CYFRAME value.
		/// </summary>
		CYFRAME = 33,

		/// <summary>
		/// The CYSIZEFRAME value.
		/// </summary>
		CYSIZEFRAME = CYFRAME,

		/// <summary>
		/// The CXMINTRACK value.
		/// </summary>
		CXMINTRACK = 34,

		/// <summary>
		/// The CYMINTRACK value.
		/// </summary>
		CYMINTRACK = 35,

		/// <summary>
		/// The CXDOUBLECLK value.
		/// </summary>
		CXDOUBLECLK = 36,

		/// <summary>
		/// The CYDOUBLECLK value.
		/// </summary>
		CYDOUBLECLK = 37,

		/// <summary>
		/// The CXICONSPACING value.
		/// </summary>
		CXICONSPACING = 38,

		/// <summary>
		/// The CYICONSPACING value.
		/// </summary>
		CYICONSPACING = 39,

		/// <summary>
		/// The MENUDROPALIGNMENT value.
		/// </summary>
		MENUDROPALIGNMENT = 40,

		/// <summary>
		/// The PENWINDOWS value.
		/// </summary>
		PENWINDOWS = 41,

		/// <summary>
		/// The DBCSENABLED value.
		/// </summary>
		DBCSENABLED = 42,

		/// <summary>
		/// The CMOUSEBUTTONS value.
		/// </summary>
		CMOUSEBUTTONS = 43,

		/// <summary>
		/// The SECURE value.
		/// </summary>
		SECURE = 44,

		/// <summary>
		/// The CXEDGE value.
		/// </summary>
		CXEDGE = 45,

		/// <summary>
		/// The CYEDGE value.
		/// </summary>
		CYEDGE = 46,

		/// <summary>
		/// The CXMINSPACING value.
		/// </summary>
		CXMINSPACING = 47,

		/// <summary>
		/// The CYMINSPACING value.
		/// </summary>
		CYMINSPACING = 48,

		/// <summary>
		/// The CXSMICON value.
		/// </summary>
		CXSMICON = 49,

		/// <summary>
		/// The CYSMICON value.
		/// </summary>
		CYSMICON = 50,

		/// <summary>
		/// The CYSMCAPTION value.
		/// </summary>
		CYSMCAPTION = 51,

		/// <summary>
		/// The CXSMSIZE value.
		/// </summary>
		CXSMSIZE = 52,

		/// <summary>
		/// The CYSMSIZE value.
		/// </summary>
		CYSMSIZE = 53,

		/// <summary>
		/// The CXMENUSIZE value.
		/// </summary>
		CXMENUSIZE = 54,

		/// <summary>
		/// The CYMENUSIZE value.
		/// </summary>
		CYMENUSIZE = 55,

		/// <summary>
		/// The ARRANGE value.
		/// </summary>
		ARRANGE = 56,

		/// <summary>
		/// The CXMINIMIZED value.
		/// </summary>
		CXMINIMIZED = 57,

		/// <summary>
		/// The CYMINIMIZED value.
		/// </summary>
		CYMINIMIZED = 58,

		/// <summary>
		/// The CXMAXTRACK value.
		/// </summary>
		CXMAXTRACK = 59,

		/// <summary>
		/// The CYMAXTRACK value.
		/// </summary>
		CYMAXTRACK = 60,

		/// <summary>
		/// The CXMAXIMIZED value.
		/// </summary>
		CXMAXIMIZED = 61,

		/// <summary>
		/// The CYMAXIMIZED value.
		/// </summary>
		CYMAXIMIZED = 62,

		/// <summary>
		/// The NETWORK value.
		/// </summary>
		NETWORK = 63,

		/// <summary>
		/// The CLEANBOOT value.
		/// </summary>
		CLEANBOOT = 67,

		/// <summary>
		/// The CXDRAG value.
		/// </summary>
		CXDRAG = 68,

		/// <summary>
		/// The CYDRAG value.
		/// </summary>
		CYDRAG = 69,

		/// <summary>
		/// The SHOWSOUNDS value.
		/// </summary>
		SHOWSOUNDS = 70,

		/// <summary>
		/// The CXMENUCHECK value.
		/// </summary>
		CXMENUCHECK = 71,

		/// <summary>
		/// The CYMENUCHECK value.
		/// </summary>
		CYMENUCHECK = 72,

		/// <summary>
		/// The SLOWMACHINE value.
		/// </summary>
		SLOWMACHINE = 73,

		/// <summary>
		/// The MIDEASTENABLED value.
		/// </summary>
		MIDEASTENABLED = 74,

		/// <summary>
		/// The MOUSEWHEELPRESENT value.
		/// </summary>
		MOUSEWHEELPRESENT = 75,

		/// <summary>
		/// The XVIRTUALSCREEN value.
		/// </summary>
		XVIRTUALSCREEN = 76,

		/// <summary>
		/// The YVIRTUALSCREEN value.
		/// </summary>
		YVIRTUALSCREEN = 77,

		/// <summary>
		/// The CXVIRTUALSCREEN value.
		/// </summary>
		CXVIRTUALSCREEN = 78,

		/// <summary>
		/// The CYVIRTUALSCREEN value.
		/// </summary>
		CYVIRTUALSCREEN = 79,

		/// <summary>
		/// The CMONITORS value.
		/// </summary>
		CMONITORS = 80,

		/// <summary>
		/// The SAMEDISPLAYFORMAT value.
		/// </summary>
		SAMEDISPLAYFORMAT = 81,

		/// <summary>
		/// The IMMENABLED value.
		/// </summary>
		IMMENABLED = 82,

		/// <summary>
		/// The CXFOCUSBORDER value.
		/// </summary>
		CXFOCUSBORDER = 83,

		/// <summary>
		/// The CYFOCUSBORDER value.
		/// </summary>
		CYFOCUSBORDER = 84,

		/// <summary>
		/// The TABLETPC value.
		/// </summary>
		TABLETPC = 86,

		/// <summary>
		/// The MEDIACENTER value.
		/// </summary>
		MEDIACENTER = 87,

		/// <summary>
		/// The REMOTESESSION value.
		/// </summary>
		REMOTESESSION = 0x1000,

		/// <summary>
		/// The REMOTECONTROL value.
		/// </summary>
		REMOTECONTROL = 0x2001,
	}

	/// <summary>
	/// SystemParameterInfo values, SPI_*
	/// </summary>
	internal enum SPI
	{
		/// <summary>
		/// The GETBEEP value.
		/// </summary>
		GETBEEP = 0x0001,

		/// <summary>
		/// The SETBEEP value.
		/// </summary>
		SETBEEP = 0x0002,

		/// <summary>
		/// The GETMOUSE value.
		/// </summary>
		GETMOUSE = 0x0003,

		/// <summary>
		/// The SETMOUSE value.
		/// </summary>
		SETMOUSE = 0x0004,

		/// <summary>
		/// The GETBORDER value.
		/// </summary>
		GETBORDER = 0x0005,

		/// <summary>
		/// The SETBORDER value.
		/// </summary>
		SETBORDER = 0x0006,

		/// <summary>
		/// The GETKEYBOARDSPEED value.
		/// </summary>
		GETKEYBOARDSPEED = 0x000A,

		/// <summary>
		/// The SETKEYBOARDSPEED value.
		/// </summary>
		SETKEYBOARDSPEED = 0x000B,

		/// <summary>
		/// The LANGDRIVER value.
		/// </summary>
		LANGDRIVER = 0x000C,

		/// <summary>
		/// The ICONHORIZONTALSPACING value.
		/// </summary>
		ICONHORIZONTALSPACING = 0x000D,

		/// <summary>
		/// The GETSCREENSAVETIMEOUT value.
		/// </summary>
		GETSCREENSAVETIMEOUT = 0x000E,

		/// <summary>
		/// The SETSCREENSAVETIMEOUT value.
		/// </summary>
		SETSCREENSAVETIMEOUT = 0x000F,

		/// <summary>
		/// The GETSCREENSAVEACTIVE value.
		/// </summary>
		GETSCREENSAVEACTIVE = 0x0010,

		/// <summary>
		/// The SETSCREENSAVEACTIVE value.
		/// </summary>
		SETSCREENSAVEACTIVE = 0x0011,

		/// <summary>
		/// The GETGRIDGRANULARITY value.
		/// </summary>
		GETGRIDGRANULARITY = 0x0012,

		/// <summary>
		/// The SETGRIDGRANULARITY value.
		/// </summary>
		SETGRIDGRANULARITY = 0x0013,

		/// <summary>
		/// The SETDESKWALLPAPER value.
		/// </summary>
		SETDESKWALLPAPER = 0x0014,

		/// <summary>
		/// The SETDESKPATTERN value.
		/// </summary>
		SETDESKPATTERN = 0x0015,

		/// <summary>
		/// The GETKEYBOARDDELAY value.
		/// </summary>
		GETKEYBOARDDELAY = 0x0016,

		/// <summary>
		/// The SETKEYBOARDDELAY value.
		/// </summary>
		SETKEYBOARDDELAY = 0x0017,

		/// <summary>
		/// The ICONVERTICALSPACING value.
		/// </summary>
		ICONVERTICALSPACING = 0x0018,

		/// <summary>
		/// The GETICONTITLEWRAP value.
		/// </summary>
		GETICONTITLEWRAP = 0x0019,

		/// <summary>
		/// The SETICONTITLEWRAP value.
		/// </summary>
		SETICONTITLEWRAP = 0x001A,

		/// <summary>
		/// The GETMENUDROPALIGNMENT value.
		/// </summary>
		GETMENUDROPALIGNMENT = 0x001B,

		/// <summary>
		/// The SETMENUDROPALIGNMENT value.
		/// </summary>
		SETMENUDROPALIGNMENT = 0x001C,

		/// <summary>
		/// The SETDOUBLECLKWIDTH value.
		/// </summary>
		SETDOUBLECLKWIDTH = 0x001D,

		/// <summary>
		/// The SETDOUBLECLKHEIGHT value.
		/// </summary>
		SETDOUBLECLKHEIGHT = 0x001E,

		/// <summary>
		/// The GETICONTITLELOGFONT value.
		/// </summary>
		GETICONTITLELOGFONT = 0x001F,

		/// <summary>
		/// The SETDOUBLECLICKTIME value.
		/// </summary>
		SETDOUBLECLICKTIME = 0x0020,

		/// <summary>
		/// The SETMOUSEBUTTONSWAP value.
		/// </summary>
		SETMOUSEBUTTONSWAP = 0x0021,

		/// <summary>
		/// The SETICONTITLELOGFONT value.
		/// </summary>
		SETICONTITLELOGFONT = 0x0022,

		/// <summary>
		/// The GETFASTTASKSWITCH value.
		/// </summary>
		GETFASTTASKSWITCH = 0x0023,

		/// <summary>
		/// The SETFASTTASKSWITCH value.
		/// </summary>
		SETFASTTASKSWITCH = 0x0024,

		/// <summary>
		/// The SETDRAGFULLWINDOWS value.
		/// </summary>
		SETDRAGFULLWINDOWS = 0x0025,

		/// <summary>
		/// The GETDRAGFULLWINDOWS value.
		/// </summary>
		GETDRAGFULLWINDOWS = 0x0026,

		/// <summary>
		/// The GETNONCLIENTMETRICS value.
		/// </summary>
		GETNONCLIENTMETRICS = 0x0029,

		/// <summary>
		/// The SETNONCLIENTMETRICS value.
		/// </summary>
		SETNONCLIENTMETRICS = 0x002A,

		/// <summary>
		/// The GETMINIMIZEDMETRICS value.
		/// </summary>
		GETMINIMIZEDMETRICS = 0x002B,

		/// <summary>
		/// The SETMINIMIZEDMETRICS value.
		/// </summary>
		SETMINIMIZEDMETRICS = 0x002C,

		/// <summary>
		/// The GETICONMETRICS value.
		/// </summary>
		GETICONMETRICS = 0x002D,

		/// <summary>
		/// The SETICONMETRICS value.
		/// </summary>
		SETICONMETRICS = 0x002E,

		/// <summary>
		/// The SETWORKAREA value.
		/// </summary>
		SETWORKAREA = 0x002F,

		/// <summary>
		/// The GETWORKAREA value.
		/// </summary>
		GETWORKAREA = 0x0030,

		/// <summary>
		/// The SETPENWINDOWS value.
		/// </summary>
		SETPENWINDOWS = 0x0031,

		/// <summary>
		/// The GETHIGHCONTRAST value.
		/// </summary>
		GETHIGHCONTRAST = 0x0042,

		/// <summary>
		/// The SETHIGHCONTRAST value.
		/// </summary>
		SETHIGHCONTRAST = 0x0043,

		/// <summary>
		/// The GETKEYBOARDPREF value.
		/// </summary>
		GETKEYBOARDPREF = 0x0044,

		/// <summary>
		/// The SETKEYBOARDPREF value.
		/// </summary>
		SETKEYBOARDPREF = 0x0045,

		/// <summary>
		/// The GETSCREENREADER value.
		/// </summary>
		GETSCREENREADER = 0x0046,

		/// <summary>
		/// The SETSCREENREADER value.
		/// </summary>
		SETSCREENREADER = 0x0047,

		/// <summary>
		/// The GETANIMATION value.
		/// </summary>
		GETANIMATION = 0x0048,

		/// <summary>
		/// The SETANIMATION value.
		/// </summary>
		SETANIMATION = 0x0049,

		/// <summary>
		/// The GETFONTSMOOTHING value.
		/// </summary>
		GETFONTSMOOTHING = 0x004A,

		/// <summary>
		/// The SETFONTSMOOTHING value.
		/// </summary>
		SETFONTSMOOTHING = 0x004B,

		/// <summary>
		/// The SETDRAGWIDTH value.
		/// </summary>
		SETDRAGWIDTH = 0x004C,

		/// <summary>
		/// The SETDRAGHEIGHT value.
		/// </summary>
		SETDRAGHEIGHT = 0x004D,

		/// <summary>
		/// The SETHANDHELD value.
		/// </summary>
		SETHANDHELD = 0x004E,

		/// <summary>
		/// The GETLOWPOWERTIMEOUT value.
		/// </summary>
		GETLOWPOWERTIMEOUT = 0x004F,

		/// <summary>
		/// The GETPOWEROFFTIMEOUT value.
		/// </summary>
		GETPOWEROFFTIMEOUT = 0x0050,

		/// <summary>
		/// The SETLOWPOWERTIMEOUT value.
		/// </summary>
		SETLOWPOWERTIMEOUT = 0x0051,

		/// <summary>
		/// The SETPOWEROFFTIMEOUT value.
		/// </summary>
		SETPOWEROFFTIMEOUT = 0x0052,

		/// <summary>
		/// The GETLOWPOWERACTIVE value.
		/// </summary>
		GETLOWPOWERACTIVE = 0x0053,

		/// <summary>
		/// The GETPOWEROFFACTIVE value.
		/// </summary>
		GETPOWEROFFACTIVE = 0x0054,

		/// <summary>
		/// The SETLOWPOWERACTIVE value.
		/// </summary>
		SETLOWPOWERACTIVE = 0x0055,

		/// <summary>
		/// The SETPOWEROFFACTIVE value.
		/// </summary>
		SETPOWEROFFACTIVE = 0x0056,

		/// <summary>
		/// The SETCURSORS value.
		/// </summary>
		SETCURSORS = 0x0057,

		/// <summary>
		/// The SETICONS value.
		/// </summary>
		SETICONS = 0x0058,

		/// <summary>
		/// The GETDEFAULTINPUTLANG value.
		/// </summary>
		GETDEFAULTINPUTLANG = 0x0059,

		/// <summary>
		/// The SETDEFAULTINPUTLANG value.
		/// </summary>
		SETDEFAULTINPUTLANG = 0x005A,

		/// <summary>
		/// The SETLANGTOGGLE value.
		/// </summary>
		SETLANGTOGGLE = 0x005B,

		/// <summary>
		/// The GETWINDOWSEXTENSION value.
		/// </summary>
		GETWINDOWSEXTENSION = 0x005C,

		/// <summary>
		/// The SETMOUSETRAILS value.
		/// </summary>
		SETMOUSETRAILS = 0x005D,

		/// <summary>
		/// The GETMOUSETRAILS value.
		/// </summary>
		GETMOUSETRAILS = 0x005E,

		/// <summary>
		/// The SETSCREENSAVERRUNNING value.
		/// </summary>
		SETSCREENSAVERRUNNING = 0x0061,

		/// <summary>
		/// The SCREENSAVERRUNNING value.
		/// </summary>
		SCREENSAVERRUNNING = SETSCREENSAVERRUNNING,

		/// <summary>
		/// The GETFILTERKEYS value.
		/// </summary>
		GETFILTERKEYS = 0x0032,

		/// <summary>
		/// The SETFILTERKEYS value.
		/// </summary>
		SETFILTERKEYS = 0x0033,

		/// <summary>
		/// The GETTOGGLEKEYS value.
		/// </summary>
		GETTOGGLEKEYS = 0x0034,

		/// <summary>
		/// The SETTOGGLEKEYS value.
		/// </summary>
		SETTOGGLEKEYS = 0x0035,

		/// <summary>
		/// The GETMOUSEKEYS value.
		/// </summary>
		GETMOUSEKEYS = 0x0036,

		/// <summary>
		/// The SETMOUSEKEYS value.
		/// </summary>
		SETMOUSEKEYS = 0x0037,

		/// <summary>
		/// The GETSHOWSOUNDS value.
		/// </summary>
		GETSHOWSOUNDS = 0x0038,

		/// <summary>
		/// The SETSHOWSOUNDS value.
		/// </summary>
		SETSHOWSOUNDS = 0x0039,

		/// <summary>
		/// The GETSTICKYKEYS value.
		/// </summary>
		GETSTICKYKEYS = 0x003A,

		/// <summary>
		/// The SETSTICKYKEYS value.
		/// </summary>
		SETSTICKYKEYS = 0x003B,

		/// <summary>
		/// The GETACCESSTIMEOUT value.
		/// </summary>
		GETACCESSTIMEOUT = 0x003C,

		/// <summary>
		/// The SETACCESSTIMEOUT value.
		/// </summary>
		SETACCESSTIMEOUT = 0x003D,

		/// <summary>
		/// The GETSERIALKEYS value.
		/// </summary>
		GETSERIALKEYS = 0x003E,

		/// <summary>
		/// The SETSERIALKEYS value.
		/// </summary>
		SETSERIALKEYS = 0x003F,

		/// <summary>
		/// The GETSOUNDSENTRY value.
		/// </summary>
		GETSOUNDSENTRY = 0x0040,

		/// <summary>
		/// The SETSOUNDSENTRY value.
		/// </summary>
		SETSOUNDSENTRY = 0x0041,

		/// <summary>
		/// The GETSNAPTODEFBUTTON value.
		/// </summary>
		GETSNAPTODEFBUTTON = 0x005F,

		/// <summary>
		/// The SETSNAPTODEFBUTTON value.
		/// </summary>
		SETSNAPTODEFBUTTON = 0x0060,

		/// <summary>
		/// The GETMOUSEHOVERWIDTH value.
		/// </summary>
		GETMOUSEHOVERWIDTH = 0x0062,

		/// <summary>
		/// The SETMOUSEHOVERWIDTH value.
		/// </summary>
		SETMOUSEHOVERWIDTH = 0x0063,

		/// <summary>
		/// The GETMOUSEHOVERHEIGHT value.
		/// </summary>
		GETMOUSEHOVERHEIGHT = 0x0064,

		/// <summary>
		/// The SETMOUSEHOVERHEIGHT value.
		/// </summary>
		SETMOUSEHOVERHEIGHT = 0x0065,

		/// <summary>
		/// The GETMOUSEHOVERTIME value.
		/// </summary>
		GETMOUSEHOVERTIME = 0x0066,

		/// <summary>
		/// The SETMOUSEHOVERTIME value.
		/// </summary>
		SETMOUSEHOVERTIME = 0x0067,

		/// <summary>
		/// The GETWHEELSCROLLLINES value.
		/// </summary>
		GETWHEELSCROLLLINES = 0x0068,

		/// <summary>
		/// The SETWHEELSCROLLLINES value.
		/// </summary>
		SETWHEELSCROLLLINES = 0x0069,

		/// <summary>
		/// The GETMENUSHOWDELAY value.
		/// </summary>
		GETMENUSHOWDELAY = 0x006A,

		/// <summary>
		/// The SETMENUSHOWDELAY value.
		/// </summary>
		SETMENUSHOWDELAY = 0x006B,

		/// <summary>
		/// The GETWHEELSCROLLCHARS value.
		/// </summary>
		GETWHEELSCROLLCHARS = 0x006C,

		/// <summary>
		/// The SETWHEELSCROLLCHARS value.
		/// </summary>
		SETWHEELSCROLLCHARS = 0x006D,

		/// <summary>
		/// The GETSHOWIMEUI value.
		/// </summary>
		GETSHOWIMEUI = 0x006E,

		/// <summary>
		/// The SETSHOWIMEUI value.
		/// </summary>
		SETSHOWIMEUI = 0x006F,

		/// <summary>
		/// The GETMOUSESPEED value.
		/// </summary>
		GETMOUSESPEED = 0x0070,

		/// <summary>
		/// The SETMOUSESPEED value.
		/// </summary>
		SETMOUSESPEED = 0x0071,

		/// <summary>
		/// The GETSCREENSAVERRUNNING value.
		/// </summary>
		GETSCREENSAVERRUNNING = 0x0072,

		/// <summary>
		/// The GETDESKWALLPAPER value.
		/// </summary>
		GETDESKWALLPAPER = 0x0073,

		/// <summary>
		/// The GETAUDIODESCRIPTION value.
		/// </summary>
		GETAUDIODESCRIPTION = 0x0074,

		/// <summary>
		/// The SETAUDIODESCRIPTION value.
		/// </summary>
		SETAUDIODESCRIPTION = 0x0075,

		/// <summary>
		/// The GETSCREENSAVESECURE value.
		/// </summary>
		GETSCREENSAVESECURE = 0x0076,

		/// <summary>
		/// The SETSCREENSAVESECURE value.
		/// </summary>
		SETSCREENSAVESECURE = 0x0077,

		/// <summary>
		/// The GETHUNGAPPTIMEOUT value.
		/// </summary>
		GETHUNGAPPTIMEOUT = 0x0078,

		/// <summary>
		/// The SETHUNGAPPTIMEOUT value.
		/// </summary>
		SETHUNGAPPTIMEOUT = 0x0079,

		/// <summary>
		/// The GETWAITTOKILLTIMEOUT value.
		/// </summary>
		GETWAITTOKILLTIMEOUT = 0x007A,

		/// <summary>
		/// The SETWAITTOKILLTIMEOUT value.
		/// </summary>
		SETWAITTOKILLTIMEOUT = 0x007B,

		/// <summary>
		/// The GETWAITTOKILLSERVICETIMEOUT value.
		/// </summary>
		GETWAITTOKILLSERVICETIMEOUT = 0x007C,

		/// <summary>
		/// The SETWAITTOKILLSERVICETIMEOUT value.
		/// </summary>
		SETWAITTOKILLSERVICETIMEOUT = 0x007D,

		/// <summary>
		/// The GETMOUSEDOCKTHRESHOLD value.
		/// </summary>
		GETMOUSEDOCKTHRESHOLD = 0x007E,

		/// <summary>
		/// The SETMOUSEDOCKTHRESHOLD value.
		/// </summary>
		SETMOUSEDOCKTHRESHOLD = 0x007F,

		/// <summary>
		/// The GETPENDOCKTHRESHOLD value.
		/// </summary>
		GETPENDOCKTHRESHOLD = 0x0080,

		/// <summary>
		/// The SETPENDOCKTHRESHOLD value.
		/// </summary>
		SETPENDOCKTHRESHOLD = 0x0081,

		/// <summary>
		/// The GETWINARRANGING value.
		/// </summary>
		GETWINARRANGING = 0x0082,

		/// <summary>
		/// The SETWINARRANGING value.
		/// </summary>
		SETWINARRANGING = 0x0083,

		/// <summary>
		/// The GETMOUSEDRAGOUTTHRESHOLD value.
		/// </summary>
		GETMOUSEDRAGOUTTHRESHOLD = 0x0084,

		/// <summary>
		/// The SETMOUSEDRAGOUTTHRESHOLD value.
		/// </summary>
		SETMOUSEDRAGOUTTHRESHOLD = 0x0085,

		/// <summary>
		/// The GETPENDRAGOUTTHRESHOLD value.
		/// </summary>
		GETPENDRAGOUTTHRESHOLD = 0x0086,

		/// <summary>
		/// The SETPENDRAGOUTTHRESHOLD value.
		/// </summary>
		SETPENDRAGOUTTHRESHOLD = 0x0087,

		/// <summary>
		/// The GETMOUSESIDEMOVETHRESHOLD value.
		/// </summary>
		GETMOUSESIDEMOVETHRESHOLD = 0x0088,

		/// <summary>
		/// The SETMOUSESIDEMOVETHRESHOLD value.
		/// </summary>
		SETMOUSESIDEMOVETHRESHOLD = 0x0089,

		/// <summary>
		/// The GETPENSIDEMOVETHRESHOLD value.
		/// </summary>
		GETPENSIDEMOVETHRESHOLD = 0x008A,

		/// <summary>
		/// The SETPENSIDEMOVETHRESHOLD value.
		/// </summary>
		SETPENSIDEMOVETHRESHOLD = 0x008B,

		/// <summary>
		/// The GETDRAGFROMMAXIMIZE value.
		/// </summary>
		GETDRAGFROMMAXIMIZE = 0x008C,

		/// <summary>
		/// The SETDRAGFROMMAXIMIZE value.
		/// </summary>
		SETDRAGFROMMAXIMIZE = 0x008D,

		/// <summary>
		/// The GETSNAPSIZING value.
		/// </summary>
		GETSNAPSIZING = 0x008E,

		/// <summary>
		/// The SETSNAPSIZING value.
		/// </summary>
		SETSNAPSIZING = 0x008F,

		/// <summary>
		/// The GETDOCKMOVING value.
		/// </summary>
		GETDOCKMOVING = 0x0090,

		/// <summary>
		/// The SETDOCKMOVING value.
		/// </summary>
		SETDOCKMOVING = 0x0091,

		/// <summary>
		/// The GETACTIVEWINDOWTRACKING value.
		/// </summary>
		GETACTIVEWINDOWTRACKING = 0x1000,

		/// <summary>
		/// The SETACTIVEWINDOWTRACKING value.
		/// </summary>
		SETACTIVEWINDOWTRACKING = 0x1001,

		/// <summary>
		/// The GETMENUANIMATION value.
		/// </summary>
		GETMENUANIMATION = 0x1002,

		/// <summary>
		/// The SETMENUANIMATION value.
		/// </summary>
		SETMENUANIMATION = 0x1003,

		/// <summary>
		/// The GETCOMBOBOXANIMATION value.
		/// </summary>
		GETCOMBOBOXANIMATION = 0x1004,

		/// <summary>
		/// The SETCOMBOBOXANIMATION value.
		/// </summary>
		SETCOMBOBOXANIMATION = 0x1005,

		/// <summary>
		/// The GETLISTBOXSMOOTHSCROLLING value.
		/// </summary>
		GETLISTBOXSMOOTHSCROLLING = 0x1006,

		/// <summary>
		/// The SETLISTBOXSMOOTHSCROLLING value.
		/// </summary>
		SETLISTBOXSMOOTHSCROLLING = 0x1007,

		/// <summary>
		/// The GETGRADIENTCAPTIONS value.
		/// </summary>
		GETGRADIENTCAPTIONS = 0x1008,

		/// <summary>
		/// The SETGRADIENTCAPTIONS value.
		/// </summary>
		SETGRADIENTCAPTIONS = 0x1009,

		/// <summary>
		/// The GETKEYBOARDCUES value.
		/// </summary>
		GETKEYBOARDCUES = 0x100A,

		/// <summary>
		/// The SETKEYBOARDCUES value.
		/// </summary>
		SETKEYBOARDCUES = 0x100B,

		/// <summary>
		/// The GETMENUUNDERLINES value.
		/// </summary>
		GETMENUUNDERLINES = GETKEYBOARDCUES,

		/// <summary>
		/// The SETMENUUNDERLINES value.
		/// </summary>
		SETMENUUNDERLINES = SETKEYBOARDCUES,

		/// <summary>
		/// The GETACTIVEWNDTRKZORDER value.
		/// </summary>
		GETACTIVEWNDTRKZORDER = 0x100C,

		/// <summary>
		/// The SETACTIVEWNDTRKZORDER value.
		/// </summary>
		SETACTIVEWNDTRKZORDER = 0x100D,

		/// <summary>
		/// The GETHOTTRACKING value.
		/// </summary>
		GETHOTTRACKING = 0x100E,

		/// <summary>
		/// The SETHOTTRACKING value.
		/// </summary>
		SETHOTTRACKING = 0x100F,

		/// <summary>
		/// The GETMENUFADE value.
		/// </summary>
		GETMENUFADE = 0x1012,

		/// <summary>
		/// The SETMENUFADE value.
		/// </summary>
		SETMENUFADE = 0x1013,

		/// <summary>
		/// The GETSELECTIONFADE value.
		/// </summary>
		GETSELECTIONFADE = 0x1014,

		/// <summary>
		/// The SETSELECTIONFADE value.
		/// </summary>
		SETSELECTIONFADE = 0x1015,

		/// <summary>
		/// The GETTOOLTIPANIMATION value.
		/// </summary>
		GETTOOLTIPANIMATION = 0x1016,

		/// <summary>
		/// The SETTOOLTIPANIMATION value.
		/// </summary>
		SETTOOLTIPANIMATION = 0x1017,

		/// <summary>
		/// The GETTOOLTIPFADE value.
		/// </summary>
		GETTOOLTIPFADE = 0x1018,

		/// <summary>
		/// The SETTOOLTIPFADE value.
		/// </summary>
		SETTOOLTIPFADE = 0x1019,

		/// <summary>
		/// The GETCURSORSHADOW value.
		/// </summary>
		GETCURSORSHADOW = 0x101A,

		/// <summary>
		/// The SETCURSORSHADOW value.
		/// </summary>
		SETCURSORSHADOW = 0x101B,

		/// <summary>
		/// The GETMOUSESONAR value.
		/// </summary>
		GETMOUSESONAR = 0x101C,

		/// <summary>
		/// The SETMOUSESONAR value.
		/// </summary>
		SETMOUSESONAR = 0x101D,

		/// <summary>
		/// The GETMOUSECLICKLOCK value.
		/// </summary>
		GETMOUSECLICKLOCK = 0x101E,

		/// <summary>
		/// The SETMOUSECLICKLOCK value.
		/// </summary>
		SETMOUSECLICKLOCK = 0x101F,

		/// <summary>
		/// The GETMOUSEVANISH value.
		/// </summary>
		GETMOUSEVANISH = 0x1020,

		/// <summary>
		/// The SETMOUSEVANISH value.
		/// </summary>
		SETMOUSEVANISH = 0x1021,

		/// <summary>
		/// The GETFLATMENU value.
		/// </summary>
		GETFLATMENU = 0x1022,

		/// <summary>
		/// The SETFLATMENU value.
		/// </summary>
		SETFLATMENU = 0x1023,

		/// <summary>
		/// The GETDROPSHADOW value.
		/// </summary>
		GETDROPSHADOW = 0x1024,

		/// <summary>
		/// The SETDROPSHADOW value.
		/// </summary>
		SETDROPSHADOW = 0x1025,

		/// <summary>
		/// The GETBLOCKSENDINPUTRESETS value.
		/// </summary>
		GETBLOCKSENDINPUTRESETS = 0x1026,

		/// <summary>
		/// The SETBLOCKSENDINPUTRESETS value.
		/// </summary>
		SETBLOCKSENDINPUTRESETS = 0x1027,

		/// <summary>
		/// The GETUIEFFECTS value.
		/// </summary>
		GETUIEFFECTS = 0x103E,

		/// <summary>
		/// The SETUIEFFECTS value.
		/// </summary>
		SETUIEFFECTS = 0x103F,

		/// <summary>
		/// The GETDISABLEOVERLAPPEDCONTENT value.
		/// </summary>
		GETDISABLEOVERLAPPEDCONTENT = 0x1040,

		/// <summary>
		/// The SETDISABLEOVERLAPPEDCONTENT value.
		/// </summary>
		SETDISABLEOVERLAPPEDCONTENT = 0x1041,

		/// <summary>
		/// The GETCLIENTAREAANIMATION value.
		/// </summary>
		GETCLIENTAREAANIMATION = 0x1042,

		/// <summary>
		/// The SETCLIENTAREAANIMATION value.
		/// </summary>
		SETCLIENTAREAANIMATION = 0x1043,

		/// <summary>
		/// The GETCLEARTYPE value.
		/// </summary>
		GETCLEARTYPE = 0x1048,

		/// <summary>
		/// The SETCLEARTYPE value.
		/// </summary>
		SETCLEARTYPE = 0x1049,

		/// <summary>
		/// The GETSPEECHRECOGNITION value.
		/// </summary>
		GETSPEECHRECOGNITION = 0x104A,

		/// <summary>
		/// The SETSPEECHRECOGNITION value.
		/// </summary>
		SETSPEECHRECOGNITION = 0x104B,

		/// <summary>
		/// The GETFOREGROUNDLOCKTIMEOUT value.
		/// </summary>
		GETFOREGROUNDLOCKTIMEOUT = 0x2000,

		/// <summary>
		/// The SETFOREGROUNDLOCKTIMEOUT value.
		/// </summary>
		SETFOREGROUNDLOCKTIMEOUT = 0x2001,

		/// <summary>
		/// The GETACTIVEWNDTRKTIMEOUT value.
		/// </summary>
		GETACTIVEWNDTRKTIMEOUT = 0x2002,

		/// <summary>
		/// The SETACTIVEWNDTRKTIMEOUT value.
		/// </summary>
		SETACTIVEWNDTRKTIMEOUT = 0x2003,

		/// <summary>
		/// The GETFOREGROUNDFLASHCOUNT value.
		/// </summary>
		GETFOREGROUNDFLASHCOUNT = 0x2004,

		/// <summary>
		/// The SETFOREGROUNDFLASHCOUNT value.
		/// </summary>
		SETFOREGROUNDFLASHCOUNT = 0x2005,

		/// <summary>
		/// The GETCARETWIDTH value.
		/// </summary>
		GETCARETWIDTH = 0x2006,

		/// <summary>
		/// The SETCARETWIDTH value.
		/// </summary>
		SETCARETWIDTH = 0x2007,

		/// <summary>
		/// The GETMOUSECLICKLOCKTIME value.
		/// </summary>
		GETMOUSECLICKLOCKTIME = 0x2008,

		/// <summary>
		/// The SETMOUSECLICKLOCKTIME value.
		/// </summary>
		SETMOUSECLICKLOCKTIME = 0x2009,

		/// <summary>
		/// The GETFONTSMOOTHINGTYPE value.
		/// </summary>
		GETFONTSMOOTHINGTYPE = 0x200A,

		/// <summary>
		/// The SETFONTSMOOTHINGTYPE value.
		/// </summary>
		SETFONTSMOOTHINGTYPE = 0x200B,

		/// <summary>
		/// The GETFONTSMOOTHINGCONTRAST value.
		/// </summary>
		GETFONTSMOOTHINGCONTRAST = 0x200C,

		/// <summary>
		/// The SETFONTSMOOTHINGCONTRAST value.
		/// </summary>
		SETFONTSMOOTHINGCONTRAST = 0x200D,

		/// <summary>
		/// The GETFOCUSBORDERWIDTH value.
		/// </summary>
		GETFOCUSBORDERWIDTH = 0x200E,

		/// <summary>
		/// The SETFOCUSBORDERWIDTH value.
		/// </summary>
		SETFOCUSBORDERWIDTH = 0x200F,

		/// <summary>
		/// The GETFOCUSBORDERHEIGHT value.
		/// </summary>
		GETFOCUSBORDERHEIGHT = 0x2010,

		/// <summary>
		/// The SETFOCUSBORDERHEIGHT value.
		/// </summary>
		SETFOCUSBORDERHEIGHT = 0x2011,

		/// <summary>
		/// The GETFONTSMOOTHINGORIENTATION value.
		/// </summary>
		GETFONTSMOOTHINGORIENTATION = 0x2012,

		/// <summary>
		/// The SETFONTSMOOTHINGORIENTATION value.
		/// </summary>
		SETFONTSMOOTHINGORIENTATION = 0x2013,

		/// <summary>
		/// The GETMINIMUMHITRADIUS value.
		/// </summary>
		GETMINIMUMHITRADIUS = 0x2014,

		/// <summary>
		/// The SETMINIMUMHITRADIUS value.
		/// </summary>
		SETMINIMUMHITRADIUS = 0x2015,

		/// <summary>
		/// The GETMESSAGEDURATION value.
		/// </summary>
		GETMESSAGEDURATION = 0x2016,

		/// <summary>
		/// The SETMESSAGEDURATION value.
		/// </summary>
		SETMESSAGEDURATION = 0x2017,
	}

	/// <summary>
	/// SystemParameterInfo flag values, SPIF_*
	/// </summary>
	[Flags]
	internal enum SPIF
	{
		/// <summary>
		/// The None value.
		/// </summary>
		None = 0,

		/// <summary>
		/// The UPDATEINIFILE value.
		/// </summary>
		UPDATEINIFILE = 0x01,

		/// <summary>
		/// The SENDCHANGE value.
		/// </summary>
		SENDCHANGE = 0x02,

		/// <summary>
		/// The SENDWININICHANGE value.
		/// </summary>
		SENDWININICHANGE = SENDCHANGE,
	}

	/// <summary>
	/// Native STATE_SYSTEM enumeration.
	/// </summary>
	[Flags]
	internal enum STATE_SYSTEM
	{
		/// <summary>
		/// The UNAVAILABLE value.
		/// </summary>
		UNAVAILABLE = 0x00000001, // Disabled

		/// <summary>
		/// The SELECTED value.
		/// </summary>
		SELECTED = 0x00000002,

		/// <summary>
		/// The FOCUSED value.
		/// </summary>
		FOCUSED = 0x00000004,

		/// <summary>
		/// The PRESSED value.
		/// </summary>
		PRESSED = 0x00000008,

		/// <summary>
		/// The CHECKED value.
		/// </summary>
		CHECKED = 0x00000010,

		/// <summary>
		/// The MIXED value.
		/// </summary>
		MIXED = 0x00000020,  // 3-state checkbox or toolbar button

		/// <summary>
		/// The INDETERMINATE value.
		/// </summary>
		INDETERMINATE = MIXED,

		/// <summary>
		/// The READONLY value.
		/// </summary>
		READONLY = 0x00000040,

		/// <summary>
		/// The HOTTRACKED value.
		/// </summary>
		HOTTRACKED = 0x00000080,

		/// <summary>
		/// The DEFAULT value.
		/// </summary>
		DEFAULT = 0x00000100,

		/// <summary>
		/// The EXPANDED value.
		/// </summary>
		EXPANDED = 0x00000200,

		/// <summary>
		/// The COLLAPSED value.
		/// </summary>
		COLLAPSED = 0x00000400,

		/// <summary>
		/// The BUSY value.
		/// </summary>
		BUSY = 0x00000800,

		/// <summary>
		/// The FLOATING value.
		/// </summary>
		FLOATING = 0x00001000,  // Children "owned" not "contained" by parent

		/// <summary>
		/// The MARQUEED value.
		/// </summary>
		MARQUEED = 0x00002000,

		/// <summary>
		/// The ANIMATED value.
		/// </summary>
		ANIMATED = 0x00004000,

		/// <summary>
		/// The INVISIBLE value.
		/// </summary>
		INVISIBLE = 0x00008000,

		/// <summary>
		/// The OFFSCREEN value.
		/// </summary>
		OFFSCREEN = 0x00010000,

		/// <summary>
		/// The SIZEABLE value.
		/// </summary>
		SIZEABLE = 0x00020000,

		/// <summary>
		/// The MOVEABLE value.
		/// </summary>
		MOVEABLE = 0x00040000,

		/// <summary>
		/// The SELFVOICING value.
		/// </summary>
		SELFVOICING = 0x00080000,

		/// <summary>
		/// The FOCUSABLE value.
		/// </summary>
		FOCUSABLE = 0x00100000,

		/// <summary>
		/// The SELECTABLE value.
		/// </summary>
		SELECTABLE = 0x00200000,

		/// <summary>
		/// The LINKED value.
		/// </summary>
		LINKED = 0x00400000,

		/// <summary>
		/// The TRAVERSED value.
		/// </summary>
		TRAVERSED = 0x00800000,

		/// <summary>
		/// The MULTISELECTABLE value.
		/// </summary>
		MULTISELECTABLE = 0x01000000,  // Supports multiple selection

		/// <summary>
		/// The EXTSELECTABLE value.
		/// </summary>
		EXTSELECTABLE = 0x02000000,  // Supports extended selection

		/// <summary>
		/// The ALERT_LOW value.
		/// </summary>
		ALERT_LOW = 0x04000000,  // This information is of low priority

		/// <summary>
		/// The ALERT_MEDIUM value.
		/// </summary>
		ALERT_MEDIUM = 0x08000000,  // This information is of medium priority

		/// <summary>
		/// The ALERT_HIGH value.
		/// </summary>
		ALERT_HIGH = 0x10000000,  // This information is of high priority

		/// <summary>
		/// The PROTECTED value.
		/// </summary>
		PROTECTED = 0x20000000,  // access to this is restricted

		/// <summary>
		/// The VALID value.
		/// </summary>
		VALID = 0x3FFFFFFF,
	}

	/// <summary>
	/// Native StockObject enumeration.
	/// </summary>
	internal enum StockObject : int
	{
		/// <summary>
		/// The WHITE_BRUSH value.
		/// </summary>
		WHITE_BRUSH = 0,

		/// <summary>
		/// The LTGRAY_BRUSH value.
		/// </summary>
		LTGRAY_BRUSH = 1,

		/// <summary>
		/// The GRAY_BRUSH value.
		/// </summary>
		GRAY_BRUSH = 2,

		/// <summary>
		/// The DKGRAY_BRUSH value.
		/// </summary>
		DKGRAY_BRUSH = 3,

		/// <summary>
		/// The BLACK_BRUSH value.
		/// </summary>
		BLACK_BRUSH = 4,

		/// <summary>
		/// The NULL_BRUSH value.
		/// </summary>
		NULL_BRUSH = 5,

		/// <summary>
		/// The HOLLOW_BRUSH value.
		/// </summary>
		HOLLOW_BRUSH = NULL_BRUSH,

		/// <summary>
		/// The WHITE_PEN value.
		/// </summary>
		WHITE_PEN = 6,

		/// <summary>
		/// The BLACK_PEN value.
		/// </summary>
		BLACK_PEN = 7,

		/// <summary>
		/// The NULL_PEN value.
		/// </summary>
		NULL_PEN = 8,

		/// <summary>
		/// The SYSTEM_FONT value.
		/// </summary>
		SYSTEM_FONT = 13,

		/// <summary>
		/// The DEFAULT_PALETTE value.
		/// </summary>
		DEFAULT_PALETTE = 15,
	}

	/// <summary>
	/// CS_*
	/// </summary>
	[Flags]
	internal enum CS : uint
	{
		/// <summary>
		/// The VREDRAW value.
		/// </summary>
		VREDRAW = 0x0001,

		/// <summary>
		/// The HREDRAW value.
		/// </summary>
		HREDRAW = 0x0002,

		/// <summary>
		/// The DBLCLKS value.
		/// </summary>
		DBLCLKS = 0x0008,

		/// <summary>
		/// The OWNDC value.
		/// </summary>
		OWNDC = 0x0020,

		/// <summary>
		/// The CLASSDC value.
		/// </summary>
		CLASSDC = 0x0040,

		/// <summary>
		/// The PARENTDC value.
		/// </summary>
		PARENTDC = 0x0080,

		/// <summary>
		/// The NOCLOSE value.
		/// </summary>
		NOCLOSE = 0x0200,

		/// <summary>
		/// The SAVEBITS value.
		/// </summary>
		SAVEBITS = 0x0800,

		/// <summary>
		/// The BYTEALIGNCLIENT value.
		/// </summary>
		BYTEALIGNCLIENT = 0x1000,

		/// <summary>
		/// The BYTEALIGNWINDOW value.
		/// </summary>
		BYTEALIGNWINDOW = 0x2000,

		/// <summary>
		/// The GLOBALCLASS value.
		/// </summary>
		GLOBALCLASS = 0x4000,

		/// <summary>
		/// The IME value.
		/// </summary>
		IME = 0x00010000,

		/// <summary>
		/// The DROPSHADOW value.
		/// </summary>
		DROPSHADOW = 0x00020000
	}

	/// <summary>
	/// WindowStyle values, WS_*
	/// </summary>
	[Flags]
	internal enum WS : uint
	{
		/// <summary>
		/// The OVERLAPPED value.
		/// </summary>
		OVERLAPPED = 0x00000000,

		/// <summary>
		/// The POPUP value.
		/// </summary>
		POPUP = 0x80000000,

		/// <summary>
		/// The CHILD value.
		/// </summary>
		CHILD = 0x40000000,

		/// <summary>
		/// The MINIMIZE value.
		/// </summary>
		MINIMIZE = 0x20000000,

		/// <summary>
		/// The VISIBLE value.
		/// </summary>
		VISIBLE = 0x10000000,

		/// <summary>
		/// The DISABLED value.
		/// </summary>
		DISABLED = 0x08000000,

		/// <summary>
		/// The CLIPSIBLINGS value.
		/// </summary>
		CLIPSIBLINGS = 0x04000000,

		/// <summary>
		/// The CLIPCHILDREN value.
		/// </summary>
		CLIPCHILDREN = 0x02000000,

		/// <summary>
		/// The MAXIMIZE value.
		/// </summary>
		MAXIMIZE = 0x01000000,

		/// <summary>
		/// The BORDER value.
		/// </summary>
		BORDER = 0x00800000,

		/// <summary>
		/// The DLGFRAME value.
		/// </summary>
		DLGFRAME = 0x00400000,

		/// <summary>
		/// The VSCROLL value.
		/// </summary>
		VSCROLL = 0x00200000,

		/// <summary>
		/// The HSCROLL value.
		/// </summary>
		HSCROLL = 0x00100000,

		/// <summary>
		/// The SYSMENU value.
		/// </summary>
		SYSMENU = 0x00080000,

		/// <summary>
		/// The THICKFRAME value.
		/// </summary>
		THICKFRAME = 0x00040000,

		/// <summary>
		/// The GROUP value.
		/// </summary>
		GROUP = 0x00020000,

		/// <summary>
		/// The TABSTOP value.
		/// </summary>
		TABSTOP = 0x00010000,

		/// <summary>
		/// The MINIMIZEBOX value.
		/// </summary>
		MINIMIZEBOX = 0x00020000,

		/// <summary>
		/// The MAXIMIZEBOX value.
		/// </summary>
		MAXIMIZEBOX = 0x00010000,

		/// <summary>
		/// The CAPTION value.
		/// </summary>
		CAPTION = BORDER | DLGFRAME,

		/// <summary>
		/// The TILED value.
		/// </summary>
		TILED = OVERLAPPED,

		/// <summary>
		/// The ICONIC value.
		/// </summary>
		ICONIC = MINIMIZE,

		/// <summary>
		/// The SIZEBOX value.
		/// </summary>
		SIZEBOX = THICKFRAME,

		/// <summary>
		/// The TILEDWINDOW value.
		/// </summary>
		TILEDWINDOW = OVERLAPPEDWINDOW,

		/// <summary>
		/// The OVERLAPPEDWINDOW value.
		/// </summary>
		OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,

		/// <summary>
		/// The POPUPWINDOW value.
		/// </summary>
		POPUPWINDOW = POPUP | BORDER | SYSMENU,

		/// <summary>
		/// The CHILDWINDOW value.
		/// </summary>
		CHILDWINDOW = CHILD,
	}

	/// <summary>
	/// Window message values, WM_*
	/// </summary>
	internal enum WM
	{
		/// <summary>
		/// The NULL value.
		/// </summary>
		NULL = 0x0000,

		/// <summary>
		/// The CREATE value.
		/// </summary>
		CREATE = 0x0001,

		/// <summary>
		/// The DESTROY value.
		/// </summary>
		DESTROY = 0x0002,

		/// <summary>
		/// The MOVE value.
		/// </summary>
		MOVE = 0x0003,

		/// <summary>
		/// The SIZE value.
		/// </summary>
		SIZE = 0x0005,

		/// <summary>
		/// The ACTIVATE value.
		/// </summary>
		ACTIVATE = 0x0006,

		/// <summary>
		/// The SETFOCUS value.
		/// </summary>
		SETFOCUS = 0x0007,

		/// <summary>
		/// The KILLFOCUS value.
		/// </summary>
		KILLFOCUS = 0x0008,

		/// <summary>
		/// The ENABLE value.
		/// </summary>
		ENABLE = 0x000A,

		/// <summary>
		/// The SETREDRAW value.
		/// </summary>
		SETREDRAW = 0x000B,

		/// <summary>
		/// The SETTEXT value.
		/// </summary>
		SETTEXT = 0x000C,

		/// <summary>
		/// The GETTEXT value.
		/// </summary>
		GETTEXT = 0x000D,

		/// <summary>
		/// The GETTEXTLENGTH value.
		/// </summary>
		GETTEXTLENGTH = 0x000E,

		/// <summary>
		/// The PAINT value.
		/// </summary>
		PAINT = 0x000F,

		/// <summary>
		/// The CLOSE value.
		/// </summary>
		CLOSE = 0x0010,

		/// <summary>
		/// The QUERYENDSESSION value.
		/// </summary>
		QUERYENDSESSION = 0x0011,

		/// <summary>
		/// The QUIT value.
		/// </summary>
		QUIT = 0x0012,

		/// <summary>
		/// The QUERYOPEN value.
		/// </summary>
		QUERYOPEN = 0x0013,

		/// <summary>
		/// The ERASEBKGND value.
		/// </summary>
		ERASEBKGND = 0x0014,

		/// <summary>
		/// The SYSCOLORCHANGE value.
		/// </summary>
		SYSCOLORCHANGE = 0x0015,

		/// <summary>
		/// The SHOWWINDOW value.
		/// </summary>
		SHOWWINDOW = 0x0018,

		/// <summary>
		/// The CTLCOLOR value.
		/// </summary>
		CTLCOLOR = 0x0019,

		/// <summary>
		/// The WININICHANGE value.
		/// </summary>
		WININICHANGE = 0x001A,

		/// <summary>
		/// The SETTINGCHANGE value.
		/// </summary>
		SETTINGCHANGE = 0x001A,

		/// <summary>
		/// The ACTIVATEAPP value.
		/// </summary>
		ACTIVATEAPP = 0x001C,

		/// <summary>
		/// The SETCURSOR value.
		/// </summary>
		SETCURSOR = 0x0020,

		/// <summary>
		/// The MOUSEACTIVATE value.
		/// </summary>
		MOUSEACTIVATE = 0x0021,

		/// <summary>
		/// The CHILDACTIVATE value.
		/// </summary>
		CHILDACTIVATE = 0x0022,

		/// <summary>
		/// The QUEUESYNC value.
		/// </summary>
		QUEUESYNC = 0x0023,

		/// <summary>
		/// The GETMINMAXINFO value.
		/// </summary>
		GETMINMAXINFO = 0x0024,

		/// <summary>
		/// The WINDOWPOSCHANGING value.
		/// </summary>
		WINDOWPOSCHANGING = 0x0046,

		/// <summary>
		/// The WINDOWPOSCHANGED value.
		/// </summary>
		WINDOWPOSCHANGED = 0x0047,

		/// <summary>
		/// The CONTEXTMENU value.
		/// </summary>
		CONTEXTMENU = 0x007B,

		/// <summary>
		/// The STYLECHANGING value.
		/// </summary>
		STYLECHANGING = 0x007C,

		/// <summary>
		/// The STYLECHANGED value.
		/// </summary>
		STYLECHANGED = 0x007D,

		/// <summary>
		/// The DISPLAYCHANGE value.
		/// </summary>
		DISPLAYCHANGE = 0x007E,

		/// <summary>
		/// The GETICON value.
		/// </summary>
		GETICON = 0x007F,

		/// <summary>
		/// The SETICON value.
		/// </summary>
		SETICON = 0x0080,

		/// <summary>
		/// The NCCREATE value.
		/// </summary>
		NCCREATE = 0x0081,

		/// <summary>
		/// The NCDESTROY value.
		/// </summary>
		NCDESTROY = 0x0082,

		/// <summary>
		/// The NCCALCSIZE value.
		/// </summary>
		NCCALCSIZE = 0x0083,

		/// <summary>
		/// The NCHITTEST value.
		/// </summary>
		NCHITTEST = 0x0084,

		/// <summary>
		/// The NCPAINT value.
		/// </summary>
		NCPAINT = 0x0085,

		/// <summary>
		/// The NCACTIVATE value.
		/// </summary>
		NCACTIVATE = 0x0086,

		/// <summary>
		/// The GETDLGCODE value.
		/// </summary>
		GETDLGCODE = 0x0087,

		/// <summary>
		/// The SYNCPAINT value.
		/// </summary>
		SYNCPAINT = 0x0088,

		/// <summary>
		/// The NCMOUSEMOVE value.
		/// </summary>
		NCMOUSEMOVE = 0x00A0,

		/// <summary>
		/// The NCLBUTTONDOWN value.
		/// </summary>
		NCLBUTTONDOWN = 0x00A1,

		/// <summary>
		/// The NCLBUTTONUP value.
		/// </summary>
		NCLBUTTONUP = 0x00A2,

		/// <summary>
		/// The NCLBUTTONDBLCLK value.
		/// </summary>
		NCLBUTTONDBLCLK = 0x00A3,

		/// <summary>
		/// The NCRBUTTONDOWN value.
		/// </summary>
		NCRBUTTONDOWN = 0x00A4,

		/// <summary>
		/// The NCRBUTTONUP value.
		/// </summary>
		NCRBUTTONUP = 0x00A5,

		/// <summary>
		/// The NCRBUTTONDBLCLK value.
		/// </summary>
		NCRBUTTONDBLCLK = 0x00A6,

		/// <summary>
		/// The NCMBUTTONDOWN value.
		/// </summary>
		NCMBUTTONDOWN = 0x00A7,

		/// <summary>
		/// The NCMBUTTONUP value.
		/// </summary>
		NCMBUTTONUP = 0x00A8,

		/// <summary>
		/// The NCMBUTTONDBLCLK value.
		/// </summary>
		NCMBUTTONDBLCLK = 0x00A9,

		/// <summary>
		/// The SYSKEYDOWN value.
		/// </summary>
		SYSKEYDOWN = 0x0104,

		/// <summary>
		/// The SYSKEYUP value.
		/// </summary>
		SYSKEYUP = 0x0105,

		/// <summary>
		/// The SYSCHAR value.
		/// </summary>
		SYSCHAR = 0x0106,

		/// <summary>
		/// The SYSDEADCHAR value.
		/// </summary>
		SYSDEADCHAR = 0x0107,

		/// <summary>
		/// The COMMAND value.
		/// </summary>
		COMMAND = 0x0111,

		/// <summary>
		/// The SYSCOMMAND value.
		/// </summary>
		SYSCOMMAND = 0x0112,

		/// <summary>
		/// The MOUSEMOVE value.
		/// </summary>
		MOUSEMOVE = 0x0200,

		/// <summary>
		/// The LBUTTONDOWN value.
		/// </summary>
		LBUTTONDOWN = 0x0201,

		/// <summary>
		/// The LBUTTONUP value.
		/// </summary>
		LBUTTONUP = 0x0202,

		/// <summary>
		/// The LBUTTONDBLCLK value.
		/// </summary>
		LBUTTONDBLCLK = 0x0203,

		/// <summary>
		/// The RBUTTONDOWN value.
		/// </summary>
		RBUTTONDOWN = 0x0204,

		/// <summary>
		/// The RBUTTONUP value.
		/// </summary>
		RBUTTONUP = 0x0205,

		/// <summary>
		/// The RBUTTONDBLCLK value.
		/// </summary>
		RBUTTONDBLCLK = 0x0206,

		/// <summary>
		/// The MBUTTONDOWN value.
		/// </summary>
		MBUTTONDOWN = 0x0207,

		/// <summary>
		/// The MBUTTONUP value.
		/// </summary>
		MBUTTONUP = 0x0208,

		/// <summary>
		/// The MBUTTONDBLCLK value.
		/// </summary>
		MBUTTONDBLCLK = 0x0209,

		/// <summary>
		/// The MOUSEWHEEL value.
		/// </summary>
		MOUSEWHEEL = 0x020A,

		/// <summary>
		/// The XBUTTONDOWN value.
		/// </summary>
		XBUTTONDOWN = 0x020B,

		/// <summary>
		/// The XBUTTONUP value.
		/// </summary>
		XBUTTONUP = 0x020C,

		/// <summary>
		/// The XBUTTONDBLCLK value.
		/// </summary>
		XBUTTONDBLCLK = 0x020D,

		/// <summary>
		/// The MOUSEHWHEEL value.
		/// </summary>
		MOUSEHWHEEL = 0x020E,

		/// <summary>
		/// The PARENTNOTIFY value.
		/// </summary>
		PARENTNOTIFY = 0x0210,

		/// <summary>
		/// The CAPTURECHANGED value.
		/// </summary>
		CAPTURECHANGED = 0x0215,

		/// <summary>
		/// The POWERBROADCAST value.
		/// </summary>
		POWERBROADCAST = 0x0218,

		/// <summary>
		/// The DEVICECHANGE value.
		/// </summary>
		DEVICECHANGE = 0x0219,

		/// <summary>
		/// The ENTERSIZEMOVE value.
		/// </summary>
		ENTERSIZEMOVE = 0x0231,

		/// <summary>
		/// The EXITSIZEMOVE value.
		/// </summary>
		EXITSIZEMOVE = 0x0232,

		/// <summary>
		/// The IME_SETCONTEXT value.
		/// </summary>
		IME_SETCONTEXT = 0x0281,

		/// <summary>
		/// The IME_NOTIFY value.
		/// </summary>
		IME_NOTIFY = 0x0282,

		/// <summary>
		/// The IME_CONTROL value.
		/// </summary>
		IME_CONTROL = 0x0283,

		/// <summary>
		/// The IME_COMPOSITIONFULL value.
		/// </summary>
		IME_COMPOSITIONFULL = 0x0284,

		/// <summary>
		/// The IME_SELECT value.
		/// </summary>
		IME_SELECT = 0x0285,

		/// <summary>
		/// The IME_CHAR value.
		/// </summary>
		IME_CHAR = 0x0286,

		/// <summary>
		/// The IME_REQUEST value.
		/// </summary>
		IME_REQUEST = 0x0288,

		/// <summary>
		/// The IME_KEYDOWN value.
		/// </summary>
		IME_KEYDOWN = 0x0290,

		/// <summary>
		/// The IME_KEYUP value.
		/// </summary>
		IME_KEYUP = 0x0291,

		/// <summary>
		/// The NCMOUSELEAVE value.
		/// </summary>
		NCMOUSELEAVE = 0x02A2,

		/// <summary>
		/// The TABLET_DEFBASE value.
		/// </summary>
		TABLET_DEFBASE = 0x02C0,
		// WM_TABLET_MAXOFFSET = 0x20,

		/// <summary>
		/// The TABLET_ADDED value.
		/// </summary>
		TABLET_ADDED = TABLET_DEFBASE + 8,

		/// <summary>
		/// The TABLET_DELETED value.
		/// </summary>
		TABLET_DELETED = TABLET_DEFBASE + 9,

		/// <summary>
		/// The TABLET_FLICK value.
		/// </summary>
		TABLET_FLICK = TABLET_DEFBASE + 11,

		/// <summary>
		/// The TABLET_QUERYSYSTEMGESTURESTATUS value.
		/// </summary>
		TABLET_QUERYSYSTEMGESTURESTATUS = TABLET_DEFBASE + 12,

		/// <summary>
		/// The CUT value.
		/// </summary>
		CUT = 0x0300,

		/// <summary>
		/// The COPY value.
		/// </summary>
		COPY = 0x0301,

		/// <summary>
		/// The PASTE value.
		/// </summary>
		PASTE = 0x0302,

		/// <summary>
		/// The CLEAR value.
		/// </summary>
		CLEAR = 0x0303,

		/// <summary>
		/// The UNDO value.
		/// </summary>
		UNDO = 0x0304,

		/// <summary>
		/// The RENDERFORMAT value.
		/// </summary>
		RENDERFORMAT = 0x0305,

		/// <summary>
		/// The RENDERALLFORMATS value.
		/// </summary>
		RENDERALLFORMATS = 0x0306,

		/// <summary>
		/// The DESTROYCLIPBOARD value.
		/// </summary>
		DESTROYCLIPBOARD = 0x0307,

		/// <summary>
		/// The DRAWCLIPBOARD value.
		/// </summary>
		DRAWCLIPBOARD = 0x0308,

		/// <summary>
		/// The PAINTCLIPBOARD value.
		/// </summary>
		PAINTCLIPBOARD = 0x0309,

		/// <summary>
		/// The VSCROLLCLIPBOARD value.
		/// </summary>
		VSCROLLCLIPBOARD = 0x030A,

		/// <summary>
		/// The SIZECLIPBOARD value.
		/// </summary>
		SIZECLIPBOARD = 0x030B,

		/// <summary>
		/// The ASKCBFORMATNAME value.
		/// </summary>
		ASKCBFORMATNAME = 0x030C,

		/// <summary>
		/// The CHANGECBCHAIN value.
		/// </summary>
		CHANGECBCHAIN = 0x030D,

		/// <summary>
		/// The HSCROLLCLIPBOARD value.
		/// </summary>
		HSCROLLCLIPBOARD = 0x030E,

		/// <summary>
		/// The QUERYNEWPALETTE value.
		/// </summary>
		QUERYNEWPALETTE = 0x030F,

		/// <summary>
		/// The PALETTEISCHANGING value.
		/// </summary>
		PALETTEISCHANGING = 0x0310,

		/// <summary>
		/// The PALETTECHANGED value.
		/// </summary>
		PALETTECHANGED = 0x0311,

		/// <summary>
		/// The HOTKEY value.
		/// </summary>
		HOTKEY = 0x0312,

		/// <summary>
		/// The PRINT value.
		/// </summary>
		PRINT = 0x0317,

		/// <summary>
		/// The PRINTCLIENT value.
		/// </summary>
		PRINTCLIENT = 0x0318,

		/// <summary>
		/// The APPCOMMAND value.
		/// </summary>
		APPCOMMAND = 0x0319,

		/// <summary>
		/// The THEMECHANGED value.
		/// </summary>
		THEMECHANGED = 0x031A,

		/// <summary>
		/// The DWMCOMPOSITIONCHANGED value.
		/// </summary>
		DWMCOMPOSITIONCHANGED = 0x031E,

		/// <summary>
		/// The DWMNCRENDERINGCHANGED value.
		/// </summary>
		DWMNCRENDERINGCHANGED = 0x031F,

		/// <summary>
		/// The DWMCOLORIZATIONCOLORCHANGED value.
		/// </summary>
		DWMCOLORIZATIONCOLORCHANGED = 0x0320,

		/// <summary>
		/// The DWMWINDOWMAXIMIZEDCHANGE value.
		/// </summary>
		DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

		/// <summary>
		/// The GETTITLEBARINFOEX value.
		/// </summary>
		GETTITLEBARINFOEX = 0x033F,

		/// <summary>
		/// The DWMSENDICONICTHUMBNAIL value.
		/// </summary>
		DWMSENDICONICTHUMBNAIL = 0x0323,

		/// <summary>
		/// The DWMSENDICONICLIVEPREVIEWBITMAP value.
		/// </summary>
		DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,

		/// <summary>
		/// The USER value.
		/// </summary>
		USER = 0x0400,

		// This is the hard-coded message value used by WinForms for Shell_NotifyIcon.
		// It's relatively safe to reuse.

		/// <summary>
		/// The TRAYMOUSEMESSAGE value.
		/// </summary>
		TRAYMOUSEMESSAGE = 0x800, // WM_USER + 1024

		/// <summary>
		/// The APP value.
		/// </summary>
		APP = 0x8000,
	}

	/// <summary>
	/// Window style extended values, WS_EX_*
	/// </summary>
	[Flags]
	internal enum WS_EX : uint
	{
		/// <summary>
		/// The None value.
		/// </summary>
		None = 0,

		/// <summary>
		/// The DLGMODALFRAME value.
		/// </summary>
		DLGMODALFRAME = 0x00000001,

		/// <summary>
		/// The NOPARENTNOTIFY value.
		/// </summary>
		NOPARENTNOTIFY = 0x00000004,

		/// <summary>
		/// The TOPMOST value.
		/// </summary>
		TOPMOST = 0x00000008,

		/// <summary>
		/// The ACCEPTFILES value.
		/// </summary>
		ACCEPTFILES = 0x00000010,

		/// <summary>
		/// The TRANSPARENT value.
		/// </summary>
		TRANSPARENT = 0x00000020,

		/// <summary>
		/// The MDICHILD value.
		/// </summary>
		MDICHILD = 0x00000040,

		/// <summary>
		/// The TOOLWINDOW value.
		/// </summary>
		TOOLWINDOW = 0x00000080,

		/// <summary>
		/// The WINDOWEDGE value.
		/// </summary>
		WINDOWEDGE = 0x00000100,

		/// <summary>
		/// The CLIENTEDGE value.
		/// </summary>
		CLIENTEDGE = 0x00000200,

		/// <summary>
		/// The CONTEXTHELP value.
		/// </summary>
		CONTEXTHELP = 0x00000400,

		/// <summary>
		/// The RIGHT value.
		/// </summary>
		RIGHT = 0x00001000,

		/// <summary>
		/// The LEFT value.
		/// </summary>
		LEFT = 0x00000000,

		/// <summary>
		/// The RTLREADING value.
		/// </summary>
		RTLREADING = 0x00002000,

		/// <summary>
		/// The LTRREADING value.
		/// </summary>
		LTRREADING = 0x00000000,

		/// <summary>
		/// The LEFTSCROLLBAR value.
		/// </summary>
		LEFTSCROLLBAR = 0x00004000,

		/// <summary>
		/// The RIGHTSCROLLBAR value.
		/// </summary>
		RIGHTSCROLLBAR = 0x00000000,

		/// <summary>
		/// The CONTROLPARENT value.
		/// </summary>
		CONTROLPARENT = 0x00010000,

		/// <summary>
		/// The STATICEDGE value.
		/// </summary>
		STATICEDGE = 0x00020000,

		/// <summary>
		/// The APPWINDOW value.
		/// </summary>
		APPWINDOW = 0x00040000,

		/// <summary>
		/// The LAYERED value.
		/// </summary>
		LAYERED = 0x00080000,

		/// <summary>
		/// The NOINHERITLAYOUT value.
		/// </summary>
		NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children

		/// <summary>
		/// The LAYOUTRTL value.
		/// </summary>
		LAYOUTRTL = 0x00400000, // Right to left mirroring

		/// <summary>
		/// The COMPOSITED value.
		/// </summary>
		COMPOSITED = 0x02000000,

		/// <summary>
		/// The NOACTIVATE value.
		/// </summary>
		NOACTIVATE = 0x08000000,

		/// <summary>
		/// The OVERLAPPEDWINDOW value.
		/// </summary>
		OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,

		/// <summary>
		/// The PALETTEWINDOW value.
		/// </summary>
		PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,
	}

	/// <summary>
	/// GetDeviceCaps nIndex values.
	/// </summary>
	internal enum DeviceCap
	{
		/// <summary>Number of bits per pixel
		/// </summary>
		BITSPIXEL = 12,

		/// <summary>
		/// Number of planes
		/// </summary>
		PLANES = 14,

		/// <summary>
		/// Logical pixels inch in X
		/// </summary>
		LOGPIXELSX = 88,

		/// <summary>
		/// Logical pixels inch in Y
		/// </summary>
		LOGPIXELSY = 90,
	}

	/// <summary>
	/// Native FO enumeration.
	/// </summary>
	internal enum FO : int
	{
		/// <summary>
		/// The MOVE value.
		/// </summary>
		MOVE = 0x0001,

		/// <summary>
		/// The COPY value.
		/// </summary>
		COPY = 0x0002,

		/// <summary>
		/// The DELETE value.
		/// </summary>
		DELETE = 0x0003,

		/// <summary>
		/// The RENAME value.
		/// </summary>
		RENAME = 0x0004,
	}

	/// <summary>
	/// "FILEOP_FLAGS", FOF_*.
	/// </summary>
	internal enum FOF : ushort
	{
		/// <summary>
		/// The MULTIDESTFILES value.
		/// </summary>
		MULTIDESTFILES = 0x0001,

		/// <summary>
		/// The CONFIRMMOUSE value.
		/// </summary>
		CONFIRMMOUSE = 0x0002,

		/// <summary>
		/// The SILENT value.
		/// </summary>
		SILENT = 0x0004,

		/// <summary>
		/// The RENAMEONCOLLISION value.
		/// </summary>
		RENAMEONCOLLISION = 0x0008,

		/// <summary>
		/// The NOCONFIRMATION value.
		/// </summary>
		NOCONFIRMATION = 0x0010,

		/// <summary>
		/// The WANTMAPPINGHANDLE value.
		/// </summary>
		WANTMAPPINGHANDLE = 0x0020,

		/// <summary>
		/// The ALLOWUNDO value.
		/// </summary>
		ALLOWUNDO = 0x0040,

		/// <summary>
		/// The FILESONLY value.
		/// </summary>
		FILESONLY = 0x0080,

		/// <summary>
		/// The SIMPLEPROGRESS value.
		/// </summary>
		SIMPLEPROGRESS = 0x0100,

		/// <summary>
		/// The NOCONFIRMMKDIR value.
		/// </summary>
		NOCONFIRMMKDIR = 0x0200,

		/// <summary>
		/// The NOERRORUI value.
		/// </summary>
		NOERRORUI = 0x0400,

		/// <summary>
		/// The NOCOPYSECURITYATTRIBS value.
		/// </summary>
		NOCOPYSECURITYATTRIBS = 0x0800,

		/// <summary>
		/// The NORECURSION value.
		/// </summary>
		NORECURSION = 0x1000,

		/// <summary>
		/// The NO_CONNECTED_ELEMENTS value.
		/// </summary>
		NO_CONNECTED_ELEMENTS = 0x2000,

		/// <summary>
		/// The WANTNUKEWARNING value.
		/// </summary>
		WANTNUKEWARNING = 0x4000,

		/// <summary>
		/// The NORECURSEREPARSE value.
		/// </summary>
		NORECURSEREPARSE = 0x8000,
	}

	/// <summary>
	/// EnableMenuItem uEnable values, MF_*
	/// </summary>
	[Flags]
	internal enum MF : uint
	{
		/// <summary>
		/// Possible return value for EnableMenuItem
		/// </summary>
		DOES_NOT_EXIST = unchecked((uint)-1),

		/// <summary>
		/// The ENABLED value.
		/// </summary>
		ENABLED = 0,

		/// <summary>
		/// The BYCOMMAND value.
		/// </summary>
		BYCOMMAND = 0,

		/// <summary>
		/// The GRAYED value.
		/// </summary>
		GRAYED = 1,

		/// <summary>
		/// The DISABLED value.
		/// </summary>
		DISABLED = 2,
	}

	/// <summary>Specifies the type of visual style attribute to set on a window.</summary>
	internal enum WINDOWTHEMEATTRIBUTETYPE : uint
	{
		/// <summary>Non-client area window attributes will be set.</summary>
		WTA_NONCLIENT = 1,
	}

	/// <summary>
	/// DWMFLIP3DWINDOWPOLICY.  DWMFLIP3D_*
	/// </summary>
	internal enum DWMFLIP3D
	{
		/// <summary>
		/// The DEFAULT value.
		/// </summary>
		DEFAULT,

		/// <summary>
		/// The EXCLUDEBELOW value.
		/// </summary>
		EXCLUDEBELOW,

		/// <summary>
		/// The EXCLUDEABOVE value.
		/// </summary>
		EXCLUDEABOVE,
		// LAST
	}

	/// <summary>
	/// DWMNCRENDERINGPOLICY. DWMNCRP_*
	/// </summary>
	internal enum DWMNCRP
	{
		/// <summary>
		/// The USEWINDOWSTYLE value.
		/// </summary>
		USEWINDOWSTYLE,

		/// <summary>
		/// The DISABLED value.
		/// </summary>
		DISABLED,

		/// <summary>
		/// The ENABLED value.
		/// </summary>
		ENABLED,
		// LAST
	}

	/// <summary>
	/// DWMWINDOWATTRIBUTE.  DWMWA_*
	/// </summary>
	internal enum DWMWA
	{
		/// <summary>
		/// The NCRENDERING_ENABLED value.
		/// </summary>
		NCRENDERING_ENABLED = 1,

		/// <summary>
		/// The NCRENDERING_POLICY value.
		/// </summary>
		NCRENDERING_POLICY,

		/// <summary>
		/// The TRANSITIONS_FORCEDISABLED value.
		/// </summary>
		TRANSITIONS_FORCEDISABLED,

		/// <summary>
		/// The ALLOW_NCPAINT value.
		/// </summary>
		ALLOW_NCPAINT,

		/// <summary>
		/// The CAPTION_BUTTON_BOUNDS value.
		/// </summary>
		CAPTION_BUTTON_BOUNDS,

		/// <summary>
		/// The NONCLIENT_RTL_LAYOUT value.
		/// </summary>
		NONCLIENT_RTL_LAYOUT,

		/// <summary>
		/// The FORCE_ICONIC_REPRESENTATION value.
		/// </summary>
		FORCE_ICONIC_REPRESENTATION,

		/// <summary>
		/// The FLIP3D_POLICY value.
		/// </summary>
		FLIP3D_POLICY,

		/// <summary>
		/// The EXTENDED_FRAME_BOUNDS value.
		/// </summary>
		EXTENDED_FRAME_BOUNDS,

		// New to Windows 7:

		/// <summary>
		/// The HAS_ICONIC_BITMAP value.
		/// </summary>
		HAS_ICONIC_BITMAP,

		/// <summary>
		/// The DISALLOW_PEEK value.
		/// </summary>
		DISALLOW_PEEK,

		/// <summary>
		/// The EXCLUDED_FROM_PEEK value.
		/// </summary>
		EXCLUDED_FROM_PEEK,

		// LAST
	}

	/// <summary>
	/// WindowThemeNonClientAttributes
	/// </summary>
	[Flags]
	internal enum WTNCA : uint
	{
		/// <summary>Prevents the window caption from being drawn.</summary>
		NODRAWCAPTION = 0x00000001,

		/// <summary>Prevents the system icon from being drawn.</summary>
		NODRAWICON = 0x00000002,

		/// <summary>Prevents the system icon menu from appearing.</summary>
		NOSYSMENU = 0x00000004,

		/// <summary>Prevents mirroring of the question mark, even in right-to-left (RTL) layout.</summary>
		NOMIRRORHELP = 0x00000008,

		/// <summary> A mask that contains all the valid bits.</summary>
		VALIDBITS = NODRAWCAPTION | NODRAWICON | NOMIRRORHELP | NOSYSMENU,
	}

	/// <summary>SetWindowPos options</summary>
	[Flags]
	internal enum SWP
	{
		/// <summary>
		/// The ASYNCWINDOWPOS value.
		/// </summary>
		ASYNCWINDOWPOS = 0x4000,

		/// <summary>
		/// The DEFERERASE value.
		/// </summary>
		DEFERERASE = 0x2000,

		/// <summary>
		/// The DRAWFRAME value.
		/// </summary>
		DRAWFRAME = 0x0020,

		/// <summary>
		/// The FRAMECHANGED value.
		/// </summary>
		FRAMECHANGED = 0x0020,

		/// <summary>
		/// The HIDEWINDOW value.
		/// </summary>
		HIDEWINDOW = 0x0080,

		/// <summary>
		/// The NOACTIVATE value.
		/// </summary>
		NOACTIVATE = 0x0010,

		/// <summary>
		/// The NOCOPYBITS value.
		/// </summary>
		NOCOPYBITS = 0x0100,

		/// <summary>
		/// The NOMOVE value.
		/// </summary>
		NOMOVE = 0x0002,

		/// <summary>
		/// The NOOWNERZORDER value.
		/// </summary>
		NOOWNERZORDER = 0x0200,

		/// <summary>
		/// The NOREDRAW value.
		/// </summary>
		NOREDRAW = 0x0008,

		/// <summary>
		/// The NOREPOSITION value.
		/// </summary>
		NOREPOSITION = 0x0200,

		/// <summary>
		/// The NOSENDCHANGING value.
		/// </summary>
		NOSENDCHANGING = 0x0400,

		/// <summary>
		/// The NOSIZE value.
		/// </summary>
		NOSIZE = 0x0001,

		/// <summary>
		/// The NOZORDER value.
		/// </summary>
		NOZORDER = 0x0004,

		/// <summary>
		/// The SHOWWINDOW value.
		/// </summary>
		SHOWWINDOW = 0x0040,
	}

	/// <summary>
	/// ShowWindow options
	/// </summary>
	internal enum SW
	{
		/// <summary>
		/// The HIDE value.
		/// </summary>
		HIDE = 0,

		/// <summary>
		/// The SHOWNORMAL value.
		/// </summary>
		SHOWNORMAL = 1,

		/// <summary>
		/// The NORMAL value.
		/// </summary>
		NORMAL = 1,

		/// <summary>
		/// The SHOWMINIMIZED value.
		/// </summary>
		SHOWMINIMIZED = 2,

		/// <summary>
		/// The SHOWMAXIMIZED value.
		/// </summary>
		SHOWMAXIMIZED = 3,

		/// <summary>
		/// The MAXIMIZE value.
		/// </summary>
		MAXIMIZE = 3,

		/// <summary>
		/// The SHOWNOACTIVATE value.
		/// </summary>
		SHOWNOACTIVATE = 4,

		/// <summary>
		/// The SHOW value.
		/// </summary>
		SHOW = 5,

		/// <summary>
		/// The MINIMIZE value.
		/// </summary>
		MINIMIZE = 6,

		/// <summary>
		/// The SHOWMINNOACTIVE value.
		/// </summary>
		SHOWMINNOACTIVE = 7,

		/// <summary>
		/// The SHOWNA value.
		/// </summary>
		SHOWNA = 8,

		/// <summary>
		/// The RESTORE value.
		/// </summary>
		RESTORE = 9,

		/// <summary>
		/// The SHOWDEFAULT value.
		/// </summary>
		SHOWDEFAULT = 10,

		/// <summary>
		/// The FORCEMINIMIZE value.
		/// </summary>
		FORCEMINIMIZE = 11,
	}

	/// <summary>
	/// Native SC enumeration.
	/// </summary>
	internal enum SC
	{
		/// <summary>
		/// The SIZE value.
		/// </summary>
		SIZE = 0xF000,

		/// <summary>
		/// The MOVE value.
		/// </summary>
		MOVE = 0xF010,

		/// <summary>
		/// The MINIMIZE value.
		/// </summary>
		MINIMIZE = 0xF020,

		/// <summary>
		/// The MAXIMIZE value.
		/// </summary>
		MAXIMIZE = 0xF030,

		/// <summary>
		/// The NEXTWINDOW value.
		/// </summary>
		NEXTWINDOW = 0xF040,

		/// <summary>
		/// The PREVWINDOW value.
		/// </summary>
		PREVWINDOW = 0xF050,

		/// <summary>
		/// The CLOSE value.
		/// </summary>
		CLOSE = 0xF060,

		/// <summary>
		/// The VSCROLL value.
		/// </summary>
		VSCROLL = 0xF070,

		/// <summary>
		/// The HSCROLL value.
		/// </summary>
		HSCROLL = 0xF080,

		/// <summary>
		/// The MOUSEMENU value.
		/// </summary>
		MOUSEMENU = 0xF090,

		/// <summary>
		/// The KEYMENU value.
		/// </summary>
		KEYMENU = 0xF100,

		/// <summary>
		/// The ARRANGE value.
		/// </summary>
		ARRANGE = 0xF110,

		/// <summary>
		/// The RESTORE value.
		/// </summary>
		RESTORE = 0xF120,

		/// <summary>
		/// The TASKLIST value.
		/// </summary>
		TASKLIST = 0xF130,

		/// <summary>
		/// The SCREENSAVE value.
		/// </summary>
		SCREENSAVE = 0xF140,

		/// <summary>
		/// The HOTKEY value.
		/// </summary>
		HOTKEY = 0xF150,

		/// <summary>
		/// The DEFAULT value.
		/// </summary>
		DEFAULT = 0xF160,

		/// <summary>
		/// The MONITORPOWER value.
		/// </summary>
		MONITORPOWER = 0xF170,

		/// <summary>
		/// The CONTEXTHELP value.
		/// </summary>
		CONTEXTHELP = 0xF180,

		/// <summary>
		/// The SEPARATOR value.
		/// </summary>
		SEPARATOR = 0xF00F,

		/// <summary>SCF_ISSECURE</summary>
		F_ISSECURE = 0x00000001,

		/// <summary>
		/// The ICON value.
		/// </summary>
		ICON = MINIMIZE,

		/// <summary>
		/// The ZOOM value.
		/// </summary>
		ZOOM = MAXIMIZE,
	}

	/// <summary>GDI+ Status codes</summary>
	internal enum Status
	{
		/// <summary>
		/// The Ok value.
		/// </summary>
		Ok = 0,

		/// <summary>
		/// The GenericError value.
		/// </summary>
		GenericError = 1,

		/// <summary>
		/// The InvalidParameter value.
		/// </summary>
		InvalidParameter = 2,

		/// <summary>
		/// The OutOfMemory value.
		/// </summary>
		OutOfMemory = 3,

		/// <summary>
		/// The ObjectBusy value.
		/// </summary>
		ObjectBusy = 4,

		/// <summary>
		/// The InsufficientBuffer value.
		/// </summary>
		InsufficientBuffer = 5,

		/// <summary>
		/// The NotImplemented value.
		/// </summary>
		NotImplemented = 6,

		/// <summary>
		/// The Win32Error value.
		/// </summary>
		Win32Error = 7,

		/// <summary>
		/// The WrongState value.
		/// </summary>
		WrongState = 8,

		/// <summary>
		/// The Aborted value.
		/// </summary>
		Aborted = 9,

		/// <summary>
		/// The FileNotFound value.
		/// </summary>
		FileNotFound = 10,

		/// <summary>
		/// The ValueOverflow value.
		/// </summary>
		ValueOverflow = 11,

		/// <summary>
		/// The AccessDenied value.
		/// </summary>
		AccessDenied = 12,

		/// <summary>
		/// The UnknownImageFormat value.
		/// </summary>
		UnknownImageFormat = 13,

		/// <summary>
		/// The FontFamilyNotFound value.
		/// </summary>
		FontFamilyNotFound = 14,

		/// <summary>
		/// The FontStyleNotFound value.
		/// </summary>
		FontStyleNotFound = 15,

		/// <summary>
		/// The NotTrueTypeFont value.
		/// </summary>
		NotTrueTypeFont = 16,

		/// <summary>
		/// The UnsupportedGdiplusVersion value.
		/// </summary>
		UnsupportedGdiplusVersion = 17,

		/// <summary>
		/// The GdiplusNotInitialized value.
		/// </summary>
		GdiplusNotInitialized = 18,

		/// <summary>
		/// The PropertyNotFound value.
		/// </summary>
		PropertyNotFound = 19,

		/// <summary>
		/// The PropertyNotSupported value.
		/// </summary>
		PropertyNotSupported = 20,

		/// <summary>
		/// The ProfileNotFound value.
		/// </summary>
		ProfileNotFound = 21,
	}

	/// <summary>
	/// Native MOUSEEVENTF enumeration.
	/// </summary>
	internal enum MOUSEEVENTF : int
	{
		// mouse event constants

		/// <summary>
		/// The LEFTDOWN value.
		/// </summary>
		LEFTDOWN = 2,

		/// <summary>
		/// The LEFTUP value.
		/// </summary>
		LEFTUP = 4
	}

	/// <summary>
	/// MSGFLT_*.  New in Vista.  Realiased in Windows 7.
	/// </summary>
	internal enum MSGFLT
	{
		// Win7 versions of this enum:

		/// <summary>
		/// The RESET value.
		/// </summary>
		RESET = 0,

		/// <summary>
		/// The ALLOW value.
		/// </summary>
		ALLOW = 1,

		/// <summary>
		/// The DISALLOW value.
		/// </summary>
		DISALLOW = 2,

		// Vista versions of this enum:
		// ADD = 1,
		// REMOVE = 2,
	}

	/// <summary>
	/// Native MSGFLTINFO enumeration.
	/// </summary>
	internal enum MSGFLTINFO
	{
		/// <summary>
		/// The NONE value.
		/// </summary>
		NONE = 0,

		/// <summary>
		/// The ALREADYALLOWED_FORWND value.
		/// </summary>
		ALREADYALLOWED_FORWND = 1,

		/// <summary>
		/// The ALREADYDISALLOWED_FORWND value.
		/// </summary>
		ALREADYDISALLOWED_FORWND = 2,

		/// <summary>
		/// The ALLOWED_HIGHER value.
		/// </summary>
		ALLOWED_HIGHER = 3,
	}

	/// <summary>
	/// Native INPUT_TYPE enumeration.
	/// </summary>
	internal enum INPUT_TYPE : uint
	{
		/// <summary>
		/// The MOUSE value.
		/// </summary>
		MOUSE = 0,
	}

	/// <summary>Shell_NotifyIcon messages.  NIM_*</summary>
	internal enum NIM : uint
	{
		/// <summary>
		/// The ADD value.
		/// </summary>
		ADD = 0,

		/// <summary>
		/// The MODIFY value.
		/// </summary>
		MODIFY = 1,

		/// <summary>
		/// The DELETE value.
		/// </summary>
		DELETE = 2,

		/// <summary>
		/// The SETFOCUS value.
		/// </summary>
		SETFOCUS = 3,

		/// <summary>
		/// The SETVERSION value.
		/// </summary>
		SETVERSION = 4,
	}

	/// <summary>SHAddToRecentDocuments flags.  SHARD_*</summary>
	internal enum SHARD
	{
		/// <summary>
		/// The PIDL value.
		/// </summary>
		PIDL = 0x00000001,

		/// <summary>
		/// The PATHA value.
		/// </summary>
		PATHA = 0x00000002,

		/// <summary>
		/// The PATHW value.
		/// </summary>
		PATHW = 0x00000003,

		/// <summary>
		/// The APPIDINFO value.
		/// </summary>
		APPIDINFO = 0x00000004, // indicates the data type is a pointer to a SHARDAPPIDINFO structure

		/// <summary>
		/// The APPIDINFOIDLIST value.
		/// </summary>
		APPIDINFOIDLIST = 0x00000005, // indicates the data type is a pointer to a SHARDAPPIDINFOIDLIST structure

		/// <summary>
		/// The LINK value.
		/// </summary>
		LINK = 0x00000006, // indicates the data type is a pointer to an IShellLink instance

		/// <summary>
		/// The APPIDINFOLINK value.
		/// </summary>
		APPIDINFOLINK = 0x00000007, // indicates the data type is a pointer to a SHARDAPPIDINFOLINK structure
	}

	/// <summary>
	/// Native SLGP enumeration.
	/// </summary>
	[Flags]
	internal enum SLGP
	{
		/// <summary>
		/// The SHORTPATH value.
		/// </summary>
		SHORTPATH = 0x1,

		/// <summary>
		/// The UNCPRIORITY value.
		/// </summary>
		UNCPRIORITY = 0x2,

		/// <summary>
		/// The RAWPATH value.
		/// </summary>
		RAWPATH = 0x4
	}

	/// <summary>Shell_NotifyIcon flags.  NIF_*</summary>
	[Flags]
	internal enum NIF : uint
	{
		/// <summary>
		/// The MESSAGE value.
		/// </summary>
		MESSAGE = 0x0001,

		/// <summary>
		/// The ICON value.
		/// </summary>
		ICON = 0x0002,

		/// <summary>
		/// The TIP value.
		/// </summary>
		TIP = 0x0004,

		/// <summary>
		/// The STATE value.
		/// </summary>
		STATE = 0x0008,

		/// <summary>
		/// The INFO value.
		/// </summary>
		INFO = 0x0010,

		/// <summary>
		/// The GUID value.
		/// </summary>
		GUID = 0x0020,

		/// <summary>
		/// Vista only.
		/// </summary>
		REALTIME = 0x0040,

		/// <summary>
		/// Vista only.
		/// </summary>
		SHOWTIP = 0x0080,

		/// <summary>
		/// The XP_MASK value.
		/// </summary>
		XP_MASK = MESSAGE | ICON | STATE | INFO | GUID,

		/// <summary>
		/// The VISTA_MASK value.
		/// </summary>
		VISTA_MASK = XP_MASK | REALTIME | SHOWTIP,
	}

	/// <summary>
	/// Shell_NotifyIcon info flags.  NIIF_*
	/// </summary>
	internal enum NIIF
	{
		/// <summary>
		/// The NONE value.
		/// </summary>
		NONE = 0x00000000,

		/// <summary>
		/// The INFO value.
		/// </summary>
		INFO = 0x00000001,

		/// <summary>
		/// The WARNING value.
		/// </summary>
		WARNING = 0x00000002,

		/// <summary>
		/// The ERROR value.
		/// </summary>
		ERROR = 0x00000003,

		/// <summary>XP SP2 and later.</summary>
		USER = 0x00000004,

		/// <summary>XP and later.</summary>
		NOSOUND = 0x00000010,

		/// <summary>Vista and later.</summary>
		LARGE_ICON = 0x00000020,

		/// <summary>Windows 7 and later</summary>
		NIIF_RESPECT_QUIET_TIME = 0x00000080,

		/// <summary>XP and later.  Native version called NIIF_ICON_MASK.</summary>
		XP_ICON_MASK = 0x0000000F,
	}

	/// <summary>
	/// AC_*
	/// </summary>
	internal enum AC : byte
	{
		/// <summary>
		/// The SRC_OVER value.
		/// </summary>
		SRC_OVER = 0,

		/// <summary>
		/// The SRC_ALPHA value.
		/// </summary>
		SRC_ALPHA = 1,
	}

	/// <summary>
	/// Native ULW enumeration.
	/// </summary>
	internal enum ULW
	{
		/// <summary>
		/// The ALPHA value.
		/// </summary>
		ALPHA = 2,

		/// <summary>
		/// The COLORKEY value.
		/// </summary>
		COLORKEY = 1,

		/// <summary>
		/// The OPAQUE value.
		/// </summary>
		OPAQUE = 4,
	}

	/// <summary>
	/// Native WVR enumeration.
	/// </summary>
	internal enum WVR
	{
		/// <summary>
		/// The ALIGNTOP value.
		/// </summary>
		ALIGNTOP = 0x0010,

		/// <summary>
		/// The ALIGNLEFT value.
		/// </summary>
		ALIGNLEFT = 0x0020,

		/// <summary>
		/// The ALIGNBOTTOM value.
		/// </summary>
		ALIGNBOTTOM = 0x0040,

		/// <summary>
		/// The ALIGNRIGHT value.
		/// </summary>
		ALIGNRIGHT = 0x0080,

		/// <summary>
		/// The HREDRAW value.
		/// </summary>
		HREDRAW = 0x0100,

		/// <summary>
		/// The VREDRAW value.
		/// </summary>
		VREDRAW = 0x0200,

		/// <summary>
		/// The VALIDRECTS value.
		/// </summary>
		VALIDRECTS = 0x0400,

		/// <summary>
		/// The REDRAW value.
		/// </summary>
		REDRAW = HREDRAW | VREDRAW,
	}

	/// <summary>
	/// Native SafeFindHandle class.
	/// </summary>
	internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
#if NET5_0_OR_GREATER
#pragma warning disable SYSLIB0003 // SecurityPermissionAttribute is obsolete
#endif
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
#if NET5_0_OR_GREATER
#pragma warning restore SYSLIB0003
#endif
		private SafeFindHandle() : base(true)
		{
		}

		/// <inheritdoc />
		protected override bool ReleaseHandle() => NativeMethods.FindClose(handle);
	}

	/// <summary>
	/// Native SafeDC class.
	/// </summary>
	internal sealed class SafeDC : SafeHandleZeroOrMinusOneIsInvalid
	{
		private static class NativeMethods
		{
			/// <summary>
			/// Native method wrapper for ReleaseDC.
			/// </summary>
			/// <param name="hWnd">The hWnd parameter.</param>
			/// <param name="hDC">The hDC parameter.</param>
			/// <returns>The result.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			[DllImport("user32.dll")]
			public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

			/// <summary>
			/// Native method wrapper for GetDC.
			/// </summary>
			/// <param name="hwnd">The hwnd parameter.</param>
			/// <returns>The result.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			[DllImport("user32.dll")]
			public static extern SafeDC GetDC(IntPtr hwnd);

			// Weird legacy function, documentation is unclear about how to use it...

			/// <summary>
			/// Native method wrapper for CreateDC.
			/// </summary>
			/// <param name="lpszDriver">The lpszDriver parameter.</param>
			/// <param name="lpszDevice">The lpszDevice parameter.</param>
			/// <param name="lpszOutput">The lpszOutput parameter.</param>
			/// <param name="lpInitData">The lpInitData parameter.</param>
			/// <returns>The result.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
			public static extern SafeDC CreateDC([MarshalAs(UnmanagedType.LPWStr)] string lpszDriver, [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice, IntPtr lpszOutput, IntPtr lpInitData);

			/// <summary>
			/// Native method wrapper for CreateCompatibleDC.
			/// </summary>
			/// <param name="hdc">The hdc parameter.</param>
			/// <returns>The result.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			[DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern SafeDC CreateCompatibleDC(IntPtr hdc);

			/// <summary>
			/// Native method wrapper for DeleteDC.
			/// </summary>
			/// <param name="hdc">The hdc parameter.</param>
			/// <returns>The result.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			[DllImport("gdi32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool DeleteDC(IntPtr hdc);
		}

		private IntPtr? _hwnd;
		private bool _created;

		/// <summary>
		/// Sets the Hwnd.
		/// </summary>
		public IntPtr Hwnd
		{
			set
			{
				Assert.NullableIsNull(_hwnd);
				_hwnd = value;
			}
		}

		private SafeDC() : base(true)
		{
		}

		/// <inheritdoc />
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			if (_created) return NativeMethods.DeleteDC(handle);
			if (!_hwnd.HasValue || _hwnd.Value == IntPtr.Zero) return true;
			return NativeMethods.ReleaseDC(_hwnd.Value, handle) == 1;
		}

		/// <summary>
		/// Performs the CreateDC operation.
		/// </summary>
		/// <param name="deviceName">The deviceName parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static SafeDC CreateDC(string deviceName)
		{
			SafeDC dc = null;
			try
			{
				// Should this really be on the driver parameter?
				dc = NativeMethods.CreateDC(deviceName, null, IntPtr.Zero, IntPtr.Zero);
			}
			finally
			{
				if (dc != null) dc._created = true;
			}

			if (!dc.IsInvalid) return dc;
			dc.Dispose();
			throw new SystemException("Unable to create a device context from the specified device information.");
		}

		/// <summary>
		/// Performs the CreateCompatibleDC operation.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static SafeDC CreateCompatibleDC(SafeDC hdc)
		{
			SafeDC dc = null;
			try
			{
				var hPtr = IntPtr.Zero;
				if (hdc != null) hPtr = hdc.handle;
				dc = NativeMethods.CreateCompatibleDC(hPtr);
				if (dc == null) HRESULT.ThrowLastError();
			}
			finally
			{
				if (dc != null) dc._created = true;
			}

			if (!dc.IsInvalid) return dc;
			dc.Dispose();
			throw new SystemException("Unable to create a device context from the specified device information.");
		}

		/// <summary>
		/// Performs the GetDC operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		public static SafeDC GetDC(IntPtr hwnd)
		{
			SafeDC dc = null;
			try
			{
				dc = NativeMethods.GetDC(hwnd);
			}
			finally
			{
				if (dc != null) dc.Hwnd = hwnd;
			}

			// GetDC does not set the last error...
			if (dc.IsInvalid) HRESULT.E_FAIL.ThrowIfFailed();
			return dc;
		}

		/// <summary>
		/// Performs the GetDesktop operation.
		/// </summary>
		/// <returns>The result.</returns>
		public static SafeDC GetDesktop()
		{
			return GetDC(IntPtr.Zero);
		}

		/// <summary>
		/// Performs the WrapDC operation.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static SafeDC WrapDC(IntPtr hdc)
		{
			// This won't actually get released by the class, but it allows an IntPtr to be converted for signatures.
			return new SafeDC { handle = hdc, _created = false, _hwnd = IntPtr.Zero };
		}
	}

	/// <summary>
	/// Native SafeHBITMAP class.
	/// </summary>
	internal sealed class SafeHBITMAP : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeHBITMAP() : base(true)
		{
		}

		/// <inheritdoc />
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle() => NativeMethods.DeleteObject(handle);
	}

	/// <summary>
	/// Native SafeGdiplusStartupToken class.
	/// </summary>
	internal sealed class SafeGdiplusStartupToken : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeGdiplusStartupToken() : base(true)
		{
		}

		/// <inheritdoc />
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			var s = NativeMethods.GdiplusShutdown(this.handle);
			return s == Status.Ok;
		}

		/// <summary>
		/// Performs the Startup operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static SafeGdiplusStartupToken Startup()
		{
			var safeHandle = new SafeGdiplusStartupToken();
			var s = NativeMethods.GdiplusStartup(out var unsafeHandle, new StartupInput(), out var output);
			if (s == Status.Ok)
			{
				safeHandle.handle = unsafeHandle;
				return safeHandle;
			}

			safeHandle.Dispose();
			throw new Exception("Unable to initialize GDI+");
		}
	}

	/// <summary>
	/// Native SafeConnectionPointCookie class.
	/// </summary>
	internal sealed class SafeConnectionPointCookie : SafeHandleZeroOrMinusOneIsInvalid
	{
		private IConnectionPoint _cp;
		// handle holds the cookie value.

		/// <summary>
		/// Initializes a new instance of the <see cref="SafeConnectionPointCookie"/> class.
		/// </summary>
		/// <param name="target">The target parameter.</param>
		/// <param name="sink">The sink parameter.</param>
		/// <param name="eventId">The eventId parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IConnectionPoint")]
		public SafeConnectionPointCookie(IConnectionPointContainer target, object sink, Guid eventId) : base(true)
		{
			Verify.IsNotNull(target, nameof(target));
			Verify.IsNotNull(sink, nameof(sink));
			Verify.IsNotDefault(eventId, nameof(eventId));
			handle = IntPtr.Zero;
			IConnectionPoint cp = null;
			try
			{
				target.FindConnectionPoint(ref eventId, out cp);
				cp.Advise(sink, out var dwCookie);
				if (dwCookie == 0)
					throw new InvalidOperationException("IConnectionPoint::Advise returned an invalid cookie.");
				handle = new IntPtr(dwCookie);
				_cp = cp;
				cp = null;
			}
			finally
			{
				Utility.SafeRelease(ref cp);
			}
		}

		/// <summary>
		/// Performs the Disconnect operation.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public void Disconnect()
		{
			ReleaseHandle();
		}

		/// <summary>
		/// Performs the ReleaseHandle operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			try
			{
				if (this.IsInvalid) return true;
				var dwCookie = handle.ToInt32();
				handle = IntPtr.Zero;
				Assert.IsNotNull(_cp);
				try
				{
					_cp.Unadvise(dwCookie);
				}
				finally
				{
					Utility.SafeRelease(ref _cp);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	/// <summary>
	/// Native BLENDFUNCTION structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct BLENDFUNCTION
	{
		// Must be AC_SRC_OVER

		/// <summary>
		/// The BlendOp field.
		/// </summary>
		public AC BlendOp;

		// Must be 0.

		/// <summary>
		/// The BlendFlags field.
		/// </summary>
		public byte BlendFlags;

		// Alpha transparency between 0 (transparent) - 255 (opaque)

		/// <summary>
		/// The SourceConstantAlpha field.
		/// </summary>
		public byte SourceConstantAlpha;

		// Must be AC_SRC_ALPHA

		/// <summary>
		/// The AlphaFormat field.
		/// </summary>
		public AC AlphaFormat;
	}

	/// <summary>
	/// Native HIGHCONTRAST structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct HIGHCONTRAST
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The dwFlags field.
		/// </summary>
		public HCF dwFlags;

		// [MarshalAs(UnmanagedType.LPWStr, SizeConst=80)]
		// public String lpszDefaultScheme;

		/// <summary>
		/// The lpszDefaultScheme field.
		/// </summary>
		public IntPtr lpszDefaultScheme;
	}

	/// <summary>
	/// Native RGBQUAD structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RGBQUAD
	{
		/// <summary>
		/// The rgbBlue field.
		/// </summary>
		public byte rgbBlue;

		/// <summary>
		/// The rgbGreen field.
		/// </summary>
		public byte rgbGreen;

		/// <summary>
		/// The rgbRed field.
		/// </summary>
		public byte rgbRed;

		/// <summary>
		/// The rgbReserved field.
		/// </summary>
		public byte rgbReserved;
	}

	/// <summary>
	/// Native BITMAPINFOHEADER structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct BITMAPINFOHEADER
	{
		/// <summary>
		/// The biSize field.
		/// </summary>
		public int biSize;

		/// <summary>
		/// The biWidth field.
		/// </summary>
		public int biWidth;

		/// <summary>
		/// The biHeight field.
		/// </summary>
		public int biHeight;

		/// <summary>
		/// The biPlanes field.
		/// </summary>
		public short biPlanes;

		/// <summary>
		/// The biBitCount field.
		/// </summary>
		public short biBitCount;

		/// <summary>
		/// The biCompression field.
		/// </summary>
		public BI biCompression;

		/// <summary>
		/// The biSizeImage field.
		/// </summary>
		public int biSizeImage;

		/// <summary>
		/// The biXPelsPerMeter field.
		/// </summary>
		public int biXPelsPerMeter;

		/// <summary>
		/// The biYPelsPerMeter field.
		/// </summary>
		public int biYPelsPerMeter;

		/// <summary>
		/// The biClrUsed field.
		/// </summary>
		public int biClrUsed;

		/// <summary>
		/// The biClrImportant field.
		/// </summary>
		public int biClrImportant;
	}

	/// <summary>
	/// Native BITMAPINFO structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct BITMAPINFO
	{
		/// <summary>
		/// The bmiHeader field.
		/// </summary>
		public BITMAPINFOHEADER bmiHeader;

		/// <summary>
		/// The bmiColors field.
		/// </summary>
		public RGBQUAD bmiColors;
	}

	// Win7 only.

	/// <summary>
	/// Native CHANGEFILTERSTRUCT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct CHANGEFILTERSTRUCT
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public uint cbSize;

		/// <summary>
		/// The ExtStatus field.
		/// </summary>
		public MSGFLTINFO ExtStatus;
	}

	/// <summary>
	/// Native CREATESTRUCT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CREATESTRUCT
	{
		/// <summary>
		/// The lpCreateParams field.
		/// </summary>
		public IntPtr lpCreateParams;

		/// <summary>
		/// The hInstance field.
		/// </summary>
		public IntPtr hInstance;

		/// <summary>
		/// The hMenu field.
		/// </summary>
		public IntPtr hMenu;

		/// <summary>
		/// The hwndParent field.
		/// </summary>
		public IntPtr hwndParent;

		/// <summary>
		/// The cy field.
		/// </summary>
		public int cy;

		/// <summary>
		/// The cx field.
		/// </summary>
		public int cx;

		/// <summary>
		/// The y field.
		/// </summary>
		public int y;

		/// <summary>
		/// The x field.
		/// </summary>
		public int x;

		/// <summary>
		/// The style field.
		/// </summary>
		public WS style;

		/// <summary>
		/// The lpszName field.
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszName;

		/// <summary>
		/// The lpszClass field.
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszClass;

		/// <summary>
		/// The dwExStyle field.
		/// </summary>
		public WS_EX dwExStyle;
	}

	/// <summary>
	/// Native SHFILEOPSTRUCT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	internal struct SHFILEOPSTRUCT
	{
		/// <summary>
		/// The hwnd field.
		/// </summary>
		public IntPtr hwnd;

		/// <summary>
		/// The wFunc field.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public FO wFunc;

		// double-null terminated arrays of LPWSTRS

		/// <summary>
		/// The pFrom field.
		/// </summary>
		public string pFrom;

		/// <summary>
		/// The pTo field.
		/// </summary>
		public string pTo;

		/// <summary>
		/// The fFlags field.
		/// </summary>
		[MarshalAs(UnmanagedType.U2)]
		public FOF fFlags;

		/// <summary>
		/// The fAnyOperationsAborted field.
		/// </summary>
		[MarshalAs(UnmanagedType.Bool)]
		public int fAnyOperationsAborted;

		/// <summary>
		/// The hNameMappings field.
		/// </summary>
		public IntPtr hNameMappings;

		/// <summary>
		/// The lpszProgressTitle field.
		/// </summary>
		public string lpszProgressTitle;
	}

	/// <summary>
	/// Native TITLEBARINFO structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct TITLEBARINFO
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The rcTitleBar field.
		/// </summary>
		public RECT rcTitleBar;

		/// <summary>
		/// The rgstate_TitleBar field.
		/// </summary>
		public STATE_SYSTEM rgstate_TitleBar;

		/// <summary>
		/// The rgstate_Reserved field.
		/// </summary>
		public STATE_SYSTEM rgstate_Reserved;

		/// <summary>
		/// The rgstate_MinimizeButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_MinimizeButton;

		/// <summary>
		/// The rgstate_MaximizeButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_MaximizeButton;

		/// <summary>
		/// The rgstate_HelpButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_HelpButton;

		/// <summary>
		/// The rgstate_CloseButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_CloseButton;
	}

	// New to Vista.

	/// <summary>
	/// Native TITLEBARINFOEX structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct TITLEBARINFOEX
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The rcTitleBar field.
		/// </summary>
		public RECT rcTitleBar;

		/// <summary>
		/// The rgstate_TitleBar field.
		/// </summary>
		public STATE_SYSTEM rgstate_TitleBar;

		/// <summary>
		/// The rgstate_Reserved field.
		/// </summary>
		public STATE_SYSTEM rgstate_Reserved;

		/// <summary>
		/// The rgstate_MinimizeButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_MinimizeButton;

		/// <summary>
		/// The rgstate_MaximizeButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_MaximizeButton;

		/// <summary>
		/// The rgstate_HelpButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_HelpButton;

		/// <summary>
		/// The rgstate_CloseButton field.
		/// </summary>
		public STATE_SYSTEM rgstate_CloseButton;

		/// <summary>
		/// The rgrect_TitleBar field.
		/// </summary>
		public RECT rgrect_TitleBar;

		/// <summary>
		/// The rgrect_Reserved field.
		/// </summary>
		public RECT rgrect_Reserved;

		/// <summary>
		/// The rgrect_MinimizeButton field.
		/// </summary>
		public RECT rgrect_MinimizeButton;

		/// <summary>
		/// The rgrect_MaximizeButton field.
		/// </summary>
		public RECT rgrect_MaximizeButton;

		/// <summary>
		/// The rgrect_HelpButton field.
		/// </summary>
		public RECT rgrect_HelpButton;

		/// <summary>
		/// The rgrect_CloseButton field.
		/// </summary>
		public RECT rgrect_CloseButton;
	}

	/// <summary>
	/// Native NOTIFYICONDATA class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential)]
	internal class NOTIFYICONDATA
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The hWnd field.
		/// </summary>
		public IntPtr hWnd;

		/// <summary>
		/// The uID field.
		/// </summary>
		public int uID;

		/// <summary>
		/// The uFlags field.
		/// </summary>
		public NIF uFlags;

		/// <summary>
		/// The uCallbackMessage field.
		/// </summary>
		public int uCallbackMessage;

		/// <summary>
		/// The hIcon field.
		/// </summary>
		public IntPtr hIcon;

		/// <summary>
		/// The char[128] field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public char[] szTip = new char[128];

		/// <summary>
		/// The state of the icon.  There are two flags that can be set independently.
		/// NIS_HIDDEN = 1.  The icon is hidden.
		/// NIS_SHAREDICON = 2.  The icon is shared.
		/// </summary>
		public uint dwState;

		/// <summary>
		/// The dwStateMask field.
		/// </summary>
		public uint dwStateMask;

		/// <summary>
		/// The char[256] field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] szInfo = new char[256];

		// Prior to Vista this was a union of uTimeout and uVersion.  As of Vista, uTimeout has been deprecated.

		/// <summary>
		/// The uVersion field.
		/// </summary>
		public uint uVersion;  // Used with Shell_NotifyIcon flag NIM_SETVERSION.

		/// <summary>
		/// The char[64] field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public char[] szInfoTitle = new char[64];

		/// <summary>
		/// The dwInfoFlags field.
		/// </summary>
		public uint dwInfoFlags;

		/// <summary>
		/// The guidItem field.
		/// </summary>
		public Guid guidItem;

		// Vista only
		private IntPtr hBalloonIcon;
	}

	/// <summary>
	/// Native PROPVARIANT class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Explicit)]
	internal class PROPVARIANT : IDisposable
	{
		private static class NativeMethods
		{
			/// <summary>
			/// Native method wrapper for PropVariantClear.
			/// </summary>
			/// <param name="pvar">The pvar parameter.</param>
			/// <returns>The result.</returns>
			[DllImport("ole32.dll")]
			internal static extern HRESULT PropVariantClear(PROPVARIANT pvar);
		}

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		[FieldOffset(0)]
		private ushort vt;

		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		[FieldOffset(8)]
		private IntPtr pointerVal;

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		[FieldOffset(8)]
		private byte byteVal;

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		[FieldOffset(8)]
		private long longVal;

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		[FieldOffset(8)]
		private short boolVal;

		/// <summary>
		/// Gets the VarType.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public VarEnum VarType => (VarEnum)vt;

		// Right now only using this for strings.

		/// <summary>
		/// Performs the GetValue operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public string GetValue() => vt == (ushort)VarEnum.VT_LPWSTR ? Marshal.PtrToStringUni(pointerVal) : null;

		/// <summary>
		/// Performs the SetValue operation.
		/// </summary>
		/// <param name="f">The f parameter.</param>
		public void SetValue(bool f)
		{
			Clear();
			vt = (ushort)VarEnum.VT_BOOL;
			boolVal = (short)(f ? -1 : 0);
		}

		/// <summary>
		/// Performs the SetValue operation.
		/// </summary>
		/// <param name="val">The val parameter.</param>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void SetValue(string val)
		{
			Clear();
			vt = (ushort)VarEnum.VT_LPWSTR;
			pointerVal = Marshal.StringToCoTaskMemUni(val);
		}

		/// <summary>
		/// Performs the Clear operation.
		/// </summary>
		public void Clear()
		{
			var hr = NativeMethods.PropVariantClear(this);
			Assert.IsTrue(hr.Succeeded);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="PROPVARIANT"/> class.
		/// </summary>
		~PROPVARIANT()
		{
			Dispose(false);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
		private void Dispose(bool disposing)
		{
			Clear();
		}
	}

	/// <summary>
	/// Native SHARDAPPIDINFO class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal class SHARDAPPIDINFO
	{
		[MarshalAs(UnmanagedType.Interface)]
		private object psi;    // The namespace location of the the item that should be added to the recent docs folder.

		[MarshalAs(UnmanagedType.LPWStr)]
		private string pszAppID;  // The id of the application that should be associated with this recent doc.
	}

	/// <summary>
	/// Native SHARDAPPIDINFOIDLIST class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal class SHARDAPPIDINFOIDLIST
	{
		/// <summary>The idlist for the shell item that should be added to the recent docs folder.</summary>
		private IntPtr pidl;

		/// <summary>The id of the application that should be associated with this recent doc.</summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		private string pszAppID;
	}

	/// <summary>
	/// Native SHARDAPPIDINFOLINK class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal class SHARDAPPIDINFOLINK
	{
		private IntPtr psl;     // An IShellLink instance that when launched opens a recently used item in the specified

		// application. This link is not added to the recent docs folder, but will be added to the
		// specified application's destination list.
		[MarshalAs(UnmanagedType.LPWStr)]
		private string pszAppID;  // The id of the application that should be associated with this recent doc.
	}

	/// <summary>
	/// Native LOGFONT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct LOGFONT
	{
		/// <summary>
		/// The lfHeight field.
		/// </summary>
		public int lfHeight;

		/// <summary>
		/// The lfWidth field.
		/// </summary>
		public int lfWidth;

		/// <summary>
		/// The lfEscapement field.
		/// </summary>
		public int lfEscapement;

		/// <summary>
		/// The lfOrientation field.
		/// </summary>
		public int lfOrientation;

		/// <summary>
		/// The lfWeight field.
		/// </summary>
		public int lfWeight;

		/// <summary>
		/// The lfItalic field.
		/// </summary>
		public byte lfItalic;

		/// <summary>
		/// The lfUnderline field.
		/// </summary>
		public byte lfUnderline;

		/// <summary>
		/// The lfStrikeOut field.
		/// </summary>
		public byte lfStrikeOut;

		/// <summary>
		/// The lfCharSet field.
		/// </summary>
		public byte lfCharSet;

		/// <summary>
		/// The lfOutPrecision field.
		/// </summary>
		public byte lfOutPrecision;

		/// <summary>
		/// The lfClipPrecision field.
		/// </summary>
		public byte lfClipPrecision;

		/// <summary>
		/// The lfQuality field.
		/// </summary>
		public byte lfQuality;

		/// <summary>
		/// The lfPitchAndFamily field.
		/// </summary>
		public byte lfPitchAndFamily;

		/// <summary>
		/// The lfFaceName field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName;
	}

	/// <summary>
	/// Native MINMAXINFO structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MINMAXINFO
	{
		/// <summary>
		/// The ptReserved field.
		/// </summary>
		public POINT ptReserved;

		/// <summary>
		/// The ptMaxSize field.
		/// </summary>
		public POINT ptMaxSize;

		/// <summary>
		/// The ptMaxPosition field.
		/// </summary>
		public POINT ptMaxPosition;

		/// <summary>
		/// The ptMinTrackSize field.
		/// </summary>
		public POINT ptMinTrackSize;

		/// <summary>
		/// The ptMaxTrackSize field.
		/// </summary>
		public POINT ptMaxTrackSize;
	}

	/// <summary>
	/// Native NONCLIENTMETRICS structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct NONCLIENTMETRICS
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The iBorderWidth field.
		/// </summary>
		public int iBorderWidth;

		/// <summary>
		/// The iScrollWidth field.
		/// </summary>
		public int iScrollWidth;

		/// <summary>
		/// The iScrollHeight field.
		/// </summary>
		public int iScrollHeight;

		/// <summary>
		/// The iCaptionWidth field.
		/// </summary>
		public int iCaptionWidth;

		/// <summary>
		/// The iCaptionHeight field.
		/// </summary>
		public int iCaptionHeight;

		/// <summary>
		/// The lfCaptionFont field.
		/// </summary>
		public LOGFONT lfCaptionFont;

		/// <summary>
		/// The iSmCaptionWidth field.
		/// </summary>
		public int iSmCaptionWidth;

		/// <summary>
		/// The iSmCaptionHeight field.
		/// </summary>
		public int iSmCaptionHeight;

		/// <summary>
		/// The lfSmCaptionFont field.
		/// </summary>
		public LOGFONT lfSmCaptionFont;

		/// <summary>
		/// The iMenuWidth field.
		/// </summary>
		public int iMenuWidth;

		/// <summary>
		/// The iMenuHeight field.
		/// </summary>
		public int iMenuHeight;

		/// <summary>
		/// The lfMenuFont field.
		/// </summary>
		public LOGFONT lfMenuFont;

		/// <summary>
		/// The lfStatusFont field.
		/// </summary>
		public LOGFONT lfStatusFont;

		/// <summary>
		/// The lfMessageFont field.
		/// </summary>
		public LOGFONT lfMessageFont;

		// Vista only

		/// <summary>
		/// The iPaddedBorderWidth field.
		/// </summary>
		public int iPaddedBorderWidth;

		/// <summary>
		/// Gets the VistaMetricsStruct.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static NONCLIENTMETRICS VistaMetricsStruct
		{
			get
			{
				var ncm = new NONCLIENTMETRICS
				{
					cbSize = Marshal.SizeOf(typeof(NONCLIENTMETRICS))
				};
				return ncm;
			}
		}

		/// <summary>
		/// Gets the XPMetricsStruct.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static NONCLIENTMETRICS XPMetricsStruct
		{
			get
			{
				var ncm = new NONCLIENTMETRICS
				{
					// Account for the missing iPaddedBorderWidth
					cbSize = Marshal.SizeOf(typeof(NONCLIENTMETRICS)) - sizeof(int)
				};
				return ncm;
			}
		}
	}

	/// <summary>Defines options that are used to set window visual style attributes.</summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct WTA_OPTIONS
	{
		// public static readonly uint Size = (uint)Marshal.SizeOf(typeof(WTA_OPTIONS));

		/// <summary>
		/// The Size field.
		/// </summary>
		public const uint Size = 8;

		/// <summary>
		/// A combination of flags that modify window visual style attributes.
		/// Can be a combination of the WTNCA constants.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used by native code.")]
		[FieldOffset(0)]
		public WTNCA dwFlags;

		/// <summary>
		/// A bitmask that describes how the values specified in dwFlags should be applied.
		/// If the bit corresponding to a value in dwFlags is 0, that flag will be removed.
		/// If the bit is 1, the flag will be added.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used by native code.")]
		[FieldOffset(4)]
		public WTNCA dwMask;
	}

	/// <summary>
	/// Native MARGINS structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MARGINS
	{
		/// <summary>Width of left border that retains its size.</summary>
		public int cxLeftWidth;

		/// <summary>Width of right border that retains its size.</summary>
		public int cxRightWidth;

		/// <summary>Height of top border that retains its size.</summary>
		public int cyTopHeight;

		/// <summary>Height of bottom border that retains its size.</summary>
		public int cyBottomHeight;
	}

	/// <summary>
	/// Native MONITORINFO class.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class MONITORINFO
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

		/// <summary>
		/// The rcMonitor field.
		/// </summary>
		public RECT rcMonitor;

		/// <summary>
		/// The rcWork field.
		/// </summary>
		public RECT rcWork;

		/// <summary>
		/// The dwFlags field.
		/// </summary>
		public int dwFlags;
	}

	/// <summary>
	/// Native POINT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		/// <summary>
		/// The x field.
		/// </summary>
		public int x;

		/// <summary>
		/// The y field.
		/// </summary>
		public int y;
	}

	/// <summary>
	/// Native RefPOINT class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential)]
	internal class RefPOINT
	{
		/// <summary>
		/// The x field.
		/// </summary>
		public int x;

		/// <summary>
		/// The y field.
		/// </summary>
		public int y;
	}

	/// <summary>
	/// Native RECT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		private int _left;
		private int _top;
		private int _right;
		private int _bottom;

		/// <summary>
		/// Performs the Offset operation.
		/// </summary>
		/// <param name="dx">The dx parameter.</param>
		/// <param name="dy">The dy parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public void Offset(int dx, int dy)
		{
			_left += dx;
			_top += dy;
			_right += dx;
			_bottom += dy;
		}

		/// <summary>
		/// Gets or sets the Left.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Left { get => _left; set => _left = value; }

		/// <summary>
		/// Gets or sets the Right.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Right { get => _right; set => _right = value; }

		/// <summary>
		/// Gets or sets the Top.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Top { get => _top; set => _top = value; }

		/// <summary>
		/// Gets or sets the Bottom.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Bottom { get => _bottom; set => _bottom = value; }

		/// <summary>
		/// Gets the Width.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Width => _right - _left;

		/// <summary>
		/// Gets the Height.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Height => _bottom - _top;

		/// <summary>
		/// Gets the Position.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public POINT Position => new POINT { x = _left, y = _top };

		/// <summary>
		/// Gets the Size.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public SIZE Size => new SIZE { cx = Width, cy = Height };

		/// <summary>
		/// Performs the Union operation.
		/// </summary>
		/// <param name="rect1">The rect1 parameter.</param>
		/// <param name="rect2">The rect2 parameter.</param>
		/// <returns>The result.</returns>
		public static RECT Union(RECT rect1, RECT rect2)
		{
			return new RECT
			{
				Left = Math.Min(rect1.Left, rect2.Left),
				Top = Math.Min(rect1.Top, rect2.Top),
				Right = Math.Max(rect1.Right, rect2.Right),
				Bottom = Math.Max(rect1.Bottom, rect2.Bottom),
			};
		}

		/// <summary>
		/// Performs the Equals operation.
		/// </summary>
		/// <param name="obj">The obj parameter.</param>
		/// <returns>The result.</returns>
		public override bool Equals(object obj)
		{
			try
			{
				var rc = (RECT)obj;
				return rc._bottom == _bottom
					&& rc._left == _left
					&& rc._right == _right
					&& rc._top == _top;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return (_left << 16 | Utility.LOWORD(_right)) ^ (_top << 16 | Utility.LOWORD(_bottom));
		}
	}

	/// <summary>
	/// Native RefRECT class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential)]
	internal class RefRECT
	{
		private int _left;
		private int _top;
		private int _right;
		private int _bottom;

		/// <summary>
		/// Initializes a new instance of the <see cref="RefRECT"/> class.
		/// </summary>
		/// <param name="left">The left parameter.</param>
		/// <param name="top">The top parameter.</param>
		/// <param name="right">The right parameter.</param>
		/// <param name="bottom">The bottom parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public RefRECT(int left, int top, int right, int bottom)
		{
			_left = left;
			_top = top;
			_right = right;
			_bottom = bottom;
		}

		/// <summary>
		/// Gets the Width.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Width => _right - _left;

		/// <summary>
		/// Gets the Height.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Height => _bottom - _top;

		/// <summary>
		/// Gets or sets the Left.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Left { get => _left; set => _left = value; }

		/// <summary>
		/// Gets or sets the Right.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Right { get => _right; set => _right = value; }

		/// <summary>
		/// Gets or sets the Top.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Top { get => _top; set => _top = value; }

		/// <summary>
		/// Gets or sets the Bottom.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Bottom { get => _bottom; set => _bottom = value; }

		/// <summary>
		/// Performs the Offset operation.
		/// </summary>
		/// <param name="dx">The dx parameter.</param>
		/// <param name="dy">The dy parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public void Offset(int dx, int dy)
		{
			_left += dx;
			_top += dy;
			_right += dx;
			_bottom += dy;
		}
	}

	/// <summary>
	/// Native SIZE structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct SIZE
	{
		/// <summary>
		/// The cx field.
		/// </summary>
		public int cx;

		/// <summary>
		/// The cy field.
		/// </summary>
		public int cy;
	}

	/// <summary>
	/// Native StartupOutput structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct StartupOutput
	{
		/// <summary>
		/// The hook field.
		/// </summary>
		public IntPtr hook;

		/// <summary>
		/// The unhook field.
		/// </summary>
		public IntPtr unhook;
	}

	/// <summary>
	/// Native StartupInput class.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class StartupInput
	{
		/// <summary>
		/// The GdiplusVersion field.
		/// </summary>
		public int GdiplusVersion = 1;

		/// <summary>
		/// The DebugEventCallback field.
		/// </summary>
		public IntPtr DebugEventCallback;

		/// <summary>
		/// The SuppressBackgroundThread field.
		/// </summary>
		public bool SuppressBackgroundThread;

		/// <summary>
		/// The SuppressExternalCodecs field.
		/// </summary>
		public bool SuppressExternalCodecs;
	}

	/// <summary>
	/// Native WIN32_FIND_DATAW class.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	[BestFitMapping(false)]
	internal class WIN32_FIND_DATAW
	{
		/// <summary>
		/// The dwFileAttributes field.
		/// </summary>
		public FileAttributes dwFileAttributes;

		/// <summary>
		/// The ftCreationTime field.
		/// </summary>
		public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

		/// <summary>
		/// The ftLastAccessTime field.
		/// </summary>
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

		/// <summary>
		/// The ftLastWriteTime field.
		/// </summary>
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

		/// <summary>
		/// The nFileSizeHigh field.
		/// </summary>
		public int nFileSizeHigh;

		/// <summary>
		/// The nFileSizeLow field.
		/// </summary>
		public int nFileSizeLow;

		/// <summary>
		/// The dwReserved0 field.
		/// </summary>
		public int dwReserved0;

		/// <summary>
		/// The dwReserved1 field.
		/// </summary>
		public int dwReserved1;

		/// <summary>
		/// The cFileName field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string cFileName;

		/// <summary>
		/// The cAlternateFileName field.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string cAlternateFileName;
	}

	/// <summary>
	/// Native WINDOWPLACEMENT class.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class WINDOWPLACEMENT
	{
		/// <summary>
		/// The length field.
		/// </summary>
		public int length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

		/// <summary>
		/// The flags field.
		/// </summary>
		public int flags;

		/// <summary>
		/// The showCmd field.
		/// </summary>
		public SW showCmd;

		/// <summary>
		/// The ptMinPosition field.
		/// </summary>
		public POINT ptMinPosition;

		/// <summary>
		/// The ptMaxPosition field.
		/// </summary>
		public POINT ptMaxPosition;

		/// <summary>
		/// The rcNormalPosition field.
		/// </summary>
		public RECT rcNormalPosition;
	}

	/// <summary>
	/// Native WINDOWPOS structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPOS
	{
		/// <summary>
		/// The hwnd field.
		/// </summary>
		public IntPtr hwnd;

		/// <summary>
		/// The hwndInsertAfter field.
		/// </summary>
		public IntPtr hwndInsertAfter;

		/// <summary>
		/// The x field.
		/// </summary>
		public int x;

		/// <summary>
		/// The y field.
		/// </summary>
		public int y;

		/// <summary>
		/// The cx field.
		/// </summary>
		public int cx;

		/// <summary>
		/// The cy field.
		/// </summary>
		public int cy;

		/// <summary>
		/// The flags field.
		/// </summary>
		public int flags;
	}

	/// <summary>
	/// Native WNDCLASSEX structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct WNDCLASSEX
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The style field.
		/// </summary>
		public CS style;

		/// <summary>
		/// The lpfnWndProc field.
		/// </summary>
		public WndProc lpfnWndProc;

		/// <summary>
		/// The cbClsExtra field.
		/// </summary>
		public int cbClsExtra;

		/// <summary>
		/// The cbWndExtra field.
		/// </summary>
		public int cbWndExtra;

		/// <summary>
		/// The hInstance field.
		/// </summary>
		public IntPtr hInstance;

		/// <summary>
		/// The hIcon field.
		/// </summary>
		public IntPtr hIcon;

		/// <summary>
		/// The hCursor field.
		/// </summary>
		public IntPtr hCursor;

		/// <summary>
		/// The hbrBackground field.
		/// </summary>
		public IntPtr hbrBackground;

		/// <summary>
		/// The lpszMenuName field.
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszMenuName;

		/// <summary>
		/// The lpszClassName field.
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszClassName;

		/// <summary>
		/// The hIconSm field.
		/// </summary>
		public IntPtr hIconSm;
	}

	/// <summary>
	/// Native MOUSEINPUT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MOUSEINPUT
	{
		/// <summary>
		/// The dx field.
		/// </summary>
		public int dx;

		/// <summary>
		/// The dy field.
		/// </summary>
		public int dy;

		/// <summary>
		/// The mouseData field.
		/// </summary>
		public int mouseData;

		/// <summary>
		/// The dwFlags field.
		/// </summary>
		public int dwFlags;

		/// <summary>
		/// The time field.
		/// </summary>
		public int time;

		/// <summary>
		/// The dwExtraInfo field.
		/// </summary>
		public IntPtr dwExtraInfo;
	}

	/// <summary>
	/// Native INPUT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct INPUT
	{
		/// <summary>
		/// The type field.
		/// </summary>
		public uint type;

		/// <summary>
		/// The mi field.
		/// </summary>
		public MOUSEINPUT mi;
	}

	/// <summary>
	/// Native UNSIGNED_RATIO structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct UNSIGNED_RATIO
	{
		/// <summary>
		/// The uiNumerator field.
		/// </summary>
		public uint uiNumerator;

		/// <summary>
		/// The uiDenominator field.
		/// </summary>
		public uint uiDenominator;
	}

	/// <summary>
	/// Native DWM_TIMING_INFO structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct DWM_TIMING_INFO
	{
		/// <summary>
		/// The cbSize field.
		/// </summary>
		public int cbSize;

		/// <summary>
		/// The rateRefresh field.
		/// </summary>
		public UNSIGNED_RATIO rateRefresh;

		/// <summary>
		/// The qpcRefreshPeriod field.
		/// </summary>
		public ulong qpcRefreshPeriod;

		/// <summary>
		/// The rateCompose field.
		/// </summary>
		public UNSIGNED_RATIO rateCompose;

		/// <summary>
		/// The qpcVBlank field.
		/// </summary>
		public ulong qpcVBlank;

		/// <summary>
		/// The cRefresh field.
		/// </summary>
		public ulong cRefresh;

		/// <summary>
		/// The cDXRefresh field.
		/// </summary>
		public uint cDXRefresh;

		/// <summary>
		/// The qpcCompose field.
		/// </summary>
		public ulong qpcCompose;

		/// <summary>
		/// The cFrame field.
		/// </summary>
		public ulong cFrame;

		/// <summary>
		/// The cDXPresent field.
		/// </summary>
		public uint cDXPresent;

		/// <summary>
		/// The cRefreshFrame field.
		/// </summary>
		public ulong cRefreshFrame;

		/// <summary>
		/// The cFrameSubmitted field.
		/// </summary>
		public ulong cFrameSubmitted;

		/// <summary>
		/// The cDXPresentSubmitted field.
		/// </summary>
		public uint cDXPresentSubmitted;

		/// <summary>
		/// The cFrameConfirmed field.
		/// </summary>
		public ulong cFrameConfirmed;

		/// <summary>
		/// The cDXPresentConfirmed field.
		/// </summary>
		public uint cDXPresentConfirmed;

		/// <summary>
		/// The cRefreshConfirmed field.
		/// </summary>
		public ulong cRefreshConfirmed;

		/// <summary>
		/// The cDXRefreshConfirmed field.
		/// </summary>
		public uint cDXRefreshConfirmed;

		/// <summary>
		/// The cFramesLate field.
		/// </summary>
		public ulong cFramesLate;

		/// <summary>
		/// The cFramesOutstanding field.
		/// </summary>
		public uint cFramesOutstanding;

		/// <summary>
		/// The cFrameDisplayed field.
		/// </summary>
		public ulong cFrameDisplayed;

		/// <summary>
		/// The qpcFrameDisplayed field.
		/// </summary>
		public ulong qpcFrameDisplayed;

		/// <summary>
		/// The cRefreshFrameDisplayed field.
		/// </summary>
		public ulong cRefreshFrameDisplayed;

		/// <summary>
		/// The cFrameComplete field.
		/// </summary>
		public ulong cFrameComplete;

		/// <summary>
		/// The qpcFrameComplete field.
		/// </summary>
		public ulong qpcFrameComplete;

		/// <summary>
		/// The cFramePending field.
		/// </summary>
		public ulong cFramePending;

		/// <summary>
		/// The qpcFramePending field.
		/// </summary>
		public ulong qpcFramePending;

		/// <summary>
		/// The cFramesDisplayed field.
		/// </summary>
		public ulong cFramesDisplayed;

		/// <summary>
		/// The cFramesComplete field.
		/// </summary>
		public ulong cFramesComplete;

		/// <summary>
		/// The cFramesPending field.
		/// </summary>
		public ulong cFramesPending;

		/// <summary>
		/// The cFramesAvailable field.
		/// </summary>
		public ulong cFramesAvailable;

		/// <summary>
		/// The cFramesDropped field.
		/// </summary>
		public ulong cFramesDropped;

		/// <summary>
		/// The cFramesMissed field.
		/// </summary>
		public ulong cFramesMissed;

		/// <summary>
		/// The cRefreshNextDisplayed field.
		/// </summary>
		public ulong cRefreshNextDisplayed;

		/// <summary>
		/// The cRefreshNextPresented field.
		/// </summary>
		public ulong cRefreshNextPresented;

		/// <summary>
		/// The cRefreshesDisplayed field.
		/// </summary>
		public ulong cRefreshesDisplayed;

		/// <summary>
		/// The cRefreshesPresented field.
		/// </summary>
		public ulong cRefreshesPresented;

		/// <summary>
		/// The cRefreshStarted field.
		/// </summary>
		public ulong cRefreshStarted;

		/// <summary>
		/// The cPixelsReceived field.
		/// </summary>
		public ulong cPixelsReceived;

		/// <summary>
		/// The cPixelsDrawn field.
		/// </summary>
		public ulong cPixelsDrawn;

		/// <summary>
		/// The cBuffersEmpty field.
		/// </summary>
		public ulong cBuffersEmpty;
	}

	/// <summary>
	/// Delegate for WndProc.
	/// </summary>
	/// <param name="hwnd">The hwnd parameter.</param>
	/// <param name="uMsg">The uMsg parameter.</param>
	/// <param name="wParam">The wParam parameter.</param>
	/// <param name="lParam">The lParam parameter.</param>
	/// <returns>The result.</returns>
	internal delegate IntPtr WndProc(IntPtr hwnd, WM uMsg, IntPtr wParam, IntPtr lParam);

	/// <summary>
	/// Delegate for WndProcHook.
	/// </summary>
	/// <param name="hwnd">The hwnd parameter.</param>
	/// <param name="uMsg">The uMsg parameter.</param>
	/// <param name="wParam">The wParam parameter.</param>
	/// <param name="lParam">The lParam parameter.</param>
	/// <param name="handled">The handled parameter.</param>
	/// <returns>The result.</returns>
	internal delegate IntPtr WndProcHook(IntPtr hwnd, WM uMsg, IntPtr wParam, IntPtr lParam, ref bool handled);

	/// <summary>
	/// Delegate for MessageHandler.
	/// </summary>
	/// <param name="uMsg">The uMsg parameter.</param>
	/// <param name="wParam">The wParam parameter.</param>
	/// <param name="lParam">The lParam parameter.</param>
	/// <param name="handled">The handled parameter.</param>
	/// <returns>The result.</returns>
	internal delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

	// Some native methods are shimmed through public versions that handle converting failures into thrown exceptions.

	/// <summary>
	/// Native NativeMethods class.
	/// </summary>
	internal static class NativeMethods
	{
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "AdjustWindowRectEx", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _AdjustWindowRectEx(ref RECT lpRect, WS dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, WS_EX dwExStyle);

		/// <summary>
		/// Performs the AdjustWindowRectEx operation.
		/// </summary>
		/// <param name="lpRect">The lpRect parameter.</param>
		/// <param name="dwStyle">The dwStyle parameter.</param>
		/// <param name="bMenu">The bMenu parameter.</param>
		/// <param name="dwExStyle">The dwExStyle parameter.</param>
		/// <returns>The result.</returns>
		public static RECT AdjustWindowRectEx(RECT lpRect, WS dwStyle, bool bMenu, WS_EX dwExStyle)
		{
			// Native version modifies the parameter in place.
			if (!_AdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle)) HRESULT.ThrowLastError();
			return lpRect;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "ChangeWindowMessageFilter", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _ChangeWindowMessageFilter(WM message, MSGFLT dwFlag);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "ChangeWindowMessageFilterEx", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _ChangeWindowMessageFilterEx(IntPtr hwnd, WM message, MSGFLT action, [In, Out, Optional] ref CHANGEFILTERSTRUCT pChangeFilterStruct);

		// Note that processes at or below SECURITY_MANDATORY_LOW_RID are not allowed to change the message filter.
		// If those processes call this function, it will fail and generate the extended error code, ERROR_ACCESS_DENIED.

		/// <summary>
		/// Performs the ChangeWindowMessageFilterEx operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="message">The message parameter.</param>
		/// <param name="action">The action parameter.</param>
		/// <param name="filterInfo">The filterInfo parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static HRESULT ChangeWindowMessageFilterEx(IntPtr hwnd, WM message, MSGFLT action, out MSGFLTINFO filterInfo)
		{
			filterInfo = MSGFLTINFO.NONE;

			bool ret;

			// This origins of this API were added for Vista.  The Ex version was added for Windows 7.
			// If we're not on either, then this message filter isolation doesn't exist.
			if (!Utility.IsOSVistaOrNewer)
				return HRESULT.S_FALSE;

			// If we're on Vista rather than Win7 then we can't use the Ex version of this function.
			// The Ex version is preferred if possible because this results in process-wide modifications of the filter
			// and is deprecated as of Win7.
			if (!Utility.IsOSWindows7OrNewer)
			{
				// Note that the Win7 MSGFLT_ALLOW/DISALLOW enum values map to the Vista MSGFLT_ADD/REMOVE
				ret = _ChangeWindowMessageFilter(message, action);
				return !ret ? (HRESULT)Win32Error.GetLastError() : HRESULT.S_OK;
			}

			var filterstruct = new CHANGEFILTERSTRUCT { cbSize = (uint)Marshal.SizeOf(typeof(CHANGEFILTERSTRUCT)) };
			ret = _ChangeWindowMessageFilterEx(hwnd, message, action, ref filterstruct);
			if (!ret) return (HRESULT)Win32Error.GetLastError();

			filterInfo = filterstruct.ExtStatus;
			return HRESULT.S_OK;
		}

		/// <summary>
		/// Native method wrapper for CombineRgn.
		/// </summary>
		/// <param name="hrgnDest">The hrgnDest parameter.</param>
		/// <param name="hrgnSrc1">The hrgnSrc1 parameter.</param>
		/// <param name="hrgnSrc2">The hrgnSrc2 parameter.</param>
		/// <param name="fnCombineMode">The fnCombineMode parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll")]
		public static extern CombineRgnResult CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, RGN fnCombineMode);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
		private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);

		/// <summary>
		/// Performs the CommandLineToArgvW operation.
		/// </summary>
		/// <param name="cmdLine">The cmdLine parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static string[] CommandLineToArgvW(string cmdLine)
		{
			var argv = IntPtr.Zero;
			try
			{
				argv = _CommandLineToArgvW(cmdLine, out var numArgs);
				if (argv == IntPtr.Zero) throw new Win32Exception();
				var result = new string[numArgs];
				for (var i = 0; i < numArgs; i++)
				{
					var currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(typeof(IntPtr)));
					result[i] = Marshal.PtrToStringUni(currArg);
				}

				return result;
			}
			finally
			{
				var p = _LocalFree(argv);
				// Otherwise LocalFree failed.
				Assert.AreEqual(IntPtr.Zero, p);
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
		private static extern SafeHBITMAP _CreateDIBSection(SafeDC hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, [Out] out IntPtr ppvBits, IntPtr hSection, int dwOffset);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
		private static extern SafeHBITMAP _CreateDIBSectionIntPtr(IntPtr hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, [Out] out IntPtr ppvBits, IntPtr hSection, int dwOffset);

		/// <summary>
		/// Performs the CreateDIBSection operation.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <param name="bitmapInfo">The bitmapInfo parameter.</param>
		/// <param name="ppvBits">The ppvBits parameter.</param>
		/// <param name="hSection">The hSection parameter.</param>
		/// <param name="dwOffset">The dwOffset parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static SafeHBITMAP CreateDIBSection(SafeDC hdc, ref BITMAPINFO bitmapInfo, out IntPtr ppvBits, IntPtr hSection, int dwOffset)
		{
			const int DIB_RGB_COLORS = 0;
			SafeHBITMAP hBitmap = null;
			if (hdc == null)
				hBitmap = _CreateDIBSectionIntPtr(IntPtr.Zero, ref bitmapInfo, DIB_RGB_COLORS, out ppvBits, hSection, dwOffset);
			else
				hBitmap = _CreateDIBSection(hdc, ref bitmapInfo, DIB_RGB_COLORS, out ppvBits, hSection, dwOffset);
			if (hBitmap.IsInvalid) HRESULT.ThrowLastError();
			return hBitmap;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn", SetLastError = true)]
		private static extern IntPtr _CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

		/// <summary>
		/// Performs the CreateRoundRectRgn operation.
		/// </summary>
		/// <param name="nLeftRect">The nLeftRect parameter.</param>
		/// <param name="nTopRect">The nTopRect parameter.</param>
		/// <param name="nRightRect">The nRightRect parameter.</param>
		/// <param name="nBottomRect">The nBottomRect parameter.</param>
		/// <param name="nWidthEllipse">The nWidthEllipse parameter.</param>
		/// <param name="nHeightEllipse">The nHeightEllipse parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse)
		{
			var ret = _CreateRoundRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect, nWidthEllipse, nHeightEllipse);
			if (ret == IntPtr.Zero) throw new Win32Exception();
			return ret;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "CreateRectRgn", SetLastError = true)]
		private static extern IntPtr _CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		/// <summary>
		/// Performs the CreateRectRgn operation.
		/// </summary>
		/// <param name="nLeftRect">The nLeftRect parameter.</param>
		/// <param name="nTopRect">The nTopRect parameter.</param>
		/// <param name="nRightRect">The nRightRect parameter.</param>
		/// <param name="nBottomRect">The nBottomRect parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
		{
			var ret = _CreateRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect);
			if (ret == IntPtr.Zero) throw new Win32Exception();
			return ret;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "CreateRectRgnIndirect", SetLastError = true)]
		private static extern IntPtr _CreateRectRgnIndirect([In] ref RECT lprc);

		/// <summary>
		/// Performs the CreateRectRgnIndirect operation.
		/// </summary>
		/// <param name="lprc">The lprc parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr CreateRectRgnIndirect(RECT lprc)
		{
			var ret = _CreateRectRgnIndirect(ref lprc);
			if (ret == IntPtr.Zero) throw new Win32Exception();
			return ret;
		}

		/// <summary>
		/// Native method wrapper for CreateSolidBrush.
		/// </summary>
		/// <param name="crColor">The crColor parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateSolidBrush(int crColor);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
		private static extern IntPtr _CreateWindowEx(
			WS_EX dwExStyle,
			[MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
			WS dwStyle,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam);

		/// <summary>
		/// Performs the CreateWindowEx operation.
		/// </summary>
		/// <param name="dwExStyle">The dwExStyle parameter.</param>
		/// <param name="lpClassName">The lpClassName parameter.</param>
		/// <param name="lpWindowName">The lpWindowName parameter.</param>
		/// <param name="dwStyle">The dwStyle parameter.</param>
		/// <param name="x">The x parameter.</param>
		/// <param name="y">The y parameter.</param>
		/// <param name="nWidth">The nWidth parameter.</param>
		/// <param name="nHeight">The nHeight parameter.</param>
		/// <param name="hWndParent">The hWndParent parameter.</param>
		/// <param name="hMenu">The hMenu parameter.</param>
		/// <param name="hInstance">The hInstance parameter.</param>
		/// <param name="lpParam">The lpParam parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr CreateWindowEx(
			WS_EX dwExStyle,
			string lpClassName,
			string lpWindowName,
			WS dwStyle,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam)
		{
			var ret = _CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
			if (ret == IntPtr.Zero) HRESULT.ThrowLastError();
			return ret;
		}

		/// <summary>
		/// Native method wrapper for DefWindowProc.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="Msg">The Msg parameter.</param>
		/// <param name="wParam">The wParam parameter.</param>
		/// <param name="lParam">The lParam parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Native method wrapper for DeleteObject.
		/// </summary>
		/// <param name="hObject">The hObject parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(IntPtr hObject);

		/// <summary>
		/// Native method wrapper for DestroyIcon.
		/// </summary>
		/// <param name="handle">The handle parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyIcon(IntPtr handle);

		/// <summary>
		/// Native method wrapper for DestroyWindow.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyWindow(IntPtr hwnd);

		/// <summary>
		/// Native method wrapper for IsWindow.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hwnd);

		/// <summary>
		/// Native method wrapper for DwmExtendFrameIntoClientArea.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="pMarInset">The pMarInset parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _DwmIsCompositionEnabled();

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", EntryPoint = "DwmGetColorizationColor", PreserveSig = true)]
		private static extern HRESULT _DwmGetColorizationColor(out uint pcrColorization, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

		/// <summary>
		/// Performs the DwmGetColorizationColor operation.
		/// </summary>
		/// <param name="pcrColorization">The pcrColorization parameter.</param>
		/// <param name="pfOpaqueBlend">The pfOpaqueBlend parameter.</param>
		/// <returns>The result.</returns>
		public static bool DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend)
		{
			// Make this call safe to make on downlevel OSes...
			if (Utility.IsOSVistaOrNewer && IsThemeActive())
			{
				var hr = _DwmGetColorizationColor(out pcrColorization, out pfOpaqueBlend);
				if (hr.Succeeded)
				{
					return true;
				}
			}

			// Default values.  If for some reason the native DWM API fails it's never enough of a reason
			// to bring down the app.  Empirically it still sometimes returns errors even when the theme service is on.
			// We'll still use the boolean return value to allow the caller to respond if they care.
			pcrColorization = 0xFF000000;
			pfOpaqueBlend = true;

			return false;
		}

		/// <summary>
		/// Performs the DwmIsCompositionEnabled operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool DwmIsCompositionEnabled()
		{
			// Make this call safe to make on downlevel OSes...
			if (!Utility.IsOSVistaOrNewer)
			{
				return false;
			}

			return _DwmIsCompositionEnabled();
		}

		/// <summary>
		/// Native method wrapper for DwmDefWindowProc.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="msg">The msg parameter.</param>
		/// <param name="wParam">The wParam parameter.</param>
		/// <param name="lParam">The lParam parameter.</param>
		/// <param name="plResult">The plResult parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DwmDefWindowProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam, out IntPtr plResult);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
		private static extern void _DwmSetWindowAttribute(IntPtr hwnd, DWMWA dwAttribute, ref int pvAttribute, int cbAttribute);

		/// <summary>
		/// Performs the DwmSetWindowAttributeFlip3DPolicy operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="flip3dPolicy">The flip3dPolicy parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void DwmSetWindowAttributeFlip3DPolicy(IntPtr hwnd, DWMFLIP3D flip3dPolicy)
		{
			Assert.IsTrue(Utility.IsOSVistaOrNewer);
			var dwPolicy = (int)flip3dPolicy;
			_DwmSetWindowAttribute(hwnd, DWMWA.FLIP3D_POLICY, ref dwPolicy, sizeof(int));
		}

		/// <summary>
		/// Performs the DwmSetWindowAttributeDisallowPeek operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="disallowPeek">The disallowPeek parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void DwmSetWindowAttributeDisallowPeek(IntPtr hwnd, bool disallowPeek)
		{
			Assert.IsTrue(Utility.IsOSWindows7OrNewer);
			var dwDisallow = (int)(disallowPeek ? Win32Value.TRUE : Win32Value.FALSE);
			_DwmSetWindowAttribute(hwnd, DWMWA.DISALLOW_PEEK, ref dwDisallow, sizeof(int));
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "EnableMenuItem")]
		private static extern int _EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable);

		/// <summary>
		/// Performs the EnableMenuItem operation.
		/// </summary>
		/// <param name="hMenu">The hMenu parameter.</param>
		/// <param name="uIDEnableItem">The uIDEnableItem parameter.</param>
		/// <param name="uEnable">The uEnable parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static MF EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable)
		{
			// Returns the previous state of the menu item, or -1 if the menu item does not exist.
			var iRet = _EnableMenuItem(hMenu, uIDEnableItem, uEnable);
			return (MF)iRet;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "RemoveMenu", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

		/// <summary>
		/// Performs the RemoveMenu operation.
		/// </summary>
		/// <param name="hMenu">The hMenu parameter.</param>
		/// <param name="uPosition">The uPosition parameter.</param>
		/// <param name="uFlags">The uFlags parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void RemoveMenu(IntPtr hMenu, SC uPosition, MF uFlags)
		{
			if (!_RemoveMenu(hMenu, (uint)uPosition, (uint)uFlags))
			{
				throw new Win32Exception();
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "DrawMenuBar", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _DrawMenuBar(IntPtr hWnd);

		/// <summary>
		/// Performs the DrawMenuBar operation.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void DrawMenuBar(IntPtr hWnd)
		{
			if (!_DrawMenuBar(hWnd)) throw new Win32Exception();
		}

		/// <summary>
		/// Native method wrapper for FindClose.
		/// </summary>
		/// <param name="handle">The handle parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FindClose(IntPtr handle);

		// Not shimming this SetLastError=true function because callers want to evaluate the reason for failure.

		/// <summary>
		/// Native method wrapper for FindFirstFileW.
		/// </summary>
		/// <param name="lpFileName">The lpFileName parameter.</param>
		/// <param name="lpFindFileData">The lpFindFileData parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFindHandle FindFirstFileW(string lpFileName, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

		// Not shimming this SetLastError=true function because callers want to evaluate the reason for failure.

		/// <summary>
		/// Native method wrapper for FindNextFileW.
		/// </summary>
		/// <param name="hndFindFile">The hndFindFile parameter.</param>
		/// <param name="lpFindFileData">The lpFindFileData parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FindNextFileW(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "GetClientRect", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _GetClientRect(IntPtr hwnd, [Out] out RECT lpRect);

		/// <summary>
		/// Performs the GetClientRect operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static RECT GetClientRect(IntPtr hwnd)
		{
			if (!_GetClientRect(hwnd, out var rc)) HRESULT.ThrowLastError();
			return rc;
		}

		[DllImport("uxtheme.dll", EntryPoint = "GetCurrentThemeName", CharSet = CharSet.Unicode)]
		private static extern HRESULT _GetCurrentThemeName(
			StringBuilder pszThemeFileName,
			int dwMaxNameChars,
			StringBuilder pszColorBuff,
			int cchMaxColorChars,
			StringBuilder pszSizeBuff,
			int cchMaxSizeChars);

		/// <summary>
		/// Performs the GetCurrentThemeName operation.
		/// </summary>
		/// <param name="themeFileName">The themeFileName parameter.</param>
		/// <param name="color">The color parameter.</param>
		/// <param name="size">The size parameter.</param>
		public static void GetCurrentThemeName(out string themeFileName, out string color, out string size)
		{
			// Not expecting strings longer than MAX_PATH.  We will return the error
			var fileNameBuilder = new StringBuilder((int)Win32Value.MAX_PATH);
			var colorBuilder = new StringBuilder((int)Win32Value.MAX_PATH);
			var sizeBuilder = new StringBuilder((int)Win32Value.MAX_PATH);

			// This will throw if the theme service is not active (e.g. not UxTheme!IsThemeActive).
			_GetCurrentThemeName(fileNameBuilder, fileNameBuilder.Capacity,
								 colorBuilder, colorBuilder.Capacity,
								 sizeBuilder, sizeBuilder.Capacity)
				.ThrowIfFailed();

			themeFileName = fileNameBuilder.ToString();
			color = colorBuilder.ToString();
			size = sizeBuilder.ToString();
		}

		/// <summary>
		/// Native method wrapper for IsThemeActive.
		/// </summary>
		/// <returns>The result.</returns>
		[DllImport("uxtheme.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsThemeActive();

		/// <summary>
		/// Performs the GetDC operation.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Obsolete("Use SafeDC.GetDC instead.", true)]
		public static void GetDC()
		{
		}

		/// <summary>
		/// Native method wrapper for GetDeviceCaps.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <param name="nIndex">The nIndex parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll")]
		public static extern int GetDeviceCaps(SafeDC hdc, DeviceCap nIndex);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", EntryPoint = "GetModuleFileName", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int _GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

		/// <summary>
		/// Performs the GetModuleFileName operation.
		/// </summary>
		/// <param name="hModule">The hModule parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static string GetModuleFileName(IntPtr hModule)
		{
			var buffer = new StringBuilder((int)Win32Value.MAX_PATH);
			while (true)
			{
				var size = _GetModuleFileName(hModule, buffer, buffer.Capacity);
				if (size == 0) HRESULT.ThrowLastError();

				// GetModuleFileName returns nSize when it's truncated but does NOT set the last error.
				// MSDN documentation says this has changed in Windows 2000+.
				if (size == buffer.Capacity)
				{
					// Enlarge the buffer and try again.
					buffer.EnsureCapacity(buffer.Capacity * 2);
					continue;
				}

				return buffer.ToString();
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr _GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

		/// <summary>
		/// Performs the GetModuleHandle operation.
		/// </summary>
		/// <param name="lpModuleName">The lpModuleName parameter.</param>
		/// <returns>The result.</returns>
		public static IntPtr GetModuleHandle(string lpModuleName)
		{
			var retPtr = _GetModuleHandle(lpModuleName);
			if (retPtr == IntPtr.Zero) HRESULT.ThrowLastError();
			return retPtr;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "GetMonitorInfo", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFO lpmi);

		/// <summary>
		/// Performs the GetMonitorInfo operation.
		/// </summary>
		/// <param name="hMonitor">The hMonitor parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static MONITORINFO GetMonitorInfo(IntPtr hMonitor)
		{
			var mi = new MONITORINFO();
			if (!_GetMonitorInfo(hMonitor, mi)) throw new Win32Exception();
			return mi;
		}

		[DllImport("gdi32.dll", EntryPoint = "GetStockObject", SetLastError = true)]
		private static extern IntPtr _GetStockObject(StockObject fnObject);

		/// <summary>
		/// Performs the GetStockObject operation.
		/// </summary>
		/// <param name="fnObject">The fnObject parameter.</param>
		/// <returns>The result.</returns>
		public static IntPtr GetStockObject(StockObject fnObject)
		{
			var retPtr = _GetStockObject(fnObject);
			return retPtr;
		}

		/// <summary>
		/// Native method wrapper for GetSystemMenu.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="bRevert">The bRevert parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

		/// <summary>
		/// Native method wrapper for GetSystemMetrics.
		/// </summary>
		/// <param name="nIndex">The nIndex parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(SM nIndex);

		// This is aliased as a macro in 32bit Windows.

		/// <summary>
		/// Performs the GetWindowLongPtr operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="nIndex">The nIndex parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr GetWindowLongPtr(IntPtr hwnd, GWL nIndex)
		{
			var ret = IntPtr.Zero;
			ret = IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, nIndex) : new IntPtr(GetWindowLongPtr32(hwnd, nIndex));
			if (ret == IntPtr.Zero) throw new Win32Exception();
			return ret;
		}

		/// <summary>
		/// Sets attributes to control how visual styles are applied to a specified window.
		/// </summary>
		/// <param name="hwnd">
		/// Handle to a window to apply changes to.
		/// </param>
		/// <param name="eAttribute">
		/// Value of type WINDOWTHEMEATTRIBUTETYPE that specifies the type of attribute to set.
		/// The value of this parameter determines the type of data that should be passed in the pvAttribute parameter.
		/// Can be the following value:
		/// <list>WTA_NONCLIENT (Specifies non-client related attributes).</list>
		/// pvAttribute must be a pointer of type WTA_OPTIONS.
		/// </param>
		/// <param name="pvAttribute">
		/// A pointer that specifies attributes to set. Type is determined by the value of the eAttribute value.
		/// </param>
		/// <param name="cbAttribute">
		/// Specifies the size, in bytes, of the data pointed to by pvAttribute.
		/// </param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("uxtheme.dll", PreserveSig = false)]
		public static extern void SetWindowThemeAttribute([In] IntPtr hwnd, [In] WINDOWTHEMEATTRIBUTETYPE eAttribute, [In] ref WTA_OPTIONS pvAttribute, [In] uint cbAttribute);

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
		private static extern int GetWindowLongPtr32(IntPtr hWnd, GWL nIndex);

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, GWL nIndex);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(IntPtr hwnd, WINDOWPLACEMENT lpwndpl);

		/// <summary>
		/// Performs the GetWindowPlacement operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static WINDOWPLACEMENT GetWindowPlacement(IntPtr hwnd)
		{
			var wndpl = new WINDOWPLACEMENT();
			if (GetWindowPlacement(hwnd, wndpl)) return wndpl;
			throw new Win32Exception();
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _GetWindowRect(IntPtr hWnd, out RECT lpRect);

		/// <summary>
		/// Performs the GetWindowRect operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		public static RECT GetWindowRect(IntPtr hwnd)
		{
			if (!_GetWindowRect(hwnd, out var rc)) HRESULT.ThrowLastError();
			return rc;
		}

		/// <summary>
		/// Native method wrapper for GdipCreateBitmapFromStream.
		/// </summary>
		/// <param name="stream">The stream parameter.</param>
		/// <param name="bitmap">The bitmap parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdipCreateBitmapFromStream(IStream stream, out IntPtr bitmap);

		/// <summary>
		/// Native method wrapper for GdipCreateHBITMAPFromBitmap.
		/// </summary>
		/// <param name="bitmap">The bitmap parameter.</param>
		/// <param name="hbmReturn">The hbmReturn parameter.</param>
		/// <param name="background">The background parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdipCreateHBITMAPFromBitmap(IntPtr bitmap, out IntPtr hbmReturn, int background);

		/// <summary>
		/// Native method wrapper for GdipCreateHICONFromBitmap.
		/// </summary>
		/// <param name="bitmap">The bitmap parameter.</param>
		/// <param name="hbmReturn">The hbmReturn parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdipCreateHICONFromBitmap(IntPtr bitmap, out IntPtr hbmReturn);

		/// <summary>
		/// Native method wrapper for GdipDisposeImage.
		/// </summary>
		/// <param name="image">The image parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdipDisposeImage(IntPtr image);

		/// <summary>
		/// Native method wrapper for GdipImageForceValidation.
		/// </summary>
		/// <param name="image">The image parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdipImageForceValidation(IntPtr image);

		/// <summary>
		/// Native method wrapper for GdiplusStartup.
		/// </summary>
		/// <param name="token">The token parameter.</param>
		/// <param name="input">The input parameter.</param>
		/// <param name="output">The output parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdiplusStartup(out IntPtr token, StartupInput input, out StartupOutput output);

		/// <summary>
		/// Native method wrapper for GdiplusShutdown.
		/// </summary>
		/// <param name="token">The token parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdiplus.dll")]
		public static extern Status GdiplusShutdown(IntPtr token);

		/// <summary>
		/// Native method wrapper for IsWindowVisible.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hwnd);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
		private static extern IntPtr _LocalFree(IntPtr hMem);

		/// <summary>
		/// Native method wrapper for MonitorFromWindow.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="dwFlags">The dwFlags parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _PostMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Performs the PostMessage operation.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="Msg">The Msg parameter.</param>
		/// <param name="wParam">The wParam parameter.</param>
		/// <param name="lParam">The lParam parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void PostMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam)
		{
			if (!_PostMessage(hWnd, Msg, wParam, lParam)) throw new Win32Exception();
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
		private static extern short _RegisterClassEx([In] ref WNDCLASSEX lpwcx);

		// Note that this will throw HRESULT_FROM_WIN32(ERROR_CLASS_ALREADY_EXISTS) on duplicate registration.
		// If needed, consider adding a Try* version of this function that returns the error code since that
		// may be ignorable.

		/// <summary>
		/// Performs the RegisterClassEx operation.
		/// </summary>
		/// <param name="lpwcx">The lpwcx parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static short RegisterClassEx(ref WNDCLASSEX lpwcx)
		{
			var ret = _RegisterClassEx(ref lpwcx);
			if (ret == 0) HRESULT.ThrowLastError();
			return ret;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "RegisterWindowMessage", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern uint _RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

		/// <summary>
		/// Performs the RegisterWindowMessage operation.
		/// </summary>
		/// <param name="lpString">The lpString parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static WM RegisterWindowMessage(string lpString)
		{
			var iRet = _RegisterWindowMessage(lpString);
			if (iRet == 0) HRESULT.ThrowLastError();
			return (WM)iRet;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetActiveWindow", SetLastError = true)]
		private static extern IntPtr _SetActiveWindow(IntPtr hWnd);

		/// <summary>
		/// Performs the SetActiveWindow operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr SetActiveWindow(IntPtr hwnd)
		{
			Verify.IsNotDefault(hwnd, nameof(hwnd));
			var ret = _SetActiveWindow(hwnd);
			if (ret == IntPtr.Zero) HRESULT.ThrowLastError();
			return ret;
		}

		// This is aliased as a macro in 32bit Windows.

		/// <summary>
		/// Performs the SetClassLongPtr operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="nIndex">The nIndex parameter.</param>
		/// <param name="dwNewLong">The dwNewLong parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr SetClassLongPtr(IntPtr hwnd, GCLP nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8) return SetClassLongPtr64(hwnd, nIndex, dwNewLong);
			return new IntPtr(SetClassLongPtr32(hwnd, nIndex, dwNewLong.ToInt32()));
		}

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetClassLong", SetLastError = true)]
		private static extern int SetClassLongPtr32(IntPtr hWnd, GCLP nIndex, int dwNewLong);

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetClassLongPtr", SetLastError = true)]
		private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, GCLP nIndex, IntPtr dwNewLong);

		/// <summary>
		/// Native method wrapper for SetErrorMode.
		/// </summary>
		/// <param name="newMode">The newMode parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern ErrorModes SetErrorMode(ErrorModes newMode);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetProcessWorkingSetSize")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimiumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

		/// <summary>
		/// Performs the SetProcessWorkingSetSize operation.
		/// </summary>
		/// <param name="hProcess">The hProcess parameter.</param>
		/// <param name="dwMinimumWorkingSetSize">The dwMinimumWorkingSetSize parameter.</param>
		/// <param name="dwMaximumWorkingSetSize">The dwMaximumWorkingSetSize parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize)
		{
			if (!_SetProcessWorkingSetSize(hProcess, new IntPtr(dwMinimumWorkingSetSize), new IntPtr(dwMaximumWorkingSetSize))) throw new Win32Exception();
		}

		// This is aliased as a macro in 32bit Windows.

		/// <summary>
		/// Performs the SetWindowLongPtr operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="nIndex">The nIndex parameter.</param>
		/// <param name="dwNewLong">The dwNewLong parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr SetWindowLongPtr(IntPtr hwnd, GWL nIndex, IntPtr dwNewLong)
			=> IntPtr.Size == 8 ? SetWindowLongPtr64(hwnd, nIndex, dwNewLong) : new IntPtr(SetWindowLongPtr32(hwnd, nIndex, dwNewLong.ToInt32()));

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
		private static extern int SetWindowLongPtr32(IntPtr hWnd, GWL nIndex, int dwNewLong);

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetWindowRgn", SetLastError = true)]
		private static extern int _SetWindowRgn(IntPtr hWnd, IntPtr hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

		/// <summary>
		/// Performs the SetWindowRgn operation.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="hRgn">The hRgn parameter.</param>
		/// <param name="bRedraw">The bRedraw parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw)
		{
			if (_SetWindowRgn(hWnd, hRgn, bRedraw) == 0) throw new Win32Exception();
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags);

		/// <summary>
		/// Performs the SetWindowPos operation.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="hWndInsertAfter">The hWndInsertAfter parameter.</param>
		/// <param name="x">The x parameter.</param>
		/// <param name="y">The y parameter.</param>
		/// <param name="cx">The cx parameter.</param>
		/// <param name="cy">The cy parameter.</param>
		/// <param name="uFlags">The uFlags parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags)
			=> _SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);

		/// <summary>
		/// Native method wrapper for SHFileOperation.
		/// </summary>
		/// <param name="lpFileOp">The lpFileOp parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll", SetLastError = false)]
		public static extern Win32Error SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		/// <summary>
		/// Native method wrapper for ShowWindow.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="nCmdShow">The nCmdShow parameter.</param>
		/// <returns>The result.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hwnd, SW nCmdShow);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _SystemParametersInfo_String(SPI uiAction, int uiParam, [MarshalAs(UnmanagedType.LPWStr)] string pvParam, SPIF fWinIni);

		/// <summary>Overload of SystemParametersInfo for getting and setting NONCLIENTMETRICS.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _SystemParametersInfo_NONCLIENTMETRICS(SPI uiAction, int uiParam, [In, Out] ref NONCLIENTMETRICS pvParam, SPIF fWinIni);

		/// <summary>Overload of SystemParametersInfo for getting and setting HIGHCONTRAST.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _SystemParametersInfo_HIGHCONTRAST(SPI uiAction, int uiParam, [In, Out] ref HIGHCONTRAST pvParam, SPIF fWinIni);

		/// <summary>
		/// Performs the SystemParametersInfo operation.
		/// </summary>
		/// <param name="uiAction">The uiAction parameter.</param>
		/// <param name="uiParam">The uiParam parameter.</param>
		/// <param name="pvParam">The pvParam parameter.</param>
		/// <param name="fWinIni">The fWinIni parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SystemParametersInfo(SPI uiAction, int uiParam, string pvParam, SPIF fWinIni)
		{
			if (!_SystemParametersInfo_String(uiAction, uiParam, pvParam, fWinIni)) HRESULT.ThrowLastError();
		}

		/// <summary>
		/// Performs the SystemParameterInfo_GetNONCLIENTMETRICS operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static NONCLIENTMETRICS SystemParameterInfo_GetNONCLIENTMETRICS()
		{
			var metrics = Utility.IsOSVistaOrNewer ? NONCLIENTMETRICS.VistaMetricsStruct : NONCLIENTMETRICS.XPMetricsStruct;
			if (!_SystemParametersInfo_NONCLIENTMETRICS(SPI.GETNONCLIENTMETRICS, metrics.cbSize, ref metrics, SPIF.None)) HRESULT.ThrowLastError();
			return metrics;
		}

		/// <summary>
		/// Performs the SystemParameterInfo_GetHIGHCONTRAST operation.
		/// </summary>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static HIGHCONTRAST SystemParameterInfo_GetHIGHCONTRAST()
		{
			var hc = new HIGHCONTRAST { cbSize = Marshal.SizeOf(typeof(HIGHCONTRAST)) };
			if (!_SystemParametersInfo_HIGHCONTRAST(SPI.GETHIGHCONTRAST, hc.cbSize, ref hc, SPIF.None)) HRESULT.ThrowLastError();
			return hc;
		}

		// This function is strange in that it returns a BOOL if TPM_RETURNCMD isn't specified, but otherwise the command Id.
		// Currently it's only used with TPM_RETURNCMD, so making the signature match that.

		/// <summary>
		/// Native method wrapper for TrackPopupMenuEx.
		/// </summary>
		/// <param name="hmenu">The hmenu parameter.</param>
		/// <param name="fuFlags">The fuFlags parameter.</param>
		/// <param name="x">The x parameter.</param>
		/// <param name="y">The y parameter.</param>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="lptpm">The lptpm parameter.</param>
		/// <returns>The result.</returns>
		[DllImport("user32.dll")]
		public static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
		private static extern IntPtr _SelectObject(SafeDC hdc, IntPtr hgdiobj);

		/// <summary>
		/// Performs the SelectObject operation.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <param name="hgdiobj">The hgdiobj parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr SelectObject(SafeDC hdc, IntPtr hgdiobj)
		{
			var ret = _SelectObject(hdc, hgdiobj);
			if (ret == IntPtr.Zero) HRESULT.ThrowLastError();
			return ret;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
		private static extern IntPtr _SelectObjectSafeHBITMAP(SafeDC hdc, SafeHBITMAP hgdiobj);

		/// <summary>
		/// Performs the SelectObject operation.
		/// </summary>
		/// <param name="hdc">The hdc parameter.</param>
		/// <param name="hgdiobj">The hgdiobj parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr SelectObject(SafeDC hdc, SafeHBITMAP hgdiobj)
		{
			var ret = _SelectObjectSafeHBITMAP(hdc, hgdiobj);
			if (ret == IntPtr.Zero) HRESULT.ThrowLastError();
			return ret;
		}

		/// <summary>
		/// Native method wrapper for SendInput.
		/// </summary>
		/// <param name="nInputs">The nInputs parameter.</param>
		/// <param name="pInputs">The pInputs parameter.</param>
		/// <param name="cbSize">The cbSize parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true)]
		public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);

		// Depending on the message, callers may want to call GetLastError based on the return value.

		/// <summary>
		/// Native method wrapper for SendMessage.
		/// </summary>
		/// <param name="hWnd">The hWnd parameter.</param>
		/// <param name="Msg">The Msg parameter.</param>
		/// <param name="wParam">The wParam parameter.</param>
		/// <param name="lParam">The lParam parameter.</param>
		/// <returns>The result.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "UnregisterClass", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _UnregisterClassAtom(IntPtr lpClassName, IntPtr hInstance);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", EntryPoint = "UnregisterClass", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _UnregisterClassName(string lpClassName, IntPtr hInstance);

		/// <summary>
		/// Performs the UnregisterClass operation.
		/// </summary>
		/// <param name="atom">The atom parameter.</param>
		/// <param name="hinstance">The hinstance parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void UnregisterClass(short atom, IntPtr hinstance)
		{
			if (!_UnregisterClassAtom(new IntPtr(atom), hinstance)) HRESULT.ThrowLastError();
		}

		/// <summary>
		/// Performs the UnregisterClass operation.
		/// </summary>
		/// <param name="lpClassName">The lpClassName parameter.</param>
		/// <param name="hInstance">The hInstance parameter.</param>
		public static void UnregisterClass(string lpClassName, IntPtr hInstance)
		{
			if (!_UnregisterClassName(lpClassName, hInstance)) HRESULT.ThrowLastError();
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UpdateLayeredWindow")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _UpdateLayeredWindow(
			IntPtr hwnd,
			SafeDC hdcDst,
			[In] ref POINT pptDst,
			[In] ref SIZE psize,
			SafeDC hdcSrc,
			[In] ref POINT pptSrc,
			int crKey,
			ref BLENDFUNCTION pblend,
			ULW dwFlags);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UpdateLayeredWindow")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool _UpdateLayeredWindowIntPtr(
			IntPtr hwnd,
			IntPtr hdcDst,
			IntPtr pptDst,
			IntPtr psize,
			IntPtr hdcSrc,
			IntPtr pptSrc,
			int crKey,
			ref BLENDFUNCTION pblend,
			ULW dwFlags);

		/// <summary>
		/// Performs the UpdateLayeredWindow operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="hdcDst">The hdcDst parameter.</param>
		/// <param name="pptDst">The pptDst parameter.</param>
		/// <param name="psize">The psize parameter.</param>
		/// <param name="hdcSrc">The hdcSrc parameter.</param>
		/// <param name="pptSrc">The pptSrc parameter.</param>
		/// <param name="crKey">The crKey parameter.</param>
		/// <param name="pblend">The pblend parameter.</param>
		/// <param name="dwFlags">The dwFlags parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void UpdateLayeredWindow(
			IntPtr hwnd,
			SafeDC hdcDst,
			ref POINT pptDst,
			ref SIZE psize,
			SafeDC hdcSrc,
			ref POINT pptSrc,
			int crKey,
			ref BLENDFUNCTION pblend,
			ULW dwFlags)
		{
			if (!_UpdateLayeredWindow(hwnd, hdcDst, ref pptDst, ref psize, hdcSrc, ref pptSrc, crKey, ref pblend, dwFlags)) HRESULT.ThrowLastError();
		}

		/// <summary>
		/// Performs the UpdateLayeredWindow operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="crKey">The crKey parameter.</param>
		/// <param name="pblend">The pblend parameter.</param>
		/// <param name="dwFlags">The dwFlags parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void UpdateLayeredWindow(IntPtr hwnd, int crKey, ref BLENDFUNCTION pblend, ULW dwFlags)
		{
			if (!_UpdateLayeredWindowIntPtr(hwnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, crKey, ref pblend, dwFlags))
				HRESULT.ThrowLastError();
		}

		[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
		private static extern void _SHAddToRecentDocs_String(SHARD uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

		// This overload is required.  There's a cast in the Shell code that causes the wrong vtbl to be used
		// if we let the marshaller convert the parameter to an IUnknown.
		[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
		private static extern void _SHAddToRecentDocs_ShellLink(SHARD uFlags, IShellLinkW pv);

		/// <summary>
		/// Performs the SHAddToRecentDocs operation.
		/// </summary>
		/// <param name="path">The path parameter.</param>
		public static void SHAddToRecentDocs(string path) => _SHAddToRecentDocs_String(SHARD.PATHW, path);

		// Win7 only.

		/// <summary>
		/// Performs the SHAddToRecentDocs operation.
		/// </summary>
		/// <param name="shellLink">The shellLink parameter.</param>
		public static void SHAddToRecentDocs(IShellLinkW shellLink) => _SHAddToRecentDocs_ShellLink(SHARD.LINK, shellLink);

		// #define DWM_SIT_DISPLAYFRAME    0x00000001  // Display a window frame around the provided bitmap
		[DllImport("dwmapi.dll", EntryPoint = "DwmGetCompositionTimingInfo")]
		private static extern HRESULT _DwmGetCompositionTimingInfo(IntPtr hwnd, ref DWM_TIMING_INFO pTimingInfo);

		/// <summary>
		/// Performs the DwmGetCompositionTimingInfo operation.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <returns>The result.</returns>
		public static DWM_TIMING_INFO? DwmGetCompositionTimingInfo(IntPtr hwnd)
		{
			if (!Utility.IsOSVistaOrNewer) return null; // API was new to Vista.
			var dti = new DWM_TIMING_INFO { cbSize = Marshal.SizeOf(typeof(DWM_TIMING_INFO)) };
			var hr = _DwmGetCompositionTimingInfo(Utility.IsOSWindows8OrNewer ? IntPtr.Zero : hwnd, ref dti);
			if (hr == HRESULT.E_PENDING) return null; // The system isn't yet ready to respond.  Return null rather than throw.
			hr.ThrowIfFailed();
			return dti;
		}

		/// <summary>
		/// Native method wrapper for DwmInvalidateIconicBitmaps.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern void DwmInvalidateIconicBitmaps(IntPtr hwnd);

		/// <summary>
		/// Native method wrapper for DwmSetIconicThumbnail.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="hbmp">The hbmp parameter.</param>
		/// <param name="dwSITFlags">The dwSITFlags parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern void DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbmp, DWM_SIT dwSITFlags);

		/// <summary>
		/// Native method wrapper for DwmSetIconicLivePreviewBitmap.
		/// </summary>
		/// <param name="hwnd">The hwnd parameter.</param>
		/// <param name="hbmp">The hbmp parameter.</param>
		/// <param name="pptClient">The pptClient parameter.</param>
		/// <param name="dwSITFlags">The dwSITFlags parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern void DwmSetIconicLivePreviewBitmap(IntPtr hwnd, IntPtr hbmp, RefPOINT pptClient, DWM_SIT dwSITFlags);

		/// <summary>
		/// Native method wrapper for SHGetItemFromDataObject.
		/// </summary>
		/// <param name="pdtobj">The pdtobj parameter.</param>
		/// <param name="dwFlags">The dwFlags parameter.</param>
		/// <param name="riid">The riid parameter.</param>
		/// <param name="ppv">The ppv parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern void SHGetItemFromDataObject(IDataObject pdtobj, DOGIF dwFlags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

		/// <summary>
		/// Native method wrapper for SHCreateItemFromParsingName.
		/// </summary>
		/// <param name="pszPath">The pszPath parameter.</param>
		/// <param name="pbc">The pbc parameter.</param>
		/// <param name="riid">The riid parameter.</param>
		/// <param name="ppv">The ppv parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern HRESULT SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

		/// <summary>
		/// Native method wrapper for Shell_NotifyIcon.
		/// </summary>
		/// <param name="dwMessage">The dwMessage parameter.</param>
		/// <param name="lpdata">The lpdata parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool Shell_NotifyIcon(NIM dwMessage, [In] NOTIFYICONDATA lpdata);

		/// <summary>
		/// Sets the User Model AppID for the current process, enabling Windows to retrieve this ID
		/// </summary>
		/// <param name="AppID">The AppID parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

		/// <summary>
		/// Retrieves the User Model AppID that has been explicitly set for the current process via SetCurrentProcessExplicitAppUserModelID.
		/// </summary>
		/// <param name="AppID">The AppID parameter.</param>
		/// <returns>The result.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("shell32.dll")]
		public static extern HRESULT GetCurrentProcessExplicitAppUserModelID([Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID);
	}
}