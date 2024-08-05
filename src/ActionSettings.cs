using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ActionSettings
    {
        const string SETTING_FILE = "action_settings.json";

        public class ActionInfo
        {
            public ActionInfo()
            {

            }
            public ActionInfo(ActionHandler handler)
            {
                Url = handler.Url;
                MaxRps = handler.MaxRPS;
                if (handler.ThreadQueue != null)
                    ThreadInfo = ThreadInfo.GetThreadInfo(handler.ThreadQueue);
                else
                    ThreadInfo = new ThreadInfo();
            }
            public string Url { get; set; }

            public int MaxRps { get; set; }

            public ThreadInfo ThreadInfo { get; set; }

            public void SetTo(ActionHandler handler)
            {

                handler.MaxRPS = MaxRps;
                if (ThreadInfo != null)
                {
                    if (ThreadInfo.Type == ThreadQueueType.None.ToString())
                        handler.ThreadQueue = null;
                    else
                        handler.ThreadQueue = ThreadInfo?.GetThreadQueue();
                }
            }
            public override string ToString()
            {
                return $"{Url} {MaxRps} {ThreadInfo}";
            }
        }

        public class ThreadInfo
        {
            public string Type { get; set; } = "None";

            public int Count { get; set; } = 1;

            public string UniqueName { get; set; }

            public ThreadQueueAttribute GetThreadQueue()
            {
                return GetThreadQueue(this);
            }
            public static ThreadQueueAttribute GetThreadQueue(ThreadInfo info)
            {
                ThreadQueueType type = Enum.Parse<ThreadQueueType>(info.Type == null ? "None" : info.Type);
                if (type == ThreadQueueType.Single)
                    return new ThreadQueueAttribute(ThreadQueueType.Single);
                else if (type == ThreadQueueType.Multiple)
                    return new ThreadQueueAttribute(ThreadQueueType.Multiple, info.Count);
                else if (type == ThreadQueueType.DataUnique)
                    return new ThreadQueueAttribute(info.UniqueName);
                return null;

            }
            public static ThreadInfo GetThreadInfo(ActionHandler handler)
            {
                if (handler.ThreadQueue != null)
                    return GetThreadInfo(handler.ThreadQueue);
                else
                    return new ThreadInfo { Type = "None" };
            }
            public static ThreadInfo GetThreadInfo(ThreadQueueAttribute threadQueue)
            {
                ThreadInfo info = new ThreadInfo();
                info.Type = threadQueue.Type.ToString();
                info.Count = threadQueue.Count;
                info.UniqueName = threadQueue.UniqueName;
                return info;
            }

            public override string ToString()
            {
                return $"{Type.ToString()}[{Count}|{UniqueName}]";
            }
        }

        public void SetAction(ActionHandler handler)
        {
            var item = Settings.Find(i => i.Url.ToLower() == handler.Url.ToLower());
            if (item != null)
                item.SetTo(handler);
        }

        public List<ActionInfo> Settings { get; set; } = new List<ActionInfo>();

        public void Save(params ActionHandler[] actions)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(SETTING_FILE, false))
            {
                this.Settings.Clear();
                foreach (var item in actions)
                    this.Settings.Add(new ActionInfo(item));
                string value = Newtonsoft.Json.JsonConvert.SerializeObject(this.Settings);
                writer.Write(value);
                writer.Flush();
            }
        }

        public void Load()
        {
            if (System.IO.File.Exists(SETTING_FILE))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(SETTING_FILE))
                {
                    string value = reader.ReadToEnd();
                    Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionInfo>>(value);
                }
            }
        }
    }
}
