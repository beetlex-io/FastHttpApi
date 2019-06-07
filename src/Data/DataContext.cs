using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class DataContxt : IDataContext
    {

        private Dictionary<string, object> mProperties = new Dictionary<string, object>();

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

        public void SetValue(string name, object value)
        {
            mProperties[name] = value;
        }

        protected bool GetProperty(string name, out object value)
        {
            return mProperties.TryGetValue(name, out value);
        }

        public bool TryGetBoolean(string name, out bool value)
        {
            object data;
            value = false;
            if (GetProperty(name, out data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Boolean)
                    {
                        string str = token.ToObject<string>();
                        return bool.TryParse((string)str, out value);
                    }
                    value = token.ToObject<bool>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Date)
                    {
                        string str = token.ToObject<string>();
                        return DateTime.TryParse((string)str, out value);
                    }
                    value = token.ToObject<DateTime>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return decimal.TryParse((string)str, out value);
                    }
                    value = token.ToObject<decimal>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return double.TryParse((string)str, out value);
                    }
                    value = token.ToObject<double>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return float.TryParse((string)str, out value);
                    }
                    value = token.ToObject<float>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return int.TryParse((string)str, out value);
                    }
                    value = token.ToObject<int>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return long.TryParse((string)str, out value);
                    }
                    value = token.ToObject<long>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return short.TryParse((string)str, out value);
                    }
                    value = token.ToObject<short>();
                    return true;
                }
                else
                    return short.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetUInt(string name, out uint value)
        {
            object data;
            value = 0;
            if (GetProperty(name, out data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return uint.TryParse((string)str, out value);
                    }
                    value = token.ToObject<uint>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return ulong.TryParse((string)str, out value);
                    }
                    value = token.ToObject<ulong>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return ushort.TryParse((string)str, out value);
                    }
                    value = token.ToObject<ushort>();
                    return true;
                }
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
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return byte.TryParse((string)str, out value);
                    }
                    value = token.ToObject<byte>();
                    return true;
                }
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
                if (data is JToken token)
                    value = token.ToObject<char>();
                else
                    return char.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetString(string name, out string value)
        {
            object data;
            value = null;
            if (GetProperty(name, out data))
            {
                if (data is JToken token)
                    value = token.ToObject<string>();
                else
                    value = (string)data;
                return true;
            }
            return false;
        }

        public object GetObject(string name, Type type)
        {
            object value;
            if (GetProperty(name, out value))
            {
                if (value is JToken token)
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mProperties)
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            return sb.ToString();
        }
    }
}
