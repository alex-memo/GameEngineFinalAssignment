# INFR 1350U – Game Engine Design & Implementation
Intro to Comptuer Graphics Project M forked version

**Group and Contributions**

Alejandro *(Alex)* Mejia Moysen - 100862130

Ethan Gibson - 100862130

Venus Poon - 100861849

![Video](https://drive.google.com/file/d/12hC4zSfltoqgINoiVY59vc4SyLsTpMUG/view?usp=drive_link)

*if link broken*
https://drive.google.com/file/d/12hC4zSfltoqgINoiVY59vc4SyLsTpMUG/view?usp=drive_link

***Alex:***

Fireball Shader

Shield Shader

Outline Shader

HealthBar Shader

Zone LUT


***Ethan:***

Unlit

Ambient

Specular

Ambient + Specular

Ambient + Specular + Toon

Ambient + Specular + Breathing


***Venus:***

Reports



**Acknowledgements**


The playable character is from Mixamo.
However, the movement code that makes the character model move is made by our group.
The current scene map is the practice range which is blocked out using Probuilder.


**Part 1: Base**


The scene consists of a practice range for players to try out different core game mechanics.
Here, players can see the effects of different barrels to dummies.
They are also able to try utilising abilities in this range in order to get a grasp of what everything does.
The model of the dummy was made by Venus, the scene made and populated by Ethan, and the code for movement and shooting were done by Alex.


**Part 2: Illumination**


In Intro to Computer Graphics, the professor introduced illumination and the different types.
In this project, the code from those classes were used and implemented into materials.
These materials were then applied to primitive spheres. There were also two more additional effects implemented.

These are: toon shader and a dynamic specular powered shader. These were done by Ethan.


**Part 3: Colour Grading**


To start off with colour grading, a screenshot was taken of the game. A neutral LUT image file was downloaded from the Intro to Computer Graphics class, as well as other neutral LUTs were taken from the internet, specifically from OBS.  From here, they were taken into Photoshop and the filters for Protanopia-type and Deuteranopia-type colour blindness filters were applied. A screenshot of the LUTs with the filter applied were screenshotted as Photoshop only applies the colour blindness filter for the app but not the actual image. The Purple LUT was made by Alex for a game mechanic, while the Protanopia LUT and Deuteranopia LUT were made by Venus for accessibility.


The following are the LUTs used:


**Purple LUT**

![Purple Lut](https://github.com/alex-memo/ProjectM-CG/blob/main/ZoneLUT.png?raw=true)


**Protanope LUT**

![Protanope Lut](https://github.com/alex-memo/ProjectM-CG/blob/main/Protanope.png?raw=true)


**Deuteranope LUT**

![Deuteranope Lut](https://github.com/alex-memo/ProjectM-CG/blob/main/Deuteranope.png?raw=true)

With these LUTs, the colour grading shaders were made. Purple LUT was made into a colour grading screen effect shader using shadergraph. The colour blindness colour grading screen effects makes use of code from Intro to Computer Graphics. This is implemented by Ethan using Canvas and Images.

**Part 4: Shaders and Effects**


There are a total of four shaders implemented in this scene. They are: the fireball shader, the outline shader, the shield shader, and the health bar shader. The themes for these were sci fi, comic, and fantasy. The fireball shader was created by Alex. It uses a scrolling texture with a gradient. The top part is covered in order to avoid distortion and then the vertex offset was added to give life to the fireball. The outline shader compares the normals of the screen, if the difference between normals are big enough, then a line is added between the different two normals. The same is done to colours. If colours are too different, an outline is overlaid onto the screen colour. The shield texture has a rim light, scroll effect, and an inner glow effect. Finally the health bar shader is made using the ben day effect. It compares if the pixel colour is greater than 0.3f, that gives us the circles. Both the health and shield bars have gradient effects. The color fades out (0.05) before the end of each bar, creating a fade-out gradient on the last portion of the bar. A smooth step function is applied across a gradient range (from -0.5 to 0.5), then multiplied by a Voronoi effect to add texture. This is applied to both health and shield. The current shield value is subtracted from the health value to create a mask, ensuring that the shield appears on top of the health bar. Colors are multiplied for both health and shield with their respective effects. This creates the intended color and blend effect. Both shield and health bars undergo a max operation for color values, with the shield overlaid on top of the health. To create the alpha mask, we maximize the premultiplied colour texture with their respective ben day effect of the shield and health bar. Then, we saturate it and the alpha mask is the result. After masking, a premultiplied alpha is calculated by saturating values to ensure no alpha exceeds 1, finalizing the transparency effect.

This was not the repository that is being worked on, this is a forked repo from Project M, which is private
=
***Project M***

*Description*

Project M is a 3v3 competitive team deathmatch, with modular loadouts, that combines a comic style and fantasy world together in a city setting.
This project contains a forked practice range that is being developed for project M for players to be able to test their abilities and tools, and learn interactions between different game elements.

![Jira Timeline](https://github.com/alex-memo/ProjectM-Assignment1/blob/main/JiraTimeline.png?raw=true)

***External Resources***

3D character from Mixamo.com *A private model for Project M is on the original repository but will not be made available in any public repo*

***Notice:*** Distribution, copying, or cloning of this repository or code is strictly prohibited as it is not allowed.
