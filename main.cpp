#define NOMINMAX

#include <fstream>
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
		//The program that computes for POMDP Model and MCTS Model using R
		const string MODELSERVICE_FILENAME = "ModelService\\bin\\Debug\\ModelService.exe";
		//The extracted commands from the replay training data
		const string COMMANDSREPOSITORY_FILENAME = "Documents\\Training\\CommandsRepository.csv";
		//The extracted resources from the replay training data
		const string RESOURCESREPOSITORY_FILENAME = "Documents\\Training\\ResourcesRepository.csv";
		//The list of ranks contianing their respective range of ability value
		const std::map<std::string, std::pair<double, double>> RANKS =
		{
			{"Bronze", {0 ,0}},
			{"Silver", {0, 0}},
			{"Gold", {0, 0}},
			{"Diamond", {0, 0}},
			{"Platinum", {0, 0}},
			{"Master", {0, 0}},
			{"Grandmaster", {0, 0}}
		};

		//The class that manages communication among Agent, Model, and Repository
		class ModelRepositoryService
		{
		private:
			//The instance of ModelRepositoryService
			static ModelRepositoryService* _instance;
			//ModelService process information
			PROCESS_INFORMATION processinformation;
			//A map of created threads where key is the function name, and value is the thread that runs the given function name
			map<string, thread*> _createdthreads = map<string, thread*>();
			//The parsed of commands repository
				//Rank	//Previous Command	//Subsequent Command
			map<string, map<string, vector<string>>> _parsedcommandsrepository = map<string, map<string, vector<string>>>();
			//A lock when handling messages
			mutex _messagelock;
			//If the agent should keep listening to the model service
			atomic<bool> _keeplisteningtomodelservice = false;

			void ParseCommandsRepository(bool& finished_parsing)
			{
				try
				{
					ifstream commandsrepository(GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME));
					string repositoryline = "", repositorycomma = "", currentrank = "", currentowner = "", previouscommand = "";

					if (commandsrepository.is_open())
					{
						cout << "Parsing Commands Repository..." << endl;
						while (getline(commandsrepository, repositoryline))
						{
							stringstream byline(repositoryline);
							for (int current_column = 0; getline(byline, repositorycomma, ','); current_column++)
							{
								//Current column is either a rank or timestamp
								if (current_column == 0)
								{
									if (currentrank == "")
									{
										if (RANKS.find(repositorycomma) != RANKS.end())
										{
											currentrank = repositorycomma;
											_parsedcommandsrepository.insert(make_pair(currentrank, map<string, vector<string>>()));
										}
									}
									else
									{
										//Get the current rank for the following line of commands
										if (RANKS.find(repositorycomma) != RANKS.end())
										{
											//Repeat last command
											_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);
											currentowner = "";
											previouscommand = "";
											currentrank = repositorycomma;
											_parsedcommandsrepository.insert(make_pair(currentrank, map<string, vector<string>>()));
										}
									}
								}

								//Get the owner of the commands
								if (current_column == 1)
								{
									if (currentowner == "")
										currentowner = repositorycomma;
									else
									{
										if (currentowner != repositorycomma)
										{
											currentowner = repositorycomma;
											_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);
											previouscommand = "";
										}
									}
									continue;
								}

								//Get the sequential commands executed by the same owner
								if (current_column == 2)
								{
									if (previouscommand == "")
										previouscommand = repositorycomma;

									//Check if the current command has been mapped
									if (_parsedcommandsrepository[currentrank].find(repositorycomma) == _parsedcommandsrepository[currentrank].end())
										//If not mapped, add the command to the map
										_parsedcommandsrepository[currentrank].insert(make_pair(repositorycomma, vector<string>()));

									_parsedcommandsrepository[currentrank][previouscommand].push_back(repositorycomma);
									previouscommand = repositorycomma;
								}
							}
						}
						_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);

						commandsrepository.close();
						finished_parsing = true;
					}
					else
						throw exception("Failed to open commands repository...");

					finished_parsing = true;
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to parse commands repository..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> ParseCommandsRepository(): \n\t" << ex.what() << endl;
					finished_parsing = false;
				}
			}

			void ParseResourcesRepository(bool& finished_parsing)
			{
				try
				{
					ifstream resourcesrepository(GetRelativeFilepathOf(RESOURCESREPOSITORY_FILENAME));
					string repositoryline = "";

					if (resourcesrepository.is_open())
					{
						cout << "Parsing Resources Repository..." << endl;
						while (getline(resourcesrepository, repositoryline))
						{

						}

						resourcesrepository.close();
						finished_parsing = true;
					}
					else
						throw exception("Failed to open commands repository...");
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to parse resources repository..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> ParseResourcesRepository(): \n\t" << ex.what() << endl;
					finished_parsing = true; //TODO Remove
				}
			}

			//Keeps waiting for the model to connect and saves the sent message to a queue
			void ListenToModelService()
			{
				DWORD dwread = 0;
				HANDLE server = INVALID_HANDLE_VALUE;
				LPSTR servername = TEXT("\\\\.\\pipe\\AgentServer");
				string message = "";
				char buffer[2048] = { 0 };

				try
				{
					while (_keeplisteningtomodelservice)
					{
						//Re-initialize variables
						dwread = 0;
						server = INVALID_HANDLE_VALUE;
						message = "";
						ZeroMemory(buffer, sizeof(buffer));

						//Create a named pipe
						server = CreateNamedPipeA(servername, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, sizeof(buffer), sizeof(buffer), 0, NULL);
						if (server != INVALID_HANDLE_VALUE)
						{
							//Wait for a client to connect to the server
							if (ConnectNamedPipe(server, NULL))
							{
								//Read the message from the model
								while (ReadFile(server, buffer, sizeof(buffer), &dwread, NULL))
									buffer[dwread] = '\0';

								//Store the message to the queue
								_messagelock.lock();
								message = string(buffer);
								Messages.push_back(message);
								_messagelock.unlock();

								//Disconnect the client
								DisconnectNamedPipe(server);
							}

							//Close the pipe
							CloseHandle(server);
						}
						else
							throw exception("Failed to create a server for model service...");

						this_thread::sleep_for(chrono::milliseconds(1000));
					}
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to keep listening to model service...";
					cerr << "Error in Agent! ModelRepositoryService -> ListenToModelService(): \n\t" << ex.what() << endl;
				}
			}

		public:
			//The sent messages by the model service
			deque<string> Messages = deque<string>();

			//Returns the created instance of ModelRepositoryService
			static ModelRepositoryService* GetModelRepositoryServiceInstance()
			{
				if (_instance == nullptr)
					_instance = new ModelRepositoryService();

				return _instance;
			}

			//Returns true if finished successfully parsing commands repository and resources repository
			bool GenerateRepository()
			{
				try
				{
					bool finishedparsingcommands = false, finishedparsingresources = false;
					auto parsecommandsrepository = new thread(&Model::ModelRepositoryService::ParseCommandsRepository, this, ref(finishedparsingcommands));
					auto parseresourcesrepository = new thread(&Model::ModelRepositoryService::ParseResourcesRepository, this, ref(finishedparsingresources));

					if (parsecommandsrepository->joinable())
						parsecommandsrepository->join();
					if (parseresourcesrepository->joinable())
						parseresourcesrepository->join();

					return (finishedparsingcommands && finishedparsingresources);
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to generate repository..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> GenerateRepository(): \n\t" << ex.what() << endl;
				}

				return false;
			}

			//Returns true if successfully executed ModelService process
			bool StartModelService()
			{
				try
				{
					STARTUPINFO startupinfo = { 0 };
					LPSTR executable = new char[MAX_PATH];
					string executablefilepath = "";

					ZeroMemory(&processinformation, sizeof(processinformation));
					ZeroMemory(&startupinfo, sizeof(startupinfo));
					startupinfo.cb = sizeof(startupinfo);
					executablefilepath = GetRelativeFilepathOf(MODELSERVICE_FILENAME);
					executable = const_cast<char *>(executablefilepath.c_str());

					//Create a process for model service
					if (!(CreateProcessA(NULL, executable, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &processinformation)))
						throw exception("Failed to create a process for model service...");

					return true;
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to start model service..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> StartModelService(): \n\t" << ex.what() << endl;
				}

				return false;
			}

			//Stops and retrieves the ModelService process return code
			void StopModelService()
			{
				try
				{
					DWORD dwprocessresult = 0;

					//Send to stop the model
					while (!SendMessageToModelService("Exit", 5))
						cout << "Failed to send an exit to model service... trying again...";

					//Wait for the process to finish its procedures
					WaitForSingleObject(processinformation.hProcess, 10000);
					if (GetExitCodeProcess(processinformation.hProcess, &dwprocessresult))
					{
						cout << "ModelService Returned Value on Exit: " << dwprocessresult << endl;
						CloseHandle(processinformation.hProcess);
						CloseHandle(processinformation.hThread);
					}
					else
						throw exception("Failed to retrieve exit code of model service...");
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to stop model service..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> StopModelService(): \n\t" << ex.what() << endl;
				}
			}

			//Returns true if successfully started to listen to model service
			bool StartListeningToModelService()
			{
				try
				{
					_keeplisteningtomodelservice = true;

					//Check if there is no a thread running
					if (_createdthreads.find("ListenToModelService") == _createdthreads.end())
					{
						auto listentomodelservice = new thread(&Model::ModelRepositoryService::ListenToModelService, this);
						listentomodelservice->detach();
						_createdthreads.insert(make_pair("ListenToModelService", listentomodelservice));
					}

					return true;
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to start listening to model service..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> StartListeningToModelService(): \n\t" << ex.what() << endl;
				}

				return false;
			}

			//Stops listening to model service
			void StopListeningToModelService()
			{
				try
				{
					_keeplisteningtomodelservice = false;

					//Check if there is a thread running
					if (_createdthreads.find("ListenToModelService") != _createdthreads.end())
					{
						if (_createdthreads["ListenToModelService"]->joinable())
							_createdthreads["ListenToModelService"]->join();

						_createdthreads.erase("ListenToModelService");
					}
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to stop listening to model service..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> StopListeningToModelService(): \n\t" << ex.what() << endl;
				}
			}

			//Returns true if successfully sent a message to model
			bool SendMessageToModelService(string message, int retries)
			{
				try
				{
					DWORD dwwrite = 0;
					HANDLE client = INVALID_HANDLE_VALUE;
					LPSTR clientname = TEXT("\\\\.\\pipe\\ModelServer");
					bool success = false;
					char buffer[2048] = { 0 };

					for (int tries = 0; ((!success) && (tries < retries)); tries++)
					{
						dwwrite = 0;
						client = INVALID_HANDLE_VALUE;
						success = false;
						ZeroMemory(&buffer, sizeof(buffer));

						strcpy_s(buffer, message.c_str());
						client = CreateFileA(clientname, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
						if (client != INVALID_HANDLE_VALUE)
						{
							if (WriteFile(client, buffer, (message.size() + 1), &dwwrite, NULL))
							{
								FlushFileBuffers(client);
								success = true;
							}
							else
								cout << "Failed to send a message on try " << tries << endl;

							CloseHandle(client);
						}
						else if (GetLastError() == ERROR_PIPE_BUSY)
						{
							if (WaitNamedPipeA(clientname, 20000))
							{
								cout << "Failed to connect to server on try" << tries << endl;
								continue;
							}
							else
								throw exception("Failed to wait the server at model service...");
						}
						else
							throw exception(("Failed to create client pipe with error code " + to_string(GetLastError())).c_str());

						this_thread::sleep_for(chrono::milliseconds(5000));
					}

					return success;
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to send a message to model service..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> SendMessageToModelService(): \n\t" << ex.what() << endl;
				}

				return false;
			}

			//Returns the filename concatenated with the current project directory. If failed, returns nullptr
			string GetRelativeFilepathOf(string filename)
			{
				LPSTR currentprojectdirectory = new char[MAX_PATH];
				string relativefilepath = "";

				try
				{
					relativefilepath = (GetCurrentDirectoryA(MAX_PATH, currentprojectdirectory) != 0) ? (((string)currentprojectdirectory) + "\\" + filename) : filename;
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to get relative filepath of filename..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> GetRelativeFilepathOf(): \n\t" << ex.what() << endl;
				}

				return relativefilepath;
			}

			//Returns a string with code as head and the message as the values
			string ConstructMessage(const vector<string>& message, string code = "")
			{
				string constructedmessage = "";

				try
				{
					constructedmessage = code;
					for (size_t iterator = 0; iterator < message.size(); iterator++)
						constructedmessage += (((code == "" && iterator == 0) ? "" : "\n") + message[iterator]);
				}
				catch (const exception& ex)
				{
					cout << "Error Occurred! Failed to construct a message..." << endl;
					cerr << "Error in Agent! ModelRepositoryService -> ContructMessage(): \n\t" << ex.what() << endl;
				}

				return constructedmessage;
			}
		};
	}

	namespace Agent
	{
		using namespace sc2;
		using namespace std;
		//The agent that plays in the environment
		class KoKeKoKoBot : public Agent
		{
			private:
				Model::ModelRepositoryService* _instance = nullptr;

				size_t CountUnitType(UNIT_TYPEID unit_type)
				{
					return Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit_type)).size();
				}

				bool TryBuildStructure(ABILITY_ID ability_type_for_structure, UNIT_TYPEID unit_type = UNIT_TYPEID::TERRAN_SCV)
				{
					const ObservationInterface* observation = Observation();
					const Unit* unit_to_build = nullptr;
					Units units = observation->GetUnits(Unit::Alliance::Self);
					for (const auto& unit : units)
					{
						for (const auto& order : unit->orders)
						{
							if (order.ability_id == ability_type_for_structure)
							{
								return false;
							}
						}

						if (unit->unit_type == unit_type)
						{
							unit_to_build = unit;
						}
					}

					if (ability_type_for_structure == ABILITY_ID::BUILD_REFINERY)
					{
						Actions()->UnitCommand(unit_to_build,
							ability_type_for_structure,
							FindNearestVespeneGeyser(unit_to_build->pos));
					}
					float rx = GetRandomScalar();
					float ry = GetRandomScalar();

					Actions()->UnitCommand(unit_to_build,
						ability_type_for_structure,
						Point2D(unit_to_build->pos.x + rx * 15.0f, unit_to_build->pos.y + ry * 15.0f));

					return true;
				}

				bool TryTrainUnit(ABILITY_ID ability_type_for_training, UNIT_TYPEID ID)
				{
					const ObservationInterface* observation = Observation();
					const Unit* unit_origin = nullptr;
					Units units = observation->GetUnits(Unit::Alliance::Self);
					for (const auto& unit : units)
					{
						if (unit->unit_type == ID)
						{
							unit_origin = unit;
						}
					}
					Actions()->UnitCommand(unit_origin, ability_type_for_training);
					return true;
				}

				bool TryResearch(ABILITY_ID ability_type_for_research, UNIT_TYPEID ID)
				{
					const ObservationInterface* observation = Observation();
					const Unit* unit_origin = nullptr;
					Units units = observation->GetUnits(Unit::Alliance::Self);
					for (const auto& unit : units)
					{
						if (unit->unit_type == ID)
						{
							unit_origin = unit;
						}
					}
					Actions()->UnitCommand(unit_origin, ability_type_for_research);
					return true;
				}

				bool TryBuildRefinery()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 75)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_REFINERY) > 0 && observation->GetMinerals() < 75)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_REFINERY);
				}
				//Command Center Units
				bool TryBuildCommandCenter()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 400)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 0 && observation->GetMinerals() < 400)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_COMMANDCENTER);
				}

				bool TryCommandCenterMorphOrbitalCommand()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}
					return (TryResearch(ABILITY_ID::MORPH_ORBITALCOMMAND, UNIT_TYPEID::TERRAN_COMMANDCENTER));
				}

				bool TryOrbitalCommandSummonMule()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_ORBITALCOMMAND) > 0)
					{
						return false;
					}
					return (TryResearch(ABILITY_ID::EFFECT_CALLDOWNMULE, UNIT_TYPEID::TERRAN_ORBITALCOMMAND));
				}

				bool TryCommandCenterMorphPlanetaryFortress()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_COMMANDCENTER) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return (TryResearch(ABILITY_ID::MORPH_PLANETARYFORTRESS, UNIT_TYPEID::TERRAN_COMMANDCENTER));
				}


				bool TrainSCV()
				{
					const ObservationInterface* observation = Observation();

					if (observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_SCV, UNIT_TYPEID::TERRAN_COMMANDCENTER);
				}
				//Command Center Units
				bool TryBuildSupplyDepot()
				{
					const ObservationInterface* observation = Observation();

					if (observation->GetFoodUsed() <= observation->GetFoodCap() - 2 && observation->GetMinerals() < 100)
						return false;

					return TryBuildStructure(ABILITY_ID::BUILD_SUPPLYDEPOT);
				}
				//Barracks Units
				bool TryBuildBarracks()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_BARRACKS);
				}

				bool TrainMarine()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_MARINE, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainReaper()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 1 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_REAPER, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainMarauder()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 25)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_MARAUDER, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TrainGhost()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 150 && observation->GetVespene() < 125)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_GHOST, UNIT_TYPEID::TERRAN_BARRACKS);
				}
				//Barracks Units

				//Barracks Addons
				bool TryBuildBarracksTechLab()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_BARRACKS);
				}

				bool TryBarracksTechLabResearchCombatShield()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_COMBATSHIELD, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBarracksTechLabResearchStimpack()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_STIMPACK, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBarracksTechLabResearchConcussiveShells()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKSTECHLAB) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_CONCUSSIVESHELLS, UNIT_TYPEID::TERRAN_BARRACKSTECHLAB);
				}

				bool TryBuildBarracksReactor()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_BARRACKS);
				}
				//Barracks addons

				//Factory Units
				bool TryBuildFactory()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_FACTORY);
				}

				bool TryTrainHellion()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_HELLION, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTransformHellionHellbat()
				{
					return TryResearch(ABILITY_ID::MORPH_HELLBAT, UNIT_TYPEID::TERRAN_HELLION);
				}

				bool TryTrainWidowMine()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 75 && observation->GetVespene() < 25)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_WIDOWMINE, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTrainSiegeTank()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 125)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_SIEGETANK, UNIT_TYPEID::TERRAN_FACTORY);
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

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_CYCLONE, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTrainHellbat()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_HELLBAT, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryTransformHellbatHellion()
				{
					return TryResearch(ABILITY_ID::MORPH_HELLION, UNIT_TYPEID::TERRAN_HELLIONTANK);
				}

				bool TryTrainThor()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 300 && observation->GetVespene() < 200)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_THOR, UNIT_TYPEID::TERRAN_FACTORY);
				}
				//Factory Units

				//Factory Addons
				bool TryBuildFactoryTechLab()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_FACTORY);
				}

				bool TryFactoryResearchInfernalPreIgniter()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_INFERNALPREIGNITER, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryFactoryResearchMagFieldAccelerator()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_MAGFIELDLAUNCHERS, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryFactoryResearchDrillingClaws()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetMinerals() < 75 && observation->GetVespene() < 75)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_DRILLINGCLAWS, UNIT_TYPEID::TERRAN_FACTORYTECHLAB);
				}

				bool TryBuildFactoryReactor()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_FACTORY);
				}
				//Factory addons

				//Starport Units
				bool TryBuildStarport()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_STARPORT);
				}

				bool  TryTrainViking()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 150 && observation->GetVespene() < 75)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_VIKINGFIGHTER, UNIT_TYPEID::TERRAN_STARPORT);
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

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_MEDIVAC, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTrainLiberator()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_LIBERATOR, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTransformLiberatorLiberatorAG()
				{
					return TryResearch(ABILITY_ID::MORPH_LIBERATORAGMODE, UNIT_TYPEID::TERRAN_LIBERATOR);
				}

				bool TryTransformLiberatorAGLiberator()
				{
					return TryResearch(ABILITY_ID::MORPH_LIBERATORAAMODE, UNIT_TYPEID::TERRAN_LIBERATORAG);
				}

				bool TryTrainRaven()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene() < 200)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_RAVEN, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryRavenSummonPointDefenseDrone()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_RAVEN) > 0)
					{
						return false;
					}
					return (TryResearch(ABILITY_ID::EFFECT_POINTDEFENSEDRONE, UNIT_TYPEID::TERRAN_RAVEN));
				}

				bool TryRavenSummonAutoTurret()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_RAVEN) > 0)
					{
						return false;
					}
					return (TryResearch(ABILITY_ID::EFFECT_AUTOTURRET, UNIT_TYPEID::TERRAN_RAVEN));
				}

				bool TryTrainBanshee()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_BANSHEE, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryTrainBattlecruiser()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_FUSIONCORE) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 400 && observation->GetVespene() < 300)
					{
						return false;
					}
					return TryTrainUnit(ABILITY_ID::TRAIN_BATTLECRUISER, UNIT_TYPEID::TERRAN_STARPORT);
				}
				//Starport Units

				//Starport Addons
				bool TryBuildStarportTechLab()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_STARPORT);
				}

				bool TryStarportResearchCorvidReactor()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_RAVENCORVIDREACTOR, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchCloakingField()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_BANSHEECLOAKINGFIELD, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchHyperflightRotors()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_BANSHEEHYPERFLIGHTROTORS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchAdvancedBallistics()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_ADVANCEDBALLISTICS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryStarportResearchRapidReignitionSystem()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_HIGHCAPACITYFUELTANKS, UNIT_TYPEID::TERRAN_STARPORTTECHLAB);
				}

				bool TryBuildStarportReactor()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 50)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_REACTOR, UNIT_TYPEID::TERRAN_STARPORT);
				}
				//Starport addons

				bool TryBuildFusionCore()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_FUSIONCORE) > 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_FUSIONCORE);
				}

				bool TryFusionCoreResearchResearchWeaponRefit()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_BATTLECRUISERWEAPONREFIT, UNIT_TYPEID::TERRAN_FUSIONCORE);
				}

				bool TryBuildArmory()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 100)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_ARMORY);
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

					return TryResearch(ABILITY_ID::RESEARCH_TERRANVEHICLEWEAPONS, UNIT_TYPEID::TERRAN_ARMORY);
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

					return TryResearch(ABILITY_ID::RESEARCH_TERRANSHIPWEAPONS, UNIT_TYPEID::TERRAN_ARMORY);
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

					return TryResearch(ABILITY_ID::RESEARCH_TERRANVEHICLEANDSHIPPLATING, UNIT_TYPEID::TERRAN_ARMORY);
				}


				bool TryBuildBunker()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_BUNKER) > 0 && observation->GetMinerals() < 150)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_BUNKER);
				}

				bool TryBuildEngineeringBay()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_COMMANDCENTER) < 1 && observation->GetMinerals() < 125)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) > 0 && observation->GetMinerals() < 125)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_ENGINEERINGBAY);
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

					return TryResearch(ABILITY_ID::RESEARCH_TERRANINFANTRYARMOR, UNIT_TYPEID::TERRAN_ENGINEERINGBAY);
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
						else if (i ==  UPGRADE_ID::TERRANINFANTRYWEAPONSLEVEL3)
						{
							return false;
						}
					}

					return TryResearch(ABILITY_ID::RESEARCH_TERRANINFANTRYWEAPONS, UNIT_TYPEID::TERRAN_ENGINEERINGBAY);
				}

				bool TryBuildGhostAcademy()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene() < 50)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 50)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_GHOSTACADEMY);
				}

				bool TryGhostAcademyResearchPersonalCloaking()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 150 && observation->GetVespene() < 150)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::RESEARCH_PERSONALCLOAKING, UNIT_TYPEID::TERRAN_GHOSTACADEMY);
				}

				bool TryGhostAcademyBuildNuke()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetMinerals() < 100 && observation->GetVespene() < 100)
					{
						return false;
					}
					return TryResearch(ABILITY_ID::BUILD_NUKE, UNIT_TYPEID::TERRAN_GHOSTACADEMY);
				}

				bool TryBuildMissileTurret()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) < 1 && observation->GetMinerals() < 100)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_MISSILETURRET) > 0 && observation->GetMinerals() < 100)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_MISSILETURRET);
				}

				bool TryBuildSensorTower()
				{
					const ObservationInterface* observation = Observation();

					if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) < 1 && observation->GetMinerals() < 125 && observation->GetVespene() < 100)
					{
						return false;
					}

					if (CountUnitType(UNIT_TYPEID::TERRAN_SENSORTOWER) > 0 && observation->GetMinerals() < 125 && observation->GetVespene() < 100)
					{
						return false;
					}

					return TryBuildStructure(ABILITY_ID::BUILD_SENSORTOWER);
				}

				const Unit* FindNearestMineralPatch(const Point2D& start)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Neutral);
					float distance = std::numeric_limits<float>::max();
					const Unit* target = nullptr;
					for (const auto& u : units)
					{
						if (u->unit_type == UNIT_TYPEID::NEUTRAL_MINERALFIELD)
						{
							float d = DistanceSquared2D(u->pos, start);
							if (d < distance) {
								distance = d;
								target = u;
							}
						}
					}
					return target;
				}

				const Unit* FindNearestVespeneGeyser(const Point2D& start)
				{
					Units units = Observation()->GetUnits(Unit::Alliance::Neutral);
					float distance = std::numeric_limits<float>::max();
					const Unit* target = nullptr;
					for (const auto& u : units)
					{
						if (u->unit_type == UNIT_TYPEID::NEUTRAL_VESPENEGEYSER)
						{
							float d = DistanceSquared2D(u->pos, start);
							if (d < distance) {
								distance = d;
								target = u;
							}
						}
					}
					return target;
				}

			public:
				//Creates the POMDP, MCTS, and starts the execution
				virtual void OnGameStart() final
				{
					//Get the instance of the service
					_instance = Model::ModelRepositoryService::GetModelRepositoryServiceInstance();
					//auto myunits = Observation()->GetUnits(Unit::Alliance::Self);
					//std::vector<std::string> unitsstring = std::vector<std::string>();
					//auto ulambda = std::for_each(myunits.begin(), myunits.end(), [&](Unit u) { unitsstring.push_back(UnitTypeToName(u.unit_type)); });

					//Instruct ModelService to start
					std::string message = _instance->ConstructMessage({ "Build Armory,Build Barracks,Build Bunker,Build Command Center,Build Engineering Bay,Build Factory,Build Liberator,Build Reactor,Build Refinery,Build Siege Tank,Build Starport,Build Supply Depot,Build Tech Lab,Build Widow Mine,Train Marine,Train SCV", "0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,3,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,3,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,1,1,0,0,1,0,0,1,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,1,0,1,2,0,1,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1,0,1,0,0,0,2,1,0,0,0,0,1,0,0,2,4,0,0,0,0,0,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,2,1,0,0,3,0,1,1,0,4,3,0,2,1,0,1,0,0,0,4,0,1,6,0,1,1,6" }, "Initialize");
					std::cout << message << std::endl;
					_instance->SendMessageToModelService(message, 5);


					//Create the MCTS and start the expansion to get the sequence of actions
					//Start the POMDP to get the sequence of actions
					//Create the MCTS+POMDP and start the expansion

					//We store the actions in a queue
				}

				virtual void OnStep() final
				{
					string m = _instance->Messages.front();
					cout << m << std::endl;

					//Observe the environment
					//Is it already 20 secs? 
						//If yes, we continue again the MCTS, start POMDP, and create the MCTS+POMDP
						//If no, keep following the actions in queue
						//If no but there is an emergency, we call the MCTS, POMDP and the combination to know what to do

					/* For Guanga */
					//Make the bot, train scv, build supply depot then build barracks then train marine
					//After sufficient marine, attack the enemy. Follow this in summary -> https://github.com/Blizzard/s2client-api/blob/master/docs/tutorial1.md
					//if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT)  0)
					//{
						TryBuildSupplyDepot();
					//}
					if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) == 0)
					{
						TryBuildBarracks();
					}
					if (CountUnitType(UNIT_TYPEID::TERRAN_REFINERY) == 0)
					{
						TryBuildRefinery();
					}
					if(CountUnitType(UNIT_TYPEID::TERRAN_MARINE) < 4)
					{
						TrainMarine();
					}
					TrainMarauder();
					//TryBuildBarracksReactor();
					TryBuildBarracksTechLab();

				}

				virtual void OnGameEnd() final
				{

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
							case UNIT_TYPEID::TERRAN_COMMANDCENTER:
							{
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_SCV);

								break;
							}
							case UNIT_TYPEID::TERRAN_SCV:
							{
								const Unit* mineral_target = FindNearestMineralPatch(unit->pos);
								if (!mineral_target)
								{
									break;
								}
								Actions()->UnitCommand(unit, ABILITY_ID::SMART, mineral_target);
								mineral_target = FindNearestVespeneGeyser(unit->pos);
								if (!mineral_target)
								{
									break;
								}
								Actions()->UnitCommand(unit, ABILITY_ID::SMART, mineral_target);

								break;
							}
							case UNIT_TYPEID::TERRAN_BARRACKS:
							{
								/*Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARINE);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARAUDER);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_REAPER);
								if (CountUnitType(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0)
								{
									Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_GHOST);
								}
								break;*/
							}
							case UNIT_TYPEID::TERRAN_FACTORY:
							{
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_HELLION);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_WIDOWMINE);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_SIEGETANK);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_CYCLONE);
								if (CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0)
								{
									Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_HELLBAT);
									Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_THOR);
								}
							}
							case UNIT_TYPEID::TERRAN_STARPORT:
							{
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_VIKINGFIGHTER);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MEDIVAC);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_RAVEN);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_BANSHEE);
								Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_LIBERATOR);
								if (CountUnitType(UNIT_TYPEID::TERRAN_FUSIONCORE) > 0)
								{
									Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_BATTLECRUISER);
								}
							}
							case UNIT_TYPEID::TERRAN_MARINE:
							{
								const GameInfo& game_info = Observation()->GetGameInfo();
								//Actions()->UnitCommand(unit, ABILITY_ID::ATTACK_ATTACK, game_info.enemy_start_locations.front());
								break;
							}
							default:
							{
								break;
							}
						}
					}
					catch (...)
					{

					}
				}

				virtual void OnUnitDestroyed(const Unit* unit) final
				{

				}

				virtual void OnUnitEnterVision(const Unit* unit) final
				{

				}

				void TryAction(string action)
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
					else if (action.find("BUILD_BARRACKSTECHLAB") != string::npos)
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
					else if (action.find("BUILD_BARRACKSREACTOR") != string::npos)
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
					else if (action.find("BUILD_FACTORYTECHLAB") != string::npos)
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
					else if (action.find("BUILD_FACTORYREACTOR") != string::npos)
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
					else if (action.find("BUILD_STARPORTREACTOR") != string::npos)
					{
						TryBuildStarportReactor();
					}
					else if (action.find("BUILD_STARPORTTECHLAB") != string::npos)
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
				//Function to find the nearest unit from a specific unit
				const Unit* FindNearestOf(Unit source, Unit destination)
				{
					Units units = Observation()->GetUnits(IsUnit(destination.unit_type));
					float distance = std::numeric_limits<float>::max();
					const Unit* target = nullptr;
					for (const auto& u : units)
					{
						if (u->unit_type == destination.unit_type)
						{
							float d = DistanceSquared2D(destination.pos, source.pos);
							if (d < distance) {
								distance = d;
								target = u;
							}
						}
					}
					return target;
				}

				//Takes unit type and alliance
				size_t CountOf(UNIT_TYPEID unit_type, Unit::Alliance alliance)
				{
					return Observation()->GetUnits(alliance, IsUnit(unit_type)).size();
				}
		};
	}
}

