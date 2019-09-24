namespace ModelService.Types
{
    /// <summary>
    /// An interface that provides function if the class can send a message to agent
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// A method to return a message-ready format string to be send to agent
        /// </summary>
        /// <returns></returns>
        string CreateMessage();
    }
}
