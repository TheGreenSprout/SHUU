# -------------->| 🌱 Sprout's Handy Unity Utils (SHUU) 🌱 |<--------------


## Thank you for downloading SHUU! I'm Sprout (TheGreenSprout in most social accounts). -> https://sprouts-garden.netlify.app

My intention when making this package is to take all of those annoying little things
that you have to do in most unity games, and make them as easy and fast to implement
as possible! You know which ones I'm talking about, those features that every project
needs, the ones that you know how to do but can't be bothered to make everytime.
That's what this package is for!
Enjoy :D


#### P.D: Use this package for anything you like, but if you do, please leave this readme untouched in order to credit me! :b




⚠️ This package requires Newtonsoft JSON and TextMeshPro.
To install it:
  - Open [Window > Package Manager]
    1.
    - Click the "+" sign and "Install package by name..."
    - Install "com.unity.nuget.newtonsoft-json"
    2.
    - Search and install "TextMeshPro" (and import it's dependencies if needed)

  ⚠️ After installing this dependency:
        + Go to [Project Settings > Player > Other Settings > Scripting Compilation > Scripting Define Symbols]
        + Click the "+" sign and type "SHUU_SAVE_DEPENDENCY" 
        + Click "Apply"


️️⚠️ For a bunch of the implemented systems to work, you'll need a few objects/scripts in every scene:
    - In your assets folder go to [SHUU > ForUser > Prefabs > PlaceOnEveryScene]
    - There you will find a "OnEveryScene" prefab. Place it on every scene
    - Feel free to modify some of the values that appear on these scripts (in the inspector window), they are made to be customized


⚠️ If you want to check out the Sample Scenes, please add them (and LoadingScene [inside the ForUser folder]) to your [Build Profiles > Scene List]



⚠️‼️ AI ASSISTED CODE
    - This package contains some AI assisted code. All scripts with AI assistance present will have a warning at the very top!




### + Current Version: 2.8.0
### - Update Date: 13-09-2025
### - Creation Date: 24-03-2025




# --> Features (check 'Documentation/SCRIPT_INFORMATION' for detailed information on each class and method):
##      + Data management:
###         - Data saving and loading (txt and json files)

###         - Data encryption (RSA, AES, and Base64)

###         - File explorer interaction (windows only)
    

##      + Handy functions:
###         - String manipulation

###         - Enum handling

###         - List manipulation


##      + Audio management:
###         - Customizable creation of sound effects

###         - Automatically keeps track of each sound
    

##      + Timer manager:
###         - Creation of custom dynamic timers.




# --> CREDITS:
##      + CodeMonkey: Some of the data saving file explorer interaction logic.

##      + Christina Creates Games: The base script for the text typewriter effect (I modified it)
###         - Link: https://www.youtube.com/watch?v=UR_Rh0c4gbY