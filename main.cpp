#define NOMINMAX

#include <iostream>
#include <queue>
#include <sstream>
#include <thread>
#include <Windows.h>
#include <string>
#include <algorithm>
#include <random>
#include <iterator>
#include <iomanip>
#include <ctime>
#include <map>

#include "sc2api/sc2_api.h"
#include "sc2lib/sc2_lib.h"
#include "sc2utils/sc2_manage_process.h"
#include "sc2utils/sc2_arg_parser.h"
#include "ModelService.h"

namespace KoKeKoKo
{
	namespace Agent
	{
		using namespace sc2;

		//Manages the agent in the environment
		class KoKeKoKoBot : public Agent
		{
			private:
				Services::ModelService* _instance;
				std::atomic<bool> _shouldkeepupdating;
				std::map<std::string, std::thread*> _threads;
				std::mutex _actionslock;
				std::queue<std::string> _actions;
				std::string _currentaction;
				std::vector<Point3D> expansions_;
				Point3D staging_location_;
				GameInfo game_info_;
				Tag scouter;

				//Returns a single action from a queue of actions
				std::string GetAnActionFromMessage()
				{
					std::string action = "";

					try
					{
						#if _DEBUG
							std::cout << "GetAnActionFromMessage() has been called!" << std::endl;
						#endif

						_actionslock.lock();
						if (!_actions.empty())
						{
							action = _actions.front();
							_actions.pop();
							#if _DEBUG
								std::cout << "GetAnActionFromMessage() -> An action has been retrieved from queue..." << std::endl;
							#endif							
						}
					}
					catch (const std::exception& ex)
					{
						std::cout << ex.what() << std::endl;
					}

					_actionslock.unlock();
					return action;
				}

				//Retrieves the recently sent messages from model service and stores in a queue of actions
				void GetMessageFromModelService()
				{
					for (int failures = 0; _shouldkeepupdating;)
					{
						try
						{
							#if _DEBUG
								std::cout << "GetMessageFromModelService() has been called!" << std::endl;
							#endif

							_actionslock.lock();
							/*for (auto message = _instance->GetMessageFromModelService(); !message.empty();)
							{
								#if _DEBUG
									std::cout << "GetMessageFromModelService() -> Retrieving message: " << message.front() << std::endl;
								#endif
								
								std::stringstream new_actions(message.front());
								message.pop();

								std::cout << "The sent actions are:" << std::endl;
								for (std::string current_action = ""; std::getline(new_actions, current_action, ',');)
								{
									std::cout << current_action << std::endl;
									_actions.push(current_action);
								}
							}*/
							_actionslock.unlock();

							//Check if there is a message again after 15 seconds
							std::this_thread::sleep_for(std::chrono::milliseconds(15000));
						}
						catch (const std::exception& ex)
						{
							std::cout << ex.what() << std::endl;
							if (++failures >= 5)
								throw std::exception("Error Occurred! Exceeded number of tries to get message from model service...");
						}
					}
				}

				void SendUpdatesToModelService()
				{
					for (int failures = 0; _shouldkeepupdating;)
					{
						try
						{
							#if _DEBUG
								std::cout << "SendUpdatesToModelService() has been called!" << std::endl;
							#endif

							const ObservationInterface* current_observation = Observation();
							std::string message = "Macromanagement:";
							
							//Send the current state of the agent
							//Macro details
							//don't forget gameloop, combine supply
							message += std::to_string(current_observation->GetGameLoop()) + ","; //Gameloop
							message += std::to_string(current_observation->GetPlayerID()) + ","; //Player ID
							message += std::to_string(current_observation->GetMinerals()) + ","; //Minerals
							message += std::to_string(current_observation->GetVespene()) + ","; //Vespene
							message += std::to_string(current_observation->GetFoodUsed()) + ","; //Supply
							message += std::to_string(CountOf(UNIT_TYPEID::TERRAN_SCV)); //No. of Workers
							for (const auto& upgrade : current_observation->GetUpgrades())
								message += ("," + upgrade.to_string()); //Upgrades
							message += ":";

							//Self Army details
							for (const auto& unit : current_observation->GetUnits(Unit::Alliance::Self))
							{
								if (unit->is_alive)
									message += (std::to_string(current_observation->GetPlayerID()) + "," + unit->unit_type.to_string() + "," + std::to_string(unit->tag) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + "\n");
							}
							message += "~";

							//Enemy Army Units
							for (const auto& unit : current_observation->GetUnits(Unit::Alliance::Enemy))
							{
								if (unit->is_alive)
									message += (std::to_string(unit->alliance) + "," + unit->unit_type.to_string() + "," + std::to_string(unit->tag) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + "\n");
							}

							#if _DEBUG
								std::cout << "SendUpdatesToModelService() -> Finished processing message..." << std::endl;
							#endif

							//Send this message to model service
							//_instance->SendMessageToModelService(message);

							//Send another update after 10 seconds
							std::this_thread::sleep_for(std::chrono::milliseconds(10000));
						}
						catch (const std::exception& ex)
						{
							std::cout << ex.what() << std::endl;
							if (++failures >= 5)
								throw std::exception("Error Occurred! Exceeeded number of tries to send updates to model service...");
						}
					}
				}

				struct IsTownHall
				{
					bool operator()(const Unit& unit)
					{
						switch (unit.unit_type.ToType())
						{
						case UNIT_TYPEID::TERRAN_COMMANDCENTER: return true;
						case UNIT_TYPEID::TERRAN_ORBITALCOMMAND: return true;
						case UNIT_TYPEID::TERRAN_ORBITALCOMMANDFLYING: return true;
						case UNIT_TYPEID::TERRAN_PLANETARYFORTRESS: return true;
						default: return false;
						}
					}
				};

				struct IsStructure
				{
					IsStructure(const ObservationInterface* obs) : observation_(obs) {};

					bool operator()(const Unit& unit) 
					{
						auto& attributes = observation_->GetUnitTypeData().at(unit.unit_type).attributes;
						bool is_structure = false;
						for (const auto& attribute : attributes) 
						{
							if (attribute == Attribute::Structure) 
							{
								is_structure = true;
							}
						}
						return is_structure;
					}

					const ObservationInterface* observation_;
				};

				struct IsArmy 
				{
					IsArmy(const ObservationInterface* obs) : observation_(obs) {}

					bool operator()(const Unit& unit) 
					{
						auto attributes = observation_->GetUnitTypeData().at(unit.unit_type).attributes;
						for (const auto& attribute : attributes) 
						{
							if (attribute == Attribute::Structure) 
							{
								return false;
							}
						}
						switch (unit.unit_type.ToType()) 
						{
							case UNIT_TYPEID::TERRAN_SCV: return false;
							case UNIT_TYPEID::TERRAN_MULE: return false;
							case UNIT_TYPEID::TERRAN_NUKE: return false;
							default: return true;
						}
					}

					const ObservationInterface* observation_;
				};

				void StoreGameInfo(GameInfo start_game_info)
				{
					game_info_ = start_game_info;
				}

				bool FindEnemyPosition(Point2D& target_pos) 
				{
					if (game_info_.enemy_start_locations.empty())
					{
						return false;
					}
					target_pos = game_info_.enemy_start_locations.front();
					return true;
				}

				bool TryFindRandomPathableLocation(const Unit* unit, Point2D& target_pos) 
				{
					// First, find a random point inside the playable area of the map.
					float playable_w = game_info_.playable_max.x - game_info_.playable_min.x;
					float playable_h = game_info_.playable_max.y - game_info_.playable_min.y;

					// The case where game_info_ does not provide a valid answer
					if (playable_w == 0 || playable_h == 0) 
					{
						playable_w = 236;
						playable_h = 228;
					}

					target_pos.x = playable_w * GetRandomFraction() + game_info_.playable_min.x;
					target_pos.y = playable_h * GetRandomFraction() + game_info_.playable_min.y;

					// Now send a pathing query from the unit to that point. Can also query from point to point,
					// but using a unit tag wherever possible will be more accurate.
					// Note: This query must communicate with the game to get a result which affects performance.
					// Ideally batch up the queries (using PathingDistanceBatched) and do many at once.
					float distance = Query()->PathingDistance(unit, target_pos);

					return distance > 0.1f;
				}

				void /*ScoutWithUnit*/Scout() 
				{
					const ObservationInterface* observation = Observation();
					const Unit* unit = nullptr;
					Units enemy_units = observation->GetUnits(Unit::Alliance::Enemy);
					Units workers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));

