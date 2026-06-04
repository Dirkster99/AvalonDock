using System;
using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Loads embedded .vstheme (or GZIP-compressed .vstheme.gz) resources from an
	/// assembly manifest as raw byte arrays, so they can be handed to
	/// <see cref="VsThemeParser"/> for parsing.
	/// </summary>
	public static class VsThemeResourceLoader
	{
		/// <summary>
		/// Reads an embedded resource from the given assembly into a byte array.
		/// </summary>
		/// <param name="assembly">The assembly that contains the embedded resource.</param>
		/// <param name="manifestResourceName">The fully qualified manifest resource name.</param>
		/// <returns>The raw bytes of the embedded resource.</returns>
		/// <exception cref="FileNotFoundException">Thrown when the resource cannot be found in the assembly.</exception>
		public static byte[] Load(Assembly assembly, string manifestResourceName)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (string.IsNullOrEmpty(manifestResourceName))
			{
				throw new ArgumentNullException(nameof(manifestResourceName));
			}

			using (var stream = assembly.GetManifestResourceStream(manifestResourceName))
			{
				if (stream == null)
				{
					throw new FileNotFoundException(
						$"Embedded resource '{manifestResourceName}' was not found in assembly '{assembly.GetName().Name}'.");
				}

				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}
	}
}