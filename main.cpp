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
				for (const auto& order : unit->orders)
				{
					if (order.ability_id == ability_type_for_training)
					{
						return false;
					}

					if (unit->unit_type == ID)
					{
						unit_origin = unit;
					}
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
				for (const auto& order : unit->orders)
				{
					if (order.ability_id == ability_type_for_research && order.progress <= 1.0f)
					{
						return false;
					}

					if (unit->unit_type == ID)
					{
						unit_origin = unit;
					}
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

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
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

		bool  TryTrainWidowMine()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 75 && observation->GetVespene < 25)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_WIDOWMINE, UNIT_TYPEID::TERRAN_FACTORY);
		}

		bool  TryTrainSiegeTank()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene < 125)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_SIEGETANK, UNIT_TYPEID::TERRAN_FACTORY);
		} 

		bool  TryTrainCyclone()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_CYCLONE, UNIT_TYPEID::TERRAN_FACTORY);
		}

		bool  TryTrainHellbat()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_HELLBAT, UNIT_TYPEID::TERRAN_FACTORY);
		}

		bool  TryTrainThor()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_FACTORYTECHLAB) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 300 && observation->GetVespene < 200)
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

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_FACTORY) < 1 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}

			return TryBuildStructure(ABILITY_ID::BUILD_STARPORT);
		}

		bool  TryTrainViking()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 150 && observation->GetVespene < 75)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_VIKINGFIGHTER, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool  TryTrainMedivac()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene < 100)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_MEDIVAC, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool  TryTrainLiberator()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene < 150)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_LIBERATOR, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool  TryTrainRaven()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 2 && observation->GetMinerals() < 100 && observation->GetVespene < 200)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_RAVEN, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool  TryTrainBanshee()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 3 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_BANSHEE, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool  TryTrainBattlecruiser()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORTTECHLAB) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_FUSIONCORE) > 0 && observation->GetFoodCap() - observation->GetFoodUsed() < 6 && observation->GetMinerals() < 400 && observation->GetVespene < 300)
			{
				return false;
			}
			return TryTrainUnit(ABILITY_ID::TRAIN_BATTLECRUISER, UNIT_TYPEID::TERRAN_STARPORT);
		}
		//Starport Units

		//Starport Addons
		bool TryBuildFactoryTechLab()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_STARPORT) > 0 && observation->GetMinerals() < 50 && observation->GetVespene() < 25)
			{
				return false;
			}
			return TryResearch(ABILITY_ID::BUILD_TECHLAB, UNIT_TYPEID::TERRAN_STARPORT);
		}

		bool TryBuildFactoryReactor()
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

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene < 150)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_FUSIONCORE) > 1 && observation->GetMinerals() < 150 && observation->GetVespene < 150)
			{
				return false;
			}

			return TryBuildStructure(ABILITY_ID::BUILD_FUSIONCORE);
		}

		bool TryBuildArmory()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_ARMORY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene < 100)
			{
				return false;
			}

			return TryBuildStructure(ABILITY_ID::BUILD_ARMORY);
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

		bool TryBuildGhostAcademy()
		{
			const ObservationInterface* observation = Observation();

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 && observation->GetMinerals() < 150 && observation->GetVespene < 50)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0 && observation->GetMinerals() < 150 && observation->GetVespene < 50)
			{
				return false;
			}

			return TryBuildStructure(ABILITY_ID::BUILD_GHOSTACADEMY);
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

			if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1 || CountUnitType(UNIT_TYPEID::TERRAN_ENGINEERINGBAY) < 1 && observation->GetMinerals() < 125 && observation->GetVespene < 100)
			{
				return false;
			}

			if (CountUnitType(UNIT_TYPEID::TERRAN_SENSORTOWER) > 0 && observation->GetMinerals() < 125 && observation->GetVespene < 100)
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
			TryBuildSupplyDepot();
			TryBuildBarracks();

		}

		virtual void OnUnitIdle(const Unit* unit) final
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

					break;
				}
				case UNIT_TYPEID::TERRAN_BARRACKS:
				{
					Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARINE);
					Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARAUDER);
					Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_REAPER);
					if (CountUnitType(UNIT_TYPEID::TERRAN_GHOSTACADEMY) > 0)
					{
						Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_GHOST);
					}
					break;
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
					Actions()->UnitCommand(unit, ABILITY_ID::ATTACK_ATTACK, game_info.enemy_start_locations.front());
					break;
				}
				default:
				{
					break;
				}

			/* For Guanga */
			//Create these functions in public
				//TryToExecuteAction - will receive an input (can be string or ABILITYTYPE ID) and execute the corresponding private function
				//FindNearestOf - will receive two input, the source and target. find the nearest target from the source
				//CountOf - will receive an input (UNITYPEID). returns the count of that unit
			//Create these functions in private
				//Execute{Ability} - the corresponding action in TryToExecuteAction
					//example: TryBuildBarracks, TryBuildSupplyDepot

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
		size_t CountOf(UNIT_TYPEID unit_type, Unit unit)
		{
			return Observation()->GetUnits(unit.alliance, IsUnit(unit_type)).size();
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
	};
}