Both PC and Android game Unity3d-based guitar hero-like game but, with a twist - music stored in text files in JSON and generated on fly during playing session. This approach allows more direct control on music.

Game logics scripts are located in Foxtrot-MuzicBox/Assets//Scripts

Current stage of development: freezed until 10 tracks are created

[Implemented features]
- Music
  - game tracks are stored as JSON text files which on musical notation. Tracks could be partially converted through converter script from MusicXML (this could be exported from MuseScore). Files consists of:
    - game level metadata, like level name, level goal, level opponent etc.
    - track structure. Track is compiled from such part as intros, couplets, choruses, bridges, solos, outros, which could be placed in different order
    - part settings like playable and unplayable staffs (lanes) with associated with them synth instruments. Each part can have different number of playable or unplayable staffs (lanes)
  - All music instruments are synth instruments. Currently available "standard" and "custom synthwave" instruments. 
    - Standard's behavior is coded, like sin, saw, click, square, click, pulse. 
    - Custom are set with three images and JSON file (I think they need a bit more attention after unfreeze)
- Game
  - Tree difficulty levels, which change such game parameters field of view (how many beats ahead player see), what types of pucks are spawned (simple, bonus, invisible/auto, mines) and how they are spawned.  (Both are need to be balanced)
  - Two level goals - "Destroy" and "Playthrough". Level continues until level goal is reach or player runs out of health. Until then track is looped between intro and outro parts
    - Destroy - on game field there is invisible (for now) enemy. Goal to destroy enemy. Each correctly activated puck deals damage to opponent
    - Playthrough - survive till track ends
  - Two skills:
    - Lullaby - track's BPM is decreased
    - NightCore - track's BPM and pitch are increased

[Roadmap]
TBD after freeze:
  - Add more at least 10 synth tracks
  - Facelift of game and especially menus (bring them to gamefield style)
  - Add player setting (at least key bidings)
  - Add support for phones (in fact already implemented but must be tweaked and well tested)
  - Publish somewhere

TBD after publishing
  - Add global leader board
  - Add new skills
  - Add albums (in fact folders)
  - Add story mode and stories
  - Add  new instruments
  - Add possibility for players to upload own tracks, instruments, stories. In fact first two already can be done through streaming assets folder of the game, but currently adding new tracks clears player scores (done to save some time on validating changes to track JSON files)

[Actual Videos]

Game loop video
https://youtu.be/1WcyEjk4AXY

Demo for Custom SynthWave instruments (A little bit about them in next message)
https://youtu.be/4vf4GGLuY2c

Demo for different difficulty levels
https://youtu.be/MeLuB8YMmY0

Video explanation:
- Game: gets list of all json music level files in appropriate folder in Streaming Assets and displays them in main menu. Each music level has a certain goal to destroy enemy (implemented) or play through track (TBD). 
- Player: can choose level, see its current stats, choose difficulty level. On difficulty level depends how far can see player, how beat puck are swpanes (for example at easy level only on one lane beat puck can be spawned), what beat pucks are spawned and their spawn chances).
- Player: Starts the level
- Game: from JSON file gets music structure and all music parts and instrumens. From begining is loaded intro part of music level and elso prepared next part, which is seen by player as in the background, while playing.
- Player: after closing promt with instructions unpause prepared level. Player see music highway (each lane is separate music instrument), on its lane beat pucks are spawned. If player clicks/presses correct correct key in time in "destroy" enemy gets damage, otherwise -player gets damage. Bonus beat if engaged correctly can replenish player health. Player has ability to use two skills: lulabely and nightcore. Top left of the screen is player's health bar on right side is enemy's
- Game: spawn beats and prepares new parts until enemy destroyed. It cicles music parts until level goal is cleared or player run out of health. If player engage note correctly game plays true note otherwise it plays false note (except for bonus note). If player tapped on mine note, nothing played but player gets damage. After goal cleared (at "destroy" level enemy health is depleted) gamed spawns only bonus pucks and after all prepared parts are played, game loads outro.


[Archived videos]
Game loop example: https://youtu.be/l3mRmoVUcms
-https://www.youtube.com/watch?v=vuVcF3qREP4
-https://www.youtube.com/watch?v=6AOiqpeGaCM
-example of fail gameplay [https://youtu.be/BXhWQPKjPS0](https://youtu.be/mY554DiK4KY)
-example of almost win (all goals are cleared until music ends all notes are become a bonus ones) and both skills [https://youtu.be/nEDgIFQN1dk](https://youtu.be/nEDgIFQN1dk)
