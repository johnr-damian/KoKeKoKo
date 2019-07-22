#define NOMINMAX
#include <sc2api/sc2_api.h>

#include <iomanip>	//for time
#include <iostream>	//for cin and cout
#include <fstream>	//for file stream
#include <future>	//for async operations
#include <thread>	//for thread
#include <map>		//for dictionary
#include <sstream>	//for string stream
#include <Windows.h> //for exes

using namespace sc2;



class ProgramUtilities
{
	private:
		static ProgramUtilities* _instance;
		std::string _currentdirectory;

		ProgramUtilities()
		{
			_currentdirectory = "";
			std::cout << "Warning! Current Directory Field is empty!" << std::endl;
		}

		ProgramUtilities(LPSTR current_directory)
		{
			_currentdirectory = current_directory;
			std::cout << "Successful Update! Current Directory: " << _currentdirectory << std::endl;
		}


	public:
		static const std::string MODELSERVICE_FILENAME;
		static const std::string COMMANDSREPOSITORY_FILENAME;
		static const std::string RESOURCESREPOSITORY_FILENAME;

		static ProgramUtilities* GetProgramUtilitiesInstance()
		{
			if (_instance == nullptr)
				_instance = new ProgramUtilities();

			return _instance;
		}

		static ProgramUtilities* GetProgramUtilitiesInstance(LPSTR current_directory)
		{
			if (_instance == nullptr)
				_instance = new ProgramUtilities(current_directory);

			return _instance;
		}

		static std::string GetProjectDirectory()
		{
			LPSTR projectdirectory = new char[MAX_PATH];

			try
			{
				return (GetCurrentDirectory(MAX_PATH, projectdirectory) != 0) ? projectdirectory : "";
			}
			catch (...)
			{
				std::cout << "Error Occurred! Failed to retrieve the project directory..." << std::endl;
				std::cerr << "Error Occurred! ProgramUtilities -> GetProjectDirectory" << std::endl;
			}

			return "";
		}

		template <typename T> static T GetRandomElement(T begin, int size)
		{
			try
			{
				unsigned long offset = 0, divisor = 0;

				divisor = (RAND_MAX + 1) / size;
				do 
				{
					offset = std::rand() / divisor;
				} while (offset >= divisor);
				std::advance(begin, offset);
			}
			catch (...)
			{
				std::cout << "Error Occured! Failed to generate a random offset... Returning the first element instead..." << std::endl;
				std::cerr << "Error Occured! ProgramUtilities -> GetRandomElement" << std::endl;
			}

			return begin;
		}

		double RunExecutableFile(std::string executable_file)
		{
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

		std::string GetRelativeFilePath(std::string filename)
		{
			std::string path;
			
			try
			{
				path = (std::string)_currentdirectory + "\\" + filename;
			}
			catch (const std::exception&)
			{
				path = "";
			}

			return path;
		}
};

class RepositoryService
{
	private:
		static RepositoryService* _instance;
		std::string _commandsrepositoryfilepath;
		std::string _resourcesrepositoryfilepath;
		//	Y axis					//X axis
		std::map<std::string, std::vector<std::string>> _originalCommandsRepository = std::map<std::string, std::vector<std::string>>();
		std::map<std::string, std::vector<std::string>> _originalResourcesRepository = std::map<std::string, std::vector<std::string>>();

		std::map<std::string, int> _listofcommands = std::map<std::string, int>();
		

		RepositoryService();

		RepositoryService(std::string commands_repository, std::string resources_repository)
		{
			_commandsrepositoryfilepath = commands_repository;
			_resourcesrepositoryfilepath = resources_repository;
		}

	public:
		FILE *s;
		int command_size = 0;

		static RepositoryService* GetRepositoryServiceInstance(std::string commands_repository, std::string resources_repository)
		{
			if (_instance == nullptr)
				_instance = new RepositoryService(commands_repository, resources_repository);

			return _instance;
		}

