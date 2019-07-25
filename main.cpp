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

			ModelRepositoryService()
			//Gets the current project directory. Afterwards, parses the COMMANDSREPOSITORY and RESOURCESREPOSITORY
			{
				try
				{
					if (GetCurrentDirectory(MAX_PATH, CurrentProjectDirectory) != 0)
					{
						IsSuccessfulInstance = true;
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

			void ParseCommandsRepository()
			{

			}

			void ParseResourcesRepository()
			{

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
			{

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
		//std::cin >> s;
		return -1;
	}
	//std::cout << modelrepositoryservice->ExecuteModelService() << std::endl;
	std::cout << "Preparing StarCraft II..." << std::endl;
	//std::cin >> s;
	coordinator->LoadSettings(argc, argv);
	coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });
	coordinator->LaunchStarcraft();
	coordinator->StartGame(sc2::kMapBelShirVestigeLE);
	while (coordinator->Update());

	return 0;
}