using System;

namespace JangLib
{
    public interface IEventManager
    {
        bool IsInitialized { get; set; }

        void Init();
        bool IsRegistered(int eventType);

        void Register<T>(int eventType, T listener) where T : Delegate;
        void UnRegister<T>(int eventType, T listener) where T : Delegate;

        void Trigger(int eventType);
        void Trigger<T>(int eventType, T eventData);
        void Trigger<T1, T2>(int eventType, T1 param1, T2 param2);
        void Trigger<T1, T2, T3>(int eventType, T1 param1, T2 param2, T3 param3);
        void Trigger<T1, T2, T3, T4>(int eventType, T1 param1, T2 param2, T3 param3, T4 param4);

        T TriggerFunc<T>(int eventType);
        T2 TriggerFunc<T1, T2>(int eventType, T1 param1);
        T3 TriggerFunc<T1, T2, T3>(int eventType, T1 param1, T2 param2);
    }
}