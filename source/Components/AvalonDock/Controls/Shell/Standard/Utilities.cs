/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Security.Cryptography;
	using System.Text;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;

	/// <summary>Represents the Utility class.</summary>
	internal static partial class Utility
	{
		/// <summary>The _osVersion value.</summary>
		private static readonly Version _osVersion = Environment.OSVersion.Version;

		/// <summary>The _presentationFrameworkVersion value.</summary>
		private static readonly Version _presentationFrameworkVersion = Assembly.GetAssembly(typeof(Window)).GetName().Version;

		/// <summary>Performs the _MemCmp operation.</summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <param name="cb">The cb value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static bool _MemCmp(IntPtr left, IntPtr right, long cb)
		{
			var offset = 0;
			for (; offset < cb - sizeof(long); offset += sizeof(long))
			{
				var left64 = Marshal.ReadInt64(left, offset);
				var right64 = Marshal.ReadInt64(right, offset);
				if (left64 != right64) return false;
			}

			for (; offset < cb; offset += sizeof(byte))
			{
				var left8 = Marshal.ReadByte(left, offset);
				var right8 = Marshal.ReadByte(right, offset);
				if (left8 != right8) return false;
			}

			return true;
		}

		/// <summary>Performs the RGB operation.</summary>
		/// <param name="c">The c value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int RGB(Color c) => c.R | (c.G << 8) | (c.B << 16);

		/// <summary>Performs the ColorFromArgbDword operation.</summary>
		/// <param name="color">The color value.</param>
		/// <returns>The result of the operation.</returns>
		public static Color ColorFromArgbDword(uint color) => Color.FromArgb((byte)((color & 0xFF000000) >> 24), (byte)((color & 0x00FF0000) >> 16), (byte)((color & 0x0000FF00) >> 8), (byte)((color & 0x000000FF) >> 0));

		/// <summary>Performs the GET_X_LPARAM operation.</summary>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int GET_X_LPARAM(IntPtr lParam) => LOWORD(lParam.ToInt32());

		/// <summary>Performs the GET_Y_LPARAM operation.</summary>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int GET_Y_LPARAM(IntPtr lParam) => HIWORD(lParam.ToInt32());

		/// <summary>Performs the HIWORD operation.</summary>
		/// <param name="i">The i value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int HIWORD(int i) => (short)(i >> 16);

		/// <summary>Performs the LOWORD operation.</summary>
		/// <param name="i">The i value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int LOWORD(int i) => (short)(i & 0xFFFF);

		/// <summary>Performs the AreStreamsEqual operation.</summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static bool AreStreamsEqual(Stream left, Stream right)
		{
			if (left == null) return right == null;
			if (right == null) return false;
			if (!left.CanRead || !right.CanRead) throw new NotSupportedException("The streams can't be read for comparison");
			if (left.Length != right.Length) return false;

			var length = (int)left.Length;

			// seek to beginning
			left.Position = 0;
			right.Position = 0;

			// total bytes read
			var totalReadLeft = 0;
			var totalReadRight = 0;

			// bytes read on this iteration
			var cbReadLeft = 0;
			var cbReadRight = 0;

			// where to store the read data
			var leftBuffer = new byte[512];
			var rightBuffer = new byte[512];

			// pin the left buffer
			var handleLeft = GCHandle.Alloc(leftBuffer, GCHandleType.Pinned);
			var ptrLeft = handleLeft.AddrOfPinnedObject();

			// pin the right buffer
			var handleRight = GCHandle.Alloc(rightBuffer, GCHandleType.Pinned);
			var ptrRight = handleRight.AddrOfPinnedObject();

			try
			{
				while (totalReadLeft < length)
				{
					Assert.AreEqual(totalReadLeft, totalReadRight);
					cbReadLeft = left.Read(leftBuffer, 0, leftBuffer.Length);
					cbReadRight = right.Read(rightBuffer, 0, rightBuffer.Length);
					// verify the contents are an exact match
					if (cbReadLeft != cbReadRight) return false;
					if (!_MemCmp(ptrLeft, ptrRight, cbReadLeft)) return false;
					totalReadLeft += cbReadLeft;
					totalReadRight += cbReadRight;
				}

				Assert.AreEqual(cbReadLeft, cbReadRight);
				Assert.AreEqual(totalReadLeft, totalReadRight);
				Assert.AreEqual(length, totalReadLeft);
				return true;
			}
			finally
			{
				handleLeft.Free();
				handleRight.Free();
			}
		}

		/// <summary>Performs the GuidTryParse operation.</summary>
		/// <param name="guidString">The guidString value.</param>
		/// <param name="guid">The guid value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool GuidTryParse(string guidString, out Guid guid)
		{
			Verify.IsNeitherNullNorEmpty(guidString, nameof(guidString));
			try
			{
				guid = new Guid(guidString);
				return true;
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}

			// Doesn't seem to be a valid guid.
			guid = default(Guid);
			return false;
		}

		/// <summary>Performs the IsFlagSet operation.</summary>
		/// <param name="value">The value value.</param>
		/// <param name="mask">The mask value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsFlagSet(int value, int mask) => (value & mask) != 0;

		/// <summary>Performs the IsFlagSet operation.</summary>
		/// <param name="value">The value value.</param>
		/// <param name="mask">The mask value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsFlagSet(uint value, uint mask) => (value & mask) != 0;

		/// <summary>Performs the IsFlagSet operation.</summary>
		/// <param name="value">The value value.</param>
		/// <param name="mask">The mask value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsFlagSet(long value, long mask) => (value & mask) != 0;

		/// <summary>Performs the IsFlagSet operation.</summary>
		/// <param name="value">The value value.</param>
		/// <param name="mask">The mask value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsFlagSet(ulong value, ulong mask) => (value & mask) != 0;

		/// <summary>Gets a value indicating whether IsOSVistaOrNewer.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsOSVistaOrNewer => _osVersion >= new Version(6, 0);

		/// <summary>Gets a value indicating whether IsOSWindows7OrNewer.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsOSWindows7OrNewer => _osVersion >= new Version(6, 1);

		/// <summary>Gets a value indicating whether IsOSWindows8OrNewer.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsOSWindows8OrNewer => _osVersion >= new Version(6, 2);

		/// <summary>Gets a value indicating whether IsPresentationFrameworkVersionLessThan4.</summary>
		public static bool IsPresentationFrameworkVersionLessThan4 => _presentationFrameworkVersion < new Version(4, 0);

		// Caller is responsible for destroying the HICON
		// Caller is responsible to ensure that GDI+ has been initialized.

		/// <summary>Performs the GenerateHICON operation.</summary>
		/// <param name="image">The image value.</param>
		/// <param name="dimensions">The dimensions value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static IntPtr GenerateHICON(ImageSource image, Size dimensions)
		{
			if (image == null) return IntPtr.Zero;

			// If we're getting this from a ".ico" resource, then it comes through as a BitmapFrame.
			// We can use leverage this as a shortcut to get the right 16x16 representation
			// because DrawImage doesn't do that for us.
			if (image is BitmapFrame bf)
			{
				bf = GetBestMatch(bf.Decoder.Frames, (int)dimensions.Width, (int)dimensions.Height);
			}
			else
			{
				// Constrain the dimensions based on the aspect ratio.
				var drawingDimensions = new Rect(0, 0, dimensions.Width, dimensions.Height);
				// There's no reason to assume that the requested image dimensions are square.
				var renderRatio = dimensions.Width / dimensions.Height;
				var aspectRatio = image.Width / image.Height;
				// If it's smaller than the requested size, then place it in the middle and pad the image.
				if (image.Width <= dimensions.Width && image.Height <= dimensions.Height)
				{
					drawingDimensions = new Rect((dimensions.Width - image.Width) / 2, (dimensions.Height - image.Height) / 2, image.Width, image.Height);
				}
				else if (renderRatio > aspectRatio)
				{
					var scaledRenderWidth = image.Width / image.Height * dimensions.Width;
					drawingDimensions = new Rect((dimensions.Width - scaledRenderWidth) / 2, 0, scaledRenderWidth, dimensions.Height);
				}
				else if (renderRatio < aspectRatio)
				{
					var scaledRenderHeight = image.Height / image.Width * dimensions.Height;
					drawingDimensions = new Rect(0, (dimensions.Height - scaledRenderHeight) / 2, dimensions.Width, scaledRenderHeight);
				}

				var dv = new DrawingVisual();
				var dc = dv.RenderOpen();
				dc.DrawImage(image, drawingDimensions);
				dc.Close();

				var bmp = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, 96, 96, PixelFormats.Pbgra32);
				bmp.Render(dv);
				bf = BitmapFrame.Create(bmp);
			}

			// Using GDI+ to convert to an HICON.
			// I'd rather not duplicate their code.
			using (var memstm = new MemoryStream())
			{
				BitmapEncoder enc = new PngBitmapEncoder();
				enc.Frames.Add(bf);
				enc.Save(memstm);
				using (var istm = new ManagedIStream(memstm))
				{
					// We are not bubbling out GDI+ errors when creating the native image fails.
					var bitmap = IntPtr.Zero;
					try
					{
						var gpStatus = NativeMethods.GdipCreateBitmapFromStream(istm, out bitmap);
						if (Status.Ok != gpStatus) return IntPtr.Zero;
						gpStatus = NativeMethods.GdipCreateHICONFromBitmap(bitmap, out var hicon);
						return Status.Ok != gpStatus ? IntPtr.Zero : hicon;
						// Caller is responsible for freeing this.
					}
					finally
					{
						Utility.SafeDisposeImage(ref bitmap);
					}
				}
			}
		}

		/// <summary>Performs the GetBestMatch operation.</summary>
		/// <param name="frames">The frames value.</param>
		/// <param name="width">The width value.</param>
		/// <param name="height">The height value.</param>
		/// <returns>The result of the operation.</returns>
		public static BitmapFrame GetBestMatch(IList<BitmapFrame> frames, int width, int height) => _GetBestMatch(frames, _GetBitDepth(), width, height);

		/// <summary>Performs the _MatchImage operation.</summary>
		/// <param name="frame">The frame value.</param>
		/// <param name="bitDepth">The bitDepth value.</param>
		/// <param name="width">The width value.</param>
		/// <param name="height">The height value.</param>
		/// <param name="bpp">The bpp value.</param>
		/// <returns>The result of the operation.</returns>
		private static int _MatchImage(BitmapFrame frame, int bitDepth, int width, int height, int bpp)
		{
			return 2 * _WeightedAbs(bpp, bitDepth, false) +
							_WeightedAbs(frame.PixelWidth, width, true) +
							_WeightedAbs(frame.PixelHeight, height, true);
		}

		/// <summary>Performs the _WeightedAbs operation.</summary>
		/// <param name="valueHave">The valueHave value.</param>
		/// <param name="valueWant">The valueWant value.</param>
		/// <param name="fPunish">The fPunish value.</param>
		/// <returns>The result of the operation.</returns>
		private static int _WeightedAbs(int valueHave, int valueWant, bool fPunish)
		{
			var diff = valueHave - valueWant;
			return diff >= 0 ? diff : (fPunish ? -2 : -1) * diff;
		}

		/// <summary>Performs the _GetBestMatch operation.</summary>
		/// <param name="frames">The frames value.</param>
		/// <param name="bitDepth">The bitDepth value.</param>
		/// <param name="width">The width value.</param>
		/// <param name="height">The height value.</param>
		/// <returns>The result of the operation.</returns>
		private static BitmapFrame _GetBestMatch(IList<BitmapFrame> frames, int bitDepth, int width, int height)
		{
			var bestScore = int.MaxValue;
			var bestBpp = 0;
			var bestIndex = 0;
			var isBitmapIconDecoder = frames[0].Decoder is IconBitmapDecoder;
			for (var i = 0; i < frames.Count && bestScore != 0; ++i)
			{
				var currentIconBitDepth = isBitmapIconDecoder ? frames[i].Thumbnail.Format.BitsPerPixel : frames[i].Format.BitsPerPixel;
				if (currentIconBitDepth == 0) currentIconBitDepth = 8;
				var score = _MatchImage(frames[i], bitDepth, width, height, currentIconBitDepth);
				if (score < bestScore)
				{
					bestIndex = i;
					bestBpp = currentIconBitDepth;
					bestScore = score;
				}
				else if (score == bestScore)
				{
					// Tie breaker: choose the higher color depth.  If that fails, choose first one.
					if (bestBpp >= currentIconBitDepth) continue;
					bestIndex = i;
					bestBpp = currentIconBitDepth;
				}
			}

			return frames[bestIndex];
		}

		// This can be cached.  It's not going to change under reasonable circumstances.

		/// <summary>The s_bitDepth value.</summary>
		private static int s_bitDepth; // = 0;

		/// <summary>Performs the _GetBitDepth operation.</summary>
		/// <returns>The result of the operation.</returns>
		private static int _GetBitDepth()
		{
			if (s_bitDepth != 0) return s_bitDepth;
			using (var dc = SafeDC.GetDesktop())
				s_bitDepth = NativeMethods.GetDeviceCaps(dc, DeviceCap.BITSPIXEL) * NativeMethods.GetDeviceCaps(dc, DeviceCap.PLANES);
			return s_bitDepth;
		}

		/// <summary>Performs the SafeDeleteFile operation.</summary>
		/// <param name="path">The path value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDeleteFile(string path)
		{
			if (!string.IsNullOrEmpty(path)) File.Delete(path);
		}

		/// <summary>Performs the SafeDeleteObject operation.</summary>
		/// <param name="gdiObject">The gdiObject value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDeleteObject(ref IntPtr gdiObject)
		{
			var p = gdiObject;
			gdiObject = IntPtr.Zero;
			if (p != IntPtr.Zero) NativeMethods.DeleteObject(p);
		}

		/// <summary>Performs the SafeDestroyIcon operation.</summary>
		/// <param name="hicon">The hicon value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDestroyIcon(ref IntPtr hicon)
		{
			var p = hicon;
			hicon = IntPtr.Zero;
			if (p != IntPtr.Zero) NativeMethods.DestroyIcon(p);
		}

		/// <summary>Performs the SafeDestroyWindow operation.</summary>
		/// <param name="hwnd">The hwnd value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDestroyWindow(ref IntPtr hwnd)
		{
			var p = hwnd;
			hwnd = IntPtr.Zero;
			if (NativeMethods.IsWindow(p)) NativeMethods.DestroyWindow(p);
		}

		/// <summary>Performs the SafeDispose operation.</summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="disposable">The disposable value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDispose<T>(ref T disposable)
			where T : IDisposable
		{
			// Dispose can safely be called on an object multiple times.
			IDisposable t = disposable;
			disposable = default;
			t?.Dispose();
		}

		/// <summary>Performs the SafeDisposeImage operation.</summary>
		/// <param name="gdipImage">The gdipImage value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void SafeDisposeImage(ref IntPtr gdipImage)
		{
			var p = gdipImage;
			gdipImage = IntPtr.Zero;
			if (p != IntPtr.Zero) NativeMethods.GdipDisposeImage(p);
		}

		/// <summary>Performs the SafeCoTaskMemFree operation.</summary>
		/// <param name="ptr">The ptr value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void SafeCoTaskMemFree(ref IntPtr ptr)
		{
			var p = ptr;
			ptr = IntPtr.Zero;
			if (p != IntPtr.Zero) Marshal.FreeCoTaskMem(p);
		}

		/// <summary>Performs the SafeFreeHGlobal operation.</summary>
		/// <param name="hglobal">The hglobal value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void SafeFreeHGlobal(ref IntPtr hglobal)
		{
			var p = hglobal;
			hglobal = IntPtr.Zero;
			if (p != IntPtr.Zero) Marshal.FreeHGlobal(p);
		}

		/// <summary>Performs the SafeRelease operation.</summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="comObject">The comObject value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void SafeRelease<T>(ref T comObject)
			where T : class
		{
			var t = comObject;
			comObject = default(T);
			if (t == null) return;
			Assert.IsTrue(Marshal.IsComObject(t));
			Marshal.ReleaseComObject(t);
		}

		/// <summary>Performs the GeneratePropertyString operation.</summary>
		/// <param name="source">The source value.</param>
		/// <param name="propertyName">The propertyName value.</param>
		/// <param name="value">The value value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void GeneratePropertyString(StringBuilder source, string propertyName, string value)
		{
			Assert.IsNotNull(source);
			Assert.IsFalse(string.IsNullOrEmpty(propertyName));
			if (source.Length != 0) source.Append(' ');
			source.Append(propertyName);
			source.Append(": ");
			if (string.IsNullOrEmpty(value))
			{
				source.Append("<null>");
			}
			else
			{
				source.Append('\"');
				source.Append(value);
				source.Append('\"');
			}
		}

		/// <summary>Performs the GenerateToString operation.</summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="object">The object value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Obsolete]
		public static string GenerateToString<T>(T @object)
			where T : struct
		{
			var sbRet = new StringBuilder();
			foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (sbRet.Length != 0) sbRet.Append(", ");
				Assert.AreEqual(0, property.GetIndexParameters().Length);
				var value = property.GetValue(@object, null);
				var format = null == value ? "{0}: <null>" : "{0}: \"{1}\"";
				sbRet.AppendFormat(format, property.Name, value);
			}

			return sbRet.ToString();
		}

		/// <summary>Performs the CopyStream operation.</summary>
		/// <param name="destination">The destination value.</param>
		/// <param name="source">The source value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void CopyStream(Stream destination, Stream source)
		{
			Assert.IsNotNull(source);
			Assert.IsNotNull(destination);
			destination.Position = 0;
			// If we're copying from, say, a web stream, don't fail because of this.
			if (source.CanSeek)
			{
				source.Position = 0;
				// Consider that this could throw because
				// the source stream doesn't know it's size...
				destination.SetLength(source.Length);
			}

			var buffer = new byte[4096];
			int cbRead;
			do
			{
				cbRead = source.Read(buffer, 0, buffer.Length);
				if (cbRead != 0) destination.Write(buffer, 0, cbRead);
			}
			while (buffer.Length == cbRead);

			// Reset the Seek pointer before returning.
			destination.Position = 0;
		}

		/// <summary>Performs the HashStreamMD5 operation.</summary>
		/// <param name="stm">The stm value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static string HashStreamMD5(Stream stm)
		{
			stm.Position = 0;
			var hashBuilder = new StringBuilder();
			using (var md5 = MD5.Create())
			{
				foreach (var b in md5.ComputeHash(stm)) hashBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
				return hashBuilder.ToString();
			}
		}

		/// <summary>Performs the EnsureDirectory operation.</summary>
		/// <param name="path">The path value.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void EnsureDirectory(string path)
		{
			if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
		}

		/// <summary>Performs the MemCmp operation.</summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <param name="cb">The cb value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool MemCmp(byte[] left, byte[] right, int cb)
		{
			Assert.IsNotNull(left);
			Assert.IsNotNull(right);
			Assert.IsTrue(cb <= Math.Min(left.Length, right.Length));

			// pin this buffer
			var handleLeft = GCHandle.Alloc(left, GCHandleType.Pinned);
			var ptrLeft = handleLeft.AddrOfPinnedObject();
			// pin the other buffer
			var handleRight = GCHandle.Alloc(right, GCHandleType.Pinned);
			var ptrRight = handleRight.AddrOfPinnedObject();
			var fRet = _MemCmp(ptrLeft, ptrRight, cb);
			handleLeft.Free();
			handleRight.Free();
			return fRet;
		}

		/// <summary>Represents the _UrlDecoder class.</summary>
		private class _UrlDecoder
		{
			/// <summary>The _encoding value.</summary>
			private readonly Encoding _encoding;

			/// <summary>The _charBuffer value.</summary>
			private readonly char[] _charBuffer;

			/// <summary>The _byteBuffer value.</summary>
			private readonly byte[] _byteBuffer;

			/// <summary>The _byteCount value.</summary>
			private int _byteCount;

			/// <summary>The _charCount value.</summary>
			private int _charCount;

			/// <summary>Initializes a new instance of the <see cref="_UrlDecoder"/> class.</summary>
			/// <param name="size">The size value.</param>
			/// <param name="encoding">The encoding value.</param>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public _UrlDecoder(int size, Encoding encoding)
			{
				_encoding = encoding;
				_charBuffer = new char[size];
				_byteBuffer = new byte[size];
			}

			/// <summary>Performs the AddByte operation.</summary>
			/// <param name="b">The b value.</param>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public void AddByte(byte b) => _byteBuffer[_byteCount++] = b;

			/// <summary>Performs the AddChar operation.</summary>
			/// <param name="ch">The ch value.</param>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public void AddChar(char ch)
			{
				_FlushBytes();
				_charBuffer[_charCount++] = ch;
			}

			/// <summary>Performs the _FlushBytes operation.</summary>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			private void _FlushBytes()
			{
				if (_byteCount <= 0) return;
				_charCount += _encoding.GetChars(_byteBuffer, 0, _byteCount, _charBuffer, _charCount);
				_byteCount = 0;
			}

			/// <summary>Performs the GetString operation.</summary>
			/// <returns>The result of the operation.</returns>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public string GetString()
			{
				_FlushBytes();
				return _charCount > 0 ? new string(_charBuffer, 0, _charCount) : string.Empty;
			}
		}

		/// <summary>Performs the UrlDecode operation.</summary>
		/// <param name="url">The url value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static string UrlDecode(string url)
		{
			if (url == null) return null;

			var decoder = new _UrlDecoder(url.Length, Encoding.UTF8);
			var length = url.Length;
			for (var i = 0; i < length; ++i)
			{
				var ch = url[i];
				if (ch == '+')
				{
					decoder.AddByte((byte)' ');
					continue;
				}

				if (ch == '%' && i < length - 2)
				{
					// decode %uXXXX into a Unicode character.
					if (url[i + 1] == 'u' && i < length - 5)
					{
						var a = _HexToInt(url[i + 2]);
						var b = _HexToInt(url[i + 3]);
						var c = _HexToInt(url[i + 4]);
						var d = _HexToInt(url[i + 5]);
						if (a >= 0 && b >= 0 && c >= 0 && d >= 0)
						{
							decoder.AddChar((char)((a << 12) | (b << 8) | (c << 4) | d));
							i += 5;

							continue;
						}
					}
					else
					{
						// decode %XX into a Unicode character.
						var a = _HexToInt(url[i + 1]);
						var b = _HexToInt(url[i + 2]);

						if (a >= 0 && b >= 0)
						{
							decoder.AddByte((byte)((a << 4) | b));
							i += 2;

							continue;
						}
					}
				}

				// Add any 7bit character as a byte.
				if ((ch & 0xFF80) == 0)
					decoder.AddByte((byte)ch);
				else
					decoder.AddChar(ch);
			}

			return decoder.GetString();
		}

		/// <summary>Performs the UrlEncode operation.</summary>
		/// <param name="url">The url value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static string UrlEncode(string url)
		{
			if (url == null) return null;
			var bytes = Encoding.UTF8.GetBytes(url);
			var needsEncoding = false;
			var unsafeCharCount = 0;
			foreach (var b in bytes)
			{
				if (b == ' ')
				{
					needsEncoding = true;
				}
				else if (!_UrlEncodeIsSafe(b))
				{
					++unsafeCharCount;
					needsEncoding = true;
				}
			}

			if (!needsEncoding) return Encoding.ASCII.GetString(bytes);
			var buffer = new byte[bytes.Length + unsafeCharCount * 2];
			var writeIndex = 0;
			foreach (var b in bytes)
			{
				if (_UrlEncodeIsSafe(b))
				{
					buffer[writeIndex++] = b;
				}
				else if (b == ' ')
				{
					buffer[writeIndex++] = (byte)'+';
				}
				else
				{
					buffer[writeIndex++] = (byte)'%';
					buffer[writeIndex++] = _IntToHex((b >> 4) & 0xF);
					buffer[writeIndex++] = _IntToHex(b & 0xF);
				}
			}

			bytes = buffer;
			Assert.AreEqual(buffer.Length, writeIndex);
			return Encoding.ASCII.GetString(bytes);
		}

		// HttpUtility's UrlEncode is slightly different from the RFC.
		// RFC2396 describes unreserved characters as alphanumeric or
		// the list "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
		// The System.Web version unnecessarily escapes '~', which should be okay...
		// Keeping that same pattern here just to be consistent.

		/// <summary>Performs the _UrlEncodeIsSafe operation.</summary>
		/// <param name="b">The b value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static bool _UrlEncodeIsSafe(byte b)
		{
			if (_IsAsciiAlphaNumeric(b)) return true;

			switch ((char)b)
			{
				case '-':
				case '_':
				case '.':
				case '!':
				// case '~':
				case '*':
				case '\'':
				case '(':
				case ')':
					return true;

				default: return false;
			}
		}

		/// <summary>Performs the _IsAsciiAlphaNumeric operation.</summary>
		/// <param name="b">The b value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static bool _IsAsciiAlphaNumeric(byte b) => b >= 'a' && b <= 'z' || b >= 'A' && b <= 'Z' || b >= '0' && b <= '9';

		/// <summary>Performs the _IntToHex operation.</summary>
		/// <param name="n">The n value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static byte _IntToHex(int n)
		{
			Assert.BoundedInteger(0, n, 16);
			return n <= 9 ? (byte)(n + '0') : (byte)(n - 10 + 'A');
		}

		/// <summary>Performs the _HexToInt operation.</summary>
		/// <param name="h">The h value.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static int _HexToInt(char h)
		{
			if (h >= '0' && h <= '9') return h - '0';
			if (h >= 'a' && h <= 'f') return h - 'a' + 10;
			if (h >= 'A' && h <= 'F') return h - 'A' + 10;
			Assert.Fail("Invalid hex character " + h);
			return -1;
		}

		/// <summary>Performs the AddDependencyPropertyChangeListener operation.</summary>
		/// <param name="component">The component value.</param>
		/// <param name="property">The property value.</param>
		/// <param name="listener">The listener value.</param>
		public static void AddDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
		{
			if (component == null) return;
			Assert.IsNotNull(property);
			Assert.IsNotNull(listener);
			var dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
			dpd.AddValueChanged(component, listener);
		}

		/// <summary>Performs the RemoveDependencyPropertyChangeListener operation.</summary>
		/// <param name="component">The component value.</param>
		/// <param name="property">The property value.</param>
		/// <param name="listener">The listener value.</param>
		public static void RemoveDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
		{
			if (component == null) return;
			Assert.IsNotNull(property);
			Assert.IsNotNull(listener);
			var dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
			dpd.RemoveValueChanged(component, listener);
		}

		/// <summary>Performs the IsThicknessNonNegative operation.</summary>
		/// <param name="thickness">The thickness value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool IsThicknessNonNegative(Thickness thickness)
		{
			if (!IsDoubleFiniteAndNonNegative(thickness.Top)) return false;
			if (!IsDoubleFiniteAndNonNegative(thickness.Left)) return false;
			if (!IsDoubleFiniteAndNonNegative(thickness.Bottom)) return false;
			if (!IsDoubleFiniteAndNonNegative(thickness.Right)) return false;
			return true;
		}

		/// <summary>Performs the IsCornerRadiusValid operation.</summary>
		/// <param name="cornerRadius">The cornerRadius value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool IsCornerRadiusValid(CornerRadius cornerRadius)
		{
			if (!IsDoubleFiniteAndNonNegative(cornerRadius.TopLeft)) return false;
			if (!IsDoubleFiniteAndNonNegative(cornerRadius.TopRight)) return false;
			if (!IsDoubleFiniteAndNonNegative(cornerRadius.BottomLeft)) return false;
			if (!IsDoubleFiniteAndNonNegative(cornerRadius.BottomRight)) return false;
			return true;
		}

		/// <summary>Performs the IsDoubleFiniteAndNonNegative operation.</summary>
		/// <param name="d">The d value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool IsDoubleFiniteAndNonNegative(double d) => !double.IsNaN(d) && !double.IsInfinity(d) && !(d < 0);
	}
}