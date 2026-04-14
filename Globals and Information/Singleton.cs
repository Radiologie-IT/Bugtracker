namespace Bugtracker.Globals_and_Information
{
    /// <summary>
    /// Singleton Class from User: Moo-Juice. Use with caution.
    /// https://stackoverflow.com/questions/16865413/implementing-singleton-inheritable-class-in-c-sharp
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance == null)
                _instance = new T();
            return _instance;
        }
    }
}