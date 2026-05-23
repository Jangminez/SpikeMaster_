using System;
using System.Collections.Generic;

namespace JangLib
{
    public abstract class BaseEventManager<U> : Singleton<U>, IBaseManager, IEventManager where U : BaseEventManager<U>, new()
    {
        public bool IsInitialized { get; set; } = false;

        private Dictionary<int, Delegate> _eventDict = new Dictionary<int, Delegate>();

        public void Init()
        {
            OnInit();
            IsInitialized = true;
        }

        protected abstract void OnInit();

        public bool IsRegistered(int eventType)
        {
            return _eventDict.ContainsKey(eventType);
        }

        public void Register<T>(int eventType, T listener) where T : Delegate
        {
            if (!_eventDict.ContainsKey(eventType))
            {
                _eventDict[eventType] = null;
            }

            _eventDict[eventType] = Delegate.Combine(_eventDict[eventType], listener);
        }

        public void UnRegister<T>(int eventType, T listener) where T : Delegate
        {
            if (_eventDict.ContainsKey(eventType) && _eventDict[eventType] != null)
            {
                _eventDict[eventType] = Delegate.Remove(_eventDict[eventType], listener);

                if (_eventDict[eventType] == null)
                {
                    _eventDict.Remove(eventType);
                }
            }
        }

        public void Trigger(int eventType)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Action;
                callback?.Invoke();
            }
        }

        public void Trigger<T>(int eventType, T eventData)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Action<T>;
                callback?.Invoke(eventData);
            }
        }

        public void Trigger<T1, T2>(int eventType, T1 param1, T2 param2)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Action<T1, T2>;
                callback?.Invoke(param1, param2);
            }
        }

        public void Trigger<T1, T2, T3>(int eventType, T1 param1, T2 param2, T3 param3)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Action<T1, T2, T3>;
                callback?.Invoke(param1, param2, param3);
            }
        }

        public void Trigger<T1, T2, T3, T4>(int eventType, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Action<T1, T2, T3, T4>;
                callback?.Invoke(param1, param2, param3, param4);
            }
        }

        public T TriggerFunc<T>(int eventType)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Func<T>;
                return callback != null ? callback.Invoke() : default;
            }

            return default;
        }

        public T2 TriggerFunc<T1, T2>(int eventType, T1 param1)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Func<T1, T2>;
                return callback != null ? callback.Invoke(param1) : default;
            }

            return default;
        }

        public T3 TriggerFunc<T1, T2, T3>(int eventType, T1 param1, T2 param2)
        {
            if (IsRegistered(eventType))
            {
                var callback = _eventDict[eventType] as Func<T1, T2, T3>;
                return callback != null ? callback.Invoke(param1, param2) : default;
            }

            return default;
        }
    }
}
