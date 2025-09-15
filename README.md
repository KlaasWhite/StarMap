# StarMap
A POC/test modloader primarly for Kerbal Space Agency   
This was inspired by https://github.com/cheese3660/KsaLoader for me to learn about assemblyloading.   
It very heavily relies on AssemblyLoadContexts to load both the game and then the mods in the game.   
The advantage of this is that different mods can use different versions of the same dependencies.   

The main goal was to be able to unload the whole game and the modding, change the mods/dlls around and then load it again.   
This could result in a simular system to how Factory feels where the mod manager lives within the game.   
Sadly it did not work out in the current way it is implemented because Harmony does not play well ALC's and it keeps references to objects within the Assemblies.   
This results in the assemblies not unloading which means I can not swap the dll's (it locks them).   
A probable way forward is using a seperate process for the game itself so the whole process can shut down, however communication between processes forms a barrier to doing it this way.   
