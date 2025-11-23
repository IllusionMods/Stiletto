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
2. Install the latest [KKAPI](https://gitgoon.dev/IllusionMods/IllusionModdingAPI/releases) (be sure to take the right version for your game KKAPI or KKSAPI). **This is important, the plugin won't load if you don't have at least v1.38**
3. Download [the latest release](https://gitgoon.dev/IllusionMods/Stiletto/releases) for your game (the .dll file). **Warning:** You only need the version specific for your game (check the prefix, for example KK = for Koikatsu). Downloading version for the wrong game or multiple versions will break things!
4. Put the .dll file into your game directory, inside BepInEx\plugins.
5. If you already had a version of Stiletto delete **_dialog.txt** and **_config.txt** in **BepInEx/config/Stiletto** (keep a backup if you made changes in these files, to reapply them if needed), they will be re-created on startup. These files are not overriden if they exists so you might miss new texts or new features.

## How to use

The plugin has two interfaces. One simplified that is only available in the Maker and one more advanced which is available everywhere (Main game / Studio / Maker).

- To access the simplified Stiletto menu you need to be in the maker and browse into Clothes -> Stiletto
- To open the **Advanced Panel** you can use **RightShift**, this can be changed in the Plugins configuration (F1). Or also click on Advanced Panel if you are in the Maker Stiletto menu

### For old Stiletto users
- For users of version < 2.4.0: the location of the heels config has changed it moved from [InstallFolder]/BepInEx/Stiletto to [InstallFolder]/BepInEx/config/Stiletto/Heels. Please manually move all your existing files (an automated migration might be developped in the future, but not yet).

### Maker interface

![Maker interface annotated](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Maker-interface-annotated.jpg)

#### 1 - Disclaimer
This disclaimer is important, **if you leave the panel without saving, you will lose your changes**. 

This is because of how Stiletto works. Stiletto will save a config file under [Install_Folder]/BepinEx/config/Stiletto/Heels/[your_shoes_name].txt and this file will be read whenever a character equips the said shoes. 
Because of that the values are not saved along with the card but on the other hand this avoids having to set the parameters for every one of your characters.
   
#### 2 - Heels settings

Here you can set the different values for the heels. 

If you select shoes that were not already set you should arrive on the heels all flatened (you probably already seen that on a few mods)
![Heels Settings 1](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels-Settings-1.jpg)

You can set the values in any order but here is a step by step show the effect of each slider

**Whole Foot Rotation**
![Heels Settings 2](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels-Settings-2.jpg)

**Ankle Rotation**
![Heels Settings 3](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels-Settings-3.jpg)
Rotates with a center point lower than Whole Foot. Basically it doesn't move the tip of the heel.

**Toes Rotation**
![Heels Settings 4](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels-Settings-4.jpg)

**Height**
![Heels Settings 5](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels-Settings-5.jpg)

**Note**
You might need to tweak a little bit between the Whole Foot and the Ankle, because in extreme values the Whole Foot will tend to give a "unnatural heel angle" like in the example below
![tweaking](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/tweaking.jpg)

#### 3 - Warp settings

Can be used to fix some shoe models if needed. This section allows to translate, rotate, scale the shoe model without impacting the feet.
As of now it only affect the "heel part" of the model (not the toes).
You can tweak the parameters here to see if it improves the result.

#### 4 - Actions

**Save Heels Settings**: again this is important, don't forget to click before leaving the Stiletto panel (opening the Advanced Panel won't reset the values)

**Advanced Panel**: opens the advanced panel which is described in the Maker/Studio interface chapter.

## Game/Studio interface

The purpose of this interface is to tweak when you are in the game/studio but you can also open it in the Maker if you want.

![advanced](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/advanced.jpg)

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
![advanced-anim](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/advanced-anim.jpg)

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
![Custom](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Custom.jpg)

Is used to customize more precisely the parameters
 - Waist angle
 - Both Legs toggle: Allow to edit legs one by one or both at the same time
 - Thigh Rotation
 - Knee Rotation
 - Ankle Rotation

You have the same three buttons as in the Animation section, please check here for more details.

**Custom toggle on Animation tab needs to be activated for these to work**

### Heel
![Heels](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Heels.jpg)

Is used to set the heels settings as in the Maker, see Maker interface > Heels settings section for more details
**Don't forget to click on save**

### Warp
![Warp](https://gitgoon.dev/IllusionMods/Stiletto/raw/branch/master/doc/Warp.jpg)

Is used to alter the shoe model as in the Maker, see Maker interface > Shoe warp section for more details
**As for the Heels settings don't forget to click on save**

## Troubleshooting

### Nothing happens
A few things might fix that
 - Make sure you have the latest KKAPI, the plugin won't load if you don't.
 - Make sure that the Active checkbox is ticked in the advanced menu, it might be that the Active was disabled at a "Wildcard" or "Path" level.
 - Click on Reload Heels/Anims Settings
 - Make sure that the heels rotation indeed have parameters set (most likely to happen in Studio/Game as the parameters are inside a tab)

If it is still not working you can open an issue with a clear reproduction scenario (with any information you can get, screenshots, name of the mod used, etc).

### Toe roll
~~Under certain conditions the toes rotation my be reverted. You just need to change the shoes (either by changing the clothing set or switching between outdoor and indoor shoes).
If someone has a 100% repro scenario don't hesitate to open an Issue for this, we will try to fix it.~~ **Fixed on 2.4.1**

## For mod makers

- The angles are in degree so this should match what you set in your CAD software
- If you want your users to have the right values without tweaking you need to provide the **[name_of_your_shoes_in_list_file].txt** along with your zipmod and ask them to put it in the right place (or provide a zip with the file structure already set).
  - The location is **BepInEx/config/Stiletto/Heels/[name_of_your_shoes_in_list_file].txt**
- It is probably easier to open your shoes in the Maker first and generate your config file from Save Heels Settings button (it will be at the same location) than making it from scratch

