﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebApi.Hal.Interfaces;

namespace WebApi.Hal.JsonConverters
{
    public class EmbeddedResourceConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resourceList = (IList<EmbeddedResource>) value;
            if (resourceList.Count == 0) return;

            writer.WriteStartObject();

            foreach (var rel in resourceList)
            {
                writer.WritePropertyName(NormalizeRel(rel.Resources[0]));
                if (rel.IsSourceAnArray)
                    writer.WriteStartArray();
                foreach (var res in rel.Resources)
                    serializer.Serialize(writer, res);
                if (rel.IsSourceAnArray)
                    writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        static string NormalizeRel(IResource res)
        {
            if (!string.IsNullOrEmpty(res.Rel)) return res.Rel;
            return "unknownRel-" + res.GetType().Name;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (IList<EmbeddedResource>).IsAssignableFrom(objectType);
        }
    }
}