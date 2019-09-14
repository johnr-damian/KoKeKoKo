#define NOMINMAX

#include <iostream>
#include <queue>
#include <sc2api/sc2_api.h>
#include <sstream>
#include <thread>
#include <Windows.h>

namespace KoKeKoKo
{
	namespace Model
	{
		using namespace std;
		//The directory of the model
		const string MODELSERVICE_FILENAME = "ModelService\\bin\\Debug\\ModelService.exe";

		//Manages the communication between agent and model
		class ModelRepositoryService
		{
			private:
				//Instace of this class
				static ModelRepositoryService* _instance;
				//The execution of the model
				PROCESS_INFORMATION _model;
				//If the agent should accept messages from model service
				atomic<bool> _shouldacceptmessages;
				//The messages from the model
				deque<string> _messages;
				//A map of created threads where key is the method name, and value is the thread
				map<string, thread*> _threads;
				//Lock for message handling
				mutex _messagelock;

				ModelRepositoryService(const ModelRepositoryService&);
				ModelRepositoryService& operator=(const ModelRepositoryService&);
				//Initializes fields and starts the model service
				ModelRepositoryService()
				{
					//Perform initializations
					_shouldacceptmessages = false;
					_messages = deque<string>();
					_threads = map<string, thread*>();

					//Start the model service
					STARTUPINFO startupinfo = { 0 };
					LPSTR executablefile = new char[MAX_PATH];
					string executabledirectory = "";

					ZeroMemory(&_model, sizeof(_model));
					ZeroMemory(&startupinfo, sizeof(startupinfo));
					startupinfo.cb = sizeof(startupinfo);
					executabledirectory = GetAbsoluteDirectoryOf(MODELSERVICE_FILENAME);
					executablefile = const_cast<char *>(executabledirectory.c_str());

					if (!CreateProcessA(NULL, executablefile, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &_model))
						throw exception(("Error Occurred! Failed to create process for model service with an exit code of " + to_string(GetLastError()) + "...").c_str());
				}

				//Waits for model service to connect and accepts any messages from model service
				void ListenForMessages()
				{
					DWORD readpointer = 0;
					HANDLE server = INVALID_HANDLE_VALUE;
					LPSTR name = TEXT("\\\\.\\pipe\\AgentServer");
					char buffer[4096] = { 0 };
					string message = "";

					for (int failures = 0; _shouldacceptmessages;)
					{
						try
						{
							//Re-initialize variables
							readpointer = 0;
							server = INVALID_HANDLE_VALUE;
							ZeroMemory(buffer, sizeof(buffer));
							message = "";

							//Create a named pipe
							server = CreateNamedPipeA(name, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, sizeof(buffer), sizeof(buffer), 0, NULL);
							if (server != INVALID_HANDLE_VALUE)
							{
								//Wait for model service to connect
								if (ConnectNamedPipe(server, NULL))
								{
									//Read the message from model service
									while (ReadFile(server, buffer, sizeof(buffer), &readpointer, NULL))
										buffer[readpointer] = '\0';

									//Enqueue the message
									_messagelock.lock();
									message = string(buffer);
									_messages.push_back(message);
									_messagelock.unlock();

									//Disconnect model service
									DisconnectNamedPipe(server);
								}

								//Close the server
								CloseHandle(server);
							}
							else
								throw exception(("Error Occurred! Failed to create a server for model service with an exit code of " + to_string(GetLastError()) + "...").c_str());
						}
						catch (const exception& ex)
						{
							cout << ex.what() << endl;
							if (++failures >= 5)
								throw exception("Error Occurred! Exceeded number of tries to create a server for model service...");
						}
					}
				}

			public:
				//Disposes the instance and terminates model service
				virtual ~ModelRepositoryService()
				{
					DWORD exitcode = 0;

					//Try to wait for 30s before releasing the process
					WaitForSingleObject(_model.hProcess, 30000);
					if (!GetExitCodeProcess(_model.hProcess, &exitcode))
						throw exception(("Error Occurred! Failed to get exit status of process with an exit code of " + to_string(GetLastError()) + "...").c_str());

					cout << "Model Service is terminated with an exit code of " << exitcode << endl;
					CloseHandle(_model.hProcess);
					CloseHandle(_model.hThread);
				}

