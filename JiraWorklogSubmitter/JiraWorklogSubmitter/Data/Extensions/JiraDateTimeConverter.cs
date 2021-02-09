using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace JiraWorklogSubmitter.Data.Extensions
{
    public class JiraDateTimeConverter : IsoDateTimeConverter
    {

        public JiraDateTimeConverter()
        {
            DateTimeFormat = "yyyy'-'MM'-'ddTHH':'mm':'ss'.'fffzzz";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                var dateTime = (DateTime)value;
                var text = dateTime.ToString(DateTimeFormat);
                text = text.Remove(text.Length - 3, 1); // Remove the colon from timezone
                writer.WriteValue(text);
            }
            else
            {
                throw new JsonSerializationException("Unexpected value when converting date. Expected DateTime");
            }
        }
    }
}