					unit = GetRandomEntry(workers);
					/*if (!unit->orders.empty()) 
					{
						return;
					}*/
					Point2D target_pos;
					if (scouter == NULL)
						scouter = unit->tag;
					if (FindEnemyPosition(target_pos)) 
					{
						if (Distance2D(unit->pos, target_pos) < 20 && enemy_units.empty()) 
						{
							if (TryFindRandomPathableLocation(unit, target_pos)) 
							{
								Actions()->UnitCommand(unit, ABILITY_ID::SMART, target_pos);
								return;
							}
						}
						else if (!enemy_units.empty())
						{
							Actions()->UnitCommand(unit, ABILITY_ID::ATTACK, enemy_units.front());
							return;
						}
						Actions()->UnitCommand(unit, ABILITY_ID::SMART, target_pos);
					}
					else {
						if (TryFindRandomPathableLocation(unit, target_pos)) 
						{
							Actions()->UnitCommand(unit, ABILITY_ID::SMART, target_pos);
						}
					}
				}

				//Gets a random unit and assigns it to the action
				bool ExecuteBuildAbility(ABILITY_ID action, UNIT_TYPEID unit = UNIT_TYPEID::TERRAN_SCV, bool redoable = false)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit));
					const Unit* target = nullptr;
					float random_x_coordinate = GetRandomScalar(), random_y_coordinate = GetRandomScalar();
					Point2D build_location = Point2D(staging_location_ + Point2D(random_x_coordinate, random_y_coordinate) * 15.0f);

					//If there should not be 2 or more unit working on the same action
					if (!redoable)
					{
						for (const auto& unit : units)
						{
							for (const auto& order : unit->orders)
							{
								if (order.ability_id == action)
									return false; //do nothing
							}
						}
					}

					if (!units.empty())
					{
						target = GetRandomEntry(units);
						if (target->tag != scouter)
						{
							if (target->build_progress != 1 && target->orders.empty())
							{
								return false;
							}
							if (action == ABILITY_ID::BUILD_REFINERY)
							{
								Actions()->UnitCommand(target, action, FindNearestOf(target->pos, UNIT_TYPEID::NEUTRAL_VESPENEGEYSER));
								return true;
							}
							else if (Query()->Placement(action, build_location, target))
							{
								Actions()->UnitCommand(target, action, build_location);
								return true;
							}
						}
					}
					return false;
				}

				bool ExecuteBuildAddOnAbility(ABILITY_ID action, Tag base_structure, bool redoable = false)
				{
					float rx = GetRandomScalar();
					float ry = GetRandomScalar();
					const Unit* unit = Observation()->GetUnit(base_structure);

					if (unit->build_progress != 1) 
					{
						return false;
					}

					Point2D build_location = Point2D(unit->pos.x + (rx * 15), unit->pos.y + (ry * 15));

					Units units = Observation()->GetUnits(Unit::Self, IsStructure(Observation()));

					if (Query()->Placement(action, unit->pos, unit)) 
					{
						Actions()->UnitCommand(unit, action);
						return true;
					}

					float distance = std::numeric_limits<float>::max();
					for (const auto& u : units) 
					{
						float d = Distance2D(u->pos, build_location);
						if (d < distance) 
						{
							distance = d;
						}
					}
					if (distance < 6) {
						return false;
					}

					if (Query()->Placement(action, build_location, unit)) 
					{
						Actions()->UnitCommand(unit, action, build_location);
						return true;
					}
					return false;
				}

				void StoreExpansions(std::vector<Point3D> expansion_holder)
				{
					expansions_ = expansion_holder;
				}

				bool TryBuildStructure(AbilityID ability_type_for_structure, Point2D location, bool isExpansion = false) {

					const ObservationInterface* observation = Observation();
					Units workers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));

					//if we have no workers Don't build
					if (workers.empty()) 
					{
						return false;
					}

					// Check to see if there is already a worker heading out to build it
					for (const auto& worker : workers) 
					{
						for (const auto& order : worker->orders) 
						{
							if (order.ability_id == ability_type_for_structure) 
							{
								return false;
							}
						}
					}

					// If no worker is already building one, get a random worker to build one
					const Unit* unit = GetRandomEntry(workers);

					// Check to see if unit can make it there
					if (Query()->PathingDistance(unit, location) < 0.1f) 
					{
						return false;
					}
					if (!isExpansion) {
						for (const auto& expansion : expansions_) 
						{
							if (Distance2D(location, Point2D(expansion.x, expansion.y)) < 7) 
							{
								return false;
							}
						}
					}
					// Check to see if unit can build there
					if (Query()->Placement(ability_type_for_structure, location) && unit->tag != scouter) 
					{
						Actions()->UnitCommand(unit, ability_type_for_structure, location);
						return true;
					}
					return false;

				}

				//Gets a random unit and assigns it to the action
				bool ExecuteTrainAbility(ABILITY_ID action, UNIT_TYPEID unit, bool redoable = false)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit));
					const Unit* target = nullptr;

					//If there should not be 2 or more unit doing the same action
					if (!redoable)
					{
						for (const auto& unit : units)
						{
							for (const auto& order : unit->orders)
							{
								if (order.ability_id == action)
									return false;
							}
						}
					}
					if (!units.empty())
					{
						target = GetRandomEntry(units);
						if (target != nullptr)
						{
							Actions()->UnitCommand(target, action);
							return true;
						}
					}
					return false;
				}

				//Gets a random unit and assigns it to the action
				bool ExecuteResearchAbility(ABILITY_ID action, UNIT_TYPEID unit, bool redoable = false)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit));
					const Unit* target = nullptr;

					//If there should not be 2 or more unit doing the same action
					if (!redoable)
					{
						for (const auto& unit : units)
						{
							for (const auto& order : unit->orders)
							{
								if (order.ability_id == action)
									return false;
							}
						}
					}

					if (!units.empty())
					{
						target = GetRandomEntry(units);
						if (target != nullptr)
						{
							Actions()->UnitCommand(target, action);
							return true;
						}
					}
					return false;

				}
				void MineIdleWorkers(const Unit* worker)//, AbilityID worker_gather_command, UnitTypeID vespene_building_type) {
				{
					const ObservationInterface* observation = Observation();
					Units bases = observation->GetUnits(Unit::Alliance::Self, IsTownHall());
					Units geysers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_REFINERY));

					const Unit* valid_mineral_patch = nullptr;

					if (bases.empty())
					{
						return;
					}

					for (const auto& geyser : geysers) {

						if (geyser->assigned_harvesters < geyser->ideal_harvesters)
						{
							Actions()->UnitCommand(worker, ABILITY_ID::HARVEST_GATHER, geyser);
							return;
						}
					}
					//Search for a base that is missing workers.
					for (const auto& base : bases)
					{
						//If we have already mined out here skip the base.
						if (base->ideal_harvesters == 0 || base->build_progress != 1)
						{
							continue;
						}
						if (base->assigned_harvesters < base->ideal_harvesters)
						{
							valid_mineral_patch = FindNearestOf(base->pos, UNIT_TYPEID::NEUTRAL_MINERALFIELD);
							Actions()->UnitCommand(worker, ABILITY_ID::HARVEST_GATHER, valid_mineral_patch);
							return;
						}
					}

					if (!worker->orders.empty()) {
						return;
					}

					//If all workers are spots are filled just go to any base.
					const Unit* random_base = GetRandomEntry(bases);
					valid_mineral_patch = FindNearestOf(random_base->pos, UNIT_TYPEID::NEUTRAL_MINERALFIELD);
					Actions()->UnitCommand(worker, ABILITY_ID::HARVEST_GATHER, valid_mineral_patch);
				}

				void ManageWorkers()//UNIT_TYPEID worker_type = UNIT_TYPEID::TERRAN_SCV , AbilityID worker_gather_command, UNIT_TYPEID vespene_building_type) 
				{
					const ObservationInterface* observation = Observation();
					Units bases = observation->GetUnits(Unit::Alliance::Self, IsTownHall());
					Units geysers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_REFINERY));

					if (bases.empty())
					{
						return;
					}

					for (const auto& base : bases) {
						//If we have already mined out or still building here skip the base.
						if (base->ideal_harvesters == 0 || base->build_progress != 1)
						{
							continue;
						}
						//if base is
						if (base->assigned_harvesters > base->ideal_harvesters)
						{
							Units workers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));

							for (const auto& worker : workers)
							{
								if (!worker->orders.empty())
								{
									if (worker->orders.front().target_unit_tag == base->tag)
									{
										//This should allow them to be picked up by mineidleworkers()
										MineIdleWorkers(worker);
										return;
									}
								}
							}
						}
					}
					Units workers = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));
					for (const auto& geyser : geysers)
					{
						if (geyser->ideal_harvesters == 0 || geyser->build_progress != 1)
						{
							continue;
						}
						if (geyser->assigned_harvesters > geyser->ideal_harvesters)
						{
							for (const auto& worker : workers)
							{
								if (!worker->orders.empty())
								{
									if (worker->orders.front().target_unit_tag == geyser->tag)
									{
										//This should allow them to be picked up by mineidleworkers()
										MineIdleWorkers(worker);
										return;
									}
								}
							}
						}
						else if (geyser->assigned_harvesters < geyser->ideal_harvesters)
						{
							for (const auto& worker : workers)
							{
								if (!worker->orders.empty())
								{
									//This should move a worker that isn't mining gas to gas
									const Unit* target = observation->GetUnit(worker->orders.front().target_unit_tag);
									if (target == nullptr)
									{
										continue;
									}
									if (target->unit_type != UNIT_TYPEID::TERRAN_REFINERY)
									{
										//This should allow them to be picked up by mineidleworkers()
										MineIdleWorkers(worker);
										return;
									}
								}
							}
						}
					}
				}

				void AttackWithUnit(Tag ally_tag, Tag enemy_tag) 
				{
					//If unit isn't doing anything make it attack.
					const ObservationInterface* observation = Observation();
					const Unit* ally_unit = observation->GetUnit(ally_tag);
					const Unit* enemy_unit = observation->GetUnit(enemy_tag);
					if (enemy_unit == nullptr) 
					{
						return;
					}

					if (ally_unit->orders.empty()) 
					{
						Actions()->UnitCommand(ally_unit, ABILITY_ID::ATTACK, enemy_unit);
						return;
					}

					//If the unit is doing something besides attacking, make it attack.
					if (ally_unit->orders.front().ability_id != ABILITY_ID::ATTACK) 
					{
						Actions()->UnitCommand(ally_unit, ABILITY_ID::ATTACK, enemy_unit);
					}
				}

				bool TryBuildRefinery()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 75)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_REFINERY) > 0 && observation->GetMinerals() < 75)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_REFINERY);
				}

				void StoreStagingLocation(Point3D stage_holder)
				{
					staging_location_ = stage_holder;
				}

				//Command Center Units
				bool TryBuildCommandCenter()
				{
					const ObservationInterface* observation = Observation();
					float minimum_distance = std::numeric_limits<float>::max();
					Point3D closest_expansion;
					for (const auto& expansion : expansions_) 
					{
						float current_distance = Distance2D(Observation()->GetStartLocation(), expansion);
						if (current_distance < .01f) 
						{
							continue;
						}

						if (current_distance < minimum_distance) 
						{
							if (Query()->Placement(ABILITY_ID::BUILD_COMMANDCENTER, expansion))
							{
								closest_expansion = expansion;
								minimum_distance = current_distance;
							}
						}
					}
					//only update staging location up till 3 bases.
					if (TryBuildStructure(ABILITY_ID::BUILD_COMMANDCENTER,closest_expansion, true) && observation->GetUnits(Unit::Self, IsTownHall()).size() < 4 && observation->GetMinerals() >= 400) 
					{
						staging_location_ = Point3D(((staging_location_.x + closest_expansion.x) / 2), ((staging_location_.y + closest_expansion.y) / 2), ((staging_location_.z + closest_expansion.z) / 2));
						return true;
					}
					return false;

				}

				bool TryCommandCenterMorphOrbitalCommand()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}
					return (ExecuteResearchAbility(ABILITY_ID::MORPH_ORBITALCOMMAND, UNIT_TYPEID::TERRAN_COMMANDCENTER));
				}

				bool TryOrbitalCommandSummonMule()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_ORBITALCOMMAND) > 0)
					{
						return false;
					}
					return (ExecuteResearchAbility(ABILITY_ID::EFFECT_CALLDOWNMULE, UNIT_TYPEID::TERRAN_ORBITALCOMMAND));
				}

				bool TryCommandCenterMorphPlanetaryFortress()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return (ExecuteResearchAbility(ABILITY_ID::MORPH_PLANETARYFORTRESS, UNIT_TYPEID::TERRAN_COMMANDCENTER));
				}

				bool TrainSCV()
				{
					const ObservationInterface* observation = Observation();
					Units bases = observation->GetUnits(Unit::Alliance::Self, IsTownHall());
					for (const auto& base : bases) 
					{
						//if there is a base with less than ideal workers
						if (base->assigned_harvesters < base->ideal_harvesters && base->build_progress == 1) 
						{
							if (observation->GetMinerals() >= 50) 
							{
								return ExecuteTrainAbility(ABILITY_ID::TRAIN_SCV, base->unit_type);
							}
						}
					}
					return false;
				}

				bool TryBuildSupplyDepot()
				{
					const ObservationInterface* observation = Observation();

					if (observation->GetFoodUsed() <= observation->GetFoodCap() - 2 && observation->GetMinerals() < 100)
						return false;

					return ExecuteBuildAbility(ABILITY_ID::BUILD_SUPPLYDEPOT);
				}
				
				//Barracks Units
				bool TryBuildBarracks()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_BARRACKS);
				}

				bool TrainMarine()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_MARINE, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainReaper()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_REAPER, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainMarauder()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 25)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_MARAUDER, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainGhost()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && CountOf(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 150 && observation->GetVespene() < 125)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_GHOST, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				//Barracks Addons
				bool TryBuildBarracksTechLab()
				{
					const ObservationInterface* observation = Observation();
					Units barracks = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_BARRACKS));

					for (const auto& barrack : barracks)
					{
						if (observation->GetUnit(barrack->add_on_tag) == nullptr)
						{
							if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
							{
								return false;
							}
							return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_TECHLAB_BARRACKS, barrack->tag);
						}
					}
					return false;
				}

				bool TryBarracksTechLabResearchCombatShield()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_COMBATSHIELD, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBarracksTechLabResearchStimpack()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_STIMPACK, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBarracksTechLabResearchConcussiveShells()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_CONCUSSIVESHELLS, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBuildBarracksReactor()
				{
					const ObservationInterface* observation = Observation();
					Units barracks = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_BARRACKS));

					for (const auto& barrack : barracks)
					{
						if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
						{
							return false;
						}
						return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_REACTOR_BARRACKS, barrack->tag);
					}
					return false;
				}

				//Factory Units
				bool TryBuildFactory()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_FACTORY);
				}

				bool TryTrainHellion()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_HELLION, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTransformHellionHellbat(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_HELLBAT);
						return true;
					}
					else
						return false;
				}

				bool TryTrainWidowMine()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 75 && observation->GetVespene() < 25)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_WIDOWMINE, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTrainSiegeTank()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 125)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_SIEGETANK, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTransformSiegeMode(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_SIEGEMODE);
						return true;
					}
					else
						return false;
				}

				bool TryTransformUnsiege(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_UNSIEGE);
						return true;
					}
					else
						return false;
				}

				bool TryTrainCyclone()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_CYCLONE, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTrainHellbat()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && CountOf(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_HELLBAT, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTransformHellbatHellion(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_HELLION);
						return true;
					}
					else
						return false;
				}

				bool TryTrainThor()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && CountOf(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 300 && observation->GetVespene() < 200)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_THOR, UNIT_TYPEID::TERRAN_FACTORY);
				}

				//Factory Addons
				bool TryBuildFactoryTechLab()
				{
					const ObservationInterface* observation = Observation();
					Units factorys = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_FACTORY));

					for (const auto& factory : factorys)
					{
						if (observation->GetUnit(factory->add_on_tag) == nullptr)
						{
							if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
							{
								return false;
							}
							return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_TECHLAB_FACTORY, factory->tag);
						}
					}
					return false;
				}

				bool TryFactoryResearchInfernalPreIgniter()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_INFERNALPREIGNITER, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryFactoryResearchMagFieldAccelerator()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_MAGFIELDLAUNCHERS, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryFactoryResearchDrillingClaws()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 75 && observation->GetVespene() < 75)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_DRILLINGCLAWS, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryBuildFactoryReactor()
				{
					const ObservationInterface* observation = Observation();
					Units factorys = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_FACTORY));

					for (const auto& factory : factorys)
					{
						if (observation->GetUnit(factory->add_on_tag) == nullptr)
						{
							if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
							{
								return false;
							}
							return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_REACTOR_FACTORY, factory->tag);
						}
					}
					return false;
				}

				//Starport Units
				bool TryBuildStarport()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountOf(UNIT_TYPEID::TERRAN_FACTORY) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_STARPORT);
				}

				bool  TryTrainViking()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 150 && observation->GetVespene() < 75)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_VIKINGFIGHTER, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTransformVikingAssault(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_VIKINGASSAULTMODE);
						return true;
					}
					else
						return false;
				}

				bool TryTransformVikingFighter(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_VIKINGFIGHTERMODE);
						return true;
					}
					else
						return false;
				}

				bool TryTrainMedivac()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_MEDIVAC, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTrainLiberator()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_LIBERATOR, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTransformLiberatorLiberatorAG(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_LIBERATORAGMODE);
						return true;
					}
					else
						return false;
				}

				bool TryTransformLiberatorAGLiberator(Tag uID)
				{
					if (uID != NULL)
					{
						const Unit* unit = Observation()->GetUnit(uID);
						Actions()->UnitCommand(unit, ABILITY_ID::MORPH_LIBERATORAAMODE);
						return true;
					}
					else
						return false;
				}

				bool TryTrainRaven()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 200)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_RAVEN, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryRavenSummonPointDefenseDrone()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_RAVEN) > 0)
					{
						return false;
					}
					return (ExecuteResearchAbility(ABILITY_ID::EFFECT_POINTDEFENSEDRONE, UNIT_TYPEID::TERRAN_RAVEN));
				}

				bool TryRavenSummonAutoTurret()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_RAVEN) > 0)
					{
						return false;
					}
					return (ExecuteResearchAbility(ABILITY_ID::EFFECT_AUTOTURRET, UNIT_TYPEID::TERRAN_RAVEN));
				}

				bool TryTrainBanshee()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_BANSHEE, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTrainBattlecruiser()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && CountOf(UNIT_TYPEID::TERRAN_FUSIONCORE) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 400 && observation->GetVespene() < 300)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_BATTLECRUISER, UNIT_TYPEID::TERRAN_STARPORT);
				}

				//Starport Addons
				bool TryBuildStarportTechLab()
				{
					const ObservationInterface* observation = Observation();
					Units starports = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_STARPORT));

					for (const auto& starport : starports)
					{
						if (observation->GetUnit(starport->add_on_tag) == nullptr)
						{
							if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
							{
								return false;
							}
							return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_TECHLAB_STARPORT, starport->tag);
						}
					}
					return false;
				}

				bool TryStarportResearchCorvidReactor()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_RAVENCORVIDREACTOR, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchCloakingField()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_BANSHEECLOAKINGFIELD, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchHyperflightRotors()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_BANSHEEHYPERFLIGHTROTORS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchAdvancedBallistics()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_ADVANCEDBALLISTICS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchRapidReignitionSystem()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_HIGHCAPACITYFUELTANKS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryBuildStarportReactor()
				{
					const ObservationInterface* observation = Observation();
					Units starports = observation->GetUnits(Unit::Self, IsUnit(UNIT_TYPEID::TERRAN_STARPORT));

					for (const auto& starport : starports)
					{
						if (observation->GetUnit(starport->add_on_tag) == nullptr)
						{
							if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
							{
								return false;
							}
							return  ExecuteBuildAddOnAbility(ABILITY_ID::BUILD_REACTOR_STARPORT, starport->tag);
						}
					}
					return false;
				}

				bool TryBuildFusionCore()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_FUSIONCORE) > 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_FUSIONCORE);
				}

				bool TryFusionCoreResearchResearchWeaponRefit()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_BATTLECRUISERWEAPONREFIT, UNIT_TYPEID::TERRAN_FUSIONCORE);
				}

				bool TryBuildArmory()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_ARMORY);
				}

				bool TryArmoryResearchVehicleWeapons()
				{
					const ObservationInterface* observation = Observation();
					for (UpgradeID i : observation->GetUpgrades())
					{
						if (observation->GetMinerals() < 100 & observation->GetVespene() < 100)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEWEAPONSLEVEL1 && observation->GetMinerals() < 175 & observation->GetVespene() < 175)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEWEAPONSLEVEL2 && observation->GetMinerals() < 250 & observation->GetVespene() < 250)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEWEAPONSLEVEL3)
						{
							return false;
						}
					}

					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_TERRANVEHICLEWEAPONS, UNIT_TYPEID::TERRAN_ARMORY);
				}

				bool TryArmoryResearchShipWeapons()
				{
					const ObservationInterface* observation = Observation();
					for (UpgradeID i : observation->GetUpgrades())
					{
						if (observation->GetMinerals() < 100 & observation->GetVespene() < 100)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANSHIPWEAPONSLEVEL1 && observation->GetMinerals() < 175 & observation->GetVespene() < 175)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANSHIPWEAPONSLEVEL2 && observation->GetMinerals() < 250 & observation->GetVespene() < 250)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANSHIPWEAPONSLEVEL3)
						{
							return false;
						}
					}

					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_TERRANSHIPWEAPONS, UNIT_TYPEID::TERRAN_ARMORY);
				}

				bool TryArmoryResearchVehicleShipPlating()
				{
					const ObservationInterface* observation = Observation();
					for (UpgradeID i : observation->GetUpgrades())
					{
						if (observation->GetMinerals() < 100 & observation->GetVespene() < 100)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEANDSHIPARMORSLEVEL1 && observation->GetMinerals() < 175 & observation->GetVespene() < 175)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEANDSHIPARMORSLEVEL2 && observation->GetMinerals() < 250 & observation->GetVespene() < 250)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANVEHICLEANDSHIPARMORSLEVEL3)
						{
							return false;
						}
					}

					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_TERRANVEHICLEANDSHIPPLATING, UNIT_TYPEID::TERRAN_ARMORY);
				}

				bool TryBuildBunker()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_BUNKER) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_BUNKER);
				}

				bool TryBuildEngineeringBay()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER) < 1 && observation->GetMinerals() < 125)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) > 0 && observation->GetMinerals() < 125)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_ENGINEERINGBAY);
				}

				bool TryEngineeringBayResearchInfantryArmor()
				{
					const ObservationInterface* observation = Observation();
					for (UpgradeID i : observation->GetUpgrades())
					{
						if (observation->GetMinerals() < 100 & observation->GetVespene() < 100)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYARMORSLEVEL1 && observation->GetMinerals() < 175 & observation->GetVespene() < 175)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYARMORSLEVEL2 && observation->GetMinerals() < 250 & observation->GetVespene() < 250)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYARMORSLEVEL3)
						{
							return false;
						}
					}

					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_TERRANINFANTRYARMOR, UNIT_TYPEID::TERRAN_ENGINEERINGBAY);
				}

				bool TryEngineeringBayResearchInfantryWeapon()
				{
					const ObservationInterface* observation = Observation();
					for (UpgradeID i : observation->GetUpgrades())
					{
						if (observation->GetMinerals() < 100 & observation->GetVespene() < 100)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYWEAPONSLEVEL1 && observation->GetMinerals() < 175 & observation->GetVespene() < 175)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYWEAPONSLEVEL2 && observation->GetMinerals() < 250 & observation->GetVespene() < 250)
						{
							return false;
						}
						else if (i == UPGRADE_ID::TERRANINFANTRYWEAPONSLEVEL3)
						{
							return false;
						}
					}

					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_TERRANINFANTRYWEAPONS, UNIT_TYPEID::TERRAN_ENGINEERINGBAY);
				}

				bool TryBuildGhostAcademy()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 50)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 50)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_GHOSTACADEMY);
				}

				bool TryGhostAcademyResearchPersonalCloaking()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::RESEARCH_PERSONALCLOAKING, UNIT_TYPEID::TERRAN_GHOSTACADEMY);
				}

				bool TryGhostAcademyBuildNuke()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_NUKE, UNIT_TYPEID::TERRAN_GHOSTACADEMY);
				}

				bool TryBuildMissileTurret()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountOf(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) < 1 && observation->GetMinerals() < 100)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_MISSILETURRET) > 0 && observation->GetMinerals() < 100)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_MISSILETURRET);
				}

				bool TryBuildSensorTower()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountOf(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) < 1 && observation->GetMinerals() < 125 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_SENSORTOWER) > 0 && observation->GetMinerals() < 125 && observation->GetVespene() < 100)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_SENSORTOWER);
				}

			public:
				bool scout = true;
				Units known_units;
				std::string GenerateInitializeString()
				{
					std::string test = "INITIALIZE;BRONZE;";
					uint32_t playerID = Observation()->GetPlayerID();
					test = test + std::to_string(playerID) + ";";
					Units units = Observation()->GetUnits(Unit::Alliance::Self);
					for (const auto& unit : units)
					{
						test = test + std::to_string(unit->tag) + "," + UnitTypeToName(unit->unit_type) + "," + std::to_string(unit->pos.x) +  "," + std::to_string(unit->pos.y) + "$";
					}
					test.pop_back();
					std::cout << test << std::endl;
					return test;
				}
				std::string GenerateUpdateString()
				{
					std::string test = "UPDATE;" + std::to_string(Observation()->GetGameLoop() / 22.4) + ";";
					Units units = Observation()->GetUnits(Unit::Alliance::Self);
					int terminate = -1, terminated = units.size();
					for (const auto& unit : units)
					{
						//test = test + std::to_string(unit->tag) + "," + UnitTypeToName(unit->unit_type) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + "$";
						test += (std::to_string(unit->tag) + "," + UnitTypeToName(unit->unit_type) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + ((++terminate < terminated) ? "$" : ""));
					}
					//test.pop_back();
					test = test + ";" + std::to_string(Observation()->GetMinerals()) + "," + std::to_string(Observation()->GetVespene()) + "," + std::to_string(Observation()->GetFoodCap()) + "," + std::to_string(CountOf(UNIT_TYPEID::TERRAN_SCV));
					auto upgrades = Observation()->GetUpgrades();
					//if (upgrades.empty())
					//	test = test; //+ ";";
					//else
					//{
					//	test = test + ",";
					//	for (const auto& upgrade : upgrades)
					//	{
					//		test = test + UpgradeIDToName(upgrade) + ",";
					//	}
					//}
					if (!upgrades.empty())
					{
						for (const auto& upgrade : upgrades)
							test = test + "," + UpgradeIDToName(upgrade);
					}

					if (!known_units.empty())
					{
						test += ";";
						terminate = -1, terminated = known_units.size();
						for (const auto& unit : known_units)
						{
							test += (std::to_string(unit->tag) + "," + UnitTypeToName(unit->unit_type) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + ((++terminate < terminated) ? "$" : ""));
						}
					}
					
					std::cout << "From GenerateUpdateString: " << test << std::endl;
					return test;
					/*if (known_units.empty())
					{
						std::cout << test << std::endl;
						return test;
					}
					else
					{
						for (const auto& unit : known_units)
						{
							test = test + std::to_string(unit->tag) + "," + UnitTypeToName(unit->unit_type) + "," + std::to_string(unit->pos.x) + "," + std::to_string(unit->pos.y) + "$";
						}
						test.pop_back();
						std::cout << test << std::endl;
						return test;
					}*/

				}
				KoKeKoKoBot()
				{
					//Perform intializations
					//_instance = Model::ModelRepositoryService::StartModelRepositoryService();
					_shouldkeepupdating = false;
					_threads = std::map<std::string, std::thread*>();
					_actions = std::queue<std::string>();
					_currentaction = "";
				}

				virtual void OnGameStart() final
				{
					auto service = Services::ModelService::CreateNewModelService();
					auto trash = service->UpdateModelService(GenerateInitializeString());
					_actions = service->UpdateModelService(GenerateUpdateString());
					std::istringstream raw_actions(_actions.front());
					std::string raw_action = "";
					while (std::getline(raw_actions, raw_action, ','))
						_actions.push(raw_action);
					_currentaction = _actions.front();
					_actions.pop();

					////We periodically get message and send updates to model service
					//StartSendingUpdatesToModelService();
					//Get possible expansion positions
					std::vector<Point3D> expansions_;
					expansions_ = search::CalculateExpansionLocations(Observation(), Query());
					StoreExpansions(expansions_);

					//Get Start location
					Point3D startLocation_ = Observation()->GetStartLocation();
					Point3D staging_location_;
					GameInfo game_info_ = Observation()->GetGameInfo();
					StoreGameInfo(game_info_);
					//startLocation_ 
					StoreStagingLocation(startLocation_);

					//We periodically get message and send updates to model service
					//StartSendingUpdatesToModelService();

					//#if _DEBUG
					//	std::cout << "Finished calling StartSendingUpdatesToModelService()! Proceeding to start the game... Getting the current action...";
					//#endif


					////while there is still no action, we wait for a message
					//while (_currentaction.empty())
					//{
					//	#if _DEBUG
					//		std::cout << "Current action is still empty! Cannot continue to the game..." << std::endl;
					//	#endif
					//	_currentaction = GetAnActionFromMessage();
					//	if (_currentaction.empty())
					//		//Wait for 5 seconds if there is still no message
					//		std::this_thread::sleep_for(std::chrono::milliseconds(5000)); 
					//}

					std::cout << _currentaction << std::endl;
				}

				virtual void OnStep() final
				{
					ManageWorkers();
					//If there is action available
					/*if (!_currentaction.empty())
					////If there is action available
					//if (!_currentaction.empty())
					//{
					//	ExecuteAbility(_currentaction);
					//	_currentaction = "";
					//}
					//else
					//	_currentaction = GetAnActionFromMessage();
					//	
					//std::cout << _currentaction << std::endl;
					auto service = Services::ModelService::CreateNewModelService();
					if (_actions.empty() || !service->ShouldOperationsContinue())
					{
						_actions = service->UpdateModelService("UPDATE");
					}
					else
						_currentaction = GetAnActionFromMessage();
						
					std::cout << _currentaction << std::endl;*/
					/*
					if (CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER) < 2 && CountOf(UNIT_TYPEID::TERRAN_ORBITALCOMMAND) < 1)
					{
						TryBuildCommandCenter();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_SCV) < 50)
					{
						TrainSCV();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 5)
					{
						TryBuildSupplyDepot();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_REFINERY) < 4)
					{
						TryBuildRefinery();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) < 2)
					{
						TryBuildBarracks();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 1 && CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) < 1)
					{
						TryBuildBarracksTechLab();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 1 && CountOf(UNIT_TYPEID::TERRAN_BARRACKSREACTOR) < 1)
					{
						TryBuildBarracksReactor();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 1)
					{
						TryCommandCenterMorphOrbitalCommand();
					}
					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) == 1)
					{
						TryBarracksTechLabResearchStimpack();
					}*/
					if (scout)
					{
						Scout();
						scout = false;
					}
					/*if (Observation()->GetGameLoop() % 500 == 0)
					{
						for (const auto& unit : known_units)
						{
							std::cout << unit->tag << " " << unit->unit_type.to_string() << std::endl;
						}
					}*/
					auto service = Services::ModelService::CreateNewModelService();
					if (_actions.empty() || !service->ShouldOperationsContinue())
					{
						_actions = service->UpdateModelService(GenerateUpdateString());
						std::istringstream raw_actions(_actions.front());
						std::string raw_action = "";
						while (std::getline(raw_actions, raw_action, ','))
							_actions.push(raw_action);
						_currentaction = _actions.front();
						_actions.pop();
						//while(Coordinator().Update());
					}

					//Do the current action
					if (ExecuteAbility(_currentaction))
					{
						_currentaction = _actions.front();
						_actions.pop();
						std::cout << _currentaction << std::endl;
					}

					/*while (!ExecuteAbility(_currentaction))
					Coordinator().SetStepSize(0);*/

				}

				virtual void OnGameEnd() final
				{
					auto service = Services::ModelService::CreateNewModelService();
					_actions = service->UpdateModelService("TERMINATE");
					service->StopModelService();
					
					//Dispose the modelrepositoryservice instance
					//_instance->~ModelRepositoryService();
					_instance = nullptr;
				}

				virtual void OnUnitCreated() final
				{

				}

				virtual void OnUnitIdle(const Unit* unit) final
				{
					if(unit->unit_type == UNIT_TYPEID::TERRAN_SCV)
						MineIdleWorkers(unit);
				}

				virtual void OnUnitDestroyed(const Unit* unit) final
				{

				}

				virtual void OnUnitEnterVision(const Unit* unit) final
				{
					/*bool isStructure = false;
					auto attributes = Observation()->GetUnitTypeData().at(unit->unit_type).attributes;
					for (const auto& attribute : attributes)
					{
						if (attribute == Attribute::Structure)
						{
							isStructure = true;
						}
					}
					if(unit->alliance == Unit::Alliance::Enemy && !isStructure && unit->last_seen_game_loop != NULL)
						known_units.insert(known_units.end(), unit);*/
					Units enemy_units = Observation()->GetUnits(Unit::Alliance::Enemy);
					/*for (const auto& unit : enemy_units)
					{
						if(unit->last_seen_game_loop != NULL)
							known_units.insert(known_units.end(), unit);
					}*/
					known_units = enemy_units;
				}

				//A helper function that finds a nearest entity from a position
				const Unit* FindNearestOf(Point2D source_position, UNIT_TYPEID target_type)
				{
					Units units = Observation()->GetUnits(IsUnit(target_type));
					const Unit* target = nullptr;
					float distance = std::numeric_limits<float>::max(), temporary_distance = 0;

					for (const auto& unit : units)
					{
						temporary_distance = DistanceSquared2D(unit->pos, source_position);
						if (temporary_distance < distance)
						{
							distance = temporary_distance;
							target = unit;
						}
					}

					return target;
				}

				//A helper function that counts entity
				size_t CountOf(UNIT_TYPEID unit_type_ID, Unit::Alliance alliance = Unit::Alliance::Self)
				{
					return Observation()->GetUnits(alliance, IsUnit(unit_type_ID)).size();
				}

				void ExecuteUnitAbility(Tag ally, Tag enemy, std::string ability)
				{
					const Unit* enemy_unit = Observation()->GetUnit(enemy);
					const Unit* ally_unit = Observation()->GetUnit(ally);
					if (ability.find("MORPH_HELLBAT") != std::string::npos)
					{
						TryTransformHellionHellbat(ally);
					}
					else if (ability.find("MORPH_HELLION") != std::string::npos)
					{
						TryTransformHellbatHellion(ally);
					}
					else if (ability.find("MORPH_SIEGEMODE") != std::string::npos)
					{
						if(Distance2D(ally_unit->pos, enemy_unit->pos) < 11)
							TryTransformSiegeMode(ally);
					}
					else if (ability.find("MORPH_UNSIEGE") != std::string::npos)
					{
						if (Distance2D(ally_unit->pos, enemy_unit->pos) > 13)
							TryTransformUnsiege(ally);
					}
					else if (ability.find("MORPH_VIKINGFIGHTERMODE") != std::string::npos)
					{
						TryTransformVikingFighter(ally);
					}
					else if (ability.find("MORPH_VIKINGASSAULTMODE") != std::string::npos)
					{
						if (enemy_unit->is_flying)
							TryTransformVikingAssault(ally);
					}
					else if (ability.find("MORPH_LIBERATORAGMODE") != std::string::npos)
					{
						TryTransformLiberatorLiberatorAG(ally);
					}
					else if (ability.find("MORPH_LIBERATORAAMODE") != std::string::npos)
					{
						if (enemy_unit->is_flying)
							TryTransformLiberatorAGLiberator(ally);
					}
					else if (ability.find("ATTACK") != std::string::npos)
					{
						AttackWithUnit(ally, enemy);
					}
					else return;
				}

				//Executes a valid action that is within the ability_type of the agent
				bool ExecuteAbility(std::string ability)
				{
					if (ability.find("BUILD_REFINERY") != std::string::npos)
					{
						return TryBuildRefinery();
					}
					else if (ability.find("BUILD_COMMANDCENTER") != std::string::npos)
					{
						return TryBuildCommandCenter();
					}
					else if (ability.find("MORPH_ORBITALCOMMAND") != std::string::npos)
					{
						return TryCommandCenterMorphOrbitalCommand();
					}
					else if (ability.find("EFFECT_CALLDOWNMULE") != std::string::npos)
					{
						return TryOrbitalCommandSummonMule();
					}
					else if (ability.find("MORPH_PLANETARYFORTRESS") != std::string::npos)
					{
						return TryCommandCenterMorphPlanetaryFortress();
					}
					else if (ability.find("TRAIN_SCV") != std::string::npos)
					{
						return TrainSCV();
					}
					else if (ability.find("BUILD_SUPPLYDEPOT") != std::string::npos)
					{
						return TryBuildSupplyDepot();
					}
					else if (ability.find("BUILD_BARRACKS") != std::string::npos)
					{
						return TryBuildBarracks();
					}
					else if (ability.find("TRAIN_MARINE") != std::string::npos)
					{
						return TrainMarine();
					}
					else if (ability.find("TRAIN_REAPER") != std::string::npos)
					{
						return TrainReaper();
					}
					else if (ability.find("TRAIN_MARAUDER") != std::string::npos)
					{
						return TrainMarauder();
					}
					else if (ability.find("TRAIN_GHOST") != std::string::npos)
					{
						return TrainGhost();
					}
					else if (ability.find("BUILD_TECHLAB_BARRACKS") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildBarracksTechLab();
					}
					else if (ability.find("RESEARCH_COMBATSHIELD") != std::string::npos)
					{
						return TryBarracksTechLabResearchCombatShield();
					}
					else if (ability.find("RESEARCH_STIMPACK") != std::string::npos)
					{
						return TryBarracksTechLabResearchStimpack();
					}
					else if (ability.find("RESEARCH_CONCUSSIVESHELLS") != std::string::npos)
					{
						return TryBarracksTechLabResearchConcussiveShells();
					}
					else if (ability.find("BUILD_REACTOR_BARRACKS") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildBarracksReactor();
					}
					else if (ability.find("BUILD_FACTORY") != std::string::npos)
					{
						return TryBuildFactory();
					}
					else if (ability.find("TRAIN_HELLION") != std::string::npos)
					{
						return TryTrainHellion();
					}
					else if (ability.find("TRAIN_WIDOWMINE") != std::string::npos)
					{
						return TryTrainWidowMine();
					}
					else if (ability.find("TRAIN_SIEGETANK") != std::string::npos)
					{
						return TryTrainSiegeTank();
					}
					else if (ability.find("TRAIN_CYCLONE") != std::string::npos)
					{
						return TryTrainCyclone();
					}
					else if (ability.find("TRAIN_HELLBAT") != std::string::npos)
					{
						return TryTrainHellbat();
					}
					else if (ability.find("TRAIN_THOR") != std::string::npos)
					{
						return TryTrainThor();
					}
					else if (ability.find("BUILD_TECHLAB_FACTORY") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildFactoryTechLab();
					}
					else if (ability.find("RESEARCH_INFERNALPREIGNITER") != std::string::npos)
					{
						return TryFactoryResearchInfernalPreIgniter();
					}
					else if (ability.find("RESEARCH_MAGFIELDLAUNCHERS") != std::string::npos)
					{
						return TryFactoryResearchMagFieldAccelerator();
					}
					else if (ability.find("RESEARCH_DRILLINGCLAWS") != std::string::npos)
					{
						return TryFactoryResearchDrillingClaws();
					}
					else if (ability.find("BUILD_REACTOR_FACTORY") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildFactoryReactor();
					}
					else if (ability.find("BUILD_STARPORT") != std::string::npos)
					{
						return TryBuildStarport();
					}
					else if (ability.find("TRAIN_VIKINGFIGHTER") != std::string::npos)
					{
						return TryTrainViking();
					}
					else if (ability.find("TRAIN_MEDIVAC") != std::string::npos)
					{
						return TryTrainMedivac();
					}
					else if (ability.find("TRAIN_LIBERATOR") != std::string::npos)
					{
						return TryTrainLiberator();
					}
					else if (ability.find("TRAIN_RAVEN") != std::string::npos)
					{
						return TryTrainRaven();
					}
					else if (ability.find("EFFECT_AUTOTURRET") != std::string::npos)
					{
						return TryRavenSummonAutoTurret();
					}
					else if (ability.find("TRAIN_BANSHEE") != std::string::npos)
					{
						return TryTrainBanshee();
					}
					else if (ability.find("TRAIN_BATTLECRUISER") != std::string::npos)
					{
						return TryTrainBattlecruiser();
					}
					else if (ability.find("BUILD_REACTOR_STARPORT") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildStarportReactor();
					}
					else if (ability.find("BUILD_TECHLAB_STARPORT") != std::string::npos) //Not on Offical Typeenums
					{
						return TryBuildStarportTechLab();
					}
					else if (ability.find("RESEARCH_HIGHCAPACITYFUELTANKS") != std::string::npos)
					{
						return TryStarportResearchRapidReignitionSystem();
					}
					else if (ability.find("RESEARCH_RAVENCORVIDREACTOR") != std::string::npos)
					{
						return TryStarportResearchCorvidReactor();
					}
					else if (ability.find("RESEARCH_BANSHEECLOAKINGFIELD") != std::string::npos)
					{
						return TryStarportResearchCloakingField();
					}
					else if (ability.find("RESEARCH_BANSHEEHYPERFLIGHTROTORS") != std::string::npos)
					{
						return TryStarportResearchHyperflightRotors();
					}
					else if (ability.find("RESEARCH_ADVANCEDBALLISTICS") != std::string::npos)
					{
						return TryStarportResearchAdvancedBallistics();
					}
					else if (ability.find("BUILD_FUSIONCORE") != std::string::npos)
					{
						return TryBuildFusionCore();
					}
					else if (ability.find("RESEARCH_BATTLECRUISERWEAPONREFIT") != std::string::npos)
					{
						return TryFusionCoreResearchResearchWeaponRefit();
					}
					else if (ability.find("BUILD_ARMORY") != std::string::npos)
					{
						return TryBuildArmory();
					}
					else if (ability.find("RESEARCH_TERRANVEHICLEWEAPONS") != std::string::npos)
					{
						return TryArmoryResearchVehicleWeapons();
					}
					else if (ability.find("RESEARCH_TERRANSHIPWEAPONS") != std::string::npos)
					{
						return TryArmoryResearchShipWeapons();
					}
					else if (ability.find("RESEARCH_TERRANVEHICLEANDSHIPPLATING") != std::string::npos)
					{
						return TryArmoryResearchVehicleShipPlating();
					}
					else if (ability.find("BUILD_BUNKER") != std::string::npos)
					{
						return TryBuildBunker();
					}
					else if (ability.find("BUILD_ENGINEERINGBAY") != std::string::npos)
					{
						return TryBuildEngineeringBay();
					}
					else if (ability.find("RESEARCH_TERRANINFANTRYWEAPONS") != std::string::npos)
					{
						return TryEngineeringBayResearchInfantryWeapon();
					}
					else if (ability.find("RESEARCH_TERRANINFANTRYARMOR") != std::string::npos)
					{
						return TryEngineeringBayResearchInfantryArmor();
					}
					else if (ability.find("BUILD_GHOSTACADEMY") != std::string::npos)
					{
						return TryBuildGhostAcademy();
					}
					else if (ability.find("RESEARCH_PERSONALCLOAKING") != std::string::npos)
					{
						return TryGhostAcademyResearchPersonalCloaking();
					}
					else if (ability.find("BUILD_NUKE") != std::string::npos)
					{
						return TryGhostAcademyBuildNuke();
					}
					else if (ability.find("BUILD_MISSILETURRET") != std::string::npos)
					{
						return TryBuildMissileTurret();
					}
					else if (ability.find("BUILD_SENSORTOWER") != std::string::npos)
					{
						return TryBuildSensorTower();
					}
					else if (ability.find("SURRENDER") != std::string::npos)
					{
						Debug()->DebugEndGame();
					}
					else
						return true;
				}				
		};
	}
}