				//Starts model service and returns the instance of this class
				static ModelRepositoryService* StartModelRepositoryService()
				{
					if (_instance == nullptr)
						_instance = new ModelRepositoryService();

					return _instance;
				}

				//Sends a message to model service and returns true if successfully sent
				bool SendMessageToModelService(string message)
				{
					DWORD writepointer = 0;
					HANDLE client = INVALID_HANDLE_VALUE;
					LPSTR name = TEXT("\\\\.\\pipe\\ModelServer");
					char buffer[4096] = { 0 };

					try
					{
						strcpy_s(buffer, message.c_str());

						client = CreateFileA(name, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
						if (client != INVALID_HANDLE_VALUE)
						{
							if (WriteFile(client, buffer, (message.size() + 1), &writepointer, NULL))
								FlushFileBuffers(client);
							else
								throw exception(("Error Occurred! Failed to send a message with an exit code of " + to_string(GetLastError()) + "...").c_str());

							//Close the client
							CloseHandle(client);
						}
						else if (GetLastError() == ERROR_PIPE_BUSY)
							throw exception(("Error Occurred! The server is currently busy with an exit code of " + to_string(GetLastError())).c_str());
						else
							throw exception(("Error Occurred! Failed to create a client pipe with an exit code of " + to_string(GetLastError())).c_str());

						return true;
					}
					catch (const exception& ex)
					{
						cout << ex.what() << endl;
					}

					return false;
				}

				//Gets the current project directory and returns the absolute directory of the file
				string GetAbsoluteDirectoryOf(string filename)
				{
					LPSTR currentdirectory = new char[MAX_PATH];
					string absolutedirectory = "";

					try
					{
						if (GetCurrentDirectoryA(MAX_PATH, currentdirectory) != 0)
							absolutedirectory = (((string)currentdirectory) + "\\" + filename);
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to get the absolute directory of the file..." << endl;
					}

					return absolutedirectory;
				}

				queue<string> GetMessageFromModelService()
				{
					queue<string> messages = queue<string>();

					try
					{
						_messagelock.lock();
						while (!_messages.empty())
						{
							messages.push(_messages.front());
						}
						_messagelock.unlock();
					}
					catch (const exception& ex)
					{
						cout << ex.what() << endl;
					}

					return messages;
				}

				//Starts accepting messages from model service
				void StartAcceptingMessages()
				{
					StopAcceptingMessages();

					_shouldacceptmessages = true;
					auto listenformessages = new thread(&Model::ModelRepositoryService::ListenForMessages, this);
					_threads.insert(make_pair("ListenForMessages", listenformessages));
				}

				//Stops accepting messages from model service
				void StopAcceptingMessages()
				{
					_shouldacceptmessages = false;

					if (_threads.find("ListenForMessages") != _threads.end())
					{
						if (_threads["ListenForMessages"]->joinable())
							_threads["ListenForMessages"]->join();

						_threads.erase("ListenForMessages");
					}
				}
		};
	}

	namespace Agent
	{
		using namespace sc2;

		//Manages the agent in the environment
		class KoKeKoKoBot : public Agent
		{
			private:
				Model::ModelRepositoryService* _instance;
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
						_actionslock.lock();
						if (!_actions.empty())
						{
							action = _actions.front();
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
							_actionslock.lock();
							for (auto message = _instance->GetMessageFromModelService(); !message.empty();)
							{
								_actions.push(message.front());
							}
							_actionslock.unlock();

							//Check if there is a message again after 5 seconds
							std::this_thread::sleep_for(std::chrono::milliseconds(5000));
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
							const ObservationInterface* current_observation = Observation();
							std::string message = "";
							
							//Send the current state of the agent
							//Macro details
							message += std::to_string(current_observation->GetPlayerID()) + ","; //Player ID
							message += std::to_string(current_observation->GetMinerals()) + ","; //Minerals
							message += std::to_string(current_observation->GetVespene()) + ","; //Vespene
							message += std::to_string(current_observation->GetFoodArmy()) + ","; //Supply Cost used by Army
							message += std::to_string(current_observation->GetFoodWorkers()) + ","; //Supply Cost used by Workers
							message += std::to_string(CountOf(UNIT_TYPEID::TERRAN_SCV)); //No. of Workers
							for (const auto& upgrade : current_observation->GetUpgrades())
								message += ("," + upgrade.to_string());
							message += "~";

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

							//Send this message to model service
							_instance->SendMessageToModelService(message);

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
					return TryResearch(ABILITY_ID::MORPH_SIEGEMODE, UNIT_TYPEID::TERRAN_SIEGETANK);
				}

