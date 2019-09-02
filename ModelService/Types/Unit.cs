﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModelService.Types
{
    /// <summary>
    /// A template for unit to be parsed either from CSV file or from Game observation
    /// </summary>
    public abstract partial class Unit
    {
        /// <summary>
        /// The current target of this unit
        /// </summary>
        protected int _current_target = -1;

        /// <summary>
        /// A list of unit that has been targeted by this unit
        /// </summary>
        protected List<Unit> _targets { get; set; } = null;

        #region Properties From Source
        /// <summary>
        /// A unique identifier of this unit
        /// </summary>
        public long UniqueID { get; protected set; } = -1;

        /// <summary>
        /// The player/alliance of this unit
        /// </summary>
        public string Owner { get; protected set; } = "";

        /// <summary>
        /// The unit type name
        /// </summary>
        public string Name { get; protected set; } = "";

        /// <summary>
        /// The current position of this unit
        /// </summary>
        public Coordinate Position { get; protected set; } = null;

        /// <summary>
        /// The current buffs that affect this unit
        /// </summary>
        /// <remarks>
        /// The string stored in this list are the name of buffs found in the API
        /// </remarks>
        public List<string> Buffs { get; protected set; } = null;
        #endregion

        #region Properties From Simulation
        /// <summary>
        /// The current health of this unit in the simulation
        /// </summary>
        public double Simulated_Health { get; set; } = -1;

        /// <summary>
        /// The current energy of this unit in the simulation
        /// </summary>
        public double Simulated_Energy { get; set; } = -1;

        /// <summary>
        /// The current ground damage of this unit it can deal in the simulation
        /// </summary>
        public double Simulated_Ground_Damage { get; set; } = -1;

        /// <summary>
        /// The current air damage of this unit it can deal in the simulation
        /// </summary>
        public double Simulated_Air_Damage { get; set; } = -1;

        /// <summary>
        /// The current armor of this unit in the simulation
        /// </summary>
        public int Simulated_Armor { get; set; } = -1;

        /// <summary>
        /// If this unit's health is below or equal 0 in the simulation
        /// </summary>
        public virtual bool IsDead => Simulated_Health <= 0;

        /// <summary>
        /// The unit to be targeted by this unit
        /// </summary>
        public virtual Unit Target
        {
            get
            {
                return ((_targets == null) || (_targets.Count == 0))? null : _targets[_current_target];
            }
        }

        /// <summary>
        /// If this unit's target health is below or equal 0, or if there is no unit to be targeted by this unit
        /// </summary>
        public virtual bool IsTargetDead => (Target == null || Target.IsDead); 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffs"></param>
        public Unit(long uid, string owner, string name, double x, double y, params string[] buffs)
        {
            UniqueID = uid;
            Owner = owner;
            Name = name;
            Position = new Coordinate(x, y);
            Buffs = new List<string>(buffs);

            _targets = new List<Unit>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Unit CreateDeepCopy();

        /// <summary>
        /// Adds a target for this unit
        /// </summary>
        /// <param name="target_unit"></param>
        public virtual void SetTarget(Unit target_unit)
        {
            _current_target++;
            _targets.Add(target_unit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This is to be use to apply static buffs or modifiers
        /// for example, increasing health, applying armor before start of battle
        /// and etc
        /// </remarks>
        public virtual void ApplyBuffsOrModifiers()
        {
            foreach (string buff in Buffs)
            {
                switch (buff)
                {
                    case "TERRANINFANTRYWEAPONSLEVEL1":
                        if (Name == "TERRAN_MARINE")
                        {
                            Simulated_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 1.6;
                            Simulated_Air_Damage = Unit.DEFINITIONS[Name].Item3 + 1.6;
                        }
                        else if(Name == "TERRAN_REAPER")
                            Simulated_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 2.5;
                        else if (Name == "TERRAN_MARAUDER")
                            Simulated_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 0.93;
                        else if(Name == "TERRAN_GHOST")
                        {
                            Simulated_Ground_Damage = Unit.DEFINITIONS[Name].Item3 + 0.93;
                            Simulated_Air_Damage = Unit.DEFINITIONS[Name].Item3 + 0.93;
                        }
                        break;
                    default:
                        break;
                }
            }
            //TODO
        }

        public virtual void UseBuffsOrModifiers()
        {
            //TODO
        }

        /// <summary>
        /// This is to be use when using a reusable buffs or modifiers.
        /// Usually this is a skill like single-target skill
        /// </summary>
        public virtual DATA_TYPE UseBuffsOrModifiers<DATA_TYPE>(DATA_TYPE value)
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deals damage to this unit's target with buffs or other modifiers
        /// </summary>
        /// <returns></returns>
        public virtual bool AttackTarget()
        {
            double damage_to_deal = 0;

            try
            {
                if (!(IsDead || IsTargetDead))
                {
                    //TODO

                    return Target.ReceiveAttackFromTarget(damage_to_deal);
                }
                else
                    throw new InvalidOperationException("This unit is either dead, or the target has been killed...");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to attack the target...");
                Trace.WriteLine($@"Error in Model! Unit -> AttackTarget(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Receives the opposing unit's damage
        /// </summary>
        /// <param name="damage_to_receive"></param>
        /// <returns></returns>
        public virtual bool ReceiveAttackFromTarget(double damage_to_receive)
        {
            try
            {
                //TODO

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in Model! Failed to receive the attack from target...");
                Trace.WriteLine($@"Error in Model! Unit -> ReceiveAttackFromTarget(): \n\t{ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is the one to be used for messaging to agent. Problem is
        /// Should we include the sequence of action done to target or 
        /// we should just let it play randomly. 
        /// 
        /// If we include sequence, then we need a way to get that sequence of
        /// action
        /// </remarks>
        public override string ToString()
        {
            //TODO
            return base.ToString();
        }
    }
}