		bool ParseCommandRepository()
		{
			try
			{
				
				freopen_s(&s, "Test.txt", "w", stdout);
				std::ifstream CommandsRepository(_commandsrepositoryfilepath);
				if (CommandsRepository.is_open())
				{
					std::cout << "Opened file" << std::endl;
					std::cout << "Scoping has been removed due to insufficient number of data" << std::endl;
					std::string replayfileline = "", previousowner = "";
					bool backtostart = false;
					
					while (std::getline(CommandsRepository, replayfileline))
					{
						std::stringstream byline(replayfileline);
						std::string bycomma = "";
						for (int column = 0; std::getline(byline, bycomma, ','); column++)
						{
							//std::cout << bycomma << " ";
							if (column == 2) //Commands
							{
								//Check if that command is existing
								if (_originalCommandsRepository.find(bycomma) == _originalCommandsRepository.end())
									_originalCommandsRepository.insert(std::make_pair(bycomma, std::vector<std::string>()));
								//Initialize the previous history of commands
								if (previousowner == "") //Initialization
									previousowner = bycomma;
								
								//_originalCommandsRepository[bycomma].insert(_originalCommandsRepository[bycomma].begin(), bycomma);		
								_originalCommandsRepository[previousowner].push_back(bycomma);
								if (_listofcommands.find(bycomma) == _listofcommands.end())
									_listofcommands.insert(std::make_pair(bycomma, 0));
								previousowner = bycomma;
								++command_size;
							}
						}
						std::cout << std::endl;
					}
				}

				return true;
			}
			catch (...)
			{

			}

			return false;
		}

		std::map<std::string, std::vector<std::string>> GetChain()
		{
			return _originalCommandsRepository;
		}

