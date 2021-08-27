using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EvernoteToNotionChrome.Service
{
    public delegate void GlobalNotificationDelegate(GlobalNotificationMessage msg);

    

    public class GlobalNotificationMessage : EventArgs
    {
        public string Message { get; private set; }
        public object Source { get; private set; }
        public object UserInfo { get; private set; }

        public GlobalNotificationMessage(string message, object source, object userInfo)
        {
            Source = source;
            Message = message;
            UserInfo = userInfo;
        }
    }

    public class GlobalNotification
    {
        public static GlobalNotification Default { get; private set; }

        public static string NotificationOutputLogInfo = "NotificationOutputLogInfo";

        public static void DispatcherRun(Dispatcher dispatcher, GlobalNotificationDelegate action, GlobalNotificationMessage msg)
        {
            GlobalNotificationDelegate func = new GlobalNotificationDelegate(action);
            dispatcher.BeginInvoke(func, msg);
        }

        static GlobalNotification()
        {
            Default = new GlobalNotification();
        }

        readonly object _locker = new object();
        Dictionary<string, Dictionary<object, GlobalNotificationDelegate>> _dic;

        public GlobalNotification()
        {
            _dic = new Dictionary<string, Dictionary<object, GlobalNotificationDelegate>>();
        }

        public void Post(string name, object source, object userInfo = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            lock (_locker)
            {
                //Debug.WriteLine("NewMessage:" + name);
                var msg = new GlobalNotificationMessage(name, source, userInfo);
                foreach (var del in _GetDelegates(name))
                    del(msg);
            }
        }

        public void Register(string name, object target, GlobalNotificationDelegate action, bool mainThread = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }


            lock (_locker)
            {
                _AddValue(name, target, action);
            }
        }

        public bool Unregister(string name, object target = null, GlobalNotificationDelegate action = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            lock (_locker)
            {
                if (action != null)
                    return _RemoveValue(name, target, action);

                if (target != null)
                    return _RemoveKey(name, target);

                return _RemoveKey(name);
            }
        }



        void _AddValue(string key1, object key2, GlobalNotificationDelegate val)
        {
            //从总的dic中取出该消息的所有注册
            var objDic = ValueOrDefault(_dic, key1);
            if (objDic == null) //如果没有注册，则新增加一个
                objDic = _dic[key1] = new Dictionary<object, GlobalNotificationDelegate>();

            var res = ValueOrDefault(objDic, key2); //从该消息中找到某个target的设置
            if (res == null) //如果这个target没有设置 则设置上去
                objDic[key2] = val;
            else
                objDic[key2] = objDic[key2] + val;
        }


        bool _RemoveValue(string key1, object key2, GlobalNotificationDelegate val)
        {
            var objDic = ValueOrDefault(_dic, key1);
            if (objDic == null)
                return false;

            var res = ValueOrDefault(objDic, key2);
            if (res == null)
                return false;
            else
                objDic[key2] = objDic[key2] - val;
            return true;
        }

        bool _RemoveKey(string key1)
        {
            return _dic.Remove(key1);
        }

        bool _RemoveKey(string key1, object key2)
        {
            var objDic = ValueOrDefault(_dic, key1);
            if (objDic == null)
                return false;
            return objDic.Remove(key2);
        }

        IEnumerable<GlobalNotificationDelegate> _GetDelegates(string key1)
        {
            var objDic = ValueOrDefault(_dic, key1);
            if (objDic != null)
                foreach (var pair in objDic)
                    yield return pair.Value;
        }


        static TValue ValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dic, TKey key)
        {
            if (dic == null)
                return default(TValue);

            TValue res;
            if (dic.TryGetValue(key, out res))
            {
                return res;
            }
            return default(TValue);
        }
    }

}
