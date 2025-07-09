========================
 LowHealth - ReadMe.txt
========================



Introduction
============


LowHealth effects is shader that can be used as post processing effect in camera to simulate player having low
health or other problems with vision. Effects include losing vision, focus, details etc.



Basic usage
===========


Add script "LowHealthController" to the camera where you want effects to be used. You can either drag the script
to camera object or use Add Component in inspector.

Use checkboxes and sliders in inspector to adjust what kind of effects you want to use, how strong they are and
at what point of player's health those effects start taking effect.

You can use "Show in editor" and slider below it to quickly test how vision looks with different health values.
Effects should be visible in Unity editor's Game view.

After that, you can simply call methods SetPlayerHealthInstantly() and SetPlayerHealthSmoothly() in class
LowHealthController to set player health from your own code when application is running. Controller class will
set desired effects for that health level.


Detailed usage
--------------

There is four different effects:
- Loss of vision (starting from corners and edges toward screen center)
- Loss of details (vision starts getting more blurred)
- Loss of colors (to either toward grayscale or bloody red)
- Seeing double (loss of focus)

You can use any combination of these by enabling/disabling wanted effects. You can also choose how low player
health need to be before effect starts. This way you can make some effects to start quite early, some effects
to start when player is near dead. In most effects you can also choose how strong the effect is at maximum,
when player health nears zero.

If you want to reset settings at any point in editor, just click "Reset" from component popup menu (small cog
icon in top right corner of component in Unity inspector). Note that default values are just a suggestion and
depending on your game, some totally different values or enabling only some of the effects may work better.

See also example scenes and scripts in Examples folder.


Direct access
-------------

Optionally, you can add script "LowHealthDirectAccess" to the camera, instead of LowHealthController. This
class gives you direct access to shader effects. You can change the amount of any effect freely either from
Unity inspector or from code using methods provided by LowHealthDirectAccess class.


Builds
------

Note that when making builds of your project which uses LowHealthShader, you may need to add that shader to
list of always included shaders.

Go to "Edit" -> ""Project Settings" -> "Graphics" and add "LowHealthShader" (may be displayed as
"Custom/LowHealthShader") to the list of "Always Included Shaders".



Additional documents
====================


Inline c# documents are included to all public classes and methods that are needed to use these effects. They
are also available online in html format:

https://www.leguar.com/unity/lowhealth/apidoc/1.1/



Feedback
========


If you are happy with this asset, please rate us or leave feedback in Unity Asset Store:

https://assetstore.unity.com/packages/slug/161911


If you have any feedback, feel free to contact:

https://www.leguar.com/contact/?about=lowhealth
