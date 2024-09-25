Both PC and Android game Unity3d-based guitar hero-like game but, with a twist - music stored in text files in JSON and generated on fly during playing session. This approach allows more direct control on music.

Game logics scripts are located in Foxtrot-MuzicBox/Assets//Scripts

Current stage of development: implementation of saving user progress and statistics and UI (this stil unstable)

[Actual Videos]
Game loop example: https://www.youtube.com/watch?v=6AOiqpeGaCM

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