using namespace KoKeKoKo;
Model::ModelRepositoryService* Model::ModelRepositoryService::_instance = nullptr;

int main(int argc, char* argv[])
{
	auto coordinator = new sc2::Coordinator();
	auto kokekokobot = new Agent::KoKeKoKoBot();
	auto modelrepositoryservice = Model::ModelRepositoryService::GetModelRepositoryServiceInstance();
	char s;

	try
	{
		//Start the model
		if (modelrepositoryservice->StartListeningToModelService())
		{
			//Start to listen to the model
			if (modelrepositoryservice->StartModelService())
			{
				//Give way to other threads first
				std::this_thread::sleep_for(std::chrono::milliseconds(3000));

				//Start generating a cached repository
				if (modelrepositoryservice->GenerateRepository())
				{
					//Start the game
					coordinator->LoadSettings(argc, argv);
					coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
					coordinator->LaunchStarcraft();
					coordinator->StartGame(sc2::kMapBelShirVestigeLE);
					while (coordinator->Update());

					//Close the connections
					modelrepositoryservice->StopListeningToModelService();
					modelrepositoryservice->StopModelService();
				}
				else
					throw std::exception("Failed to parse repository...");
			}
			else
				throw std::exception("Failed to start the model...");
		}
		else
			throw std::exception("Failed on starting a server for model...");
	}
	catch (...)
	{
		std::cout << "Error Occurred! Failed to catch an application error..." << std::endl;
		std::cerr << "Error in main()! An unhandled exception occurred...." << std::endl;

		//Close the connections
		modelrepositoryservice->StopListeningToModelService();
		modelrepositoryservice->StopModelService();
	}

	return 0;
}