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
	const string MODELSERVICE_FILENAME = "ModelService\\bin\\Release\\ModelService.exe";				//Contains the POMDP Model, MCTS Model, and both
	const string REPOSITORY_FILENAME = "Documents\\Data\\Repository.csv";					//The parsed and combination of CommandsRepository and ResourcesRepository
	const string COMMANDSREPOSITORY_FILENAME = "Documents\\Data\\CommandsRepository.csv";	//The preprocessed training data from replay files
	const string RESOURCESREPOSITORY_FILENAME = "Documents\\Data\\ResourcesRepository.csv";	//The preprocessed training data from replay files
	class ModelRepositoryService
	{
		private:
			static ModelRepositoryService* _instance;						//The instance of the service

			//Constructor
			ModelRepositoryService()
			{
				try
				{
					IsSuccessfulInstance = (GetCurrentDirectory(MAX_PATH, CurrentProjectDirectory) != 0);
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to get current project directory..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ModelRepositoryService()" << endl;
					IsSuccessfulInstance = false;
				}
			}

			//Reads the COMMANDSREPOSITORY_FILENAME and Creates a tally of commands by rank
			void ParseCommandsRepository(bool* is_successful)
			{
				try
				{
					ifstream commandsrepository(GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME));
					string commandsrepositoryline = "", commandsrepositorycomma = "", previouscommand = "";

					if (commandsrepository.is_open())
					{
						cout << "Successful! Opened " << GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME) << "..." << endl;
						while (getline(commandsrepository, commandsrepositoryline))
						//Read line by line
						{
							stringstream streambyline(commandsrepositoryline);
							for (int column = 0; getline(streambyline, commandsrepositorycomma, ','); column++)
							{
								if (column == 2)
								//The current column is a command
								{
									//Initialize previous command
									if (previouscommand == "")
										previouscommand = commandsrepositorycomma;

									//Check if that command is existing
									if (_toberevisedcommands.find(commandsrepositorycomma) == _toberevisedcommands.end())
									{
										_toberevisedcommands.insert(make_pair(commandsrepositorycomma, vector<string>()));
										_toberevisedlist.insert(make_pair(commandsrepositorycomma, 0));
									}

									//Update fields
									previouscommand = commandsrepositorycomma;
								}
							}
						}
						cout << "Successful! Parsed " << GetRelativeFilepathOf(COMMANDSREPOSITORY_FILENAME) << "..." << endl;
						commandsrepository.close();
						*is_successful = true;
					}					
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to parse commands repository..." << endl;
					cerr << "Error Occurred! ModelRepositoryService -> ParseCommandsRepository()" << endl;
					*is_successful = false;
				}
			}

			//Reads the RESOURCESREPOSITORY_FILENAME and Creates a sequence of resources by rank
			void ParseResourcesRepository(bool* is_successful)
			{

			}

		public:
			bool IsSuccessfulInstance = false;								//If the creation of ModelRepositoryService instance is successful
			LPSTR CurrentProjectDirectory = new char[MAX_PATH];				//Contains the project directory

			map<string, vector<string>> _toberevisedcommands = map<string, vector<string>>();
			map<string, int> _toberevisedlist = map<string, int>();
			map<string, int> _toberevisedrepo = map<string, int>();

			//Returns the created ModelRepositoryService instance
			static ModelRepositoryService* GetModelRepositoryServiceInstance()
			{
				if (_instance == nullptr)
					_instance = new ModelRepositoryService();

				return _instance;
			}

			bool GenerateRepository()
			{
				bool parsecommands_successful = false, parseresources_successful = false;				
				auto test = new thread(&ModelRepositoryService::ParseCommandsRepository, this, &parsecommands_successful);
				ofstream repo(GetRelativeFilepathOf(REPOSITORY_FILENAME));
				int fileline = 0;
				if (repo.is_open())
				{
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					repo << "Example of file line" << endl;
					++fileline;
					
					if (test->joinable())
					{
						test->join();
						if (!parsecommands_successful)
							return false;
						repo << endl;
						++fileline;
						repo << endl;
						++fileline;

						repo << "Bronze" << endl;
						_toberevisedrepo.insert(make_pair("Bronze", fileline));
						++fileline;
						for (auto states : _toberevisedlist)
						{
							repo << states.first << ",";
						}
						_toberevisedrepo.insert(make_pair("BronzeStates", fileline));
						++fileline;
						for (auto yaxis : _toberevisedcommands)
						{
							for (auto xaxis : _toberevisedlist)
								repo << count(yaxis.second.begin(), yaxis.second.end(), xaxis.first) << ",";
						}
						_toberevisedrepo.insert(make_pair("BronzeTransition", fileline));
						++fileline;
						repo.close();
						return true;
					}
				}
				
				return false;
			}

			//Returns the cocanated current directory and filename
			string GetRelativeFilepathOf(string filename)
			{
				return ((string)CurrentProjectDirectory) + "\\" + filename;
			}

			//Creates REPOSITORY_FILENAME. Stores the important lines in the file. Generates all important data for the model
			//Getcorrespondingenum
			double RunExecutableFile(std::string executable_file)
			{
				cout << executable_file << endl;
				STARTUPINFO startupinfo;
				PROCESS_INFORMATION processinformation;
				DWORD dword;
				LPSTR executablefilepath = new char[MAX_PATH];
				double processresult = -1;

				try
				{
					ZeroMemory(&startupinfo, sizeof(startupinfo));
					startupinfo.cb = sizeof(startupinfo);
					ZeroMemory(&processinformation, sizeof(processinformation));
					executablefilepath = const_cast<char *>(executable_file.c_str());

					if (CreateProcessA(NULL, executablefilepath, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &processinformation))
					{
						std::cout << "Successful Execution! Running " << executablefilepath << std::endl;

						WaitForSingleObject(processinformation.hProcess, INFINITE);
						processresult = GetExitCodeProcess(processinformation.hProcess, &dword);
						CloseHandle(processinformation.hProcess);
						CloseHandle(processinformation.hThread);
					}
					else
						throw processresult;
				}
				catch (...)
				{
					std::cout << "Error Occurred! Failed to execute the executable file..." << std::endl;
					std::cerr << "Error Occurred! ProgramUtilities -> RunExecutableFile" << std::endl;
				}

				return processresult;
			}
	};

	using namespace sc2;
	class KoKeKoKoBot : public Agent
	{
		private:
			//TryToExecuteAbility
			//TryToFind
			//More Utility function
		public:

	};
}