		std::map<std::string, int> GetList()
		{
			return _listofcommands;
		}
};

class Bot : public Agent {
public:
    virtual void OnGameStart() final 
	{
		std::string replayfilepath = "", replayfileline = "", markovchainfilepath = "";

		std::cout << "Gathering Data for the Model...";
		while (replayfilepath.empty())
		{
			std::cout << "\n\tEnter Input File Path: ";
			std::cin >> replayfilepath;
		}

		testcommand.insert(testcommand.begin(), "Train SCV");
		testcommand.insert(testcommand.begin(), "Train Marine");
		testcommand.insert(testcommand.begin(), "Build Supply Depot");
		testcommand.insert(testcommand.begin(), "Build Barracks");
		testcommand.insert(testcommand.begin(), "Attack");

		try
		{
			std::ifstream replayfile(replayfilepath);
			if (replayfile.is_open())
			{
				std::cout << "\n\n\t\tSuccessfully opened the file '" << replayfilepath << "'!" << std::endl;

				int currentscope = 0, nextscope = 20;
				bool getnextcolumn = false;
				while (std::getline(replayfile, replayfileline))
				{
					std::cout << "\t\t\tCurrent Line: " << replayfileline << std::endl;					
										
					std::stringstream byline(replayfileline);
					std::string bycomma = "";
					for (int column = 0; std::getline(byline, bycomma, ','); column++)
					{
						std::cout << "\t\t\t\tCurrent Column: " << bycomma << std::endl;

						//Current column is a timestamp
						if (column == 1)
						{
							//Convert the string to seconds
							const char* timecolumn = bycomma.c_str();
							int time = 0, minutes = 0, seconds = 0;
							sscanf_s(timecolumn, "%d:%d", &minutes, &seconds);
							time = (minutes * 60) + seconds;
							std::cout << "\t\t\t\t\tConverted Timestamp: " << time << std::endl;

							//Check if the current seconds is not before the next interval
							if (time > nextscope)
							{
								std::cout << "\t\t\t\t\tUpdating the current scope..." << std::endl;
								std::cout << "\t\t\t\t\t\tFrom Scope: " << currentscope << " -> To Scope: " << nextscope << std::endl;
								currentscope = nextscope;
								nextscope += 20;
								std::cout << "\t\t\t\t\t\tScope: " << currentscope << " -> Next Scope: " << nextscope << std::endl;
							}

							//Check if the current scope have a markov chain
							if (markovchainMacro.find(currentscope) == markovchainMacro.end())
							{
								std::cout << "\t\t\t\t\tSuccessfully added the current scope to the Markov Chain!" << std::endl;
								markovchainMacro.insert(std::make_pair(currentscope, std::vector<std::string>()));
							}
						}
						
						//If current column is a command, get the next column
						if (column == 3)
							getnextcolumn = (bycomma == "Cmd") ? true : false;

						//If current column is the command value
						if ((column == 4) && getnextcolumn)
						{
							//std::cout << "\t\t\t\t\tInserting the command value..." << std::endl;
							//markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), bycomma);

							std::cout << "Doing an impromptu preprocessing..." << std::endl;
							if (bycomma.find("Train SCV") != std::string::npos)
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), "Train SCV");
							else if(bycomma.find("Train Marine") != std::string::npos)
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), "Train Marine");
							else if(bycomma.find("Build Supply Depot") != std::string::npos)
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), "Build Supply Depot");
							else if(bycomma.find("Build Barracks") != std::string::npos)
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), "Build Barracks");
							else if(bycomma.find("Attack") != std::string::npos)
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), "Attack");
							else
								markovchainMacro[currentscope].insert(markovchainMacro[currentscope].begin(), *ProgramUtilities::GetRandomElement(testcommand.begin(), testcommand.size()));
						}
					}
				}

				std::cout << "\n\n\t\tClosing the file '" << replayfilepath << "'..." << std::endl;
				replayfile.close();
			}			
		}
		catch (...)
		{
			std::cout << "An error occured in reading the data!" << std::endl;
			std::cin >> replayfilepath;
			exit(-1);
		}

		std::cout << "\n\nCreating a simple Markov Chain...";
		/*while (markovchainfilepath.empty())
		{
			std::cout << "\n\tEnter Output File Path: ";
			std::cin >> markovchainfilepath;
		}*/

		std::cout << "Markov Chain Size: " << markovchainMacro.size() << std::endl;
		if (markovchainMacro.size() <= 0)
			return;

		try
		{
			//std::ofstream markovchainfile(markovchainfilepath);
			//if (markovchainfile.is_open())
			//{
				std::cout << "\n\n\t\tGenerating the Markov Chain..." << std::endl;
				for (auto const& element : markovchainMacro)
				{
					//markovchainfile << element.first << " -> [";
					std::cout << element.first << std::endl;
					std::cout << "\t\t\tMarkov Chain Scope: " << element.first << std::endl;
					std::cout << "\t\t\tNumber of States: " << element.second.size() << std::endl;

					//markovchainfile << element.second[0] << "(" << (std::count(element.second.begin(), element.second.end(), element.second[0]) / element.second.size())*100 << "%)";
					//for (auto value = std::next(element.second.begin()); value != element.second.end(); value++)
						//markovchainfile << ", " << *value << "(" << (std::count(element.second.begin(), element.second.end(), *value) / element.second.size())*100 << "%)";

					for (auto const& value : element.second)
						std::cout << "\t" << value << "(" << (std::count(element.second.begin(), element.second.end(), value) / element.second.size()) * 100 << "%)" << std::endl;

					//markovchainfile << "," << std::endl;
				}
				std::cout << "\n\n\t\tFinished generating the Markov Chain!" << std::endl;
			//}
		}
		catch (...)
		{
			std::cout << "An error occured in creating the markov chain!" << std::endl;
			std::cin >> markovchainfilepath;
			exit(-1);
		}

		std::cout << "\n\n\n\nProceeding to the Game!" << std::endl;
    }

    virtual void OnStep() final 
	{
		int gameloop = Observation()->GetGameLoop();

		std::cout << "Game Loop: " << gameloop << std::endl;
		if (gameloop > gamescope)
		{
			std::cout << "Updating markov chain..." << std::endl;
			gamescope = nextgamescope;
			nextgamescope += 448;
			markovscope = nextmarkovscope;
			nextmarkovscope += 20;
		}

		if (markovchainMacro.find(markovscope) == markovchainMacro.end())
			return;

		std::cout << "Number of States in current Markov Chain: " << markovchainMacro[markovscope].size();

		//Pick a random element
		std::string action = *ProgramUtilities::GetRandomElement(markovchainMacro[markovscope].begin(), markovchainMacro[markovscope].size());
		std::cout << "Picked Action: " << action << std::endl;
		TryToDo(action);
    }

	virtual void OnUnitIdle(const Unit* unit) final
	{
		std::cout << "\t\tlocation: OnUnitIdle" << std::endl;
		std::cout << "\t\t\tIdle unit: " << UnitTypeToName(unit->unit_type) << std::endl;
		
		switch (unit->unit_type.ToType())
		{
			//TODO
			//Improve Terran SCV abilities
			case UNIT_TYPEID::TERRAN_SCV:
			{
				const Unit* target_mineral = FindNearestMineralPatch(unit->pos);
				if (!target_mineral)
					break;

				Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_SCV, target_mineral);
				break;
			}
		}
	}

