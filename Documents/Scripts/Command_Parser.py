import glob, os
import pandas as pd
from operator import itemgetter
import sc2reader


path_directory = r'C:\Users\Thienzaw\Documents\Thesis\Raw Data V3\Grandmaster\Grandmaster Testing 4\*.SC2Replay'
files = glob.glob(path_directory)

player_index = 317

# with open('CommandRepository-Testing.csv', 'a', encoding='utf-8') as f:
#     f.write("Grandmaster")
#     f.write("\n")


for name in files:

    replay = sc2reader.load_replay(name)
    event_names = set([event.name for event in replay.events])
    events_of_type = {name: [] for name in event_names}

    for event in replay.events:
        events_of_type[event.name].append(event)

    unit_born_events = events_of_type["UnitBornEvent"]
    unit_init_events = events_of_type["UnitInitEvent"]
    upgrade_complete_event = events_of_type["UpgradeCompleteEvent"]
    player_stats_events = events_of_type["PlayerStatsEvent"]
    unit_position_event = events_of_type["UnitPositionsEvent"]
    unit_died_event = events_of_type["UnitDiedEvent"]

    filename = os.path.basename(name)
    with open('CommandRepository-Testing.csv', 'a', encoding='utf-8') as f:
        f.write(filename)
        f.write("\n")


    def command_function(player, counter):
        command_list = []
        for ube in unit_born_events:
            if ube.unit_type_name == 'SCV' and ube.second > 0 and ube.control_pid == player:
                result = [ube.second, counter, 'Train ' + ube.unit_type_name, 'Economy']
                command_list.append(result)

            elif ube.unit_type_name not in ['SCV'] and ube.second > 0 and ube.control_pid == player:
                result = [ube.second, counter, 'Train ' + ube.unit_type_name, 'Army']
                command_list.append(result)

        for uie in unit_init_events:
            if uie.unit_type_name in ('SupplyDepot', 'Refinery', 'CommandCenter', 'OrbitalCommand') \
                    and uie.control_pid == player:

                result = [uie.second, counter, 'Build ' + uie.unit_type_name, 'Economy']
                command_list.append(result)

            elif uie.unit_type_name in (
                'Barracks', 'Factory', 'Starport', 'Bunker', 'MissileTurret', 'PlanetaryFortress') \
                    and uie.control_pid == player:

                result = [uie.second, counter, 'Build ' + uie.unit_type_name, 'Army']
                command_list.append(result)
            elif uie.unit_type_name in ('EngineeringBay', 'GhostAcademy', 'FusionCore', 'Armory', 'StarportReactor',
                                        'BarracksReactor', 'FactoryTechLab', 'FactoryReactor', 'TechLab', 'SensorTower',
                                        'BarracksTechLab', 'StarportTechLab') and uie.control_pid == player:

                result = [uie.second, counter, 'Build ' + uie.unit_type_name, 'Tech']
                command_list.append(result)

        for uce in upgrade_complete_event:
            if uce.second > 0 and uce.pid == player:
                result = [uce.second, counter, 'Upgrade ' + uce.upgrade_type_name, 'Tech']
                command_list.append(result)

        sorted_main_list = sorted(command_list, key=itemgetter(0))
        df = pd.DataFrame(sorted_main_list)
        df.to_csv('CommandRepository-Testing.csv', mode='a', header=None, index=None)


    command_function(1, player_index)
    player_index = player_index + 1
    command_function(2, player_index)
    player_index = player_index + 1

