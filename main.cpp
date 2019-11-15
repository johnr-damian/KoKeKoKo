#define NOMINMAX

#include <iostream>
#include <queue>
#include <sc2api/sc2_api.h>
#include <sstream>
#include <thread>
#include <Windows.h>
#include <iomanip>
#include <ctime>

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

				//Gets a random unit and assigns it to the action
				bool ExecuteBuildAbility(ABILITY_ID action, UNIT_TYPEID unit = UNIT_TYPEID::TERRAN_SCV, bool redoable = false)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit));
					const Unit* target = nullptr;
					float random_x_coordinate = GetRandomScalar(), random_y_coordinate = GetRandomScalar();

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

					target = units.front();
					if (action == ABILITY_ID::BUILD_REFINERY)
						Actions()->UnitCommand(target, action, FindNearestOf(target->pos, UNIT_TYPEID::NEUTRAL_VESPENEGEYSER));
					else
						Actions()->UnitCommand(target, action, Point2D((target->pos.x + random_x_coordinate) * 15.0f, (target->pos.y + random_y_coordinate) * 15.0f));
					return true;
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

					target = units.front();
					Actions()->UnitCommand(target, action);
					return true;
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

					target = units.front();
					Actions()->UnitCommand(target, action);
					return true;
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

				//Command Center Units
				bool TryBuildCommandCenter()
				{
					const ObservationInterface* observation = Observation();

					if (CountOf(UNIT_TYPEID::TERRAN_SUPPLYDEPOT, Unit::Alliance::Self) < 1 && observation->GetMinerals() < 400)
					{
						return false;
					}

					if (CountOf(UNIT_TYPEID::TERRAN_COMMANDCENTER, Unit::Alliance::Self) > 0 && observation->GetMinerals() < 400)
					{
						return false;
					}

					return ExecuteBuildAbility(ABILITY_ID::BUILD_COMMANDCENTER);
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

					if (observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50)
					{
						return false;
					}
					return ExecuteTrainAbility(ABILITY_ID::TRAIN_SCV, UNIT_TYPEID::TERRAN_COMMANDCENTER);
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

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_BARRACKS);
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

					if (CountOf(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_BARRACKS);
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

				bool TryTransformHellionHellbat()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_HELLBAT, UNIT_TYPEID::TERRAN_HELLION);
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

				bool TryTransformSiegeMode()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_SIEGEMODE, UNIT_TYPEID::TERRAN_SIEGETANK);
				}

				bool TryTransformUnsiege()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_UNSIEGE, UNIT_TYPEID::TERRAN_SIEGETANK);
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

				bool TryTransformHellbatHellion()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_HELLION, UNIT_TYPEID::TERRAN_HELLIONTANK);
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

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_FACTORY);
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

					if (CountOf(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_FACTORY);
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

				bool TryTransformVikingAssault()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_VIKINGASSAULTMODE, UNIT_TYPEID::TERRAN_VIKINGFIGHTER);
				}

				bool TryTransformVikingFighter()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_VIKINGFIGHTERMODE, UNIT_TYPEID::TERRAN_VIKINGASSAULT);
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

				bool TryTransformLiberatorLiberatorAG()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_LIBERATORAGMODE, UNIT_TYPEID::TERRAN_LIBERATOR);
				}

				bool TryTransformLiberatorAGLiberator()
				{
					return ExecuteResearchAbility(ABILITY_ID::MORPH_LIBERATORAAMODE, UNIT_TYPEID::TERRAN_LIBERATORAG);
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

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_STARPORT);
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

					if (CountOf(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return ExecuteResearchAbility(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_STARPORT);
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
					_actions = service->UpdateModelService("UPDATE");
					_currentaction = _actions.front();
					_actions.pop();

					////We periodically get message and send updates to model service
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

					//Do the current action
					ExecuteAbility(_currentaction);
					_currentaction = _actions.front();
					_actions.pop();
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
					try
					{
						switch (unit->unit_type.ToType())
						{
							case UNIT_TYPEID::TERRAN_SCV:
							{
								double probability = (((double)rand()) / RAND_MAX);

								if (probability <= 0.5)
									Actions()->UnitCommand(unit, ABILITY_ID::SMART, FindNearestOf(unit->pos, UNIT_TYPEID::NEUTRAL_MINERALFIELD));
								else
									Actions()->UnitCommand(unit, ABILITY_ID::SMART, FindNearestOf(unit->pos, UNIT_TYPEID::NEUTRAL_VESPENEGEYSER));

								break;
							}
						}
					}
					catch (const std::exception& ex)
					{
						std::cout << ex.what() << std::endl;
					}
				}

				virtual void OnUnitDestroyed(const Unit* unit) final
				{

				}

				virtual void OnUnitEnterVision(const Unit* unit) final
				{

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
				size_t CountOf(UNIT_TYPEID unit_type, Unit::Alliance alliance = Unit::Alliance::Self)
				{
					return Observation()->GetUnits(alliance, IsUnit(unit_type)).size();
				}

				//Executes a valid action that is within the ability_type of the agent
				void ExecuteAbility(std::string ability)
				{
					if (ability.find("BUILD_REFINERY") != std::string::npos)
					{
						TryBuildRefinery();
					}
					else if (ability.find("BUILD_COMMANDCENTER") != std::string::npos)
					{
						TryBuildCommandCenter();
					}
					else if (ability.find("MORPH_ORBITALCOMMAND") != std::string::npos)
					{
						TryCommandCenterMorphOrbitalCommand();
					}
					else if (ability.find("EFFECT_CALLDOWNMULE") != std::string::npos)
					{
						TryOrbitalCommandSummonMule();
					}
					else if (ability.find("MORPH_PLANETARYFORTRESS") != std::string::npos)
					{
						TryCommandCenterMorphPlanetaryFortress();
					}
					else if (ability.find("TRAIN_SCV") != std::string::npos)
					{
						TrainSCV();
					}
					else if (ability.find("BUILD_SUPPLYDEPOT") != std::string::npos)
					{
						TryBuildSupplyDepot();
					}
					else if (ability.find("BUILD_BARRACKS") != std::string::npos)
					{
						TryBuildBarracks();
					}
					else if (ability.find("TRAIN_MARINE") != std::string::npos)
					{
						TrainMarine();
					}
					else if (ability.find("TRAIN_REAPER") != std::string::npos)
					{
						TrainReaper();
					}
					else if (ability.find("TRAIN_MARAUDER") != std::string::npos)
					{
						TrainMarauder();
					}
					else if (ability.find("TRAIN_GHOST") != std::string::npos)
					{
						TrainGhost();
					}
					else if (ability.find("BUILD_BARRACKSTECHLAB") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildBarracksTechLab();
					}
					else if (ability.find("RESEARCH_COMBATSHIELD") != std::string::npos)
					{
						TryBarracksTechLabResearchCombatShield();
					}
					else if (ability.find("RESEARCH_STIMPACK") != std::string::npos)
					{
						TryBarracksTechLabResearchStimpack();
					}
					else if (ability.find("RESEARCH_CONCUSSIVESHELLS") != std::string::npos)
					{
						TryBarracksTechLabResearchConcussiveShells();
					}
					else if (ability.find("BUILD_BARRACKSREACTOR") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildBarracksReactor();
					}
					else if (ability.find("BUILD_FACTORY") != std::string::npos)
					{
						TryBuildFactory();
					}
					else if (ability.find("TRAIN_HELLION") != std::string::npos)
					{
						TryTrainHellion();
					}
					else if (ability.find("MORPH_HELLBAT") != std::string::npos)
					{
						TryTransformHellionHellbat();
					}
					else if (ability.find("TRAIN_WIDOWMINE") != std::string::npos)
					{
						TryTrainWidowMine();
					}
					else if (ability.find("TRAIN_SIEGETANK") != std::string::npos)
					{
						TryTrainSiegeTank();
					}
					else if (ability.find("MORPH_SIEGEMODE") != std::string::npos)
					{
						TryTransformSiegeMode();
					}
					else if (ability.find("MORPH_UNSIEGE") != std::string::npos)
					{
						TryTransformUnsiege();
					}
					else if (ability.find("TRAIN_CYCLONE") != std::string::npos)
					{
						TryTrainCyclone();
					}
					else if (ability.find("TRAIN_HELLBAT") != std::string::npos)
					{
						TryTrainHellbat();
					}
					else if (ability.find("MORPH_HELLION") != std::string::npos)
					{
						TryTransformHellbatHellion();
					}
					else if (ability.find("TRAIN_THOR") != std::string::npos)
					{
						TryTrainThor();
					}
					else if (ability.find("BUILD_FACTORYTECHLAB") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildFactoryTechLab();
					}
					else if (ability.find("RESEARCH_INFERNALPREIGNITER") != std::string::npos)
					{
						TryFactoryResearchInfernalPreIgniter();
					}
					else if (ability.find("RESEARCH_MAGFIELDLAUNCHERS") != std::string::npos)
					{
						TryFactoryResearchMagFieldAccelerator();
					}
					else if (ability.find("RESEARCH_DRILLINGCLAWS") != std::string::npos)
					{
						TryFactoryResearchDrillingClaws();
					}
					else if (ability.find("BUILD_FACTORYREACTOR") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildFactoryReactor();
					}
					else if (ability.find("BUILD_STARPORT") != std::string::npos)
					{
						TryBuildStarport();
					}
					else if (ability.find("TRAIN_VIKINGFIGHTER") != std::string::npos)
					{
						TryTrainViking();
					}
					else if (ability.find("MORPH_VIKINGFIGHTERMODE") != std::string::npos)
					{
						TryTransformVikingFighter();
					}
					else if (ability.find("MORPH_VIKINGASSAULTMODE") != std::string::npos)
					{
						TryTransformVikingAssault();
					}
					else if (ability.find("TRAIN_MEDIVAC") != std::string::npos)
					{
						TryTrainMedivac();
					}
					else if (ability.find("TRAIN_LIBERATOR") != std::string::npos)
					{
						TryTrainLiberator();
					}
					else if (ability.find("MORPH_LIBERATORAGMODE") != std::string::npos)
					{
						TryTransformLiberatorLiberatorAG();
					}
					else if (ability.find("MORPH_LIBERATORAAMODE") != std::string::npos)
					{
						TryTransformLiberatorAGLiberator();
					}
					else if (ability.find("TRAIN_RAVEN") != std::string::npos)
					{
						TryTrainRaven();
					}
					else if (ability.find("EFFECT_AUTOTURRET") != std::string::npos)
					{
						TryRavenSummonAutoTurret();
					}
					else if (ability.find("TRAIN_BANSHEE") != std::string::npos)
					{
						TryTrainBanshee();
					}
					else if (ability.find("TRAIN_BATTLECRUISER") != std::string::npos)
					{
						TryTrainBattlecruiser();
					}
					else if (ability.find("BUILD_STARPORTREACTOR") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildStarportReactor();
					}
					else if (ability.find("BUILD_STARPORTTECHLAB") != std::string::npos) //Not on Offical Typeenums
					{
						TryBuildStarportTechLab();
					}
					else if (ability.find("RESEARCH_HIGHCAPACITYFUELTANKS") != std::string::npos)
					{
						TryStarportResearchRapidReignitionSystem();
					}
					else if (ability.find("RESEARCH_RAVENCORVIDREACTOR") != std::string::npos)
					{
						TryStarportResearchCorvidReactor();
					}
					else if (ability.find("RESEARCH_BANSHEECLOAKINGFIELD") != std::string::npos)
					{
						TryStarportResearchCloakingField();
					}
					else if (ability.find("RESEARCH_BANSHEEHYPERFLIGHTROTORS") != std::string::npos)
					{
						TryStarportResearchHyperflightRotors();
					}
					else if (ability.find("RESEARCH_ADVANCEDBALLISTICS") != std::string::npos)
					{
						TryStarportResearchAdvancedBallistics();
					}
					else if (ability.find("BUILD_FUSIONCORE") != std::string::npos)
					{
						TryBuildFusionCore();
					}
					else if (ability.find("RESEARCH_BATTLECRUISERWEAPONREFIT") != std::string::npos)
					{
						TryFusionCoreResearchResearchWeaponRefit();
					}
					else if (ability.find("BUILD_ARMORY") != std::string::npos)
					{
						TryBuildArmory();
					}
					else if (ability.find("RESEARCH_TERRANVEHICLEWEAPONS") != std::string::npos)
					{
						TryArmoryResearchVehicleWeapons();
					}
					else if (ability.find("RESEARCH_TERRANSHIPWEAPONS") != std::string::npos)
					{
						TryArmoryResearchShipWeapons();
					}
					else if (ability.find("RESEARCH_TERRANVEHICLEANDSHIPPLATING") != std::string::npos)
					{
						TryArmoryResearchVehicleShipPlating();
					}
					else if (ability.find("BUILD_BUNKER") != std::string::npos)
					{
						TryBuildBunker();
					}
					else if (ability.find("BUILD_ENGINEERINGBAY") != std::string::npos)
					{
						TryBuildEngineeringBay();
					}
					else if (ability.find("RESEARCH_TERRANINFANTRYWEAPONS") != std::string::npos)
					{
						TryEngineeringBayResearchInfantryWeapon();
					}
					else if (ability.find("RESEARCH_TERRANINFANTRYARMOR") != std::string::npos)
					{
						TryEngineeringBayResearchInfantryArmor();
					}
					else if (ability.find("BUILD_GHOSTACADEMY") != std::string::npos)
					{
						TryBuildGhostAcademy();
					}
					else if (ability.find("RESEARCH_PERSONALCLOAKING") != std::string::npos)
					{
						TryGhostAcademyResearchPersonalCloaking();
					}
					else if (ability.find("BUILD_NUKE") != std::string::npos)
					{
						TryGhostAcademyBuildNuke();
					}
					else if (ability.find("BUILD_MISSILETURRET") != std::string::npos)
					{
						TryBuildMissileTurret();
					}
					else if (ability.find("BUILD_SENSORTOWER") != std::string::npos)
					{
						TryBuildSensorTower();
					}
					else if (ability.find("SURRENDER") != std::string::npos)
					{
						Debug()->DebugEndGame();
					}
				}

				//Starts to check if there is a message and sends updates to model service
				void StartSendingUpdatesToModelService()
				{
					StopSendingUpdatesToModelService();

					_shouldkeepupdating = true;
					auto getmessagefrommodelservice = new std::thread(&KoKeKoKo::Agent::KoKeKoKoBot::GetMessageFromModelService, this);
					_threads.insert(std::make_pair("GetMessageFromModelService", getmessagefrommodelservice));
					auto sendupdatestomodelservice = new std::thread(&KoKeKoKo::Agent::KoKeKoKoBot::SendUpdatesToModelService, this);
					_threads.insert(std::make_pair("SendUpdatesToModelService", sendupdatestomodelservice));
				}

				//Stops checking messages and sending updates to model service
				void StopSendingUpdatesToModelService()
				{
					_shouldkeepupdating = false;

					if (_threads.find("GetMessageFromModelService") != _threads.end())
					{
						if (_threads["GetMessageFromModelService"]->joinable())
							_threads["GetMessageFromModelService"]->join();

						_threads.erase("GetMessageFromModelService");
					}
					if (_threads.find("SendUpdatesToModelService") != _threads.end())
					{
						if (_threads["SendUpdatesToModelService"]->joinable())
							_threads["SendUpdatesToModelService"]->join();

						_threads.erase("SendUpdatesToModelService");
					}
				}				
		};
	}
}

Services::ModelService* Services::ModelService::Instance = nullptr;
int main(int argc, char* argv[])
{
	try
	{
		sc2::Coordinator* coordinator = new sc2::Coordinator();
		KoKeKoKo::Agent::KoKeKoKoBot* kokekokobot = new KoKeKoKo::Agent::KoKeKoKoBot();
		Services::ModelService* modelservice = Services::ModelService::CreateNewModelService();

		/*auto reply = modelservice->UpdateModelService("UPDATE");
		for (int counter = 0; counter < 1000; counter++)
		{
			while (!reply.empty())
			{
				std::cout << "Message: " << reply.front() << std::endl;
				reply.pop();
			}

			if (!modelservice->ShouldOperationsContinue())
				reply = modelservice->UpdateModelService("UPDATE");
		}

		modelservice->StopModelService();
		char c;
		std::cin >> c;*/

		/*coordinator->LoadSettings(argc, argv);
		coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
		coordinator->LaunchStarcraft();
		coordinator->StartGame(sc2::kMapBelShirVestigeLE);
		while (coordinator->Update());*/
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