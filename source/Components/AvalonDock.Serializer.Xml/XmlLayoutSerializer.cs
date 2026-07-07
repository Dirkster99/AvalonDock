using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization.Dto;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// XML implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup (reconnecting content, previous containers, callbacks).
	/// </summary>
	public class XmlLayoutSerializer : LayoutSerializerBase
	{
		private static readonly XmlSerializer DtoSerializer = new XmlSerializer(typeof(LayoutRootDto));

		/// <summary>
		/// Matches capitalised <c>True</c>/<c>False</c> values of the boolean layout
		/// attributes as written by AvalonDock v4 and earlier, which
		/// <see cref="XmlSerializer"/> rejects (xsd:boolean is lower-case). Only these
		/// named attributes are matched so string values such as <c>Title</c> or
		/// <c>ContentId</c> are never altered. The list is frozen together with the
		/// legacy format: attributes added in v5 or later can never occur in legacy
		/// files, so it does not need updating when DTO properties are added.
		/// </summary>
		private static readonly Regex LegacyBooleanRegex = new Regex(
			"\\b(IsSelected|IsLastFocusedDocument|IsMaximized|CanClose|CanFloat|CanShowOnHover"
			+ "|CanHide|CanAutoHide|CanDockAsTabbedDocument|CanDock|CanMove|ShowHeader)"
			+ "=\"(True|False)\"",
			RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		public XmlLayoutSerializer(IDockingManager manager)
			: base(manager)
		{
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, LayoutRootDto dto)
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);
			DtoSerializer.Serialize(stream, dto, ns);
		}

		/// <inheritdoc/>
		protected override LayoutRootDto DeserializeCore(Stream stream)
		{
			// Decode the stream ourselves (honouring a BOM when present) and deserialize
			// from the resulting string. Reading from already-decoded text makes the XML
			// prolog's encoding declaration irrelevant, so legacy layouts persisted with a
			// mismatched declaration (e.g. encoding="utf-16" on UTF-8 bytes) load instead
			// of failing with "There is no Unicode byte order mark".
			string xml;
			using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
				xml = reader.ReadToEnd();

			// Legacy layouts store booleans as "True"/"False"; normalise to xsd:boolean.
			xml = LegacyBooleanRegex.Replace(xml,
				m => m.Groups[1].Value + "=\"" + m.Groups[2].Value.ToLowerInvariant() + "\"");

			using var textReader = new StringReader(xml);
			return (LayoutRootDto)DtoSerializer.Deserialize(textReader);
		}
	}
}