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
	class ModelRepositoryService
	{
		private:
			//The instance of ModelRepositoryService
			static ModelRepositoryService* _instance;
			//A map of created threads. The string is the function passed to the thread parameter
			map<string, thread*> _createdthreads = map<string, thread*>();
			//The parsed commandsrepository
				//Rank	//Owner Command	  //Subsequent Commands from Owner Command
			map<string, map<string, vector<string>>> _parsedcommandsrepository = map<string, map<string, vector<string>>>();

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
				int count = 0;
				while (_parsedcommandsrepository.size() == 0 && count <= 100)
				{
					cout << "Hi" << endl;
					count++;
				}
			}

			void ParseCommandsRepository(bool* file_exists)
			//Opens then reads CommandsRepository. Creates an internal storage of parsed commands
			{
				try
				{
					ifstream commandsrepository(GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME));
					string commandsrepositoryline = "", commandsrepositorycomma = "", previouscommand = "";

					if (commandsrepository.is_open())
					{
						*file_exists = true;
						cout << "Successful! Parsing CommandsRepository..." << endl;

						while (getline(commandsrepository, commandsrepositoryline))
						//Read the CommandsRepository per line
						{
							cout << "Hello" << endl;
						}
					}
					else
						throw new exception("Failed to open CommandsRepository...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to parse CommandsRepository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ParseCommandsRepository(" << *file_exists << ")" << endl;
					*file_exists = false;
				}
			}

			void ParseResourcesRepository(bool* file_exists)
			//TODO
			{
				try
				{
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
					}
					else
						throw new exception("Failed to open ResourcesRepository...");
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to parse ResourcesRepository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ParseResourcesRepository(" << *file_exists << ")" << endl;
					*file_exists = false;
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
			//A string that pertains to the current project directory
			LPSTR CurrentProjectDirectory = new char[MAX_PATH];

			static ModelRepositoryService* GetModelRepositoryServiceInstance()
			//Returns the created instance of ModelRepositoryService
			{
				if (_instance == nullptr)
					_instance = new ModelRepositoryService();

				return _instance;
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
	while (s != 'a')
	{
		std::cin >> s;
	}
	coordinator->LoadSettings(argc, argv);
	coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
	coordinator->LaunchStarcraft();
	coordinator->StartGame(sc2::kMapBelShirVestigeLE);
	while (coordinator->Update());

	return 0;
}