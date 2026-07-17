/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;

	// disambiguate with System.Runtime.InteropServices.STATSTG
	using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

	// All these methods return void.  Does the standard marshaller convert them to HRESULTs?

	/// <summary>
	/// Represents the managed I Stream.
	/// </summary>
	internal sealed class ManagedIStream : IStream, IDisposable
	{
		/// <summary>
		/// The sTGTY STREAM field.
		/// </summary>
		private const int STGTY_STREAM = 2;

		/// <summary>
		/// The sTGM READWRITE field.
		/// </summary>
		private const int STGM_READWRITE = 2;

		/// <summary>
		/// The lOCK EXCLUSIVE field.
		/// </summary>
		private const int LOCK_EXCLUSIVE = 2;

		/// <summary>
		/// The source field.
		/// </summary>
		private Stream _source;

		/// <summary>
		/// Initializes a new instance of the <see cref="ManagedIStream"/> class.
		/// </summary>
		/// <param name="source">The source.</param>
		public ManagedIStream(Stream source)
		{
			Verify.IsNotNull(source, nameof(source));
			_source = source;
		}

		/// <summary>
		/// Executes the validate operation.
		/// </summary>
		private void _Validate()
		{
			if (_source == null) throw new ObjectDisposedException("this");
		}

		// Comments are taken from MSDN IStream documentation.

		/// <summary>
		/// Executes the clone operation.
		/// </summary>
		/// <param name="ppstm">The ppstm.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
		[Obsolete("The method is not implemented", true)]
		public void Clone(out IStream ppstm)
		{
			ppstm = null;
			HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
		}

		/// <summary>
		/// Executes the commit operation.
		/// </summary>
		/// <param name="grfCommitFlags">The grf Commit Flags.</param>
		public void Commit(int grfCommitFlags)
		{
			_Validate();
			_source.Flush();
		}

		/// <summary>
		/// Executes the copy To operation.
		/// </summary>
		/// <param name="pstm">The pstm.</param>
		/// <param name="cb">The cb.</param>
		/// <param name="pcbRead">The pcb Read.</param>
		/// <param name="pcbWritten">The pcb Written.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
		{
			Verify.IsNotNull(pstm, nameof(pstm));
			_Validate();
			// Reasonably sized buffer, don't try to copy large streams in bulk.
			var buffer = new byte[4096];
			long cbWritten = 0;

			while (cbWritten < cb)
			{
				var cbRead = _source.Read(buffer, 0, buffer.Length);
				if (cbRead == 0) break;
				// COM documentation is a bit vague here whether NULL is valid for the third parameter.
				// Going to assume it is, as most implementations I've seen treat it as optional.
				// It's possible this will break on some IStream implementations.
				pstm.Write(buffer, cbRead, IntPtr.Zero);
				cbWritten += cbRead;
			}

			if (pcbRead != IntPtr.Zero) Marshal.WriteInt64(pcbRead, cbWritten);
			if (pcbWritten != IntPtr.Zero) Marshal.WriteInt64(pcbWritten, cbWritten);
		}

		/// <summary>
		/// Executes the lock Region operation.
		/// </summary>
		/// <param name="libOffset">The lib Offset.</param>
		/// <param name="cb">The cb.</param>
		/// <param name="dwLockType">The dw Lock Type.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
		[Obsolete("The method is not implemented", true)]
		public void LockRegion(long libOffset, long cb, int dwLockType) => HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");

		/// <summary>
		/// Executes the read operation.
		/// </summary>
		/// <param name="pv">The pv.</param>
		/// <param name="cb">The cb.</param>
		/// <param name="pcbRead">The pcb Read.</param>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void Read(byte[] pv, int cb, IntPtr pcbRead)
		{
			_Validate();
			var cbRead = _source.Read(pv, 0, cb);
			if (pcbRead != IntPtr.Zero) Marshal.WriteInt32(pcbRead, cbRead);
		}

		/// <summary>
		/// Executes the revert operation.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
		[Obsolete("The method is not implemented", true)]
		public void Revert() => HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");

		/// <summary>
		/// Executes the seek operation.
		/// </summary>
		/// <param name="dlibMove">The dlib Move.</param>
		/// <param name="dwOrigin">The dw Origin.</param>
		/// <param name="plibNewPosition">The plib New Position.</param>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
		{
			_Validate();
			var position = _source.Seek(dlibMove, (SeekOrigin)dwOrigin);
			if (plibNewPosition != IntPtr.Zero) Marshal.WriteInt64(plibNewPosition, position);
		}

		/// <summary>
		/// Sets the set Size.
		/// </summary>
		/// <param name="libNewSize">The lib New Size.</param>
		public void SetSize(long libNewSize)
		{
			_Validate();
			_source.SetLength(libNewSize);
		}

		/// <summary>
		/// Executes the stat operation.
		/// </summary>
		/// <param name="pstatstg">The pstatstg.</param>
		/// <param name="grfStatFlag">The grf Stat Flag.</param>
		public void Stat(out STATSTG pstatstg, int grfStatFlag)
		{
			pstatstg = default;
			_Validate();
			pstatstg.type = STGTY_STREAM;
			pstatstg.cbSize = _source.Length;
			pstatstg.grfMode = STGM_READWRITE;
			pstatstg.grfLocksSupported = LOCK_EXCLUSIVE;
		}

		/// <summary>
		/// Executes the unlock Region operation.
		/// </summary>
		/// <param name="libOffset">The lib Offset.</param>
		/// <param name="cb">The cb.</param>
		/// <param name="dwLockType">The dw Lock Type.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
		[Obsolete("The method is not implemented", true)]
		public void UnlockRegion(long libOffset, long cb, int dwLockType) => HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");

		/// <summary>
		/// Executes the write operation.
		/// </summary>
		/// <param name="pv">The pv.</param>
		/// <param name="cb">The cb.</param>
		/// <param name="pcbWritten">The pcb Written.</param>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void Write(byte[] pv, int cb, IntPtr pcbWritten)
		{
			_Validate();
			_source.Write(pv, 0, cb);
			if (pcbWritten != IntPtr.Zero) Marshal.WriteInt32(pcbWritten, cb);
		}

		/// <summary>
		/// Executes the dispose operation.
		/// </summary>
		public void Dispose() => _source = null;
	}
}