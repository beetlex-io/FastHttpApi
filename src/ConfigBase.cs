using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ConfigBase<T> where T : new()
    {

        public void Save()
        {
            string filename = typeof(T).Name + ".json";
            string config = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            lock (this)
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
                {
                    writer.Write(config);
                    writer.Flush();
                }
            }
        }

        private static T mInstance = default(T);

        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    string filename = typeof(T).Name + ".json";
                    if (System.IO.File.Exists(filename))
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(filename, Encoding.UTF8))
                        {
                            string value = reader.ReadToEnd();
                            mInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                        }
                    }
                    else
                    {
                        mInstance = new T();
                    }
                }
                return mInstance;
            }
        }

        public T GetInstance()
        {
            return Instance;
        }

    }
}
