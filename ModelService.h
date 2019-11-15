#pragma once
namespace Services
{
	using namespace std;

	//Facilitates the communication to C# Model, and manages time-related methods.
	class ModelService
	{
		private:
			//Instance of the current ModelService. This provides access to methods for
			//commmunicating with C# Model and management of time for operations.
			static ModelService* Instance;
			//The running C# Model.
			PROCESS_INFORMATION Model;
			//The time that this service must communicate with C# Model to update about
			//the effects of C++ Agent's actions in the environment.
			time_t NextUpdateTime;

			ModelService(const ModelService&);
			ModelService& operator=(const ModelService&);
			//Initializes the required properties to handle communication with C# Model.
			//In addition, it starts the C# Model using CreateProcessA() method.
			ModelService()
			{
				//Perform fields initialization
				Model = { 0 };
				NextUpdateTime = { 0 };

				//Prepare fields for starting C# Model
				STARTUPINFO startupinfo = { 0 };
				LPSTR modelabsolutedirectory = new char[MAX_PATH];
				LPSTR activeprojectdirectory = new char[MAX_PATH];
				ZeroMemory(&startupinfo, sizeof(startupinfo));
				ZeroMemory(&Model, sizeof(Model));
				startupinfo.cb = sizeof(startupinfo);

				//Get the current project directory
				if (GetCurrentDirectoryA(MAX_PATH, activeprojectdirectory) != 0)
				{
					#if _DEBUG
						string absolutedirectory = (((string)activeprojectdirectory) + "\\ModelService\\bin\\Debug\\ModelService.exe");
					#else
						string absolutedirectory = (((string)activeprojectdirectory) + "\\ModelService\\bin\\Release\\ModelService.exe");
					#endif
					modelabsolutedirectory = const_cast<char*>(absolutedirectory.c_str());
					cout << "(C++)Successfully retrieved the current directory!" << endl;

					//Start the C# Model
					/*if (CreateProcessA(NULL, modelabsolutedirectory, NULL, NULL, FALSE, 0, NULL, NULL, &startupinfo, &Model))
						cout << "(C++)Successfully started the C# Model!" << endl;
					else
						throw exception("Failed to start the C# Model...");*/
				}
				else
					throw exception("Failed to get the current directory...");
			}

		public:
			//Creates an instance of ModelService and returns it initialized.
			static ModelService* CreateNewModelService()
			{
				if (Instance == nullptr)
					Instance = new ModelService();

				return Instance;
			}

			static ModelService* GetExistingModelService()
			{
				if (Instance == nullptr)
					return Services::ModelService::CreateNewModelService();

				return Instance;
			}

			//Checks if the current system time is less than the next update time
			bool ShouldOperationsContinue()
			{
				time_t current_time = chrono::system_clock::to_time_t(chrono::system_clock::now());

				return (current_time < NextUpdateTime);
			}

			//Updates the ModelService by sending an update message about the environment
			//to C# Model. Afterwards, it returns a sequence of message to be parsed by
			//the bot and apply a corresponding action. Lastly, it updates the NextUpdateTime.
			queue<string> UpdateModelService(string update)
			{
				queue<string> messages = queue<string>();

				//Prepare fields for starting the server
				char readerbuffer[4096] = { 0 };
				char writerbuffer[4096] = { 0 };
				DWORD readerpointer = 0;
				DWORD writerpointer = 0;
				HANDLE server = INVALID_HANDLE_VALUE;
				LPSTR name = TEXT("\\\\.\\pipe\\AgentServer");
				ZeroMemory(readerbuffer, sizeof(readerbuffer));
				ZeroMemory(writerbuffer, sizeof(writerbuffer));

				try
				{
					//Start the server
					server = CreateNamedPipeA(name, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT, PIPE_UNLIMITED_INSTANCES, sizeof(writerbuffer), sizeof(readerbuffer), NMPWAIT_USE_DEFAULT_WAIT, NULL);
					if (server != INVALID_HANDLE_VALUE)
					{
						//Wait for C# Model to connect
						if (ConnectNamedPipe(server, NULL))
						{
							cout << "(C++)The C# Model has sucessfully connected to C++ Agent!" << endl;

							//Send the update message to C# Model
							update = update + "\r\n"; //Append the terminator for C# Model
							strcpy_s(writerbuffer, update.c_str());
							if (WriteFile(server, writerbuffer, update.size(), &writerpointer, NULL))
							{
								FlushFileBuffers(server);
								cout << "(C++)Successfully sent the update message to C# Model!" << endl;

								//Read the C# Model's reply
								if (ReadFile(server, readerbuffer, sizeof(readerbuffer), &readerpointer, NULL))
								{
									readerbuffer[readerpointer] = '\0';
									cout << "(C++)Successfully recieved C# Model's message!" << endl;

									//Enqueue the C# Model's message
									string raw_message = string(readerbuffer), current_message = "";
									istringstream parsed_message(raw_message);
									while (getline(parsed_message, current_message, ';'))
										messages.push(current_message);

									//Dequeue the NextUpdateTime
									struct tm raw_time = { 0 };
									istringstream time(messages.front());
									time >> get_time(&raw_time, "%m:%d:%Y:%H:%M:%S");
									NextUpdateTime = mktime(&raw_time);
									messages.pop();

									//Disconnect C# Model
									DisconnectNamedPipe(server);
								}
								else
									throw exception("Failed to recieve C# Model's message...");
							}
							else
								throw exception("Failed to send the update message to C# Model...");
						}

						//Close the server
						CloseHandle(server);
					}
					else
						throw exception("Failed to start the server...");
				}
				catch (const exception& ex)
				{
					cout << "(C++)Error Occurred! " << ex.what() << endl;
					messages = queue<string>();
				}

				return messages;
			}

			//Terminates the C# Model and disposes the instance of this class.
			void StopModelService()
			{
				//Close the C# Model
				WaitForSingleObject(Model.hProcess, INFINITE);
				CloseHandle(Model.hProcess);
				CloseHandle(Model.hThread);
				cout << "(C++)Successfully closed the C# Model..." << endl;

				//Dispose this class
				Instance = nullptr;
			}
	};
}