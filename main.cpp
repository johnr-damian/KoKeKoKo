#include <sc2api/sc2_api.h>

#include <iostream>

using namespace sc2;

class Bot : public Agent {
public:
    virtual void OnGameStart() final 
	{
		std::printf("Kokekoko! Please check my initial observation of the game!");
			std::printf("\tPlayer ID: %d\n", Observation()->GetPlayerID());
			std::printf("\tGame Loop: %d\n", Observation()->GetGameLoop());
			//std::printf("\tOur Units: %d\n", &(Observation()->GetUnits(Unit::Self).size));
			//std::printf("\tNeutral Units: %d\n", &(Observation()->GetUnits(Unit::Neutral).size));
			//std::printf("\tEnemy Visible Units: %d\n", &(Observation()->GetUnits(Unit::Enemy).size));
			
			std::cout << "\tPower Sources: " << std::endl;
			for (const PowerSource & powersource : Observation()->GetPowerSources())
			{
				std::cout << "\t" << powersource.tag << "\tPosition(" << powersource.position.x << ", " << powersource.position.y << ")\n";
			}
			//std::printf("\tCamera Position: (%d, %d)\n", Observation()->GetCameraPos().x, Observation()->GetCameraPos().y);

		std::cout << "My attributes are: " << std::endl;
			std::cout << "Minerals: " << Observation()->GetMinerals() << std::endl;
			std::cout << "Vespene: " << Observation()->GetVespene() << std::endl;
			std::cout << "Food Cap: " << Observation()->GetFoodCap() << std::endl;
			std::cout << "Food Used: " << Observation()->GetFoodUsed() << std::endl;
    }

    virtual void OnStep() final 
	{
		/*std::cout << "Is Game Loop already 30 seconds? " << (Observation()->GetGameLoop() % 30 == 0) << std::endl;
		if (Observation()->GetGameLoop() % 30 == 0)
		{
			CreateMonteCarloTree();
		}
		else
		{
			std::cout << "Monte Carlo Tree is ignored" << std::endl;
		}*/

		TryBuildSupplyDepot();
		TryBuildBarracks();
    }

	virtual void OnUnitIdle(const Unit* unit) final
	{
		std::cout << "\t\tlocation: OnUnitIdle" << std::endl;
		std::cout << "\t\t\tIdle unit: " << UnitTypeToName(unit->unit_type) << std::endl;
		
		switch (unit->unit_type.ToType())
		{
			case UNIT_TYPEID::TERRAN_COMMANDCENTER:
			{
				std::cout << "\t\t\tAction: " << AbilityTypeToName(ABILITY_ID::TRAIN_SCV) << std::endl;

				Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_SCV);
				break;
			}
			case UNIT_TYPEID::TERRAN_SCV:
			{
				std::cout << "\t\t\tAction: " << AbilityTypeToName(ABILITY_ID::SMART) << std::endl;

				const Unit* target_mineral = FindNearestMineralPatch(unit->pos);
				if (!target_mineral)
					break;
				Actions()->UnitCommand(unit, ABILITY_ID::SMART, target_mineral);
				break;
			}
			case UNIT_TYPEID::TERRAN_BARRACKS: 
			{
				Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARINE);
				break;
			}
			case UNIT_TYPEID::TERRAN_MARINE: 
			{
				const GameInfo& game_info = Observation()->GetGameInfo();
				Actions()->UnitCommand(unit, ABILITY_ID::ATTACK_ATTACK, game_info.enemy_start_locations.front());
				break;
			}
			default:
			{
				std::cout << "\t\t\t-default-" << std::endl;

				break;
			}
		}
	}

private:
	void CreateMonteCarloTree()
	{
		std::cout << "Creating Monte Carlo Tree" << std::endl;

		int randomness = GetRandomInteger(1, 5);
		switch (randomness)
		{
		case 1:
		{
			std::cout << "We build barrack with idle worker" << std::endl;
			std::cout << "Idle Workers: " << Observation()->GetIdleWorkerCount() << std::endl;
			if (Observation()->GetIdleWorkerCount() > 0)
			{
				std::cout << "We have idle worker!" << std::endl;
				Units workers = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_SCV));


				if (workers.empty())
				{
					std::cout << "What! No workers!" << std::endl;
				}
				else
				{
					for (const auto& worker : workers)
					{
						for (const auto& order : worker->orders)
						{
							if (order.ability_id == ABILITY_ID::BUILD_BARRACKS)
								return;
						}
					}

					const Unit* unit = GetRandomEntry(workers);
					auto pos = Point2D(Observation()->GetStartLocation().x + GetRandomInteger(10, 100), Observation()->GetStartLocation().y + GetRandomInteger(10, 100));
					while (!Query()->Placement(ABILITY_ID::BUILD_BARRACKS, pos))
					{
						std::cout << "Fuck no pos, how to get pos" << std::endl;
					}

					Actions()->UnitCommand(unit, ABILITY_ID::BUILD_BARRACKS, pos);
				}
			}
		}
		case 4:
		{
			std::cout << "Train SCV" << std::endl;

			Units cmdcntr = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_COMMANDCENTER));

			if (cmdcntr.empty())
				return;

			Actions()->UnitCommand(cmdcntr.front(), ABILITY_ID::TRAIN_SCV);

			break;
		}
		case 2:
		{
			std::cout << "We train army" << std::endl;
			Units barracks = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_BARRACKS));

			if (barracks.empty())
				return;

			const Unit* unit = GetRandomEntry(barracks);
			Actions()->UnitCommand(unit, ABILITY_ID::TRAIN_MARINE);

			break;
		}
		case 3:
		{
			std::cout << "We attack" << std::endl;

			Units army = Observation()->GetUnits(Unit::Alliance::Self, IsUnit(UNIT_TYPEID::TERRAN_MARINE));

			if (army.empty())
				return;

			Actions()->UnitCommand(army, ABILITY_ID::ATTACK_ATTACK, Observation()->GetGameInfo().enemy_start_locations.front());

			break;
		}
		}
	}

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