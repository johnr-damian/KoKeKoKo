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
	using namespace std;
	//The POMDP Model Computation and MCTS Model Computation using R
	const string MODELSERVICE_FILENAME = "ModelService\\bin\\Debug\\ModelService.exe";
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
					int linenumber = 0, statescount = 0, transitionscount = 0, scount = 0, tcount = 0;

					if (repository.is_open())
					{
						//Place the Commands Repository first
						for (auto rank : _parsedcommandsrepository)
						{
							//Insert Rank
							repository << rank.first << endl;
							_repositoryfilehash.insert(make_pair((rank.first + "C"), ++linenumber));
							//The States
							statescount = rank.second.size();
							transitionscount = pow(statescount, 2);
							scount = 1;
							tcount = 1;
							for (auto states : rank.second)
								repository << states.first << ((scount++ < statescount) ? "," : "");
							repository << endl;
							_repositoryfilehash.insert(make_pair((rank.first + "States"), ++linenumber));
							//The Transition Matrix
							for (auto xstates : rank.second)
							{
								for (auto ystates : rank.second)
									repository << count(xstates.second.begin(), xstates.second.end(), ystates.first) << ((tcount++ < transitionscount) ? "," : "");
							}
							repository << endl;
							_repositoryfilehash.insert(make_pair((rank.first + "Transition"), ++linenumber));
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
					string commandsrepositoryline = "", commandsrepositorycomma = "", currentrank = "", currentowner = "", previouscommand = "";

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
								//If the current column is either a Rank or timestamp
								if (column == 0)
								{
									if (currentrank == "")
									{
										//Get the current rank for the following line of commands
										if (find(RANKS.begin(), RANKS.end(), commandsrepositorycomma) != RANKS.end())
										{
											currentrank = commandsrepositorycomma;
											_parsedcommandsrepository.insert(make_pair(currentrank, map<string, vector<string>>()));
										}
									}
									else
									{
										//Get the current rank for the following line of commands
										if (find(RANKS.begin(), RANKS.end(), commandsrepositorycomma) != RANKS.end())
										{
											//Repeat last command
											_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);
											currentowner = "";
											previouscommand = "";
											currentrank = commandsrepositorycomma;
											_parsedcommandsrepository.insert(make_pair(currentrank, map<string, vector<string>>()));
										}
									}
									continue;
								}

								//Get the owner of the commands
								if (column == 1)
								{
									if (currentowner == "")
										currentowner = commandsrepositorycomma;
									else
									{
										if (currentowner != commandsrepositorycomma)
										{
											currentowner = commandsrepositorycomma;
											_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);
											previouscommand = "";
										}
									}
									continue;
								}

								//Get the sequential commands executed by the same owner
								if (column == 2)
								{
									if (previouscommand == "")
										previouscommand = commandsrepositorycomma;

									//Check if the current command has been mapped
									if (_parsedcommandsrepository[currentrank].find(commandsrepositorycomma) == _parsedcommandsrepository[currentrank].end())
										//If not mapped, add the command to the map
										_parsedcommandsrepository[currentrank].insert(make_pair(commandsrepositorycomma, vector<string>()));

									_parsedcommandsrepository[currentrank][previouscommand].push_back(commandsrepositorycomma);
									previouscommand = commandsrepositorycomma;
								}
							}
						}
						_parsedcommandsrepository[currentrank][previouscommand].push_back(previouscommand);

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
					startupinfo.dwFlags = STARTF_USESHOWWINDOW;
					startupinfo.wShowWindow = SW_MINIMIZE;
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

			//int ExecuteModelService()
			////Calls the ModelService.exe and returns the AbilityType ID
			//{
			//	int abilityid = RunExecutableFile(KoKeKoKo::MODELSERVICE_FILENAME);
			//	return abilityid;
			//}

			void ExecuteModelService(int* process_return)
			{
				*process_return = RunExecutableFile(KoKeKoKo::MODELSERVICE_FILENAME);
			}
	};
	


	using namespace sc2;
	class KoKeKoKoBot : public Agent
	{
		private:
			ModelRepositoryService* _modelrepositoryservice;
			queue<string> _actions;
			string _currentrank = "";
			bool _generateactions = false, _generatingactions = false, _renewplan = false;

			template <typename T> static T GetRandomElement(T begin, int size)
			//Returns a random element from a container
			{
				try
				{
					unsigned long offset = 0, divisor = 0;

					divisor = (RAND_MAX + 1) / size;
					do
					{
						offset = rand() / size;
					} while (offset >= divisor);
					advance(begin, offset);
				}
				catch (...)
				{
					cout << "Error Occurred! Failed to get a random element..." << endl;
					cerr << "Error Occurred! KoKeKoKoBot -> GetRandomElement(" << begin << ", " << size << ")" << endl;
				}

				return begin;
			}

			void GenerateActions()
			{
				//try
				//{
				//	SECURITY_ATTRIBUTES securityattributes;
				//	ZeroMemory(&securityattributes, sizeof(securityattributes));
				//	securityattributes.nLength = sizeof(SECURITY_ATTRIBUTES);
				//	securityattributes.bInheritHandle = TRUE;
				//	securityattributes.lpSecurityDescriptor = NULL;
				//	HANDLE childstdin_in = NULL, childstdin_out = NULL, childstdout_in = NULL, childstdout_out = NULL;

				//	bool testpipe = CreatePipe(&childstdout_in, &childstdout_out, &securityattributes, 0);
				//	if (!testpipe)
				//		throw new exception("Failed to create pipe...");
				//	bool ensurepipe = SetHandleInformation(childstdout_in, HANDLE_FLAG_INHERIT, 0);
				//	if (!ensurepipe)
				//		throw new exception("Failed to ensure...");
				//	bool testpipe2 = CreatePipe(&childstdin_in, &childstdin_out, &securityattributes, 0);
				//	if (!testpipe2)
				//		throw new exception("Failed to create pipe...");
				//	bool ensurepipe2 = SetHandleInformation(childstdin_out, HANDLE_FLAG_INHERIT, 0);
				//	if (!ensurepipe2)
				//		throw new exception("Failed to ensure...");

				//	STARTUPINFO startupinfo;
				//	PROCESS_INFORMATION processinformation;
				//	DWORD dword;
				//	LPSTR executablefilepath = new char[MAX_PATH];
				//	int processresult = -1;

				//	ZeroMemory(&startupinfo, sizeof(startupinfo));
				//	startupinfo.cb = sizeof(startupinfo);
				//	startupinfo.hStdError = childstdout_out;
				//	startupinfo.hStdInput = childstdout_out;
				//	startupinfo.hStdOutput = childstdin_in;
				//	startupinfo.dwFlags |= STARTF_USESTDHANDLES;
				//	startupinfo.dwFlags |= CREATE_NO_WINDOW;
				//	ZeroMemory(&processinformation, sizeof(processinformation));
				//	executablefilepath = const_cast<char *>(MODELSERVICE_FILENAME.c_str());

				//	//int testint = _modelrepositoryservice->ExecuteModelService();
				//	//if (testint != 0)
				//		//cout << testint << endl;
				//	if (!(CreateProcessA(NULL, executablefilepath, NULL, NULL, TRUE, 0, NULL, NULL, &startupinfo, &processinformation)))
				//		throw new exception("Failed to created process...");
				//	else
				//		cout << GetLastError() << endl;
				//	WaitForSingleObject(processinformation.hProcess, INFINITE);
				//	DWORD dwread, dwwrite;
				//	char chbuf[4096];
				//	bool testw = false, testr = false;
				//	while (_generateactions)
				//	{
				//		_generatingactions = true;

				//		if (_renewplan)
				//		//If there is an urgency or need to update difficulty
				//		{
				//			_renewplan = true;
				//		}
				//		else
				//		{
				//			

				//			testw = WriteFile(childstdin_out, "Test Writing to Child", dwread, &dwwrite, NULL);
				//			CloseHandle(childstdin_out);
				//			testr = ReadFile(childstdout_in, chbuf, 4096, &dwread, NULL);
				//			CloseHandle(childstdout_in);
				//			cout << chbuf << endl;
				//		}
				//	}

				//	CloseHandle(processinformation.hProcess);
				//	CloseHandle(processinformation.hThread);
				//}
				//catch (...)
				//{
				//	cout << "Error Occurred! Failed to generate actions..." << endl;
				//	cerr << "Error Occurred! KoKeKoKoBot -> GenerateActions()" << endl;
				//	_generatingactions = false;
				//}

				HANDLE hpipe = INVALID_HANDLE_VALUE;
				BOOL fcon = FALSE;
				char buffer[1024];
				string reply = "", s = "";
				DWORD dwread = 0, dwwrite = 0;
				LPSTR pipename = TEXT("\\\\.\\pipe\\Kokekoko");
				
				hpipe = CreateNamedPipeA(pipename, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, 512, 512, 0, NULL);

				int testresult = 0;
				
				if (hpipe != INVALID_HANDLE_VALUE)
				{
					auto testthread = thread(&ModelRepositoryService::ExecuteModelService, _modelrepositoryservice, &testresult);
					
					fcon = ConnectNamedPipe(hpipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
					if (fcon)
					{
						
						
						
						while (ReadFile(hpipe, buffer, sizeof(buffer) - 1, &dwread, NULL))
						{
							buffer[dwread] = '\0';
							cout << buffer << endl;
							break;
						}
						cout << "Let's start replying: ";
						cin >> reply;
						while (reply != "exit")
						{
							cout << "Write to Child: ";
							cin >> reply;
							WriteFile(hpipe, reply.c_str(), sizeof(reply.c_str()), &dwwrite, NULL);
						}
					}

					CloseHandle(hpipe);
				}

				/*hpipe = CreateFile(TEXT("\\\\.\\pipe\\Pipe"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
				if (hpipe != INVALID_HANDLE_VALUE)
				{
					WriteFile(hpipe, "Hello Pipe!\n", sizeof(buffer), &dwwrite, NULL);
					while (ReadFile(hpipe, buffer, sizeof(buffer) - 1, &dwread, NULL) != FALSE)
					{
						buffer[dwread] = '\0';
						cout << buffer << endl;
						if (buffer == "exit")
							break;
						WriteFile(hpipe, "I copy my child!", sizeof(buffer) - 1, &dwwrite, NULL);

					}
					CloseHandle(hpipe);
				}*/
			}

		public:
			void InitializeBotParameters(string enemy_difficulty)
			//A temporary function? Sets the initial difficulty and starts the thread
			{
				_currentrank = enemy_difficulty;
				_generateactions = true;
				_generatingactions = false;
				_renewplan = false;
				GenerateActions();
			}

			virtual void OnGameStart() final
			//On game start, we need to create the MCTS tree, start execution of POMDP and create the combination of the two
			{
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
	kokekokobot->InitializeBotParameters("Bronze");
	std::cin >> s;
	coordinator->LoadSettings(argc, argv);		
	coordinator->SetParticipants({ sc2::CreateParticipant(sc2::Race::Terran, kokekokobot), sc2::CreateComputer(sc2::Race::Terran, sc2::Difficulty::VeryEasy) });	
	coordinator->LaunchStarcraft();
	coordinator->StartGame(sc2::kMapBelShirVestigeLE);
	while (coordinator->Update());

	return 0;
}