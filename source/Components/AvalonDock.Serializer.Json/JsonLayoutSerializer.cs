using System;
using System.IO;
using System.Text;
using AvalonDock.Core;
using AvalonDock.Core.Serialization.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// JSON implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup. Serializes DTOs directly with Newtonsoft.Json.
	/// </summary>
	public class JsonLayoutSerializer : LayoutSerializerBase
	{
		private readonly Formatting _formatting;

		private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			Converters = { new LayoutDtoJsonConverter() },
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		/// <param name="settings">json serializer settings</param>
		public JsonLayoutSerializer(IDockingManager manager, JsonSerializerSettings settings = null)
			: base(manager)
		{
			if (settings != null)
			{
				_settings = settings;
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
		/// Custom JSON converter for polymorphic DTO dispatch using a "_type" discriminator property.
		/// </summary>
		private sealed class LayoutDtoJsonConverter : JsonConverter
		{
			/// <inheritdoc/>
			public override bool CanConvert(Type objectType)
			{
				return objectType == typeof(LayoutFloatingWindowDto)
					   || objectType == typeof(LayoutContentDto)
					   || objectType == typeof(LayoutPositionableGroupDto);
			}

			/// <inheritdoc/>
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var innerSerializer = CreateSerializerWithoutThis(serializer);
				var jo = JObject.FromObject(value, innerSerializer);
				jo.AddFirst(new JProperty("_type", value.GetType().Name));
				jo.WriteTo(writer);
			}

			/// <inheritdoc/>
			public override object ReadJson(
				JsonReader reader,
				Type objectType,
				object existingValue,
				JsonSerializer serializer)
			{
				var jo = JObject.Load(reader);
				var typeName = jo["_type"]?.Value<string>();

				var concreteType = ResolveType(typeName, objectType);
				var result = Activator.CreateInstance(concreteType);
				using var jr = jo.CreateReader();
				CreateSerializerWithoutThis(serializer).Populate(jr, result);

				return result;
			}

			private static Type ResolveType(string typeName, Type baseType)
			{
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

			private static JsonSerializer CreateSerializerWithoutThis(JsonSerializer parent)
			{
				var s = new JsonSerializer
				{
					NullValueHandling = parent.NullValueHandling,
					DefaultValueHandling = parent.DefaultValueHandling,
				};

				foreach (var converter in parent.Converters)
				{
					if (converter is LayoutDtoJsonConverter)
					{
						continue;
					}

					s.Converters.Add(converter);
				}

				return s;
			}
		}
	}
}