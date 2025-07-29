using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace JobMan;

public class DefaultWorkItemDefinitionSerializer : IWorkItemDefinitionSerializer
{
    internal JsonSerializerOptions SerializerOptions { get; private set; }

    public DefaultWorkItemDefinitionSerializer()
    {
        SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public InvokeData FromJson(string json)
    {
        InvokeData data = JsonSerializer.Deserialize<InvokeData>(json, SerializerOptions);

        // Temporary type correction. Work on later ...
        for (int i = 0; i < data.PropertyTypes.Length; i++)
        {
            string typeStr = data.PropertyTypes[i];

            Type type = JobManGlobals.WorkServerOptions.TypeResolver.Get(typeStr);
            object value = data.ArgumentValues[i];

            if (value == null)
                continue;

            Type valueType = value.GetType();

            if (!type.IsAssignableFrom(valueType))
            {
                // System.Text.Json does not support JObject, so handle as string
                TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
                if (typeConverter.CanConvertFrom(valueType))
                    data.ArgumentValues[i] = typeConverter.ConvertFrom(value);
                else
                {
                    if (value is string strVal)
                    {
                        data.ArgumentValues[i] = typeConverter.ConvertFromInvariantString(strVal);
                    }
                    else
                    {
                        string valueStr = Convert.ToString(value, CultureInfo.InvariantCulture);
                        data.ArgumentValues[i] = typeConverter.ConvertFromInvariantString(valueStr);
                    }
                }
            }
        }

        return data;
    }

    public string ToJson(InvokeData data)
    {
        return JsonSerializer.Serialize(data, SerializerOptions);
    }
}
