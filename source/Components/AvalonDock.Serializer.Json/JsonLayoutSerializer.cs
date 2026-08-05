using System;
using System.IO;
using System.Linq;
using System.Text;
using AvalonDock.Core;
using AvalonDock.Core.Serialization.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// JSON implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup. Serializes DTOs directly with Newtonsoft.Json.
	/// </summary>
	public class JsonLayoutSerializer : LayoutSerializerBase
	{
		/// <summary>Name of the property carrying the concrete DTO type of a polymorphic value.</summary>
		private const string DiscriminatorName = "_type";

		private readonly Formatting _formatting;

		private readonly JsonSerializerSettings _settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		/// <param name="settings">
		/// Optional serializer settings. They are honoured except for the two pieces the layout DTOs
		/// cannot round trip without, which are added when missing: the contract resolver that writes
		/// the type discriminator and the converter that reads it.
		/// </param>
		public JsonLayoutSerializer(IDockingManager manager, JsonSerializerSettings settings = null)
			: base(manager)
		{
			// DefaultValueHandling.Ignore is deliberately not used. It drops every member that holds
			// the default of its CLR type, which for the layout DTOs is the opposite of what is
			// wanted: properties such as CanHide, CanFloat or CanDockAsTabbedDocument default to true,
			// so writing them only when they are true would silently turn every false back into true
			// on load. The same applies to PreviousContainerIndex, whose default is -1 rather than 0.
			_settings = settings != null
				? new JsonSerializerSettings(settings)
				: new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

			if (!(_settings.ContractResolver is DiscriminatorContractResolver))
			{
				_settings.ContractResolver = new DiscriminatorContractResolver();
			}

			if (!_settings.Converters.OfType<LayoutDtoJsonConverter>().Any())
			{
				_settings.Converters.Add(new LayoutDtoJsonConverter());
			}

			_formatting = Formatting.Indented;
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, LayoutRootDto dto)
		{
			var json = JsonConvert.SerializeObject(dto, _formatting, _settings);
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

			return JsonConvert.DeserializeObject<LayoutRootDto>(json, _settings);
		}

		/// <summary>
		/// Determines whether values of the given type are stored in properties or lists declared with
		/// an abstract base, and therefore need a type discriminator to be rebuilt.
		/// </summary>
		/// <param name="objectType">The type to test.</param>
		/// <returns><see langword="true"/> when the type takes part in a polymorphic DTO hierarchy.</returns>
		private static bool IsPolymorphicDto(Type objectType)
		{
			return objectType != null
				&& (typeof(LayoutFloatingWindowDto).IsAssignableFrom(objectType)
					|| typeof(LayoutContentDto).IsAssignableFrom(objectType)
					|| typeof(LayoutPositionableGroupDto).IsAssignableFrom(objectType));
		}

		/// <summary>
		/// Writes the type discriminator as an ordinary property of every polymorphic DTO.
		/// </summary>
		/// <remarks>
		/// The discriminator has to come from the contract rather than from a <see cref="JsonConverter"/>.
		/// A converter that writes it has to serialize the object itself, and the only way to do that
		/// without re-entering the converter is to hand the work to a serializer the converter was
		/// removed from - which then writes the entire subtree without discriminators. Adding the
		/// property to the contract keeps every value on the normal pipeline, so nested panes, pane
		/// groups and contents all carry their own discriminator.
		/// </remarks>
		private sealed class DiscriminatorContractResolver : DefaultContractResolver
		{
			/// <inheritdoc/>
			protected override JsonObjectContract CreateObjectContract(Type objectType)
			{
				var contract = base.CreateObjectContract(objectType);

				// Abstract bases are never instantiated, so only concrete types carry the value.
				if (!IsPolymorphicDto(objectType) || objectType.IsAbstract)
				{
					return contract;
				}

				contract.Properties.Insert(0, new JsonProperty
				{
					PropertyName = DiscriminatorName,
					UnderlyingName = DiscriminatorName,
					PropertyType = typeof(string),
					DeclaringType = objectType,
					ValueProvider = new ConstantValueProvider(objectType.Name),
					Readable = true,

					// Read back by the converter, which needs it before the instance exists.
					Writable = false,
					Order = -2,
				});

				return contract;
			}
		}

		/// <summary>Supplies a fixed value for a synthesized JSON property.</summary>
		private sealed class ConstantValueProvider : IValueProvider
		{
			private readonly object _value;

			/// <summary>Initializes a new instance of the <see cref="ConstantValueProvider"/> class.</summary>
			/// <param name="value">The value to report for every target.</param>
			public ConstantValueProvider(object value)
			{
				_value = value;
			}

			/// <inheritdoc/>
			public object GetValue(object target) => _value;

			/// <inheritdoc/>
			public void SetValue(object target, object value)
			{
			}
		}

		/// <summary>
		/// Rebuilds a polymorphic DTO as the concrete class named by its "_type" discriminator.
		/// </summary>
		private sealed class LayoutDtoJsonConverter : JsonConverter
		{
			/// <inheritdoc/>
			/// <remarks>The discriminator is written by <see cref="DiscriminatorContractResolver"/>.</remarks>
			public override bool CanWrite => false;

			/// <inheritdoc/>
			/// <remarks>
			/// Every type of the three hierarchies matches, not just their abstract bases: Newtonsoft
			/// resolves the converter from the type declared at the reading site, which for a nested
			/// value can be either the abstract base or the concrete class.
			/// </remarks>
			public override bool CanConvert(Type objectType) => IsPolymorphicDto(objectType);

			/// <inheritdoc/>
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotSupportedException(
					"Writing is handled by the normal contract, see CanWrite.");
			}

			/// <inheritdoc/>
			public override object ReadJson(
				JsonReader reader,
				Type objectType,
				object existingValue,
				JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.Null)
				{
					return null;
				}

				var jo = JObject.Load(reader);
				var typeName = jo[DiscriminatorName]?.Value<string>();

				var concreteType = ResolveType(typeName, objectType);
				var result = Activator.CreateInstance(concreteType);

				using var jr = jo.CreateReader();

				// The serializer is passed on unchanged so that the children of this value are read
				// through this converter as well.
				serializer.Populate(jr, result);

				return result;
			}

			private static Type ResolveType(string typeName, Type baseType)
			{
				// A property or list declared with a concrete DTO type carries no ambiguity, so JSON
				// written without a discriminator - by hand, or by a version that did not emit one -
				// still reads as long as the declared type says what to build.
				if (string.IsNullOrEmpty(typeName))
				{
					return baseType != null && !baseType.IsAbstract
						? baseType
						: throw new JsonSerializationException(
							$"Missing DTO type discriminator for abstract base type '{baseType?.Name}'.");
				}

				switch (typeName)
				{
					case nameof(LayoutDocumentFloatingWindowDto): return typeof(LayoutDocumentFloatingWindowDto);
					case nameof(LayoutAnchorableFloatingWindowDto): return typeof(LayoutAnchorableFloatingWindowDto);
					case nameof(LayoutDocumentDto): return typeof(LayoutDocumentDto);
					case nameof(LayoutAnchorableDto): return typeof(LayoutAnchorableDto);
					case nameof(LayoutPanelDto): return typeof(LayoutPanelDto);
					case nameof(LayoutDocumentPaneDto): return typeof(LayoutDocumentPaneDto);
					case nameof(LayoutDocumentPaneGroupDto): return typeof(LayoutDocumentPaneGroupDto);
					case nameof(LayoutAnchorablePaneDto): return typeof(LayoutAnchorablePaneDto);
					case nameof(LayoutAnchorablePaneGroupDto): return typeof(LayoutAnchorablePaneGroupDto);
					default:
						throw new JsonSerializationException(
							$"Unknown DTO type discriminator '{typeName}' for base type '{baseType.Name}'.");
				}
			}
		}
	}
}