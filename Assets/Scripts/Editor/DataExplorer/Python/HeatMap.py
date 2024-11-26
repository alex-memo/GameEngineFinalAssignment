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
import matplotlib.image as mpimg

def open_image(path):
    if platform.system() == 'Windows':
        os.startfile(path)
    elif platform.system() == 'Darwin':  # macOS
        subprocess.call(['open', path])
    else:  # Linux
        subprocess.call(['xdg-open', path])

#construct the path to your CSV file
csv_file_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'HeatMapTempData.txt')
UnityEngine.Debug.Log(f"CSV file path: {csv_file_path}")

#construct the path to the minimap image
minimap_image_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'MiniMapRenderTex.png')
UnityEngine.Debug.Log(f"Minimap image path: {minimap_image_path}")

#check if the minimap image exists
if not os.path.exists(minimap_image_path):
    UnityEngine.Debug.LogError("Minimap image not found at: " + minimap_image_path)
    sys.exit()

#load the minimap image
minimap_img = mpimg.imread(minimap_image_path)

#lists to hold x and z coordinates of death positions
x_coords = []
z_coords = []

#read the csv file
with open(csv_file_path, 'r', newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        dpos_str = row['DPos']
        dpos_str = dpos_str.strip()
        if dpos_str in ["'-", "-", "", "'-'"]:
            continue  #no death positions recorded
        #clean up the DPos string
        dpos_str = dpos_str.strip('"').strip("'").strip()
        #split the DPos string into individual positions
        positions = dpos_str.split('|')
        for pos_str in positions:
            #clean up each position string
            pos_str = pos_str.strip().strip("'")
            #split into x, y, z
            coords = pos_str.split(',')
            if len(coords) != 3:
                UnityEngine.Debug.Log(f"Invalid position data: {pos_str}")
                continue
            try:
                x, y, z = map(float, coords)
                x_coords.append(x)
                z_coords.append(z)
            except ValueError:
                UnityEngine.Debug.Log(f"Invalid coordinate values: {coords}")
                continue

if not x_coords or not z_coords:
    UnityEngine.Debug.Log("No death positions found.")
    sys.exit()

#generate the heatmap
plt.figure(figsize=(10, 8))

#define the map bounds (adjust as necessary)
map_width = 206.2
map_height = 247.4
x_min, x_max = -map_width / 2, map_width / 2  # Map width
z_min, z_max = -map_height / 2, map_height / 2   # Map height

#load the minimap image as the background
plt.imshow(minimap_img, extent=[x_min, x_max, z_min, z_max], origin='upper')

#generate a 2D histogram
heatmap, xedges, yedges = np.histogram2d(x_coords, z_coords, bins=(50, 50), range=[[x_min, x_max], [z_min, z_max]])

#overlay the heatmap
extent = [xedges[0], xedges[-1], yedges[0], yedges[-1]]
plt.imshow(heatmap.T, extent=extent, origin='upper', cmap='hot', alpha=0.6, interpolation='nearest')

plt.colorbar(label='Death Count')
plt.xlabel('X Position')
plt.ylabel('Z Position')
# calculate total number of death positions (sample size)
sample_size = len(x_coords)

plt.title(f'Heatmap of Death Positions (Sample Size: {sample_size})')

#save the plot to a file
output_image_path = os.path.join(os.getcwd(), 'DataExplorer', 'Temp', 'death_positions_heatmap.png')
plt.savefig(output_image_path)

UnityEngine.Debug.Log(f'Heatmap saved to {output_image_path}')
open_image(output_image_path)
plt.close()