static sc2::Difficulty GetDifficultyFromString(std::string InDifficulty)
{
	if (InDifficulty == "VeryEasy")
	{
		return sc2::Difficulty::VeryEasy;
	}
	if (InDifficulty == "Easy")
	{
		return sc2::Difficulty::Easy;
	}
	if (InDifficulty == "Medium")
	{
		return sc2::Difficulty::Medium;
	}
	if (InDifficulty == "MediumHard")
	{
		return sc2::Difficulty::MediumHard;
	}
	if (InDifficulty == "Hard")
	{
		return sc2::Difficulty::Hard;
	}
	if (InDifficulty == "HardVeryHard")
	{
		return sc2::Difficulty::HardVeryHard;
	}
	if (InDifficulty == "VeryHard")
	{
		return sc2::Difficulty::VeryHard;
	}
	if (InDifficulty == "CheatVision")
	{
		return sc2::Difficulty::CheatVision;
	}
	if (InDifficulty == "CheatMoney")
	{
		return sc2::Difficulty::CheatMoney;
	}
	if (InDifficulty == "CheatInsane")
	{
		return sc2::Difficulty::CheatInsane;
	}

	return sc2::Difficulty::Easy;
}

static sc2::Race GetRaceFromString(const std::string & RaceIn)
{
	std::string race(RaceIn);
	std::transform(race.begin(), race.end(), race.begin(), ::tolower);

	if (race == "terran")
	{
		return sc2::Race::Terran;
	}
	else if (race == "protoss")
	{
		return sc2::Race::Protoss;
	}
	else if (race == "zerg")
	{
		return sc2::Race::Zerg;
	}
	else if (race == "random")
	{
		return sc2::Race::Random;
	}

	return sc2::Race::Random;
}

