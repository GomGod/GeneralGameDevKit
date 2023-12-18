namespace GeneralGameDevKit.Utils
{
    //########################## Sample Code ##########################
    //public class SomethingClass : NonMonoSingleton<SomethingClass>
    //{
    // --- your codes..
    //}
    //
    //public void OtherMethod()
    //{
    //      SomethingClass.Instance.[your-method,fields..];
    //}
    //################################################################

    /// <summary>
    /// Singleton class for normal classes.
    /// </summary>
    /// <typeparam name="T">Type of class for using singleton.</typeparam>
    public class NonMonoSingleton<T> where T : class, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                instance ??= new T();
                return instance;
            }
        }
    }
}