#include <sc2api/sc2_api.h>

#include <iomanip>	//for time
#include <iostream>	//for cin and cout
#include <fstream>	//for file stream
#include <map>		//for dictionary
#include <sstream>	//for string stream

using namespace sc2;


template <typename randomizer>
randomizer RandomAction(randomizer begin, randomizer end)
{
	const unsigned long n = std::distance(begin, end);
	const unsigned long divisor = (RAND_MAX + 1) / n;

	unsigned long k;
	do
	{
		k = std::rand() / divisor;
	} while (k >= n);

	std::advance(begin, k);
	return begin;
}

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

		try
		{
			std::ifstream replayfile(replayfilepath);
			if (replayfile.is_open())
			{
				std::cout << "\n\n\t\tSuccessfully opened the file '" << replayfilepath << "'!" << std::endl;
				while (std::getline(replayfile, replayfileline))
				{
					std::cout << "\t\t\tCurrent Line: " << replayfileline << std::endl;
					
					bool getnextcolumn = false;
					std::stringstream byline(replayfileline);
					std::string bycomma = "";
					std::time_t currentseconds = 0;
					for (int column = 1; std::getline(byline, bycomma, ','); column++)
					{
						std::cout << "\t\t\t\tCurrent Column: " << bycomma << std::endl;

						//Current column is a timestamp
						if (column == 1)
						{
							//Convert the string timestamp to seconds
							struct tm timestamp = { 0 };
							const char* timecolumn = bycomma.c_str();
							int hours = 0, minutes = 0, seconds = 0;
							sscanf_s(timecolumn, "%d:%d:$d", &hours, &minutes, &seconds);
							timestamp.tm_hour = hours;
							timestamp.tm_min = minutes;
							timestamp.tm_sec = seconds;
							std::time_t timeseconds = mktime(&timestamp);
							std::cout << "\t\t\t\t\tConverted Timestamp: " << timeseconds << std::endl;

							//Check if the current seconds is new
							if (markovchainMacro.find(seconds) == markovchainMacro.end())
							{
								//Check if the current seconds is a multiple of 20 seconds
								if ((seconds % SECONDSINTERVAL) == 0)
								{
									std::cout << "\t\t\t\t\tCurrent seconds is a multiple of 20 seconds...True!" << std::endl;
									markovchainMacro.insert(std::make_pair(seconds, std::vector<std::string>()));
									currentseconds = seconds;
									std::cout << "\t\t\t\t\tSuccessfully added the current seconds to markov chain!" << std::endl;
								}
							}
						}
						
						//If current column is a command, get the next column
						getnextcolumn = ((column == 3) && (bycomma == "Cmd")) ? true : false;

						//If current column is the command value
						if ((column == 4) && getnextcolumn)
						{
							std::cout << "\t\t\t\t\tInserting the command value..." << std::endl;
							markovchainMacro[currentseconds].insert(markovchainMacro[currentseconds].end(), bycomma);
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
		while (markovchainfilepath.empty())
		{
			std::cout << "\n\tEnter Output File Path: ";
			std::cin >> markovchainfilepath;
		}

		try
		{
			std::ofstream markovchainfile(markovchainfilepath);
			if (markovchainfile.is_open())
			{
				std::cout << "\n\n\t\tGenerating the Markov Chain..." << std::endl;
				for (auto const& element : markovchainMacro)
				{
					markovchainfile << element.first << " -> [";

					markovchainfile << element.second[0] << "(" << (std::count(element.second.begin(), element.second.end(), element.second[0]) / element.second.size())*100 << "%)";
					for (auto value = std::next(element.second.begin()); value != element.second.end(); value++)
						markovchainfile << ", " << *value << "(" << (std::count(element.second.begin(), element.second.end(), *value) / element.second.size())*100 << "%)";

					markovchainfile << "," << std::endl;
				}
				std::cout << "\n\n\t\tFinished generating the Markov Chain!" << std::endl;
			}
		}
		catch (...)
		{
			std::cout << "An error occured in creating the markov chain!" << std::endl;
			std::cin >> markovchainfilepath;
			exit(-1);
		}

		//Initialize the current chain
		currentsecondschain = markovchainMacro.begin()->first;
		std::cout << "\n\n\n\nSuccessfully finished initializing the current chain!" << std::endl;
		std::cout << "\tCurrent Chain: " << currentsecondschain << std::endl;
    }

    virtual void OnStep() final 
	{
		time_t gameloop = Observation()->GetGameLoop();
		if (gameloop % 20 == 0)
		{
			std::cout << "Game Loop: " << gameloop << std::endl;
			for (auto const& element : markovchainMacro)
			{
				if (element.first == gameloop)
					break;
			}
		}

		//Pick a random element
		std::string action = *RandomAction(markovchainMacro[gameloop].begin(), markovchainMacro[gameloop].end());
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
	std::map<std::time_t, std::vector<std::string>> markovchainMacro; 
	/*
		0:00 -> [Train SCV, Build Siege Tank, ..., Attack],
		0:20 -> [Build Auto Turret, Train SCV, ..., Train Marine],
		...
		n:nn -> [xxxx, xxxx, xxxx, ..., xxxx]
	*/
	const int SECONDSINTERVAL = 20;		//The agreed frame/seconds to observe the game environment
	time_t currentsecondschain = 0;		//The specific chain we are looking at during the game


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

int main(int argc, char* argv[]) {
    Coordinator coordinator;
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
		std::cout << "\t\t\t\tlocation: main->ln 150" << std::endl;
		std::cout << std::endl << "\t\t\t~" << std::endl;
    }

    return 0;
}