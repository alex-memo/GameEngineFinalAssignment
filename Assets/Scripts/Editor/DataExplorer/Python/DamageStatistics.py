import os
import csv
import sys
import platform
import subprocess
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import numpy as np
import UnityEngine

def open_image(path):
    if platform.system() == 'Windows':
        os.startfile(path)
    elif platform.system() == 'Darwin':  # macOS
        subprocess.call(['open', path])
    else:
        subprocess.call(['xdg-open', path])

csv_file_path = os.path.abspath(os.getcwd())
csv_file_path += "/DataExplorer/Temp/MatchDamageTempData.txt"
UnityEngine.Debug.Log(f"CSV file path: {csv_file_path}")

#data structures to hold total damage and counts per ID
tool_damages = {}
tool_counts = {}
tool_sample_size = 0

other_damage_damages = {}
other_damage_counts = {}
other_damage_sample_size = 0

ability_damages = {}
ability_counts = {}
ability_sample_size = 0

#method to get label for IDs
def get_label(id):
    if id == -999:
        return "Zone Damage"
    elif id == -998:
        return "Ionization Damage"
    elif id < 0:
        return "Unknown"
    else:
        return str(id)

#read the CSV file
with open(csv_file_path, 'r', newline='', encoding='utf-8') as csvfile:
    reader = csv.DictReader(csvfile)
    for row_num, row in enumerate(reader, start=1):
        dmg_field = row.get('Dmg', '').strip()
        if not dmg_field or dmg_field in ["'-", "-", "'-'"]:
            continue  #skip if 'Dmg' field is empty or contains placeholder values
        dmg_entries = dmg_field.strip('"').split('|')
        for dmg_entry in dmg_entries:
            dmg_entry = dmg_entry.strip()
            if not dmg_entry:
                continue  #skip empty entries
            dmg_parts = dmg_entry.split(',')
            if len(dmg_parts) != 3:
                UnityEngine.Debug.Log(f"Invalid damage entry at row {row_num}: '{dmg_entry}'")
                continue  #skip invalid entries
            try:
                is_tool_str, id_str, damage_str = dmg_parts
                is_tool = int(is_tool_str)
                id = int(id_str)
                damage = float(damage_str)

                #fix the database error: If is_tool == 0 and id == 3, change is_tool to 1
                #this is a one-time fix for the database error
                if is_tool == 0 and id == 3:
                    is_tool = 1
                    UnityEngine.Debug.Log(f"Corrected is_tool value at row {row_num}: ID {id}")

                if is_tool == 1:
                    #tool damage
                    if id > -500:
                        #include in main tool damage stats
                        tool_damages[id] = tool_damages.get(id, 0) + damage
                        tool_counts[id] = tool_counts.get(id, 0) + 1
                        tool_sample_size += 1  #increment sample size for tools
                    else:
                        #tool ID less than or equal to -500, include in other damage sources
                        other_damage_damages[id] = other_damage_damages.get(id, 0) + damage
                        other_damage_counts[id] = other_damage_counts.get(id, 0) + 1
                        other_damage_sample_size += 1  #increment sample size for other damage sources
                elif is_tool == 0:
                    #ability damage
                    ability_damages[id] = ability_damages.get(id, 0) + damage
                    ability_counts[id] = ability_counts.get(id, 0) + 1
                    ability_sample_size += 1  #increment sample size for abilities
                else:
                    UnityEngine.Debug.Log(f"Invalid 'is_tool' value at row {row_num}: '{is_tool_str}'")
                    continue  #skip if 'is_tool' is not 0 or 1
            except ValueError as e:
                UnityEngine.Debug.Log(f"Value error at row {row_num}: {e}")
                continue  #skip entries with parsing errors

#calculate average damages
tool_avg_damages = {id: tool_damages[id] / tool_counts[id] for id in tool_damages}
other_damage_avg_damages = {id: other_damage_damages[id] / other_damage_counts[id] for id in other_damage_damages}
ability_avg_damages = {id: ability_damages[id] / ability_counts[id] for id in ability_damages}

#map special negative IDs to labels
def map_negative_ids(ids_list):
    labels = []
    for id in ids_list:
        label = get_label(id)
        labels.append(label)
    return labels

#generate bar chart for tools (IDs > -500)
if tool_avg_damages:
    ids = list(tool_avg_damages.keys())
    averages = [tool_avg_damages[id] for id in ids]

    #map IDs to labels for plotting
    labels = [get_label(id) for id in ids]

    plt.figure(figsize=(10, 6))
    plt.bar(labels, averages, color='skyblue')
    plt.xlabel('Tool ID')
    plt.ylabel('Average Damage')

    #update the title to include the sample size
    plt.title(f'Average Damage per Tool ID (Sample Size: {tool_sample_size})')

    plt.xticks(rotation=45, ha='right')  # Rotate labels if necessary
    plt.grid(axis='y', linestyle='--', alpha=0.7)

    #save the plot
    output_image_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'tool_damage_statistics.png')
    plt.tight_layout()
    plt.savefig(output_image_path, dpi=300)
    UnityEngine.Debug.Log(f"Tool damage statistics saved to {output_image_path}")
    open_image(output_image_path)
    plt.close()
else:
    UnityEngine.Debug.Log("No tool damage data available to plot.")

#generate bar chart for other damage sources (IDs <= -500)
if other_damage_avg_damages:
    ids = list(other_damage_avg_damages.keys())
    averages = [other_damage_avg_damages[id] for id in ids]

    #map ids to labels for plotting
    labels = [get_label(id) for id in ids]

    plt.figure(figsize=(10, 6))
    plt.bar(labels, averages, color='orange')
    plt.xlabel('Source')
    plt.ylabel('Average Damage')

    #update the title to include the sample size
    plt.title(f'Other Damage Sources (Sample Size: {other_damage_sample_size})')

    plt.xticks(rotation=45, ha='right')  # Rotate labels if necessary
    plt.grid(axis='y', linestyle='--', alpha=0.7)

    #save the plot
    output_image_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'other_damage_sources.png')
    plt.tight_layout()
    plt.savefig(output_image_path, dpi=300)
    UnityEngine.Debug.Log(f"Other damage sources saved to {output_image_path}")
    open_image(output_image_path)
    plt.close()
else:
    UnityEngine.Debug.Log("No other damage data available to plot.")

#generate bar chart for abilities
if ability_avg_damages:
    ids = list(ability_avg_damages.keys())
    averages = [ability_avg_damages[id] for id in ids]

    #map ids to labels for plotting
    labels = [get_label(id) for id in ids]

    plt.figure(figsize=(10, 6))
    plt.bar(labels, averages, color='salmon')
    plt.xlabel('Ability ID')
    plt.ylabel('Average Damage')

    #update the title to include the sample size
    plt.title(f'Average Damage per Ability ID (Sample Size: {ability_sample_size})')

    plt.xticks(rotation=45, ha='right')  # Rotate labels if necessary
    plt.grid(axis='y', linestyle='--', alpha=0.7)

    #save the plot
    output_image_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'ability_damage_statistics.png')
    plt.tight_layout()
    plt.savefig(output_image_path, dpi=300)
    UnityEngine.Debug.Log(f"Ability damage statistics saved to {output_image_path}")
    open_image(output_image_path)
    plt.close()
else:
    UnityEngine.Debug.Log("No ability damage data available to plot.")