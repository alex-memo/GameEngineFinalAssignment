import UnityEngine
import os
import csv
import sys
import platform
import subprocess
from collections import defaultdict
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from PIL import Image

def open_image(path):
    if platform.system() == 'Windows':
        os.startfile(path)
    elif platform.system() == 'Darwin':  #why is macos called darwin?
        subprocess.call(['open', path])
    else:  #linux
        subprocess.call(['xdg-open', path])

#path
csv_file_path = os.path.abspath(os.getcwd())
csv_file_path += "/DataExplorer/Temp/AbilityUsageTempData.txt"
UnityEngine.Debug.Log(csv_file_path)

#dictionary to hold the total counts for each ability
ability_counts = defaultdict(int)

#variable to hold the total sample size
total_sample_size = 0

# Read the CSV file
with open(csv_file_path, 'r', newline='', encoding='utf-8') as csvfile:
    reader = csv.DictReader(csvfile)
    for row_num, row in enumerate(reader, start=1):
        if row_num == 1:
            continue  #skip the first data row if it's a summary row

        #get the count of times this loadout was used
        loadout_sum_str = row['PlayerLoadouts (Sum)'].replace(',', '')
        try:
            loadout_sum = int(loadout_sum_str)
        except ValueError:
            UnityEngine.Debug.Log(f"Row {row_num}: Invalid integer value for loadout sum: '{row['PlayerLoadouts (Sum)']}'")
            continue  #skip this row if the value is invalid

        #get the abilities and parse them into a list
        abilities_str = row['Abilities']

        #clean up the abilities string
        abilities_str = abilities_str.strip('"').strip("'")
        abilities_list = abilities_str.split(',')

        #count of valid abilities in this row
        num_abilities_in_row = 0

        for ability in abilities_list:
            ability = ability.strip().strip("'")
            if ability != '-1':
                try:
                    ability_id = int(ability)
                    ability_counts[ability_id] += loadout_sum
                    num_abilities_in_row += 1  #increment the count of valid abilities
                except ValueError:
                    UnityEngine.Debug.Log(f"Row {row_num}: Invalid ability ID '{ability}'")
                    continue  #skip invalid ability IDs

        #update the total sample size
        total_sample_size += num_abilities_in_row * loadout_sum

#prepare data for plotting
abilities = list(ability_counts.keys())
counts = [ability_counts[ability] for ability in abilities]

#sort abilities numerically
abilities_sorted, counts_sorted = zip(*sorted(zip(abilities, counts)))

#convert ability IDs back to strings for labeling
abilities_labels = [str(ability) for ability in abilities_sorted]

#plot the bar chart
plt.figure(figsize=(10, 6))
plt.bar(abilities_labels, counts_sorted, color='skyblue')
plt.xlabel('Ability ID')
plt.ylabel('Total Usage Count')

#update the title to include the sample size
plt.title(f'Total Usage of Abilities (Sample Size: {total_sample_size})')

plt.xticks(rotation=45)
plt.grid(axis='y', linestyle='--', alpha=0.7)
plt.tight_layout()

#save the plot to a file
output_image_path = os.path.join(os.path.abspath(os.getcwd()), 'DataExplorer', 'Temp', 'ability_usage_plot.png')
plt.savefig(output_image_path, dpi=300)

UnityEngine.Debug.Log(f'Plot saved to {output_image_path}')
open_image(output_image_path)  #open the image with the default software
plt.close()  #close the plot to avoid overlap with the next plot