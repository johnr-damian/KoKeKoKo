#pragma once
namespace Services
{
	using namespace std;

	//Facilitates the communication to C# Model, and manages the outgoing messages
	//from C++ Agent using named pipes.
	class ModelService
	{
		private:
			//Instance of the current service that provide methods for messaging C# Model.
			static ModelService* Instance;
			//A lock implementation for threading
			mutex ThreadLocker;
			//The running thread of C# Model.
			PROCESS_INFORMATION Model;
			//The recieved messages from C# Model.
			queue<string> IncomingMessages;
			//The messages to be sent to C# Model.
			queue<string> OutgoingMessages;
			//The running thread that keeps receiving messages from C# Model.
			tuple<thread *, bool> KeepReadingMessages;
			//The running thread that keeps sending messages to C# Model.
			tuple<thread *, bool> KeepWritingMessages;

			ModelService(const ModelService&);
			ModelService& operator=(const ModelService&);
			//Initializes the required properties to handle communication to C# Model, and runs the C# Model.
			ModelService()
			{
				//Perform initializations
				IncomingMessages = queue<string>();
				OutgoingMessages = queue<string>();
				KeepReadingMessages = tuple<thread *, bool>(nullptr, false);
				KeepWritingMessages = tuple<thread *, bool>(nullptr, false);

				//Prepare fields for starting C# Model
				STARTUPINFO startupinfo = { 0 };
				LPSTR absolutedirectory = new char[MAX_PATH];
				LPSTR currentdirectory = new char[MAX_PATH];
				ZeroMemory(&startupinfo, sizeof(startupinfo));
				ZeroMemory(&Model, sizeof(Model));
				startupinfo.cb = sizeof(startupinfo);

				//Get the absolute directory of C# Model
				if (GetCurrentDirectoryA(MAX_PATH, currentdirectory) != 0)
				{
					#if _DEBUG
						string directory = (((string)currentdirectory) + "\\ModelService\\bin\\Debug\\ModelService.exe");
					#else
						string directory = (((string)currentdirectory) + "\\ModelService\\bin\\Release\\ModelService.exe");
					#endif
					absolutedirectory = const_cast<char *>(directory.c_str());
					cout << "Successfully retrieved the current directory!" << endl;

					////Start the C# Model
					//if (CreateProcessA(NULL, absolutedirectory, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &Model))
					//	cout << "Successfully started the C# Model!" << endl;
					//else
					//	throw exception("Failed to start the C# Model...");
				}
				else
					throw exception("Failed to get the current directory...");
			}

			//The process that keeps recieving messages from C# Model.
			void ReadMessagesFromModel()
			{
				//Check if the task has been cancelled
				ThreadLocker.lock();
				if (get<1>(KeepReadingMessages))
				{
					ThreadLocker.unlock();
					return;
				}
				ThreadLocker.unlock();

				//Prepare fields to create the server
				DWORD readerpointer = 0;
				HANDLE server = INVALID_HANDLE_VALUE;
				LPSTR servername = TEXT("\\\\.\\pipe\\AgentServer");
				char buffer[4096] = { 0 };
				ZeroMemory(buffer, sizeof(buffer));

				//Create the server where the C# Model will connect to
				server = CreateNamedPipeA(servername, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, sizeof(buffer), sizeof(buffer), 0, NULL);
				if (server != INVALID_HANDLE_VALUE)
				{
					//Wait for the C# Model to connect
					if (ConnectNamedPipe(server, NULL))
					{
						cout << "The C# Model has successfully connected to C++ Agent!" << endl;

						while (true)
						{
							while (!PeekNamedPipe(server, NULL, NULL, NULL, NULL, NULL))
							{
								cout << "Waiting for a message..." << endl;
								this_thread::sleep_for(chrono::milliseconds(2000));
							}

							while (ReadFile(server, buffer, sizeof(buffer), &readerpointer, NULL))
								buffer[readerpointer] = '\0';

							string message = string(buffer);
							cout << "Recieved a message from C# Model! Your message: " << message << endl;
							if (message == "exit")
								break;
						}


						DisconnectNamedPipe(server);
					}

					//Close the server
					CloseHandle(server);
				}
				else
					throw exception("Failed to create a server...");
			}

			void SendMessagesToModel()
			{
				
			}


		public:
			#pragma region Services Methods
			//Creates an initialized ModelService and runs the C# Model.
			static ModelService* CreateNewModelService()
			{
				if (Instance == nullptr)
					Instance = new ModelService();

				return Instance;
			}

			//Starts the ModelService by creating then starting the server and client
			void StartModelService()
			{
				auto keepreadingmessages = new thread(&Services::ModelService::ReadMessagesFromModel, this);
				KeepReadingMessages = make_tuple(keepreadingmessages, false);
			}

			//Stops the ModelService by stopping the server and client, and waiting for the C# Model to terminate.
			//In addition, it also disposes this instance and other properties
			void StopModelService()
			{
				//Send the cancellation request
				ThreadLocker.lock();
				get<1>(KeepReadingMessages) = true;
				get<1>(KeepWritingMessages) = true;
				ThreadLocker.unlock();

				//Close the server and client
				if (get<0>(KeepReadingMessages) != nullptr)
					get<0>(KeepReadingMessages)->join();
				if (get<0>(KeepWritingMessages) != nullptr)
					get<0>(KeepWritingMessages)->join();

				//Close the C# Model
				WaitForSingleObject(Model.hProcess, INFINITE);
				CloseHandle(Model.hProcess);
				CloseHandle(Model.hThread);

				//Dispose this class
				Instance = nullptr;
			}
			#pragma endregion

			void Test()
			{

			}
	};
}