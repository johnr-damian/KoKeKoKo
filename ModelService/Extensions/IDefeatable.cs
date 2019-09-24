namespace ModelService.Types
{
    /// <summary>
    /// An interface for an entity that can be defeated and 
    /// has opponent that can be defeated
    /// </summary>
    public interface IDefeatable
    {
        /// <summary>
        /// If the entity is defeated
        /// </summary>
        bool IsDefeated { get; }

        /// <summary>
        /// If the target of this entity is defeated
        /// </summary>
        bool IsOpposingDefeated { get; }
    }
}
