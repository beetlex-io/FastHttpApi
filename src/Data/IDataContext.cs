using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public interface IDataContext
    {

        string this[string name] { get; }

        void SetValue(string name, object value);

        bool TryGetBoolean(string name, out bool value);

        bool TryGetString(string name, out string value);

        bool TryGetDateTime(string name, out DateTime value);

        bool TryGetDecimal(string name, out decimal value);

        bool TryGetFloat(string name, out float value);

        bool TryGetDouble(string name, out double value);

        bool TryGetUShort(string name, out ushort value);

        bool TryGetUInt(string name, out uint value);

        bool TryGetULong(string name, out ulong value);

        bool TryGetInt(string name, out int value);

        bool TryGetLong(string name, out long value);

        bool TryGetShort(string name, out short value);

        object GetObject(string name, Type type);

        bool TryGetByte(string name, out byte value);

        bool TryGetChar(string name, out char value);

    }


}
