/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Reflection;
	using System.Runtime.InteropServices;

	/// <summary>Represents the Win32Error structure.</summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct Win32Error
	{
		/// <summary>The _value value.</summary>
		[FieldOffset(0)]
		private readonly int _value;

		// NOTE: These public static field declarations are automatically
		// picked up by (HRESULT's) ToString through reflection.

		/// <summary>The ERROR_SUCCESS value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_SUCCESS = new Win32Error(0);

		/// <summary>The ERROR_INVALID_FUNCTION value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_INVALID_FUNCTION = new Win32Error(1);

		/// <summary>The ERROR_FILE_NOT_FOUND value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_FILE_NOT_FOUND = new Win32Error(2);

		/// <summary>The ERROR_PATH_NOT_FOUND value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_PATH_NOT_FOUND = new Win32Error(3);

		/// <summary>The ERROR_TOO_MANY_OPEN_FILES value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_TOO_MANY_OPEN_FILES = new Win32Error(4);

		/// <summary>The ERROR_ACCESS_DENIED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_ACCESS_DENIED = new Win32Error(5);

		/// <summary>The ERROR_INVALID_HANDLE value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_INVALID_HANDLE = new Win32Error(6);

		/// <summary>The ERROR_OUTOFMEMORY value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_OUTOFMEMORY = new Win32Error(14);

		/// <summary>The ERROR_NO_MORE_FILES value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_NO_MORE_FILES = new Win32Error(18);

		/// <summary>The ERROR_SHARING_VIOLATION value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_SHARING_VIOLATION = new Win32Error(32);

		/// <summary>The ERROR_INVALID_PARAMETER value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_INVALID_PARAMETER = new Win32Error(87);

		/// <summary>The ERROR_INSUFFICIENT_BUFFER value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_INSUFFICIENT_BUFFER = new Win32Error(122);

		/// <summary>The ERROR_NESTING_NOT_ALLOWED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_NESTING_NOT_ALLOWED = new Win32Error(215);

		/// <summary>The ERROR_KEY_DELETED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_KEY_DELETED = new Win32Error(1018);

		/// <summary>The ERROR_NOT_FOUND value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_NOT_FOUND = new Win32Error(1168);

		/// <summary>The ERROR_NO_MATCH value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_NO_MATCH = new Win32Error(1169);

		/// <summary>The ERROR_BAD_DEVICE value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_BAD_DEVICE = new Win32Error(1200);

		/// <summary>The ERROR_CANCELLED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_CANCELLED = new Win32Error(1223);

		/// <summary>The ERROR_CLASS_ALREADY_EXISTS value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_CLASS_ALREADY_EXISTS = new Win32Error(1410);

		/// <summary>The ERROR_INVALID_DATATYPE value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Win32Error ERROR_INVALID_DATATYPE = new Win32Error(1804);

		/// <summary>Initializes a new instance of the <see cref="Win32Error"/> struct.</summary>
		/// <param name="i">The i value.</param>
		public Win32Error(int i) => _value = i;

		/// <summary>Converts the value.</summary>
		/// <param name="error">The error value.</param>
		/// <returns>The result of the operation.</returns>
		public static explicit operator HRESULT(Win32Error error)
		{
			// #define __HRESULT_FROM_WIN32(x)
			//     ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))
			return error._value <= 0 ? new HRESULT((uint)error._value) : HRESULT.Make(true, Facility.Win32, error._value & 0x0000FFFF);
		}

		// Method version of the cast operation

		/// <summary>Performs the ToHRESULT operation.</summary>
		/// <returns>The result of the operation.</returns>
		public HRESULT ToHRESULT() => (HRESULT)this;

		/// <summary>Performs the GetLastError operation.</summary>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static Win32Error GetLastError() => new Win32Error(Marshal.GetLastWin32Error());

		/// <summary>Performs the Equals operation.</summary>
		/// <param name="obj">The obj value.</param>
		/// <returns>The result of the operation.</returns>
		public override bool Equals(object obj)
		{
			try
			{
				return ((Win32Error)obj)._value == _value;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		/// <summary>Performs the GetHashCode operation.</summary>
		/// <returns>The result of the operation.</returns>
		public override int GetHashCode() => _value.GetHashCode();

		/// <summary>Determines whether two values are equal.</summary>
		/// <param name="errLeft">The errLeft value.</param>
		/// <param name="errRight">The errRight value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool operator ==(Win32Error errLeft, Win32Error errRight)
		{
			return errLeft._value == errRight._value;
		}

		/// <summary>Determines whether two values are not equal.</summary>
		/// <param name="errLeft">The errLeft value.</param>
		/// <param name="errRight">The errRight value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool operator !=(Win32Error errLeft, Win32Error errRight) => errLeft != errRight;
	}

	/// <summary>Defines Facility values.</summary>
	internal enum Facility
	{
		/// <summary>The Null value.</summary>
		Null = 0,

		/// <summary>The Rpc value.</summary>
		Rpc = 1,

		/// <summary>The Dispatch value.</summary>
		Dispatch = 2,

		/// <summary>The Storage value.</summary>
		Storage = 3,

		/// <summary>The Itf value.</summary>
		Itf = 4,

		/// <summary>The Win32 value.</summary>
		Win32 = 7,

		/// <summary>The Windows value.</summary>
		Windows = 8,

		/// <summary>The Control value.</summary>
		Control = 10,

		/// <summary>The Ese value.</summary>
		Ese = 0xE5E,

		/// <summary>The WinCodec value.</summary>
		WinCodec = 0x898,
	}

	/// <summary>Represents the HRESULT structure.</summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct HRESULT
	{
		/// <summary>The _value value.</summary>
		[FieldOffset(0)]
		private readonly uint _value;

		// NOTE: These public static field declarations are automatically
		// picked up by ToString through reflection.

		/// <summary>The S_OK value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT S_OK = new HRESULT(0x00000000);

		/// <summary>The S_FALSE value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT S_FALSE = new HRESULT(0x00000001);

		/// <summary>The E_PENDING value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_PENDING = new HRESULT(0x8000000A);

		/// <summary>The E_NOTIMPL value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_NOTIMPL = new HRESULT(0x80004001);

		/// <summary>The E_NOINTERFACE value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_NOINTERFACE = new HRESULT(0x80004002);

		/// <summary>The E_POINTER value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_POINTER = new HRESULT(0x80004003);

		/// <summary>The E_ABORT value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_ABORT = new HRESULT(0x80004004);

		/// <summary>The E_FAIL value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_FAIL = new HRESULT(0x80004005);

		/// <summary>The E_UNEXPECTED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_UNEXPECTED = new HRESULT(0x8000FFFF);

		/// <summary>The STG_E_INVALIDFUNCTION value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT STG_E_INVALIDFUNCTION = new HRESULT(0x80030001);

		/// <summary>The REGDB_E_CLASSNOTREG value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT REGDB_E_CLASSNOTREG = new HRESULT(0x80040154);

		/// <summary>The DESTS_E_NO_MATCHING_ASSOC_HANDLER value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT DESTS_E_NO_MATCHING_ASSOC_HANDLER = new HRESULT(0x80040F03);

		/// <summary>The DESTS_E_NORECDOCS value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT DESTS_E_NORECDOCS = new HRESULT(0x80040F04);

		/// <summary>The DESTS_E_NOTALLCLEARED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT DESTS_E_NOTALLCLEARED = new HRESULT(0x80040F05);

		/// <summary>The E_ACCESSDENIED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_ACCESSDENIED = new HRESULT(0x80070005);

		/// <summary>The E_OUTOFMEMORY value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_OUTOFMEMORY = new HRESULT(0x8007000E);

		/// <summary>The E_INVALIDARG value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT E_INVALIDARG = new HRESULT(0x80070057);

		/// <summary>The INTSAFE_E_ARITHMETIC_OVERFLOW value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT INTSAFE_E_ARITHMETIC_OVERFLOW = new HRESULT(0x80070216);

		/// <summary>The COR_E_OBJECTDISPOSED value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT COR_E_OBJECTDISPOSED = new HRESULT(0x80131622);

		/// <summary>The WC_E_GREATERTHAN value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT WC_E_GREATERTHAN = new HRESULT(0xC00CEE23);

		/// <summary>The WC_E_SYNTAX value.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly HRESULT WC_E_SYNTAX = new HRESULT(0xC00CEE2D);

		/// <summary>Initializes a new instance of the <see cref="HRESULT"/> struct.</summary>
		/// <param name="i">The i value.</param>
		public HRESULT(uint i) => _value = i;

		/// <summary>Performs the Make operation.</summary>
		/// <param name="severe">The severe value.</param>
		/// <param name="facility">The facility value.</param>
		/// <param name="code">The code value.</param>
		/// <returns>The result of the operation.</returns>
		public static HRESULT Make(bool severe, Facility facility, int code)
		{
			// #define MAKE_HRESULT(sev,fac,code) \
			//    ((HRESULT) (((unsigned long)(sev)<<31) | ((unsigned long)(fac)<<16) | ((unsigned long)(code))) )

			// Severity has 1 bit reserved.
			// bitness is enforced by the boolean parameter.

			// Facility has 11 bits reserved (different than SCODES, which have 4 bits reserved)
			// MSDN documentation incorrectly uses 12 bits for the ESE facility (e5e), so go ahead and let that one slide.
			// And WIC also ignores it the documented size...
			Assert.Implies((int)facility != (int)((int)facility & 0x1FF), facility == Facility.Ese || facility == Facility.WinCodec);
			// Code has 4 bits reserved.
			Assert.AreEqual(code, code & 0xFFFF);

			return new HRESULT((uint)((severe ? (1 << 31) : 0) | ((int)facility << 16) | code));
		}

		/// <summary>Gets the Facility value.</summary>
		public Facility Facility => GetFacility((int)_value);

		/// <summary>Performs the GetFacility operation.</summary>
		/// <param name="errorCode">The errorCode value.</param>
		/// <returns>The result of the operation.</returns>
		public static Facility GetFacility(int errorCode)
		{
			// #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
			return (Facility)((errorCode >> 16) & 0x1fff);
		}

		/// <summary>Gets the Code value.</summary>
		public int Code => GetCode((int)_value);

		/// <summary>Performs the GetCode operation.</summary>
		/// <param name="error">The error value.</param>
		/// <returns>The result of the operation.</returns>
		public static int GetCode(int error)
		{
			// #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
			return (int)(error & 0xFFFF);
		}

		/// <summary>Performs the ToString operation.</summary>
		/// <returns>The result of the operation.</returns>
		public override string ToString()
		{
			// Use reflection to try to name this HRESULT.
			// This is expensive, but if someone's ever printing HRESULT strings then
			// I think it's a fair guess that they're not in a performance critical area
			// (e.g. printing exception strings).
			// This is less error prone than trying to keep the list in the function.
			// To properly add an HRESULT's name to the ToString table, just add the HRESULT
			// like all the others above.
			//
			// CONSIDER: This data is static.  It could be cached
			// after first usage for fast lookup since the keys are unique.
			foreach (var publicStaticField in typeof(HRESULT).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (publicStaticField.FieldType == typeof(HRESULT))
				{
					var hr = (HRESULT)publicStaticField.GetValue(null);
					if (hr == this)
						return publicStaticField.Name;
				}
			}

			// Try Win32 error codes also
			if (Facility == Facility.Win32)
			{
				foreach (var publicStaticField in typeof(Win32Error).GetFields(BindingFlags.Static | BindingFlags.Public))
				{
					if (publicStaticField.FieldType == typeof(Win32Error))
					{
						var error = (Win32Error)publicStaticField.GetValue(null);
						if ((HRESULT)error == this)
							return "HRESULT_FROM_WIN32(" + publicStaticField.Name + ")";
					}
				}
			}

			// If there's no good name for this HRESULT,
			// return the string as readable hex (0x########) format.
			return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", _value);
		}

		/// <summary>Performs the Equals operation.</summary>
		/// <param name="obj">The obj value.</param>
		/// <returns>The result of the operation.</returns>
		public override bool Equals(object obj)
		{
			try
			{
				return ((HRESULT)obj)._value == _value;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		/// <summary>Performs the GetHashCode operation.</summary>
		/// <returns>The result of the operation.</returns>
		public override int GetHashCode() => _value.GetHashCode();

		/// <summary>Determines whether two values are equal.</summary>
		/// <param name="hrLeft">The hrLeft value.</param>
		/// <param name="hrRight">The hrRight value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool operator ==(HRESULT hrLeft, HRESULT hrRight) => hrLeft._value == hrRight._value;

		/// <summary>Determines whether two values are not equal.</summary>
		/// <param name="hrLeft">The hrLeft value.</param>
		/// <param name="hrRight">The hrRight value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool operator !=(HRESULT hrLeft, HRESULT hrRight) => !(hrLeft == hrRight);

		/// <summary>Gets a value indicating whether Succeeded.</summary>
		public bool Succeeded => (int)_value >= 0;

		/// <summary>Gets a value indicating whether Failed.</summary>
		public bool Failed => (int)_value < 0;

		/// <summary>Performs the ThrowIfFailed operation.</summary>
		public void ThrowIfFailed() => ThrowIfFailed(null);

		/// <summary>Performs the ThrowIfFailed operation.</summary>
		/// <param name="message">The message value.</param>
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Only recreating Exceptions that were already raised.")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void ThrowIfFailed(string message)
		{
			if (!Failed) return;
			if (string.IsNullOrEmpty(message))
				message = ToString();
#if DEBUG
			else
				message += " (" + ToString() + ")";
#endif
			// Wow.  Reflection in a throw call.  Later on this may turn out to have been a bad idea.
			// If you're throwing an exception I assume it's OK for me to take some time to give it back.
			// I want to convert the HRESULT to a more appropriate exception type than COMException.
			// Marshal.ThrowExceptionForHR does this for me, but the general call uses GetErrorInfo
			// if it's set, and then ignores the HRESULT that I've provided.  This makes it so this
			// call works the first time but you get burned on the second.  To avoid this, I use
			// the overload that explicitly ignores the IErrorInfo.
			// In addition, the function doesn't allow me to set the Message unless I go through
			// the process of implementing an IErrorInfo and then use that.  There's no stock
			// implementations of IErrorInfo available and I don't think it's worth the maintenance
			// overhead of doing it, nor would it have significant value over this approach.
			var e = Marshal.GetExceptionForHR((int)_value, new IntPtr(-1));
			Assert.IsNotNull(e);
			// ArgumentNullException doesn't have the right constructor parameters,
			// (nor does Win32Exception...)
			// but E_POINTER gets mapped to NullReferenceException,
			// so I don't think it will ever matter.
			Assert.IsFalse(e is ArgumentNullException);

			// If we're not getting anything better than a COMException from Marshal,
			// then at least check the facility and attempt to do better ourselves.
			if (e.GetType() == typeof(COMException))
			{
				switch (Facility)
				{
					case Facility.Win32: e = new Win32Exception(Code, message); break;
					default: e = new COMException(message, (int)_value); break;
				}
			}
			else
			{
				var cons = e.GetType().GetConstructor(new[] { typeof(string) });
				if (null != cons)
				{
					e = cons.Invoke(new object[] { message }) as Exception;
					Assert.IsNotNull(e);
				}
			}

			throw e;
		}

		/// <summary>Performs the ThrowLastError operation.</summary>
		public static void ThrowLastError()
		{
			((HRESULT)Win32Error.GetLastError()).ThrowIfFailed();
			// Only expecting to call this when we're expecting a failed GetLastError()
			Assert.Fail();
		}
	}
}