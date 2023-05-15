namespace DeepDreams.Services
{
    public interface IServiceLocator
    {
        static IServiceLocator Instance { get; }
        void Register<T>(T serviceInstance) where T : class, IService;
        void Unregister<T>() where T : class, IService;
        T GetService<T>() where T : class, IService;
    }
}