# League Themed RPG Bot

Work-in-progress.

Project development rate will be irregular and slow due to my lack of time and effort to continue developing at a consistent rate.

----

Player data is saved/loaded in a json file called `playerData.json` adjacent to the executable

The bot token is loaded from a file called `token.txt` adjacent to the executable, of course the client won't work if there's no token

Json files containing required game data such as `weapons`, `armor`, `boots`, `enemies`, `skills` are found in their respective folders adjacent to the executable

Ensure the `weapons`, `armor`, and `boots`, folder contain json files with valid `Item` data, NOTHING else. Same goes with `skills` folder --> `Skill` data and `enemies` folder --> `Enemy` data

Each data folder can also have directories within which can also store valid json data - this is useful for sorting; ex `weapons/starter` contains json files with starter weapon data
