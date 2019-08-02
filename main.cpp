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
		//The summary of parsed COMMANDSREPOSITORY and RESOURCESREPOSITORY
		const string REPOSITORYSERVICE_FILENAME = "Documents\\Data\\Repository.csv";
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
				//ModelService startup information
				STARTUPINFO startupinfo;
				//ModelService process information
				PROCESS_INFORMATION processinformation;
				//A pipe from the ModelRepositoryService to ModelService
				HANDLE _server = INVALID_HANDLE_VALUE;
				//A pipe from the ModelService to ModelRepositoryService
				HANDLE _client = INVALID_HANDLE_VALUE;
				//A map of created threads where key is the function name, and value is the thread that runs the given function name
				map<string, thread*> _createdthreads = map<string, thread*>();
				//A lock when handling messages
				mutex _messagelock;

				void CreateRepositoryFile()
				{

				}

				void ParseCommandsRepository()
				{

				}

				void ParseResourcesRepository()
				{

				}

				//Keeps listening to the model and pushes the sent messages to a queue
				void ListenToModel()
				{
					try
					{
						DWORD dwread = 0;
						string message = "";
						char buffer[2048];
						bool isnewmessage = false;

						while (KeepListeningToModel)
						{
							while (ReadFile(_server, buffer, sizeof(buffer) - 1, &dwread, NULL))
							{
								buffer[dwread] = '\0';
								isnewmessage = true;
							}

							if (isnewmessage)
							{
								_messagelock.lock();
								message = string(buffer);								
								Messages.push_back(message);
								_messagelock.unlock();

								isnewmessage = false;
							}

							this_thread::sleep_for(chrono::milliseconds(2000));
						}
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to keep listening to model service..." << endl;
						cerr << "Error in Agent! ModelRepositoryService -> ListenToModel(): \n\t" << ex.what() << endl;
					}
				}

			public:
				deque<string> Messages = deque<string>();
				//If the program should keep listening to model
				bool KeepListeningToModel = false;

				//Returns the created instance of ModelRepositoryService
				static ModelRepositoryService* GetModelRepositoryServiceInstance()
				{
					if (_instance == nullptr)
						_instance = new ModelRepositoryService();

					return _instance;
				}

				bool GenerateRepository()
				{
					try
					{
						return true;
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to generate repository..." << endl;
						cerr << "Error in Agent! ModelRepositoryService -> GenerateRepository(): \n\t" << ex.what() << endl;
					}

					return false;
				}

				//Starts the ModelService and performs handshake messages
				bool ExecuteModelService()
				{
					try
					{
						DWORD dwprocess = 0, dwread = 0;
						LPSTR executable = new char[MAX_PATH], serverpipename = new char[MAX_PATH], clientpipename = new char[MAX_PATH];
						string executablefilepath = GetRelativeFilepathOf(MODELSERVICE_FILENAME), sucessmessage = "";
						char buffer[2048];
						bool successconnection = false;

						//Perform Initialization
						ZeroMemory(&startupinfo, sizeof(startupinfo));
						ZeroMemory(&processinformation, sizeof(processinformation));
						startupinfo.cb = sizeof(startupinfo);
						executable = const_cast<char *>(executablefilepath.c_str());
						serverpipename = TEXT("\\\\.\\pipe\\AgentServer");
						clientpipename = TEXT("\\\\.\\pipe\\ModelServer");

						//Create a pipe to ModelService
						_server = CreateNamedPipeA(serverpipename, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, 2048, 2048, 0, NULL);
						if (_server != INVALID_HANDLE_VALUE)
						{
							//Since the pipe to ModelService is a success
							//Start running the process
							if (CreateProcessA(NULL, executable, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &processinformation))
							{
								//Read the message of the model
								WaitForSingleObject(processinformation.hProcess, 3000);
								for (int retry = 0; ((!successconnection) && (retry < 5)); retry++)
								{
									cout << "Waiting for the ModelService to send a message... Retries Left: " << retry << endl;
									while (ReadFile(_server, buffer, sizeof(buffer) - 1, &dwread, NULL))
									{
										cout << "Reading the message..." << endl;
										buffer[dwread] = '\0';
									}

									sucessmessage = string(buffer);
									if (sucessmessage == "Success!")
										successconnection = true;
								}

								//If successfully received a message from model
								if (successconnection)
								{
									//Start to keep listening to the model service
									KeepListeningToModel = true;
									auto listentomodel = new thread(&Model::ModelRepositoryService::ListenToModel, this);
									listentomodel->detach();
									_createdthreads.insert(make_pair("ListenToModel", listentomodel));

									//Create the client pipe to model service
									_client = CreateFileA(clientpipename, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
									if (_client != INVALID_HANDLE_VALUE)
									{
										if (WaitForAMessage("Ready Reply", 5))
										{
											cout << "received!" << endl;
										}
										else
											throw exception("Failed to receive confirmation from model service...");
									}
									else
										throw exception("Failed to start a client pipe to the model service...");
								}
								else
									throw exception("Failed to receive a message from model service...");
							}
							else
								throw exception("Failed to start model service...");
						}
						else
							throw exception("Failed to create a server pipe for the model service...");

						return true;
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to execute model service..." << endl;
						cerr << "Error in Agent! ModelRepositoryService -> ExecuteModelService(): \n\t" << ex.what() << endl;
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

				bool SendAMessageToModel(string message)
				{
					try
					{
						DWORD dwwrite = 0;
						char buffer[2048];

						strcpy_s(buffer, message.c_str());

						if (_client != INVALID_HANDLE_VALUE)
						{
							WriteFile(_client, buffer, sizeof(buffer), &dwwrite, NULL);
							FlushFileBuffers(_client);

							return true;
						}
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to send a message to model..." << endl;
						cerr << "Error in Agent! ModelRepositoryService -> SendAMessageToModel(): \n\t" << ex.what() << endl;
					}

					return false;
				}

				bool WaitForAMessage(string expected_message, int retries)
				{
					try
					{
						bool isfound = false;
						string message = "";

						_messagelock.lock();
						//If there is no message
						for (int retry = retries; ((Messages.empty()) && (retry > 0)); retry--)
						{
							cout << "Waiting for message..." << endl;
						}

						for (int retry = retries; ((!isfound) && (retry > 0)); retry--)
						{
							for (auto elements : Messages)
							{
								if (elements == expected_message)
								{
									isfound = true;
									break;
								}
							}
						}
						_messagelock.unlock();

						return isfound;
					}
					catch (const exception& ex)
					{
						cout << "Error Occurred! Failed to wait for a message..." << endl;
						cerr << "Error in Agent! ModelRepositoryService -> WaitForAMessage(): \n\t" << ex.what() << endl;
					}

					return false;
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

			public:
				virtual void OnGameStart() final
				{

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

	modelrepositoryservice->ExecuteModelService();
	char s;
	std::cin >> s;
	return 0;
}