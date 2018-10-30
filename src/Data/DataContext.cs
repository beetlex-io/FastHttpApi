using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class DataContxt : IDataContext
    {

        private Dictionary<string, string> mProperties = new Dictionary<string, string>(4);

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

        public void Add(string name, string value)
        {
            mProperties[name] = value;
        }

        protected bool GetProperty(string name, out string value)
        {
            return mProperties.TryGetValue(name, out value);
        }

        public object GetObject(string name, Type type)
        {
            string value;
            if (GetProperty(name, out value))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
            }
            return null;
        }

        public bool TryGetBoolean(string name, out bool value)
        {
            string data;
            value = false;
            if (GetProperty(name, out data))
            {
                return Boolean.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetDateTime(string name, out DateTime value)
        {
            string data;
            value = DateTime.Now;
            if (GetProperty(name, out data))
            {
                return DateTime.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return decimal.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetDouble(string name, out double value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return double.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetFloat(string name, out float value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return float.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetInt(string name, out int value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return int.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetLong(string name, out long value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return long.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetShort(string name, out short value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return short.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetString(string name, out string value)
        {
            string data;
            value = null;
            if (GetProperty(name, out data))
            {
                value = data;
                return true;
            }
            return false;
        }

        public bool TryGetUInt(string name, out uint value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return uint.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetULong(string name, out ulong value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return ulong.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return ushort.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetByte(string name, out byte value)
        {
            string data;
            value = 0;
            if (GetProperty(name, out data))
            {
                return byte.TryParse(data, out value);
            }
            return false;
        }

        public bool TryGetChar(string name, out char value)
        {
            string data;
            value = (char)0;
            if (GetProperty(name, out data))
            {
                return char.TryParse(data, out value);
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
