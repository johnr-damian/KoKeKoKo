using System.Collections.Generic;

namespace ModelService
{
    /// <summary>
    /// An interface for classes that represents an actor in model simulations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IActor<T> where T : class
    {
        /// <summary>
        /// If the actor is already defeated in a simulation.
        /// </summary>
        bool IsDefeated { get; }

        /// <summary>
        /// Returns a list of available actions that can be executed given by the current state
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GeneratePotentialActions();

        /// <summary>
        /// Returns an exact copy of the current state of actor
        /// </summary>
        /// <returns></returns>
        T Copy();

        /// <summary>
        /// Applies the chosen action generated from <see cref="GeneratePotentialActions"/>.
        /// </summary>
        /// <param name="chosen_action"></param>
        void ApplyChosenAction(string chosen_action);
    }
}
