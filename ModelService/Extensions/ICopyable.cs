namespace ModelService.Types
{
    /// <summary>
    /// An interface that provides function if the class must be copyable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICopyable<T>
    {
        /// <summary>
        /// A method that returns the instance of the class
        /// </summary>
        /// <returns></returns>
        T GetShallowCopy();

        /// <summary>
        /// A method that creates a new instance with same values of the class
        /// </summary>
        /// <returns></returns>
        T GetDeepCopy();
    }
}