				bool TryTransformUnsiege()
				{
					return TryResearch(ABILITY_ID::MORPH_UNSIEGE, UNIT_TYPEID::TERRAN_SIEGETANK);
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
					return TryResearch(ABILITY_ID::MORPH_VIKINGASSAULTMODE, UNIT_TYPEID::TERRAN_VIKINGFIGHTER);
				}

				bool TryTransformVikingFighter()
				{
					return TryResearch(ABILITY_ID::MORPH_VIKINGFIGHTERMODE, UNIT_TYPEID::TERRAN_VIKINGASSAULT);
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
					_instance = Model::ModelRepositoryService::StartModelRepositoryService();
					_shouldkeepupdating = false;
					_threads = std::map<std::string, std::thread*>();
					_actions = std::queue<std::string>();
					_currentaction = "";
				}

				virtual void OnGameStart() final
				{
					//We periodically get message and send updates to model service
					StartSendingUpdatesToModelService();

					//while there is still no action, we wait for a message
					while (_currentaction.empty())
					{
						_currentaction = GetAnActionFromMessage();
						if (_currentaction.empty())
							//Wait for 5 seconds if there is still no message
							std::this_thread::sleep_for(std::chrono::milliseconds(5000)); 
					}

					std::cout << _currentaction << std::endl;
				}

				virtual void OnStep() final
				{
					//If there is action available
					if (!_currentaction.empty())
					{
						ExecuteAbility(_currentaction);
						_currentaction = "";
					}
					else
						_currentaction = GetAnActionFromMessage();
						
					std::cout << _currentaction << std::endl;
				}

