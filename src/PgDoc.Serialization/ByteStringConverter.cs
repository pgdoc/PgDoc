// Copyright 2016 Flavien Charlon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Newtonsoft.Json;

namespace PgDoc.Serialization
{
    public class ByteStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ByteString) || objectType == typeof(ByteString?);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string encodedData = (string)reader.Value;

            if (encodedData == null)
            {
                return null;
            }
            else
            {
                byte[] data = Convert.FromBase64String(encodedData);
                return new ByteString(data);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ByteString data = (ByteString)value;

            writer.WriteValue(Convert.ToBase64String(data.ToByteArray()));
        }
    }
}
