namespace SettingsModel.ExtensionMethods
{
	using System;
	using System.Runtime.InteropServices;
	using System.Security;

	/// <summary>Source: <see href="http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx"/>.</summary>
	internal static class SecureStringExtensionMethod
	{
		public static string ConvertToUnsecureString(this SecureString securePassword)
		{
			if (securePassword == null) throw new ArgumentNullException(nameof(securePassword));

			var unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
			}
		}

		public static SecureString ConvertToSecureString(this string password)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));

			var securePassword = new SecureString();
			foreach (var c in password)
				securePassword.AppendChar(c);

			securePassword.MakeReadOnly();
			return securePassword;
		}
	}
}
