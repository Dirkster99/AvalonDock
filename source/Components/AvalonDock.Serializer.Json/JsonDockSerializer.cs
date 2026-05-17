using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// System.Text.Json implementation of <see cref="IDockSerializer"/>.
	/// Supports polymorphic serialization of dock layout trees.
	/// </summary>
	public sealed class JsonDockSerializer : IDockSerializer
	{
		private readonly JsonSerializerOptions _options;

		public JsonDockSerializer()
		{
			_options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = { new JsonStringEnumConverter() }
			};
		}

		public JsonDockSerializer(JsonSerializerOptions options)
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