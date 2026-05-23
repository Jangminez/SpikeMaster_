namespace JangLib
{
    public interface IBaseManager
    {
        bool IsInitialized { get; }
        void Init();
    }
}