struct ConnectionOptions
{
	int32_t GamePort;
	int32_t StartPort;
	std::string ServerAddress;
	bool ComputerOpponent;
	sc2::Difficulty ComputerDifficulty;
	sc2::Race ComputerRace;
};

static void ParseArguments(int argc, char *argv[], ConnectionOptions &connect_options)
{
	sc2::ArgParser arg_parser(argv[0]);
	arg_parser.AddOptions({
		{ "-g", "--GamePort", "Port of client to connect to", false },
		{ "-o", "--StartPort", "Starting server port", false },
		{ "-l", "--LadderServer", "Ladder server address", false },
		{ "-c", "--ComputerOpponent", "If we set up a computer oppenent" },
		{ "-a", "--ComputerRace", "Race of computer oppent"},
		{ "-d", "--ComputerDifficulty", "Difficulty of computer oppenent"}
		});
	arg_parser.Parse(argc, argv);
	std::string GamePortStr;
	if (arg_parser.Get("GamePort", GamePortStr)) {
		connect_options.GamePort = atoi(GamePortStr.c_str());
	}
	std::string StartPortStr;
	if (arg_parser.Get("StartPort", StartPortStr)) {
		connect_options.StartPort = atoi(StartPortStr.c_str());
	}
	arg_parser.Get("LadderServer", connect_options.ServerAddress);
	std::string CompOpp;
	if (arg_parser.Get("ComputerOpponent", CompOpp))
	{
		connect_options.ComputerOpponent = true;
		std::string CompRace;
		if (arg_parser.Get("ComputerRace", CompRace))
		{
			connect_options.ComputerRace = GetRaceFromString(CompRace);
		}
		std::string CompDiff;
		if (arg_parser.Get("ComputerDifficulty", CompDiff))
		{
			connect_options.ComputerDifficulty = GetDifficultyFromString(CompDiff);
		}

	}
	else
	{
		connect_options.ComputerOpponent = false;
	}
}

