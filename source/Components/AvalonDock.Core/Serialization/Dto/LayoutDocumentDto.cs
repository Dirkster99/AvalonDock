#nullable disable
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a layout document.
	/// </summary>
	[XmlType("LayoutDocument")]
	public class LayoutDocumentDto : LayoutContentDto
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutDocumentDto"/> class.
		/// </summary>
		public LayoutDocumentDto()
		{
			CanClose = true;
			CanCloseDefault = true;
		}

		/// <summary>Gets or sets the description.</summary>
		[XmlAttribute]
		public string Description { get; set; }

		/// <summary>Gets or sets a value indicating whether this document can be moved.</summary>
		[XmlAttribute]
		public bool CanMove { get; set; } = true;

		/// <summary>Determines whether the CanMove property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanMove() => !CanMove;
	}
}