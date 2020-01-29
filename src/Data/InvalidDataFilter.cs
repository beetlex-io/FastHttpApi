using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class InvalidDataFilter
    {
        private CharItem[] Items = new CharItem[256];

        public void Add(params string[] items)
        {
            foreach (var item in items)
            {
                var bytes = Encoding.UTF8.GetByteCount(item);
                if (bytes == item.Length)
                {
                    var kw = System.Web.HttpUtility.UrlEncode(item);
                    OnAdd(item);
                    OnAdd(kw);
                }
            }
        }

        private void OnAdd(string item)
        {
            byte index = (byte)char.ToUpper(item[0]);
            if (Items[index] == null)
            {
                Items[index] = new CharItem(item[0]);
            }
            Items[index].Add(item, 1);

            index = (byte)char.ToLower(item[0]);
            if (Items[index] == null)
            {
                Items[index] = new CharItem(item[0]);
            }
            Items[index].Add(item, 1);
        }

        public List<string> Match(string value)
        {
            List<string> result = new List<string>();
            int offset = 0;
            while (offset < value.Length)
            {
                var item = OnMatch(value, offset);
                if (item == null)
                {
                    offset += 1;
                }
                else
                {
                    offset += item.Data.Length;
                    result.Add(item.Data);
                }
            }
            return result;
        }

        public int IsMatch(string value)
        {
            int count = 0;

            int offset = 0;
            while (offset < value.Length)
            {
                var item = OnMatch(value, offset);
                if (item == null)
                {
                    offset += 1;
                }
                else
                {
                    count++;
                    offset += item.Data.Length;
                }
            }
            return count;
        }
        private CharItem OnMatch(string value, int offset)
        {
            byte index = (byte)value[offset];
            var item = Items[index];
            if (item == null)
                return null;
            else
                return item.Eof ? item : item.Match(value, offset + 1);
        }

        public class CharItem
        {
            public CharItem(char c)
            {
                Value = c;
            }

            public bool IsMatch(char value)
            {
                return Value == value || Value == char.ToLower(value) || Value == char.ToUpper(value);
            }


            public char Value { get; set; }

            public bool Eof { get; set; } = false;

            public string Data { get; set; }

            private CharItem[] Items = new CharItem[256];

            public void Add(string key, int offset)
            {
                if (offset == key.Length)
                {
                    Data = key;
                    Eof = true;
                }
                else
                {
                    byte index = (byte)char.ToUpper(key[offset]);
                    if (Items[index] == null)
                    {
                        Items[index] = new CharItem(key[offset]);
                    }
                    Items[index].Add(key, offset + 1);

                    index = (byte)char.ToLower(key[offset]);
                    if (Items[index] == null)
                    {
                        Items[index] = new CharItem(key[offset]);
                    }
                    Items[index].Add(key, offset + 1);
                }
            }

            public CharItem Match(string value, int offset)
            {
                if (offset < value.Length)
                {
                    byte index = (byte)value[offset];
                    var item = Items[index];
                    if (item != null)
                    {
                        if (item.IsMatch(value[offset]))
                        {
                            if (item.Eof)
                                return item;
                            else
                                return item.Match(value, offset + 1);

                        }
                    }
                }
                return null;
            }
        }

    }
}