Services::ModelService* Services::ModelService::Instance = nullptr;
int main(int argc, char* argv[])
{
#ifdef _DEBUG
	try
	{
		sc2::Coordinator coordinator; //= new sc2::Coordinator();
		KoKeKoKo::Agent::KoKeKoKoBot* kokekokobot = new KoKeKoKo::Agent::KoKeKoKoBot();
		Services::ModelService* modelservice = Services::ModelService::CreateNewModelService();

		//auto trash = modelservice->UpdateModelService("INITIALIZE;BRONZE;1;4333502465,TERRAN_SCV,117.500000,27.500000$4331929601,TERRAN_SCV,117.500000,24.500000$4331667457,TERRAN_SCV,116.500000,22.500000$4332191745,TERRAN_SCV,115.500000,22.500000$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4331143169,TERRAN_SCV,117.500000,22.500000$4331405313,TERRAN_SCV,117.500000,23.500000$4332716033,TERRAN_SCV,114.500000,22.500000$4334026753,TERRAN_SCV,117.500000,28.500000$4333764609,TERRAN_SCV,112.500000,22.500000$4333240321,TERRAN_SCV,113.500000,22.500000$4332978177,TERRAN_SCV,117.500000,26.500000$4332453889,TERRAN_SCV,117.500000,25.500000");
		//auto reply = modelservice->UpdateModelService("UPDATE;0.000000;4333502465,TERRAN_SCV,117.500000,27.500000$4331929601,TERRAN_SCV,117.500000,24.500000$4331667457,TERRAN_SCV,116.500000,22.500000$4332191745, TERRAN_SCV, 115.500000, 22.500000$4330881025, TERRAN_COMMANDCENTER, 114.500000, 25.500000$4331143169, TERRAN_SCV, 117.500000, 22.500000$4331405313, TERRAN_SCV, 117.500000, 23.500000$4332716033, TERRAN_SCV, 114.500000, 22.500000$4334026753, TERRAN_SCV, 117.500000, 28.500000$4333764609, TERRAN_SCV, 112.500000, 22.500000$4333240321, TERRAN_SCV, 113.500000, 22.500000$4332978177, TERRAN_SCV, 117.500000, 26.500000$4332453889, TERRAN_SCV, 117.500000, 25.500000; 50, 0, 15, 12");
		////auto reply2 = modelservice->UpdateModelService("UPDATE;0.178571;4333502465,TERRAN_SCV,117.529297,27.407227$4331929601,TERRAN_SCV,117.548828,24.417969$4331667457,TERRAN_SCV,116.583008,22.452881$4332191745,TERRAN_SCV,115.587891,22.460938$4331405313,TERRAN_SCV,117.561035,23.426758$4332716033,TERRAN_SCV,114.590332,22.467529$4332978177,TERRAN_SCV,117.534180,26.409668$4333240321,TERRAN_SCV,113.592285,22.472168$4331143169,TERRAN_SCV,117.574707,22.440674$4334026753,TERRAN_SCV,117.525635,28.407227$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4332453889,TERRAN_SCV,117.540039,25.413330$4333764609,TERRAN_SCV,112.433594,22.538086;50,0,15,12");
		//auto reply2 = modelservice->UpdateModelService("UPDATE;0.000000;4333502465,TERRAN_SCV,26.500000,132.500000$4331667457,TERRAN_SCV,27.500000,137.500000$4331929601,TERRAN_SCV,26.500000,135.500000$4332191745,TERRAN_SCV,28.500000,137.500000$4330881025,TERRAN_COMMANDCENTER,29.500000,134.500000$4332453889,TERRAN_SCV,26.500000,134.500000$4332716033,TERRAN_SCV,29.500000,137.500000$4334026753,TERRAN_SCV,26.500000,131.500000$4333764609,TERRAN_SCV,31.500000,137.500000$4333240321,TERRAN_SCV,30.500000,137.500000$4332978177,TERRAN_SCV,26.500000,133.500000$4331405313,TERRAN_SCV,26.500000,136.500000$4331143169,TERRAN_SCV,26.500000,137.500000;50,0,15,12;");
		////auto reply3 = modelservice->UpdateModelService("UPDATE;0.357143;4333502465,TERRAN_SCV,117.606201,27.168457$4331929601,TERRAN_SCV,117.675781,24.204102$4331667457,TERRAN_SCV,116.798828,22.330078$4332191745,TERRAN_SCV,115.816406,22.359375$4332978177,TERRAN_SCV,117.623047,26.176270$4332716033,TERRAN_SCV,114.824463,22.383545$4334026753,TERRAN_SCV,117.593506,28.166016$4332453889,TERRAN_SCV,117.645264,25.187500$4333764609,TERRAN_SCV,112.214355,22.655518$4331143169,TERRAN_SCV,117.769287,22.285156$4331405313,TERRAN_SCV,117.719727,23.236328$4333240321,TERRAN_SCV,113.832031,22.398926$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000;50,0,15,12");
		//auto reply3 = modelservice->UpdateModelService("UPDATE;12.276786;4333502465,TERRAN_SCV,24.850830,137.475830$4331667457,TERRAN_SCV,24.991455,137.302246$4331929601,TERRAN_SCV,62.431152,118.181396$4332191745,TERRAN_SCV,27.337158,139.648193$4331143169,TERRAN_SCV,24.959961,138.628174$4332978177,TERRAN_SCV,29.632813,137.344238$4331405313,TERRAN_SCV,24.374512,136.118164$4332453889,TERRAN_SCV,23.692627,135.552246$4330881025,TERRAN_COMMANDCENTER,29.500000,134.500000$4337696769,TERRAN_SCV,27.334473,136.447266$4333764609,TERRAN_SCV,27.287598,136.567871$4332716033,TERRAN_SCV,28.985352,137.447510$4334026753,TERRAN_SCV,24.963623,136.077148$4333240321,TERRAN_SCV,26.071777,134.062256;25,0,15,13;");
		////auto reply4 = modelservice->UpdateModelService("UPDATE;24.687500;4333502465,TERRAN_SCV,119.745605,22.269531$4331929601,TERRAN_SCV,119.650879,23.916016$4331667457,TERRAN_SCV,116.367188,21.016602$4332191745,TERRAN_SCV,115.566162,19.329346$4332453889,TERRAN_SCV,117.425049,25.103027$4337696769,TERRAN_SCV,117.026611,23.959229$4338483201,TERRAN_SCV,117.390381,24.464844$4331405313,TERRAN_SCV,119.754395,22.265869$4333240321,TERRAN_SCV,119.642822,26.289551$4332716033,TERRAN_SCV,114.079102,20.420898$4334026753,TERRAN_SCV,116.650879,20.335693$4331143169,TERRAN_SCV,119.042969,21.370117$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4332978177,TERRAN_SCV,117.816650,22.483643$4333764609,TERRAN_SCV,52.738770,86.368408;125,0,15,14");
		////auto reply5 = modelservice->UpdateModelService("UPDATE;48.616071;4333502465,TERRAN_SCV,119.232422,22.610596$4331929601,TERRAN_SCV,119.629150,24.948975$4332191745,TERRAN_SCV,115.712646,20.219238$4332716033,TERRAN_SCV,114.154297,21.368408$4333764609,TERRAN_SCV,49.269775,127.868652$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4334026753,TERRAN_SCV,116.650879,20.335693$4337696769,TERRAN_SCV,118.069580,23.303223$4339269633,TERRAN_SCV,116.658203,23.551514$4339007489,TERRAN_REFINERY,121.500000,29.500000$4332453889,TERRAN_SCV,120.630615,24.685303$4332978177,TERRAN_SCV,119.044189,21.369385$4331667457,TERRAN_SCV,116.600586,20.340088$4331405313,TERRAN_SCV,119.752441,22.268066$4331143169,TERRAN_SCV,119.043457,21.370117$4333240321,TERRAN_SCV,119.642822,26.289551;240,0,15,148797356056,TERRAN_REFINERY,22.500000,130.500000$4335861761,TERRAN_SCV,29.809082,138.215332$4335337473,TERRAN_SCV,35.995361,135.128662$4338221057,TERRAN_SUPPLYDEPOTLOWERED,28.000000,130.000000$4334288897,TERRAN_COMMANDCENTER,29.500000,134.500000$4339793921,TERRAN_BARRACKS,37.500000,134.500000");
		////auto reply6 = modelservice->UpdateModelService("UPDATE;48.794643;4333502465,TERRAN_SCV,118.682129,22.237305$4331929601,TERRAN_SCV,119.628662,25.593506$4332191745,TERRAN_SCV,116.001709,20.740723$4332453889,TERRAN_SCV,120.630615,24.685303$4333240321,TERRAN_SCV,119.642822,26.289551$4331667457,TERRAN_SCV,116.386719,20.341064$4339007489,TERRAN_REFINERY,121.500000,29.500000$4332978177,TERRAN_SCV,119.044189,21.369385$4333764609,TERRAN_SCV,49.935059,127.644287$4334026753,TERRAN_SCV,116.650879,20.335693$4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4332716033,TERRAN_SCV,114.201416,21.942383$4337696769,TERRAN_SCV,117.587402,23.599609$4339269633,TERRAN_SCV,116.658691,23.550537$4331143169,TERRAN_SCV,119.043457,21.370117$4331405313,TERRAN_SCV,119.752441,22.268066;245,0,15,148797356056,TERRAN_REFINERY,22.500000,130.500000$4335861761,TERRAN_SCV,29.809082,138.215332$4335337473,TERRAN_SCV,35.995361,135.128662$4338221057,TERRAN_SUPPLYDEPOTLOWERED,28.000000,130.000000$4334288897,TERRAN_COMMANDCENTER,29.500000,134.500000$4339793921,TERRAN_BARRACKS,37.500000,134.500000");
		//for (int counter = 0; counter < 1000; counter++)
		//{
		//	if (!reply.empty())
		//	{
		//		std::cout << "Current Action: " << reply.front() << std::endl;
		//		reply.pop();
		//	}


		//	if (!modelservice->ShouldOperationsContinue())
		//		reply = modelservice->UpdateModelService("UPDATE;0.000000;4330881025,TERRAN_COMMANDCENTER,114.500000,25.500000$4333502465,TERRAN_SCV,117.500000,27.500000$4331929601,TERRAN_SCV,117.500000,24.500000$4331143169,TERRAN_SCV,117.500000,22.500000$4332191745,TERRAN_SCV,115.500000,22.500000$4331405313,TERRAN_SCV,117.500000,23.500000$4331667457,TERRAN_SCV,116.500000,22.500000$4333764609,TERRAN_SCV,112.500000,22.500000$4334026753,TERRAN_SCV,117.500000,28.500000$4333240321,TERRAN_SCV,113.500000,22.500000$4332978177,TERRAN_SCV,117.500000,26.500000$4332716033,TERRAN_SCV,114.500000,22.500000$4332453889,TERRAN_SCV,117.500000,25.500000;50,0,15,12");
		//	else
		//		std::this_thread::sleep_for(std::chrono::milliseconds(1000));
		//}

		//modelservice->StopModelService();
		//char c;
		//std::cin >> c;
		coordinator.SetMultithreaded(true);
		coordinator.LoadSettings(argc, argv);
		coordinator.SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
		//coordinator.SetStepSize(22);
		coordinator.LaunchStarcraft();
		coordinator.StartGame(sc2::kMapBelShirVestigeLE);
		while (coordinator.Update());
	}
	catch (const std::exception& ex)
	{
		std::cout << "(C++)Error Occurred! " << ex.what() << std::endl;
	}
	catch (...)
	{
		std::cout << "(C++)An Application Error Occurred! Please debug the program." << std::endl;
	}

	return 0;
}
#else
	try{
		KoKeKoKo::Agent::KoKeKoKoBot* kokekokobot = new KoKeKoKo::Agent::KoKeKoKoBot();
		ConnectionOptions Options;
		ParseArguments(argc, argv, Options);

		sc2::Coordinator coordinator;
		if (!coordinator.LoadSettings(argc, argv)) {
			return 0;
		}

		// Add the custom bot, it will control the players.
		int num_agents;
		if (Options.ComputerOpponent)
		{
			num_agents = 1;
			coordinator.SetParticipants({
				CreateParticipant(sc2::Race::Terran, kokekokobot),
				CreateComputer(Options.ComputerRace, Options.ComputerDifficulty)
				});
		}
		else
		{
			num_agents = 2;
			coordinator.SetParticipants({
				CreateParticipant(sc2::Race::Terran, kokekokobot),
				});
		}

		// Start the game.

		// Step forward the game simulation.
		std::cout << "Connecting to port " << Options.GamePort << std::endl;
		coordinator.Connect(Options.GamePort);
		coordinator.SetupPorts(num_agents, Options.StartPort, false);
		// Step forward the game simulation.
		coordinator.JoinGame();
		coordinator.SetTimeoutMS(10000);
		std::cout << " Successfully joined game" << std::endl;
		while (coordinator.Update()) {
		}
	}
	catch (const std::exception& ex)
	{
		std::cout << "(C++)Error Occurred! " << ex.what() << std::endl;
	}
	catch (...)
	{
		std::cout << "(C++)An Application Error Occurred! Please debug the program." << std::endl;
	}

	return 0;
}
#endif
