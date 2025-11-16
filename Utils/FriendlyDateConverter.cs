using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoApi.Utils
{

    public class FriendlyDateConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Accept multiple formats if you want
            if (reader.TokenType == JsonTokenType.String &&
                DateTime.TryParse(reader.GetString(), out var dt))
            {
                return dt;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Example output: "March 12, 2025 at 3:15 PM"
                writer.WriteStringValue(value.Value.ToString("MMMM dd, yyyy 'at' h:mm tt"));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