KoKeKoKo::ModelRepositoryService* KoKeKoKo::ModelRepositoryService::_instance = nullptr;
int main(int argc, char* argv[])
{
	KoKeKoKo::KoKeKoKoBot* kokekokobot = new KoKeKoKo::KoKeKoKoBot();
	sc2::Coordinator* coordinator = new sc2::Coordinator();
	KoKeKoKo::ModelRepositoryService* modelrepositoryservice = KoKeKoKo::ModelRepositoryService::GetModelRepositoryServiceInstance();
	char s;

	std::cout << modelrepositoryservice->CurrentProjectDirectory << std::endl;
	if (modelrepositoryservice->GenerateRepository())
	{
		int stateline = modelrepositoryservice->_toberevisedrepo["BronzeStates"];
		int transitionline = modelrepositoryservice->_toberevisedrepo["BronzeTransition"];
		int result = modelrepositoryservice->RunExecutableFile(modelrepositoryservice->GetRelativeFilepathOf(KoKeKoKo::MODELSERVICE_FILENAME + " " + std::to_string(stateline) + " " + std::to_string(transitionline)));

		std::cout << result << std::endl;
	}
	//system("");
	//modelrepositoryservice->RunExecutableFile("C:\\Users\\Kelsey\\source\\repos\\KoKeKoKo\\SC2API_Binary_vs2017\\RProject1\\Script.R");
	//int test = system("C:\\Users\\Kelsey\\source\\repos\\KoKeKoKo\\SC2API_Binary_vs2017\\RProject1\\Script.R");
	//std::cout << test << std::endl;
	std::cin >> s;

	return 0;
}