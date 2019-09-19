using ModelService.Types;
using System;
using System.Collections.Generic;

namespace ModelService.Micromanagement
{
    public partial class Micromanagement
    {
        /// <summary>
        /// The highest-level abstraction of prediction algorithm. It computes
        /// the potential average of the army and statically computes who will won using
        /// the Lanchester formula. It returns the winner's army and the value won from this
        /// battle
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns>The winner's army can be owner or enemy. But the value will always be the owner</returns>
        public Tuple<string, double> LanchesterBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, double> battle_result = null;

            try
            {

            }
            catch(Exception ex)
            {
                Console.WriteLine($@"LanchesterBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// A high-level abstraction of prediction algorithm. It gets a random potential damage
        /// from the triangular distribution. Like the <see cref="LanchesterBasedPrediction(TargetPolicy)"/>, it
        /// does not consider the decreasing energy of the unit.
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, double> StaticBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, double> battle_result = null;

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($@"StaticBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }

        /// <summary>
        /// A high-level abstraction of prediction algorithm, but more detailed. Unlike <see cref="StaticBasedPrediction(TargetPolicy)"/>,
        /// It considers the decreasing of energy of the unit and as well as the health of each unit
        /// </summary>
        /// <param name="target_policy"></param>
        /// <returns></returns>
        public Tuple<string, double> DynamicBasedPrediction(TargetPolicy target_policy)
        {
            Tuple<string, double> battle_result = null;

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($@"DynamicBasedPrediction() -> {ex.Message}...");
                battle_result = null;
            }

            return battle_result;
        }
    }
}