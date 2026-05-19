using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// System.Text.Json implementation of <see cref="ILayoutSerializer"/>.
	/// Supports polymorphic serialization of dock layout trees.
	/// </summary>
	public sealed class JsonLayoutSerializer : ILayoutSerializer
	{
		private readonly JsonSerializerOptions _options;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		public JsonLayoutSerializer()
		{
			_options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = { new JsonStringEnumConverter() }
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		/// <param name="options"></param>
		public JsonLayoutSerializer(JsonSerializerOptions options)
		{
			_options = options;
		}

		public string Serialize<T>(T value)
		{
			return JsonSerializer.Serialize(value, _options);
		}

		public T? Deserialize<T>(string text)
		{
			return JsonSerializer.Deserialize<T>(text, _options);
		}

		public T? Load<T>(Stream stream)
		{
			return JsonSerializer.Deserialize<T>(stream, _options);
		}

		public void Save<T>(Stream stream, T value)
		{
			JsonSerializer.Serialize(stream, value, _options);
		}
	}
}