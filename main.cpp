#define NOMINMAX

#include <fstream>
#include <iostream>
#include <sc2api/sc2_api.h>
#include <sstream>
#include <thread>
#include <Windows.h>

namespace KoKeKoKo
{
	using namespace std;
	//The POMDP Model Computation and MCTS Model Computation using R
	const string MODELSERVICE_FILENAME = "ModelService\\bin\\Release\\ModelService.exe";
	//The parsed and combination of COMMANDSREPOSITORY and RESOURCESREPOSITORY
	const string REPOSITORYSERVICE_FILENAME = "Documents\\Data\\Repository.csv";
	//The extracted commands from the replay data
	const string COMMANDSREPOSITORY_FILENAME = "Documents\\Data\\CommandsRepository.csv";
	//The extracted resources from the replay data
	const string RESOURCESREPOSITORY_FILENAME = "Documents\\Data\\ResourcesRepository.csv";
	const list<string> RANKS = { "Bronze", "Silver", "Gold", "Diamond", "Platinum", "Master", "Grandmaster" };

	class ModelRepositoryService
	{
		private:
			//The instance of ModelRepositoryService
			static ModelRepositoryService* _instance;
			//A map of created threads. The string is the function passed to the thread parameter
			map<string, thread*> _createdthreads = map<string, thread*>();
			//A map of important line in the REPOSITORYSERVICE
			map<string, int> _repositoryfilehash = map<string, int>();
			//The parsed commandsrepository
				//Rank	//Owner Command	  //Subsequent Commands from Owner Command
			map<string, map<string, vector<string>>> _parsedcommandsrepository = map<string, map<string, vector<string>>>();
			//If the corresponding function is finished parsing
			bool _finishedparsingcommands = false, _finishedparsingresources = false;

