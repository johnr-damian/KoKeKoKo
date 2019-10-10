import glob, os
import pandas as pd
import sc2reader
import math

path_directory = r'C:\Users\Thienzaw\Documents\Thesis\Raw Data V3\Grandmaster\Grandmaster Testing 4\*.SC2Replay'
files = glob.glob(path_directory)

counter = 317

# with open('ResourceRepository-Testing.csv', 'a', encoding='utf-8') as f:
#     f.write("Grandmaster")
#     f.write("\n")

for name in files:
    replay = sc2reader.load_replay(name)
    event_names = set([event.name for event in replay.events])
    events_of_type = {name: [] for name in event_names}

    for event in replay.events:
        events_of_type[event.name].append(event)

    unit_born_event = events_of_type["UnitBornEvent"]
    unit_init_event = events_of_type["UnitInitEvent"]
    upgrade_complete_event = events_of_type["UpgradeCompleteEvent"]
    player_stats_event = events_of_type["PlayerStatsEvent"]
    unit_position_event = events_of_type["UnitPositionsEvent"]
    unit_died_event = events_of_type["UnitDiedEvent"]

    filename = os.path.basename(name)
    with open('ResourceRepository-Testing.csv', 'a', encoding='utf-8') as f:
        f.write(filename)
        f.write("\n")

    def resources_function(player, counter):
        resource_list = []
        upgrade_list = []
        upgrades_list = []
        dup_upgrade_list = []
        final_upgrade_list = []
        for pse in player_stats_event:
            if pse.pid == player and pse.second > 0:
                resource = [pse.second, counter, pse.minerals_current, pse.vespene_current, pse.workers_active_count]
                resource_list.append(resource)

        for uce in upgrade_complete_event:
            if uce.pid == player and uce.second > 0:
                second = int(math.ceil(uce.second / 10.0)) * 10
                upgrade = str(uce.upgrade_type_name + " ")
                upgrade_list.append(upgrade)
                s = ''.join(upgrade_list)
                combine = [second, s]
                upgrades_list.append(combine)

        prev = None
        for row in upgrades_list:
            if prev is not None and prev == row[0]:
                dup_upgrade_list.append(row)
            prev = row[0]

        for i in upgrades_list[:]:
            if i in dup_upgrade_list:
                upgrades_list.remove(i)

        for item in resource_list:
            for elements in upgrades_list:
                if item[0] == elements[0]:
                    item.insert(5, elements[1])
            final_upgrade_list.append(item)

        combat_frame = pd.DataFrame(final_upgrade_list)
        combat_frame.to_csv('ResourceRepository-Testing.csv', mode='a', header=None, index=None)


    resources_function(1, counter)
    counter = counter + 1
    resources_function(2, counter)
    counter = counter + 1