private:
			//Seconds				Command
	std::map<int, std::vector<std::string>> markovchainMacro; 
	/*
		00 -> [Train SCV, Build Siege Tank, ..., Attack],
		20 -> [Build Auto Turret, Train SCV, ..., Train Marine],
		...
		nnnn -> [xxxx, xxxx, xxxx, ..., xxxx]
	*/
	std::vector<std::string> testcommand; //For random commands
	const int SECONDSINTERVAL = 20;		//The agreed frame/seconds to observe the game environment
	double gamescope = 0, nextgamescope = 448, markovscope = 0, nextmarkovscope = 20;


	bool TryBuildStructure(ABILITY_ID ability)
	{
		const ObservationInterface* observation = Observation();

		const Unit* target_unit = nullptr;
		Units units = observation->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));
		for (const auto& unit : units)
		{
			//I'm questioning this one. Why do we need to loop through orders
			for (const auto& order : unit->orders)
			{
				if (order.ability_id == ability)
					continue;
			}

			target_unit = unit;
		}

		if (target_unit == nullptr)
			return false;

		float rx = GetRandomScalar(), ry = GetRandomScalar();
		Actions()->UnitCommand(target_unit, ability, Point2D(target_unit->pos.x + rx * 15.0f, target_unit->pos.y + ry * 15.0f));
		return true;
	}

	bool TryBuildBarracks() 
	{
		const ObservationInterface* observation = Observation();

		if (CountUnitType(UNIT_TYPEID::TERRAN_SUPPLYDEPOT) < 1) {
			return false;
		}

		if (CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) > 0 && CountUnitType(UNIT_TYPEID::TERRAN_BARRACKS) <= 1) {
			return false;
		}

		return TryBuildStructure(ABILITY_ID::BUILD_BARRACKS);
	}

	bool TryBuildSupplyDepot()
	{
		const ObservationInterface* observation = Observation();

		if (observation->GetFoodUsed() <= observation->GetFoodCap() - 2)
			return false;

		return TryBuildStructure(ABILITY_ID::BUILD_SUPPLYDEPOT);
	}

	const Unit* FindNearestMineralPatch(const Point2D& start)
	{
		const ObservationInterface* observation = Observation();

		Units units = observation->GetUnits(Unit::Alliance::Neutral, IsUnit(UNIT_TYPEID::NEUTRAL_MINERALFIELD));
		const Unit* target_unit = nullptr;
		float distance = std::numeric_limits<float>::max();

		for (const auto& unit : units)
		{
			float d = DistanceSquared2D(unit->pos, start);
			if (d < distance)
			{
				distance = d;
				target_unit = unit;
			}
		}

		return target_unit;
	}

	size_t CountUnitType(UNIT_TYPEID unit)
	{
		return Observation()->GetUnits(Unit::Alliance::Self, IsUnit(unit)).size();
	}

	void TryToDo(std::string action)
	{
		if (action == "Train SCV")
		{
			Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_COMMANDCENTER));
			if (units.empty())
				return;

			Actions()->UnitCommand(units.front(), ABILITY_ID::TRAIN_SCV);
		}
		else if (action.find("Build Barracks") != std::string::npos)
		{
			TryBuildBarracks();
		}
		else if (action.find("Build Supply Depot") != std::string::npos)
		{
			TryBuildSupplyDepot();
		}
		else if (action == "Train Marine")
		{
			Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_BARRACKS));
			if (units.empty())
				return;

			Actions()->UnitCommand(units.front(), ABILITY_ID::TRAIN_MARINE);
		}
		else if (action.find("Attack") != std::string::npos)
		{
			Units units = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_MARINE));
			if (units.empty())
				return;

			Actions()->UnitCommand(units, ABILITY_ID::ATTACK_ATTACK, Observation()->GetGameInfo().enemy_start_locations.front());
		}
	}
};

ProgramUtilities* ProgramUtilities::_instance = nullptr;
const std::string ProgramUtilities::MODELSERVICE_FILENAME = "ModelService.exe";
const std::string ProgramUtilities::COMMANDSREPOSITORY_FILENAME = "CommandsRepository.csv";
const std::string ProgramUtilities::RESOURCESREPOSITORY_FILENAME = "ResourcesRepository.csv";
RepositoryService* RepositoryService::_instance = nullptr;