				virtual void OnGameEnd() final
				{
					StopSendingUpdatesToModelService();
					
					//Dispose the modelrepositoryservice instance
					_instance->~ModelRepositoryService();
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
					if (action.find("BUILD_REFINERY") != string::npos)
					{
						TryBuildRefinery();
					}
					else if (action.find("BUILD_COMMANDCENTER") != string::npos)
					{
						TryBuildCommandCenter();
					}
					else if (action.find("MORPH_ORBITALCOMMAND") != string::npos)
					{
						TryCommandCenterMorphOrbitalCommand();
					}
					else if (action.find("EFFECT_CALLDOWNMULE") != string::npos)
					{
						TryOrbitalCommandSummonMule();
					}
					else if (action.find("MORPH_PLANETARYFORTRESS") != string::npos)
					{
						TryCommandCenterMorphPlanetaryFortress();
					}
					else if (action.find("TRAIN_SCV") != string::npos)
					{
						TrainSCV();
					}
					else if (action.find("BUILD_SUPPLYDEPOT") != string::npos)
					{
						TryBuildSupplyDepot();
					}
					else if (action.find("BUILD_BARRACKS") != string::npos)
					{
						TryBuildBarracks();
					}
					else if (action.find("TRAIN_MARINE") != string::npos)
					{
						TrainMarine();
					}
					else if (action.find("TRAIN_REAPER") != string::npos)
					{
						TrainReaper();
					}
					else if (action.find("TRAIN_MARAUDER") != string::npos)
					{
						TrainMarauder();
					}
					else if (action.find("TRAIN_GHOST") != string::npos)
					{
						TrainGhost();
					}
					else if (action.find("BUILD_BARRACKSTECHLAB") != string::npos) //Not on Offical Typeenums
					{
						TryBuildBarracksTechLab();
					}
					else if (action.find("RESEARCH_COMBATSHIELD") != string::npos)
					{
						TryBarracksTechLabResearchCombatShield();
					}
					else if (action.find("RESEARCH_STIMPACK") != string::npos)
					{
						TryBarracksTechLabResearchStimpack();
					}
					else if (action.find("RESEARCH_CONCUSSIVESHELLS") != string::npos)
					{
						TryBarracksTechLabResearchConcussiveShells();
					}
					else if (action.find("BUILD_BARRACKSREACTOR") != string::npos) //Not on Offical Typeenums
					{
						TryBuildBarracksReactor();
					}
					else if (action.find("BUILD_FACTORY") != string::npos)
					{
						TryBuildFactory();
					}
					else if (action.find("TRAIN_HELLION") != string::npos)
					{
						TryTrainHellion();
					}
					else if (action.find("MORPH_HELLBAT") != string::npos)
					{
						TryTransformHellionHellbat();
					}
					else if (action.find("TRAIN_WIDOWMINE") != string::npos)
					{
						TryTrainWidowMine();
					}
					else if (action.find("TRAIN_SIEGETANK") != string::npos)
					{
						TryTrainSiegeTank();
					}
					else if (action.find("MORPH_SIEGEMODE") != string::npos)
					{
						TryTransformSiegeMode();
					}
					else if (action.find("MORPH_UNSIEGE") != string::npos)
					{
						TryTransformUnsiege();
					}
					else if (action.find("TRAIN_CYCLONE") != string::npos)
					{
						TryTrainCyclone();
					}
					else if (action.find("TRAIN_HELLBAT") != string::npos)
					{
						TryTrainHellbat();
					}
					else if (action.find("MORPH_HELLION") != string::npos)
					{
						TryTransformHellbatHellion();
					}
					else if (action.find("TRAIN_THOR") != string::npos)
					{
						TryTrainThor();
					}
					else if (action.find("BUILD_FACTORYTECHLAB") != string::npos) //Not on Offical Typeenums
					{
						TryBuildFactoryTechLab();
					}
					else if (action.find("RESEARCH_INFERNALPREIGNITER") != string::npos)
					{
						TryFactoryResearchInfernalPreIgniter();
					}
					else if (action.find("RESEARCH_MAGFIELDLAUNCHERS") != string::npos)
					{
						TryFactoryResearchMagFieldAccelerator();
					}
					else if (action.find("RESEARCH_DRILLINGCLAWS") != string::npos)
					{
						TryFactoryResearchDrillingClaws();
					}
					else if (action.find("BUILD_FACTORYREACTOR") != string::npos) //Not on Offical Typeenums
					{
						TryBuildFactoryReactor();
					}
					else if (action.find("BUILD_STARPORT") != string::npos)
					{
						TryBuildStarport();
					}
					else if (action.find("TRAIN_VIKINGFIGHTER") != string::npos)
					{
						TryTrainViking();
					}
					else if (action.find("MORPH_VIKINGFIGHTERMODE") != string::npos)
					{
						TryTransformVikingFighter();
					}
					else if (action.find("MORPH_VIKINGASSAULTMODE") != string::npos)
					{
						TryTransformVikingAssault();
					}
					else if (action.find("TRAIN_MEDIVAC") != string::npos)
					{
						TryTrainMedivac();
					}
					else if (action.find("TRAIN_LIBERATOR") != string::npos)
					{
						TryTrainLiberator();
					}
					else if (action.find("MORPH_LIBERATORAGMODE") != string::npos)
					{
						TryTransformLiberatorLiberatorAG();
					}
					else if (action.find("MORPH_LIBERATORAAMODE") != string::npos)
					{
						TryTransformLiberatorAGLiberator();
					}
					else if (action.find("TRAIN_RAVEN") != string::npos)
					{
						TryTrainRaven();
					}
					else if (action.find("EFFECT_AUTOTURRET") != string::npos)
					{
						TryRavenSummonAutoTurret();
					}
					else if (action.find("TRAIN_BANSHEE") != string::npos)
					{
						TryTrainBanshee();
					}
					else if (action.find("TRAIN_BATTLECRUISER") != string::npos)
					{
						TryTrainBattlecruiser();
					}
					else if (action.find("BUILD_STARPORTREACTOR") != string::npos) //Not on Offical Typeenums
					{
						TryBuildStarportReactor();
					}
					else if (action.find("BUILD_STARPORTTECHLAB") != string::npos) //Not on Offical Typeenums
					{
						TryBuildStarportTechLab();
					}
					else if (action.find("RESEARCH_HIGHCAPACITYFUELTANKS") != string::npos)
					{
						TryStarportResearchRapidReignitionSystem();
					}
					else if (action.find("RESEARCH_RAVENCORVIDREACTOR") != string::npos)
					{
						TryStarportResearchCorvidReactor();
					}
					else if (action.find("RESEARCH_BANSHEECLOAKINGFIELD") != string::npos)
					{
						TryStarportResearchCloakingField();
					}
					else if (action.find("RESEARCH_BANSHEEHYPERFLIGHTROTORS") != string::npos)
					{
						TryStarportResearchHyperflightRotors();
					}
					else if (action.find("RESEARCH_ADVANCEDBALLISTICS") != string::npos)
					{
						TryStarportResearchAdvancedBallistics();
					}
					else if (action.find("BUILD_FUSIONCORE") != string::npos)
					{
						TryBuildFusionCore();
					}
					else if (action.find("RESEARCH_BATTLECRUISERWEAPONREFIT") != string::npos)
					{
						TryFusionCoreResearchResearchWeaponRefit();
					}
					else if (action.find("BUILD_ARMORY") != string::npos)
					{
						TryBuildArmory();
					}
					else if (action.find("RESEARCH_TERRANVEHICLEWEAPONS") != string::npos)
					{
						TryArmoryResearchVehicleWeapons();
					}
					else if (action.find("RESEARCH_TERRANSHIPWEAPONS") != string::npos)
					{
						TryArmoryResearchShipWeapons();
					}
					else if (action.find("RESEARCH_TERRANVEHICLEANDSHIPPLATING") != string::npos)
					{
						TryArmoryResearchVehicleShipPlating();
					}
					else if (action.find("BUILD_BUNKER") != string::npos)
					{
						TryBuildBunker();
					}
					else if (action.find("BUILD_ENGINEERINGBAY") != string::npos)
					{
						TryBuildEngineeringBay();
					}
					else if (action.find("RESEARCH_TERRANINFANTRYWEAPONS") != string::npos)
					{
						TryEngineeringBayResearchInfantryWeapon();
					}
					else if (action.find("RESEARCH_TERRANINFANTRYARMOR") != string::npos)
					{
						TryEngineeringBayResearchInfantryArmor();
					}
					else if (action.find("BUILD_GHOSTACADEMY") != string::npos)
					{
						TryBuildGhostAcademy();
					}
					else if (action.find("RESEARCH_PERSONALCLOAKING") != string::npos)
					{
						TryGhostAcademyResearchPersonalCloaking();
					}
					else if (action.find("BUILD_NUKE") != string::npos)
					{
						TryGhostAcademyBuildNuke();
					}
					else if (action.find("BUILD_MISSILETURRET") != string::npos)
					{
						TryBuildMissileTurret();
					}
					else if (action.find("BUILD_SENSORTOWER") != string::npos)
					{
						TryBuildSensorTower();
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

using namespace KoKeKoKo;
Model::ModelRepositoryService* Model::ModelRepositoryService::_instance = nullptr;

int main(int argc, char* argv[])
{
	try
	{
		auto coordinator = new sc2::Coordinator();
		auto kokekokobot = new Agent::KoKeKoKoBot();
		auto modelrepositoryservice = Model::ModelRepositoryService::StartModelRepositoryService();

		//Start accepting messages
		modelrepositoryservice->StartAcceptingMessages();

		//Start the game
		coordinator->LoadSettings(argc, argv);
		coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
		coordinator->LaunchStarcraft();
		coordinator->StartGame(sc2::kMapBelShirVestigeLE);
		while (coordinator->Update());
	}
	catch (const std::exception& ex)
	{
		std::cout << ex.what() << std::endl;
	}
	catch (...)
	{
		std::cout << "An Application error occurred! Stopping the program immediately...";
	}

	std::cout << "Press enter to continue..." << std::endl;
	system("PAUSE");
	return 0;
}