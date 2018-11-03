using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class DataContxt : IDataContext
    {

        private Dictionary<string, object> mProperties = new Dictionary<string, object>(4);

        public string this[string name]
        {
            get
            {
                string result;
                TryGetString(name, out result);
                return result;
            }
        }

        public void Clear()
        {
            mProperties.Clear();
        }

        public void Add(string name, object value)
        {
            mProperties[name] = value;
        }

        protected bool GetProperty(string name, out object value)
        {
            return mProperties.TryGetValue(name, out value);
        }

        public object GetObject(string name, Type type)
        {
            object value;
            if (GetProperty(name, out value))
            {
                JToken token = value as JToken;
                if (token != null)
                {
                    JProperty jProperty = token as JProperty;
                    if (jProperty != null)
                        return jProperty.Value.ToObject(type);
                    else
                        return token.ToObject(type);
                }
                else
                {
                    if (value is string)
                        return Newtonsoft.Json.JsonConvert.DeserializeObject((string)value, type);
                    return value;
                }
            }
            return null;
        }

        public bool TryGetBoolean(string name, out bool value)
        {
            object data;
            value = false;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<bool>();
                else
                    return Boolean.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDateTime(string name, out DateTime value)
        {
            object data;
            value = DateTime.Now;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<DateTime>();
                else
                    return DateTime.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<decimal>();
                else
                    return decimal.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDouble(string name, out double value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<double>();
                else
                    return double.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetFloat(string name, out float value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<float>();
                else
                    return float.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetInt(string name, out int value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<int>();
                else
                    return int.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetLong(string name, out long value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<long>();
                else
                    return long.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetShort(string name, out short value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<short>();
                else
                    return short.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetString(string name, out string value)
        {
            object data;
            value = null;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<string>();
                else
                    value = (string)data;
                return true;
            }
            return false;
        }

        public bool TryGetUInt(string name, out uint value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<uint>();
                else
                    return uint.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetULong(string name, out ulong value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<ulong>();
                else
                    return ulong.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<ushort>();
                else
                    return ushort.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetByte(string name, out byte value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<byte>();
                else
                    return byte.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetChar(string name, out char value)
        {
            object data;
            value = (char)0;
            if (GetProperty(name, out data))
            {
                JToken token = data as JToken;
                if (token != null)
                    value = token.ToObject<char>();
                else
                    return char.TryParse((string)data, out value);
            }
            return false;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mProperties)
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            return sb.ToString();
        }
    }
}