int main(int argc, char* argv[]) {
    /*Coordinator coordinator;
    coordinator.LoadSettings(argc, argv);

    Bot bot;
    coordinator.SetParticipants({
        CreateParticipant(Race::Terran, &bot),
        CreateComputer(Race::Terran)
    });

    coordinator.LaunchStarcraft();
    coordinator.StartGame(sc2::kMapBelShirVestigeLE);

    while (coordinator.Update()) 
	{
		std::cout << std::endl << "\t\t\t~" << std::endl;
		std::cout << "\t\t\tlocation: main->coordinator.Update()" << std::endl;
		std::cout << std::endl << "\t\t\t~" << std::endl;
    }

    return 0;*/

	char s;

	Bot* bot = new Bot();
	Coordinator* coordinator = new Coordinator();
	ProgramUtilities* utilities = nullptr;
	RepositoryService* repository = nullptr;
	std::string currentdirectory = "";

	//Get the current directory
	currentdirectory = ProgramUtilities::GetProjectDirectory();
	//Initalize the utilities
	if (currentdirectory == "")
		utilities = ProgramUtilities::GetProgramUtilitiesInstance(); //Use hardcoded, if GetProjectDirectory() fails
	else
		utilities = ProgramUtilities::GetProgramUtilitiesInstance(const_cast<char *>(currentdirectory.c_str()));

	//Connect to the repository
	try
	{
		repository = RepositoryService::GetRepositoryServiceInstance(utilities->GetRelativeFilePath(ProgramUtilities::COMMANDSREPOSITORY_FILENAME), utilities->GetRelativeFilePath(ProgramUtilities::RESOURCESREPOSITORY_FILENAME));
		repository->ParseCommandRepository(); //fuck threading

		std::cout << "Number of commands: " << repository->GetList().size() << std::endl;
		std::cout << "Commands are: ";
		for (auto list : repository->GetList())
		{
			std::cout << "\n\t" << list.first << std::endl;
		}
		std::cout << "\n\nMatrix: \n\t\t\t\t\t\t";
		int count = 0;
		for (auto list : repository->GetChain())
		{
			std::cout << ++count << ". " << list.first << "\t|\t";
		}
		std::cout << std::endl;
		for (auto yaxis : repository->GetChain())
		{
			std::cout << yaxis.first << " \t";
			for (auto list : repository->GetList())
			{
				std::cout << std::count(yaxis.second.begin(), yaxis.second.end(), list.first) << " \t";
			}
			std::cout << std::endl;

		}
		std::cout << "\n\nMatrix (Percentage): \n\t\t\t\t\t\t";
		count = 0;
		for (auto list : repository->GetChain())
		{
			std::cout << ++count << ". " << list.first << "\t|\t";
		}
		std::cout << std::endl;
		for (auto yaxis : repository->GetChain())
		{
			std::cout << yaxis.first << " \t";
			for (auto list : repository->GetList())
			{
				std::cout << (((double)std::count(yaxis.second.begin(), yaxis.second.end(), list.first))/repository->GetList().size())*100 << " \t";
			}
			std::cout << std::endl;

		}
		fclose(repository->s);
		freopen_s(&repository->s, "ForR.csv", "w", stdout);
		for (auto list : repository->GetList())
			std::cout << "\"" << list.first << "\"" << ",";
		std::cout << "\b\n";
		double row_sum = 0, total = 0;
		for (auto yaxis : repository->GetChain())
		{
			for (auto list : repository->GetList())
			{
				//row_sum += (((double)std::count(yaxis.second.begin(), yaxis.second.end(), list.first)) / ((double)repository->command_size));

				row_sum += std::count(yaxis.second.begin(), yaxis.second.end(), list.first);
				//std::cout << (((double)std::count(yaxis.second.begin(), yaxis.second.end(), list.first)) / ((double)repository->command_size)) * 100 << ",";
				//std::cout << std::count(yaxis.second.begin(), yaxis.second.end(), list.first) << ", ";
			}
			//std::cout << std::endl << row_sum << std::endl;
			for (auto list : repository->GetList())
			{
				//std::cout << (((double)std::count(yaxis.second.begin(), yaxis.second.end(), list.first)) / ((double)row_sum)) * 100 << ",";
				total += (std::count(yaxis.second.begin(), yaxis.second.end(), list.first) / row_sum);
				std::cout << std::count(yaxis.second.begin(), yaxis.second.end(), list.first) << ",";
			}

			//std::cout << "\t\t" << row_sum << "\t\t" << total << std::endl;

			row_sum = 0;
			total = 0;
		}
	}
	catch (...)
	{
		std::cout << "Error Occurred! Failed to connect to the repository... Exiting..." << std::endl;
		std::cerr << "Error Occurred! ->main" << std::endl;
		exit(-1);
	}

	//Start the game
	//coordinator->LoadSettings(argc, argv);
	//coordinator->SetParticipants({ CreateParticipant(Race::Terran, bot), CreateComputer(Race::Terran, Difficulty::VeryEasy) });

	std::cin >> s;

	return 0;
}