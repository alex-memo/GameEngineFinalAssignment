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

def open_image(path):
    if platform.system() == 'Windows':
        os.startfile(path)
    elif platform.system() == 'Darwin':  # macOS
        subprocess.call(['open', path])
    else:  # Linux
        subprocess.call(['xdg-open', path])

#path
csv_file_path = os.path.abspath(os.getcwd())
csv_file_path += "/DataExplorer/Temp/ToolsUsageTempData.txt"  # Updated file name
UnityEngine.Debug.Log(csv_file_path)

#dictionary to hold the total counts for each tool
tool_counts = defaultdict(int)

#variable to hold the total sample size
total_sample_size = 0

#read the CSV file
with open(csv_file_path, 'r', newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row_num, row in enumerate(reader, start=1):
        if row_num == 1:
            continue  #skip the first data row as it's the summary row
        #get the count of times this loadout was used
        loadout_sum_str = row['PlayerLoadouts (Sum)'].replace(',', '')
        try:
            loadout_sum = int(loadout_sum_str)
        except ValueError:
            UnityEngine.Debug.Log(f"Row {row_num}: Invalid integer value for loadout sum: '{row['PlayerLoadouts (Sum)']}'")
            continue  #skip this row if the value is invalid

        #get the tools field
        tool_str = row['Tools']

        #clean up the tool string
        tool_str = tool_str.strip('"').strip("'").strip()
        tools_in_row = tool_str.split(',')

        #count of valid tools in this row
        num_tools_in_row = 0

        for tool in tools_in_row:
            tool = tool.strip().strip("'")
            if tool != '-1':
                try:
                    tool_id = int(tool)
                    tool_counts[tool_id] += loadout_sum
                    num_tools_in_row += 1  #increment the count of valid tools
                except ValueError:
                    UnityEngine.Debug.Log(f"Row {row_num}: Invalid tool ID '{tool}'")
                    continue  #skip invalid tool IDs

        #update the total sample size
        total_sample_size += num_tools_in_row * loadout_sum

#prepare data for plotting
tools = list(tool_counts.keys())
counts = [tool_counts[tool] for tool in tools]

#sort tools numerically
tools_sorted, counts_sorted = zip(*sorted(zip(tools, counts)))

#convert tool IDs back to strings for labeling
tools_labels = [str(tool) for tool in tools_sorted]

#plot the bar chart
plt.figure(figsize=(10, 6))
plt.bar(tools_labels, counts_sorted, color='green')
plt.xlabel('Tool ID')
plt.ylabel('Total Usage Count')

#update the title to include the sample size
plt.title(f'Total Usage of Tools (Sample Size: {total_sample_size})')

plt.xticks(rotation=45)
plt.tight_layout()

# save the plot to a file
output_image_path = os.path.join(os.path.abspath(os.getcwd()), 'DataExplorer', 'Temp', 'tool_usage_plot.png')
plt.savefig(output_image_path, dpi=300)
UnityEngine.Debug.Log(f'Plot saved to {output_image_path}')
open_image(output_image_path)
plt.close()