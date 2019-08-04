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
		const string COMMANDSREPOSITORY_FILENAME = "Documents\\Data\\CommandsRepository.csv";
		//The extracted resources from the replay training data
		const string RESOURCESREPOSITORY_FILENAME = "Documents\\Data\\ResourcesRepository.csv";

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

							}

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
						finished_parsing = false;
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
						WaitForSingleObject(processinformation.hProcess, INFINITE);
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
		};
	}

	namespace Agent
	{
		using namespace sc2;
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

		//The agent that plays in the environment
		class KoKeKoKoBot : public Agent
		{
			private:
				Model::ModelRepositoryService* _instance = nullptr;

			public:
				virtual void OnGameStart() final
				{
					//Get the instance of the service
					_instance = Model::ModelRepositoryService::GetModelRepositoryServiceInstance();


				}

				virtual void OnStep() final
				{

				}

				virtual void OnGameEnd() final
				{

				}				

				virtual void OnUnitCreated(const Unit* unit) final
				{

				}

				virtual void OnUnitIdle(const Unit* unit) final
				{

				}

				virtual void OnUnitDestroyed(const Unit* unit) final
				{

				}

				virtual void OnUnitEnterVision(const Unit* unit) final
				{

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

		return 1;
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