			ModelRepositoryService()
			//Gets the current project directory. Afterwards, parses the COMMANDSREPOSITORY and RESOURCESREPOSITORY. It is a
			//successful instance if both repositories exists and can be open
			{
				try
				{
					if (GetCurrentDirectory(MAX_PATH, CurrentProjectDirectory) != 0)
					{
						IsSuccessfulInstance = GenerateRepository();
					}
					else
						throw new exception("Failed to get current directory...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to create instance of ModelRepositoryService..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ModelRepositoryService()" << endl;
					IsSuccessfulInstance = false;
				}
			}

			void CreateRepositoryFile()
			{
				int failed_connection = 0;
				IsSuccessfulRepository = false;
				while ((!(_finishedparsingcommands && _finishedparsingresources)) && (failed_connection <= 5))
				{
					this_thread::sleep_for(chrono::milliseconds(1000));
					failed_connection++;
					cout << "Still not finish" << endl;
					_finishedparsingresources = true;
				}
				if (failed_connection > 5)
					return;

				try
				{
					ofstream repository(GetRelativeFilepathOf(REPOSITORYSERVICE_FILENAME));
					int linenumber = 0;

					if (repository.is_open())
					{
						//Place the Commands Repository first
						for (auto rank : _parsedcommandsrepository)
						{
							//Insert Rank
							repository << rank.first << endl;
							_repositoryfilehash.insert(make_pair((rank.first + "C"), ++linenumber));
							//The States
							int statescount = rank.second.size(), scount = 1;
							for (auto states : rank.second)
								repository << states.first << ((scount < statescount++) ? "," : "");
							repository << endl;
							_repositoryfilehash.insert(make_pair(("States" + rank.first[0]), ++linenumber));
							//The Transition Matrix
							int transitionscount = statescount * 2, tcount = 1;
							for (auto xstates : rank.second)
							{
								repository << xstates.first << " -> ";
								for (auto ystates : xstates.second)
								{
									repository << ystates << " ";
									//repository << count(ystates.second.begin(), ystates.second.end(), xstates.first) << ((tcount < transitionscount++) ? "," : "");
									//repository << endl;
								}
								repository << endl;
							}
							repository << endl;
							_repositoryfilehash.insert(make_pair(("Transition" + rank.first[0]), ++linenumber));
						}

						//Place the Resources Repository second

						IsSuccessfulRepository = true;
						repository.close();
					}
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to create repository file..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> CreateRepositoryFile()" << endl;
					IsSuccessfulRepository = false;
				}
			}

			void ParseCommandsRepository(bool* file_exists)
			//Opens then reads CommandsRepository. Creates an internal storage of parsed commands
			{
				try
				{
					_finishedparsingcommands = false;
					ifstream commandsrepository(GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME));
					string commandsrepositoryline = "", commandsrepositorycomma = "", currentrank = "", previouscommand = "", previousowner = "";

					if (commandsrepository.is_open())
					{
						*file_exists = true;
						cout << "Successful! Parsing CommandsRepository..." << endl;

						while (getline(commandsrepository, commandsrepositoryline))
						//Read the CommandsRepository per line
						{
							stringstream byline(commandsrepositoryline);
							for (int column = 0; getline(byline, commandsrepositorycomma, ','); column++) 
							{								
								if (find(RANKS.begin(), RANKS.end(), commandsrepositorycomma) != RANKS.end())
								//Check if the current string is a rank
								{
									//We set the current rank by the seen rank in file
									currentrank = commandsrepositorycomma; 
									//We add the current rank in the parsed commands repository
									_parsedcommandsrepository.insert(make_pair(currentrank, map<string, vector<string>>()));
									cout << "Parsing Current Rank: " << currentrank << endl;
									continue;
								}

								if (column == 2)
								//This assumes that the data is grouped by the same player
								//If the current column is the commands
								{
									//Perform initialization of command tracking
									if (previouscommand == "")
										previouscommand = commandsrepositorycomma;

									//Check if the current command is existing in the list
									if (_parsedcommandsrepository[currentrank].find(commandsrepositorycomma) == _parsedcommandsrepository[currentrank].end())
										_parsedcommandsrepository[currentrank].insert(make_pair(commandsrepositorycomma, vector<string>()));

									//If existing
									_parsedcommandsrepository[currentrank][previouscommand].push_back(commandsrepositorycomma);
									//Update previous command
									previouscommand = commandsrepositorycomma;
								}
							}
						}

						_finishedparsingcommands = true;
						commandsrepository.close();
					}
					else
						throw new exception("Failed to open CommandsRepository...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to parse CommandsRepository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ParseCommandsRepository(" << *file_exists << ")" << endl;
					*file_exists = false;
					_finishedparsingcommands = false;
				}
			}

			void ParseResourcesRepository(bool* file_exists)
			//TODO
			{
				try
				{
					_finishedparsingresources = false;
					ifstream resourcesrepository(GetRelativeFilepathOf(RESOURCESREPOSITORY_FILENAME));
					string resourcesrepositoryline = "", resourcesrepositorycomma = "";

					if (resourcesrepository.is_open())
					{
						*file_exists = true;
						cout << "Successful! Parsing ResourcesRepository..." << endl;

						while (getline(resourcesrepository, resourcesrepositoryline))
						//Read the ResourcesRepository per line
						{

						}

						_finishedparsingresources = true;
						resourcesrepository.close();
					}
					else
						throw new exception("Failed to open ResourcesRepository...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to parse ResourcesRepository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ParseResourcesRepository(" << *file_exists << ")" << endl;
					*file_exists = false;
					_finishedparsingresources = false;
				}
			}

			int RunExecutableFile(string executable_filename)
			//Creates a process and runs the executable file
			{
				STARTUPINFO startupinfo;
				PROCESS_INFORMATION processinformation;
				DWORD dword;
				LPSTR executablefilepath = new char[MAX_PATH];
				int processresult = -1;

				try
				{
					ZeroMemory(&startupinfo, sizeof(startupinfo));
					startupinfo.cb = sizeof(startupinfo);
					ZeroMemory(&processinformation, sizeof(processinformation));
					executablefilepath = const_cast<char *>(executable_filename.c_str());

					if (CreateProcessA(NULL, executablefilepath, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &processinformation))
					{
						WaitForSingleObject(processinformation.hProcess, INFINITE);
						processresult = GetExitCodeProcess(processinformation.hProcess, &dword);
						CloseHandle(processinformation.hProcess);
						CloseHandle(processinformation.hThread);
					}
					else
						throw new exception("Failed to create process...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to run the executable file..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> RunExecutableFile(" << executable_filename << ")" << endl;
					processresult = -1;
				}

				return processresult;
			}

			string GetRelativeFilepathOf(string filename)
			//Returns the filename concatenated with the current project directory
			{
				return ((string)CurrentProjectDirectory) + "\\" + filename;
			}

		public:
			//If the instance of ModelRepositoryService is successful
			bool IsSuccessfulInstance = false;
			//If the Repository file has been created
			bool IsSuccessfulRepository = false;
			//A string that pertains to the current project directory
			LPSTR CurrentProjectDirectory = new char[MAX_PATH];

			static ModelRepositoryService* GetModelRepositoryServiceInstance()
			//Returns the created instance of ModelRepositoryService
			{
				if (_instance == nullptr)
					_instance = new ModelRepositoryService();

				return _instance;
			}

			void StopThread()
			//Tries to join all joinable threads in the _createdthreads
			{
				try
				{
					for (auto thread : _createdthreads)
					{
						if (thread.second->joinable())
						{
							thread.second->join();
							_createdthreads.erase(thread.first);
						}
					}
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to join all created threads..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> StopThread()" << endl;
				}
			}

			void StopThread(string thread_name)
			//Tries to join the given thread
			{
				try
				{
					auto iterator = _createdthreads.find(thread_name);
					if (iterator != _createdthreads.end())
					{
						if (_createdthreads[thread_name]->joinable())
						{
							_createdthreads[thread_name]->join();
							_createdthreads.erase(iterator);
						}
					}
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to join " << thread_name << "..." << endl;
					cerr << "Error Occurred! ModelRepositoryService(" << thread_name << ")" << endl;
				}
			}

			bool GenerateRepository()
			//Creates threads to parse the repositories and another thread to create the repository file
			//Returns true if both repositories exists and openable
			{
				bool iscommandsrepositoryexists = false, isresourcesrepositoryexists = false;

				try
				{
					auto parsecommandsrepository = new thread(&ModelRepositoryService::ParseCommandsRepository, this, &iscommandsrepositoryexists);
					auto parseresourcesrepository = new thread(&ModelRepositoryService::ParseResourcesRepository, this, &isresourcesrepositoryexists);
					auto createrepository = new thread(&ModelRepositoryService::CreateRepositoryFile, this);

					if (_createdthreads.find("ParseCommandsRepository") == _createdthreads.end())
						_createdthreads.insert(make_pair("ParseCommandsRepository", parsecommandsrepository));
					else
					{
						if (parsecommandsrepository->joinable())
							parsecommandsrepository->join();
						throw new exception("Failed to map ParseCommandsRepository thread! An existing thread is in the map...");
					}
					if (_createdthreads.find("ParseResourcesRepository") == _createdthreads.end())
						_createdthreads.insert(make_pair("ParseResourcesRepository", parseresourcesrepository));
					else
					{
						if (parseresourcesrepository->joinable())
							parseresourcesrepository->join();
						throw new exception("Failed to map ParseResourcesRepository thread! An existing thread is in the map...");
					}
					if (_createdthreads.find("CreateRepositoryFile") == _createdthreads.end())
						_createdthreads.insert(make_pair("CreateRepositoryFile", createrepository));
					else
					{
						if (createrepository->joinable())
							createrepository->join();
						throw new exception("Failed to map CreateRepositoryFile thread! An existing thread is in the map...");
					}

					//TOBEREMOVED
					isresourcesrepositoryexists = true;
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to generate repository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> GenerateRepository()" << endl;
					StopThread("ParseCommandsRepository");
					StopThread("ParseResourcesRepository");
					StopThread("CreateRepositoryFile");
				}

				return iscommandsrepositoryexists && isresourcesrepositoryexists;
			}

			int ExecuteModelService()
			//Calls the ModelService.exe and returns the AbilityType ID
			{
				int abilityid = RunExecutableFile(KoKeKoKo::MODELSERVICE_FILENAME);
				return abilityid;
			}
	};


	using namespace sc2;
	class KoKeKoKoBot : public Agent
	{
		private:

		public:
			virtual void OnGameStart() final
			//On game start, we need to create the MCTS tree, start execution of POMDP and create the combination of the two
			{
				auto modelrepositoryservice = KoKeKoKo::ModelRepositoryService::GetModelRepositoryServiceInstance();
				//Create the MCTS and start the expansion to get the sequence of actions
				//Start the POMDP to get the sequence of actions
				//Create the MCTS+POMDP and start the expansion

				//We store the actions in a queue
			}

			virtual void OnStep() final
			{
				//Observe the environment
				//Is it already 20 secs? 
					//If yes, we continue again the MCTS, start POMDP, and create the MCTS+POMDP
					//If no, keep following the actions in queue
					//If no but there is an emergency, we call the MCTS, POMDP and the combination to know what to do

				/* For Guanga */
				//Make the bot, train scv, build supply depot then build barracks then train marine
				//After sufficient marine, attack the enemy. Follow this in summary -> https://github.com/Blizzard/s2client-api/blob/master/docs/tutorial1.md
				
			}

			virtual void OnUnitIdle(const Unit* unit) final
			{

			}

			/* For Guanga */
			//Create these functions in public
				//TryToExecuteAction - will receive an input (can be string or ABILITYTYPE ID) and execute the corresponding private function
				//FindNearestOf - will receive two input, the source and target. find the nearest target from the source
				//CountOf - will receive an input (UNITYPEID). returns the count of that unit
			//Create these functions in private
				//Execute{Ability} - the corresponding action in TryToExecuteAction
					//example: TryBuildBarracks, TryBuildSupplyDepot

	};
}

KoKeKoKo::ModelRepositoryService* KoKeKoKo::ModelRepositoryService::_instance = nullptr;
int main(int argc, char* argv[])
{
	auto kokekokobot = new KoKeKoKo::KoKeKoKoBot();
	auto coordinator = new sc2::Coordinator();
	auto modelrepositoryservice = KoKeKoKo::ModelRepositoryService::GetModelRepositoryServiceInstance();
	char s;

	if (!(modelrepositoryservice->IsSuccessfulInstance))
	{
		std::cout << "There is a problem in the ModelRepositoryService, please try to resolve the issue to start the game." << std::endl;
		std::cin >> s;
		return -1;
	}
	//std::cout << modelrepositoryservice->ExecuteModelService() << std::endl;
	std::cout << "Preparing StarCraft II..." << std::endl;
	std::cin >> s;
	coordinator->LoadSettings(argc, argv);
	coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
	coordinator->LaunchStarcraft();
	coordinator->StartGame(sc2::kMapBelShirVestigeLE);
	while (coordinator->Update());

	return 0;
}