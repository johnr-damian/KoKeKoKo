import glob
import os
import pandas as pd
import sc2reader


path_directory = r'C:\Users\Thienzaw\Documents\Thesis\Raw Data V3\Replay Files\Grandmaster\Grandmaster Training 4\*.SC2Replay'
files = glob.glob(path_directory)

player_index = 791
player_counter = 791

# with open('ArmiesRepository.csv', 'a', encoding='utf-8') as f:
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

    with open('ArmiesRepository.csv', 'a', encoding='utf-8') as f:
        f.write(filename)
        f.write("\n")


    def initial_combat(player, counter):
        initial_unit_list = []
        died_unit_list = []
        final_unit_list = []
        for ube in unit_born_events:
            if ube.control_pid == player and ube.second > 0:
                s = str(ube.unit)
                unit, unit_id = s.split(' ', 1)
                initial_unit = [-1, counter, unit_id, unit, ube.x, ube.y]
                initial_unit_list.append(initial_unit)

        for ude in unit_died_event:
            s = str(ude.unit)
            unit, unit_id = s.split(' ', 1)
            died_unit = [ude.second, counter, unit_id, unit, ude.x, ude.y]
            died_unit_list.append(died_unit)

        for item in initial_unit_list:
            for value in died_unit_list:
                if item[2] == value[2]:
                    final_unit_list.append(value)
                    break
            else:
                final_unit_list.append(item)

        create_frame = pd.DataFrame(final_unit_list)
        create_frame.to_csv('ArmiesRepository.csv', mode='a', header=None, index=None)


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
            second = -1
            position = ([*units.values()])
            dict_list = ([*units])
            position_list = [[x, y] for x, y in position]
            for index, item in enumerate(position_list):
                if index == 0:
                    s = str(dict_list[index])
                    unit, unit_id = s.split(' ', 1)
                    item.insert(0, second)
                    item.insert(1, counter)
                    item.insert(2, unit_id)
                    item.insert(3, unit)
                    if index == len(position_list) - 1:
                        damage_unit_list.extend(position_list)

                else:
                    s = str(dict_list[index])
                    unit, unit_id = s.split(' ', 1)
                    item.insert(0, second)
                    item.insert(1, counter)
                    item.insert(2, unit_id)
                    item.insert(3, unit)
                    if index == len(position_list) - 1:
                        damage_unit_list.extend(position_list)

        for item in damage_unit_list:
            if item[2] in survive_unit_list:
                combat_unit_list.append(item)
        return combat_unit_list


    initial_combat(1, player_counter)
    player_counter = player_counter + 1
    initial_combat(2, player_counter)
    player_counter = player_counter + 1

    with open('ArmiesRepository.csv', 'a', encoding='utf-8') as f:
        f.write("End")
        f.write("\n")

    player1_count = len(combat_func(1, 2, player_index))
    player1_list = combat_func(1, 2, player_index)
    player_index = player_index + 1
    player2_count = len(combat_func(2, 1, player_index))
    player2_list = combat_func(2, 1, player_index)
    player_index = player_index + 1

    if player1_count > player2_count:
        combat_frame = pd.DataFrame(player1_list)
        combat_frame.to_csv('ArmiesRepository.csv', mode='a', header=None, index=None)
    else:
        combat_frame = pd.DataFrame(player2_list)
        combat_frame.to_csv('ArmiesRepository.csv', mode='a', header=None, index=None)










