import glob, os
import pandas as pd
import sc2reader


path_directory = r'C:\Users\Thienzaw\Documents\Thesis\Raw Data V3\Silver\Silver\16-Bit LE (15) Cyclone rush attempt.SC2Replay'
files = glob.glob(path_directory)

player_index = 3
player_counter = 3

# with open('ArmyRepository-Train.csv', 'a', encoding='utf-8') as f:
#     f.write("Bronze")
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
    with open('ArmyRepository-Train.csv', 'a', encoding='utf-8') as f:
        f.write(filename)
        f.write("\n")


    def initial_combat(player, counter):
        initial_unit_list = []
        for ube in unit_born_events:
            if ube.control_pid == player and ube.second > 0:
                s = str(ube.unit)
                unit, unit_id = s.split(' ', 1)
                initial_unit = [ube.second, counter, unit, unit_id, ube.x, ube.y]
                initial_unit_list.append(initial_unit)
        create_frame = pd.DataFrame(initial_unit_list)
        create_frame.to_csv('ArmyRepository-Train.csv', mode='a', header=None, index=None)



    def combat_func(player, killer_player, counter):

        born_unit_list = []
        died_unit_list = []
        survive_unit_list = []
        damage_unit_list = []
        combat_unit_list = []

        for ube in unit_born_events:
            if ube.second > 0 and ube.control_pid == player:
                s = str(ube.unit)
                unit, unit_id = s.split(' ', 1)
                born_unit = unit_id
                born_unit_list.append(born_unit)

        for ude in unit_died_event:
            if ude.second > 0 and ude.killing_player_id == killer_player:
                s = str(ude.unit)
                unit, unit_id = s.split(' ', 1)
                died_unit = unit_id
                died_unit_list.append(died_unit)

        for item in born_unit_list:
            if item not in died_unit_list:
                survive_unit_list.append(item)

        for upe in unit_position_event:
            units = upe.units
            second = upe.second
            position = ([*units.values()])
            dict_list = ([*units])
            position_list = [[x, y] for x, y in position]
            for index, item in enumerate(position_list):
                if index == 0:
                    s = str(dict_list[index])
                    unit, unit_id = s.split(' ', 1)
                    item.insert(0, second)
                    item.insert(1, counter)
                    item.insert(2, unit)
                    item.insert(3, unit_id)
                    if index == len(position_list) - 1:
                        damage_unit_list.extend(position_list)

                else:
                    s = str(dict_list[index])
                    unit, unit_id = s.split(' ', 1)
                    item.insert(0, second)
                    item.insert(1, counter)
                    item.insert(2, unit)
                    item.insert(3, unit_id)
                    if index == len(position_list) - 1:
                        damage_unit_list.extend(position_list)

        for item in damage_unit_list:
            if item[3] in survive_unit_list:
                combat_unit_list.append(item)

        combat_frame = pd.DataFrame(combat_unit_list)
        combat_frame.to_csv('ArmyRepository-Train.csv', mode='a', header=None, index=None)

    initial_combat(1, player_counter)
    player_counter = player_counter + 1
    initial_combat(2, player_counter)
    player_counter = player_counter + 1

    with open('ArmyRepository-Train.csv', 'a', encoding='utf-8') as f:
        f.write("End")
        f.write("\n")

    combat_func(1, 2, player_index)
    player_index = player_index + 1
    combat_func(2, 1, player_index)
    player_index = player_index + 1






