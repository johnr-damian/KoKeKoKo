#include <sc2api/sc2_api.h>

#include <iostream>
#include <fstream>
#include <map>
#include <sstream>
#include <string>

using namespace sc2;

class Bot : public Agent {
public:
    virtual void OnGameStart() final 
	{
		std::string replayfilepath = "", replayfileline = "";

		std::cout << "Gathering Data for the Model...";
		while (replayfilepath.empty())
		{
			std::cout << "\n\tEnter File Path: ";
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
					
					std::stringstream byline(replayfileline);
					std::string bycomma = "";
					while (std::getline(byline, bycomma, ','))
					{
						std::cout << "\t\t\t\tCurrent Column: " << bycomma << std::endl;
						
						
					}
				}

				std::cout << "\n\n\t\tClosing the file '" << replayfilepath << "'..." << std::endl;
				replayfile.close();
			}
		}
		catch (...)
		{

		}
    }

    virtual void OnStep() final 
	{

    }

	virtual void OnUnitIdle(const Unit* unit) final
	{
		std::cout << "\t\tlocation: OnUnitIdle" << std::endl;
		std::cout << "\t\t\tIdle unit: " << UnitTypeToName(unit->unit_type) << std::endl;
		
		switch (unit->unit_type.ToType())
		{

		}
	}

private:
	std::map<std::string, std::vector<std::string>> markovchain;

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