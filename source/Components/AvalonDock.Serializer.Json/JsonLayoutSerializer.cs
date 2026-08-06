using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AvalonDock.Core;
using AvalonDock.Core.Serialization.Dto;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// JSON implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup. Serializes DTOs directly with System.Text.Json.
	/// </summary>
	public class JsonLayoutSerializer : LayoutSerializerBase
	{
		/// <summary>Name of the property carrying the concrete type of a polymorphic value.</summary>
		private const string DiscriminatorName = "_type";

		private readonly JsonSerializerOptions _options;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		/// <param name="options">
		/// Optional serializer options. They are honoured except for the type resolver, which is
		/// replaced by the one declaring how the layout DTOs are told apart; the layout cannot round
		/// trip without it.
		/// </param>
		public JsonLayoutSerializer(IDockingManager manager, JsonSerializerOptions options = null)
			: base(manager)
		{
			_options = options != null
				? new JsonSerializerOptions(options)
				: new JsonSerializerOptions
				{
					WriteIndented = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				};

			var resolver = new DefaultJsonTypeInfoResolver();
			resolver.Modifiers.Add(DeclarePolymorphicDtos);
			_options.TypeInfoResolver = resolver;
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, LayoutRootDto dto)
		{
			var json = JsonSerializer.Serialize(dto, _options);
			var bytes = Encoding.UTF8.GetBytes(json);
			stream.Write(bytes, 0, bytes.Length);
		}

		/// <inheritdoc/>
		protected override LayoutRootDto DeserializeCore(Stream stream)
		{
			string json;
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
			{
				json = reader.ReadToEnd();
			}

			return JsonSerializer.Deserialize<LayoutRootDto>(json, _options);
		}

		/// <summary>
		/// Declares the concrete types that can appear where the layout DTOs are stored under an
		/// abstract base, so that each one is written with a discriminator and rebuilt as itself.
		/// </summary>
		/// <param name="typeInfo">The contract being built.</param>
		/// <remarks>
		/// Configured here rather than through attributes on the DTOs, which keeps AvalonDock.Core
		/// free of a dependency on a JSON library. Only the three bases that actually appear as a
		/// declared type need this; a property or list declared with a concrete DTO carries no
		/// ambiguity and is written without a discriminator.
		/// </remarks>
		private static void DeclarePolymorphicDtos(JsonTypeInfo typeInfo)
		{
			if (typeInfo.Type == typeof(LayoutPositionableGroupDto))
			{
				// The children of LayoutPanel, LayoutDocumentPaneGroup and LayoutAnchorablePaneGroup.
				typeInfo.PolymorphismOptions = Polymorphism(
					(typeof(LayoutPanelDto), nameof(LayoutPanelDto)),
					(typeof(LayoutDocumentPaneDto), nameof(LayoutDocumentPaneDto)),
					(typeof(LayoutDocumentPaneGroupDto), nameof(LayoutDocumentPaneGroupDto)),
					(typeof(LayoutAnchorablePaneDto), nameof(LayoutAnchorablePaneDto)),
					(typeof(LayoutAnchorablePaneGroupDto), nameof(LayoutAnchorablePaneGroupDto)));
			}
			else if (typeInfo.Type == typeof(LayoutContentDto))
			{
				// The children of LayoutDocumentPane, which holds documents and anchorables alike.
				typeInfo.PolymorphismOptions = Polymorphism(
					(typeof(LayoutDocumentDto), nameof(LayoutDocumentDto)),
					(typeof(LayoutAnchorableDto), nameof(LayoutAnchorableDto)));
			}
			else if (typeInfo.Type == typeof(LayoutFloatingWindowDto))
			{
				// The floating windows of the layout root.
				typeInfo.PolymorphismOptions = Polymorphism(
					(typeof(LayoutDocumentFloatingWindowDto), nameof(LayoutDocumentFloatingWindowDto)),
					(typeof(LayoutAnchorableFloatingWindowDto), nameof(LayoutAnchorableFloatingWindowDto)));
			}
		}

		/// <summary>Builds the polymorphism options for one abstract base.</summary>
		/// <param name="derivedTypes">The concrete types and the names they are stored under.</param>
		/// <returns>The options.</returns>
		private static JsonPolymorphismOptions Polymorphism(params (Type Type, string Discriminator)[] derivedTypes)
		{
			var options = new JsonPolymorphismOptions
			{
				TypeDiscriminatorPropertyName = DiscriminatorName,
			};

			foreach (var derived in derivedTypes)
			{
				options.DerivedTypes.Add(new JsonDerivedType(derived.Type, derived.Discriminator));
			}

			return options;
		}
	}
}
