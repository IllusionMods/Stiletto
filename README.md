# Stiletto

Plugin for Koikatsu / Koikatsu Sunshine with the following features:
  - Define a heeled pose, that can affect 4 parameters, for any kind of shoes:
    - Whole feet rotation
    - Ankle rotation
    - Toes rotation
    - Height of the heels
  - Save different parameters setups for different game animations/poses (for example some animations might not need a height parameter to stay aligned with other characters)
    - Stiletto can be disabled totally for a pose/animation
    - Each parameters can be enabled/disabled separately
    - Knee bend can also be set
    - Both legs can be set separately
  - Warp shoe model (rotate/scale/translate) to help fix some mods
    
## How to install

1. Install the latest [BepInEx](https://github.com/BepInEx/BepInEx/releases)
2. Install the latest [KKAPI](https://github.com/IllusionMods/IllusionModdingAPI/releases) (be sure to take the right version for your game KKAPI or KKSAPI).
3. Download [the latest release](https://github.com/IllusionMods/Stiletto/releases) for your game (the .dll file). **Warning:** You only need the version specific for your game (check the prefix, for example KK = for Koikatsu). Downloading version for the wrong game or multiple versions will break things!
4. Put the .dll file into your game directory, inside BepInEx\plugins.
5. If you already had a version of Stiletto delete **_dialog.txt** and **_config.txt** in **BepInEx/config/Stiletto** (keep a backup if you made changes in these files, to reapply them if needed), they will be re-created on startup. These files are not overriden if they exists so you might miss new texts or new features.

## How to use

The plugin has two interfaces. One simplified for the Maker and one more advanced for the Main game / Studio.

- To access the Maker Stiletto menu you need to browse Clothes -> Stiletto
- To open the **Advanced window** you can use **RightShift** this can be changed in the Plugins configuration (F1). Or click on Advanced Panel in the Maker Stiletto menu

### For old Stiletto users
- For users of version < 2.4.0: the location of the heels config has changed it moved from [InstallFolder]/BepInEx/Stiletto to [InstallFolder]/BepInEx/config/Stiletto/Heels. Please manually move all your existing files (an automated migration might be developped in the future, but not yet).

### Maker interface

![Maker_anoted](https://github.com/Cleep2/Stiletto/assets/106453167/849f7b57-990b-4cd6-9ad1-491b30ae9d8e)

#### 1 - Disclaimer
This disclaimer is important, **if you leave the panel without saving, you will lose your changes**. 

This is because of how Stiletto works. Stiletto will save a config file under [Install_Folder]/BepinEx/config/Stiletto/Heels/[your_shoes_name].txt and this file will be read whenever a character equips the said shoes. 
Because of that the values are not saved along with the card but on the other hand this avoids having to set the parameters for every one of your characters.
   
#### 2 - Heels settings

Here you can set the different values for the heels. 

If you select shoes that were not already set you should arrive on the heels all flatened (you probably already seen that on a few mods)
![Heels Settings 1](https://github.com/Cleep2/Stiletto/assets/106453167/867f01f8-b524-448e-819f-30654040b7a8)

You can set the values in any order but here is a step by step show the effect of each slider

**Whole Foot Rotation**
![Heels Settings 2](https://github.com/Cleep2/Stiletto/assets/106453167/f880b605-df31-48f2-b8ea-6f6391874dd9)

**Ankle Rotation**
![Heels Settings 3](https://github.com/Cleep2/Stiletto/assets/106453167/a0293a80-6ec1-4f49-9a3f-2e995fbcfe75)
Rotates with a center point lower than Whole Foot. Basically it doesn't move the tip of the heel.

**Toes Rotation**
![Heels Settings 4](https://github.com/Cleep2/Stiletto/assets/106453167/6ca0c963-add0-4886-b4ab-b793354ad8ec)

**Height**
![Heels Settings 5](https://github.com/Cleep2/Stiletto/assets/106453167/1948a102-de1e-440c-9046-c6e587ac84c1)

**Note**
You might need to tweak a little bit between the Whole Foot and the Ankle, because in extreme values the Whole Foot will tend to give a "unnatural heel angle" like in the example below
![tweaking](https://github.com/Cleep2/Stiletto/assets/106453167/33bc8bf5-ebbd-48e4-8235-d31650dc6dd1)

#### 3 - Warp settings

Can be used to fix some shoe models if needed. This section allows to translate, rotate, scale the shoe model without impacting the feet.
As of now it only affect the "heel part" of the model (not the toes).
You can tweak the parameters here to see if it improves the result.

#### 4 - Actions

**Save Heels Settings**: again this is important, don't forget to click before leaving the Stiletto panel (opening the Advanced Panel won't reset the values)

**Advanced Panel**: opens the advanced panel which is described in the Maker/Studio interface chapter.

## Game/Studio interface

The purpose of this interface is to tweak when you are in the game/studio but you can also open it in the Maker if you want.

![advanced](https://github.com/Cleep2/Stiletto/assets/106453167/d431ad58-6d50-4974-821b-e8dee32aae7f)

### Reload Heels/Anims Settings

This button is visible on all tabs, it re-checks all the heels settings and flags files in the Stilleto folder and clears the plugin cache. You should use it after you saved a new setting, if you added manually a new file while the game is running or if things start to act weird.

The other features will be described from top to bottom

### Selection

The top of the window is static and relates to the current selection (character/pose)

Switch: change the character selection cycling, two option are possible:
 - All characters: all the characters currently loaded in the game
 - Current characters: only the characters in the current scene (e.g: H-Scene in an open space)
Name: name of the selected character
Heel: name of the current selected heels on the current character
Anim Path: name of the animation group of the current animation/pose
Anim Name: name of the current animation/pose
Total: count of available characters (use Previous/Next to cycle between them)

Under this section you have access to four tabs

### Animation
![advanced-anim](https://github.com/Cleep2/Stiletto/assets/106453167/b0f1f1d3-53f4-48b0-8cae-a4b7ed7e178d)

Is used to set the flags for the current animation. By flags we mean to enable/disable some parameters
 - Active: enable/disable Stiletto for the current animation
 - Toe roll: enable/disable the toes rotation the current animation
 - Ankle roll: enable/disable the ankle rotation the current animation
 - Height: enable/disable the height offset the current animation
 - Knee bend: enable/disable the knee bending (this is an automatic change depending on the rest of the parameters, you can try it to see if it looks better in your case)
 - Custom pose: enable/disable the use of customized pose (see section below)

Below the parameters you have three buttons under the "save these settings" title.
 - For all animations: will create a "wildcard" which will apply these flags to all unknown animations. **If a "Path" or "Name" config is set for an animation the "Wildcard" config will not apply**
 - For the whole Animation Path: will apply these flags to all unknown animations under the Anim Path (e.g if the path is "adv" all the animations like "adv/Stand_00_00", "adv/Stand_01_00", etc). **If a "Name" config is set for an animation the "Path" config will not apply**
 - For this specific Animation Name: will apply these settings only to the current animation. This settings will always apply over the two other "wildcards" (all animations, and animation path)

### Custom
![advanced-custom](https://github.com/Cleep2/Stiletto/assets/106453167/ddba47b1-1c5f-4723-9242-5b9e9dcb8cf8)

Is used to customize more precisely the parameters
 - Waist angle: sets the waist angle
 - Both Legs toggle: Allow to edit legs one by one or both at the same time
 - Thigh rotation: sets the thigh/knee rotation
 - Whole foot rotation:

You have the same three buttons as in the Animation section, please check here for more details.

**Custom toggle on Animation tab needs to be activated for these to work**

### Heel
![advanced-heels](https://github.com/Cleep2/Stiletto/assets/106453167/dcfd2028-4921-414f-b6ee-5a28a03b2b79)

Is used to set the heels settings as in the Maker, see Maker interface > Heels settings section for more details
**Don't forget to click on save**

### Warp
![advanced-warp](https://github.com/Cleep2/Stiletto/assets/106453167/0db88022-f872-4d3e-b249-ed6531b21e0d)

Is used to alter the shoe model as in the Maker, see Maker interface > Shoe warp section for morde details
**As for the Heels settings don't forget to click on save**

## Troubleshooting

### Nothing happens
A few things might fix that
 - Make sure that the Active checkbox is ticked in the advanced menu, it might be that the Active was disabled at a "Wildcard" or "Path" level.
 - Click on Reload Heels/Anims Settings
 - Make sure that the heels rotation indeed have parameters set (most likely to happen in Studio/Game as the parameters are inside a tab)

If it is still not working you can open an issue with a clear reproduction scenario (with any information you can get, screenshots, name of the mod used, etc).

### Toe roll
Under certain conditions the toes rotation my be reverted. You just need to change the shoes (either by changing the clothing set or switching between outdoor and indoor shoes).
If someone has a 100% repro scenario don't hesitate to open an Issue for this, we will try to fix it.

## For mod makers

- The angles are in degree so this should match what you set in your CAD software
- If you want your users to have the right values without tweaking you need to provide the **[name_of_your_shoes_in_list_file].txt** along with your zipmod and ask them to put it in the right place (or provide a zip with the file structure already set).
  - The location is **BepInEx/config/Stiletto/Heels/[name_of_your_shoes_in_list_file].txt**
- It is probably easier to open your shoes in the Maker first and generate your config file from Save Heels Settings button (it will be at the same location) than making it from scratch

