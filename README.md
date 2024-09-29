Both PC and Android game Unity3d-based guitar hero-like game but, with a twist - music stored in text files in JSON and generated on fly during playing session. This approach allows more direct control on music.

Game logics scripts are located in Foxtrot-MuzicBox/Assets//Scripts

Current stage of development: implementation of saving user progress and statistics and UI (this stil unstable)

[Actual Videos]

Game loop example: https://www.youtube.com/watch?v=6AOiqpeGaCM
Video explanation:
- Game: get list of all json music level files in appropriate folder in Streaming Assets and displays them in main menu. Each music level has a certain goal to destroy enemy (implemented) or play through track (TBD). 
- Player: can choose level, see its current stats, choose difficulty level. On difficulty level depends how far can see player, how beat puck are swpanes (for example at easy level only on one lane beat puck can be spawned), what beat pucks are spawned and their spawn chances).
- Player: Starts the level
- Game: from JSON file gets music structure and all music parts and instrumens. From begining is loaded intro part of music level and elso prepared next part, which is seen by player as in the background, while playing.
- Player: after closing promt with instructions unpause prepared level. Player see music highway (each lane is separate music instrument), on its lane beat pucks are spawned. If player clicks/presses correct correct key in time in "destroy" enemy gets damage, otherwise -player gets damage. Bonus beat if engaged correctly can replenish player health. Player has ability to use two skills: lulabely and nightcore.
- Game: spawn beats and prepares new parts until enemy destroyed. It cicles music parts until level goal is cleared or player run out of health. If player engage note correctly game plays true note otherwise it plays false note (except for bonus note). If player tapped on mine note, nothing played but player gets damage. After goal cleared (at "destroy" level enemy health is depleted) gamed spawns only bonus pucks and after all prepared parts are played, game loads outro.


[Implemented features]
- 4 simple synth instruments (see /Foxtrot-MuzicBox/Assets/Scripts/AudioGenerator):
  - saw,
  - sin,
  - square,
  - pulse
- loading music from text file, text file notation is close to classic music sheet's notation but in JSON (see /Foxtrot-MuzicBox/Assets/StreamingAssets/SynthMusic/TestMusicLvL.json for example). 
  - each music composotion supports different parts (intro, Couplet, Chorus, Solo, Bridge, Outro).
  - each part in one composition could have several variant's
  - each part can have several playable or unplayable intstrument tracks. different part can have different set of instruments.
  - parts order is set in text file, depending on game mode it is planned, than parts would repeat until success or fail of mode's goal 
- three difficulty levels
- 2 player skills
  - NightCore - increasing speed and pitch
  - Lullaby - decreasing speed
- 1 game mode: duel
- various beat pucks (notes):
  - regular
  - mine
  - bonus
  - invisible(auto)
 
[Planned features]
- more instruments
- more game modes
- story mode


[Archived videos]
example of fail gameplay [https://youtu.be/BXhWQPKjPS0](https://youtu.be/mY554DiK4KY)
example of almost win (all goals are cleared until music ends all notes are become a bonus ones) and both skills [https://youtu.be/nEDgIFQN1dk](https://youtu.be/nEDgIFQN1dk)
