![cryofall_logo_big01](https://user-images.githubusercontent.com/865384/55807472-4c969300-5af3-11e9-8c14-7a99f9915423.png)

This is the source code for [CryoFall](http://cryofall.com) game (doesn't include the engine source code and cannot be used separately from the game). It's also shipped with the game in `<game folder>/Core` folder. Client and Server (and Editor) are all using the same code but Editor also using Editor.mpk mod to provide the editor capabilities.

This repository is created to help with the game modding and also to get feedback and pull requests from our community!

The game code is provided in C#, XAML and HLSL code for game scripts & content, UI, and shaders respectively.
All the game code could be live edited and the changes are applied immediately by the game engine.

Modding prerequisites
-----
Please follow [the guide on official forums](http://forums.atomictorch.com/index.php?topic=1027.0).

Contributing
-----
Pull requests are welcome, but it's always best to contact us before you start any work to ensure your contribution could be useful to the project.
Please make your code is compliant with Microsoft recommended coding conventions:
* [General Naming Conventions](https://msdn.microsoft.com/en-us/library/ms229045%28v=vs.110%29.aspx) 
* [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx)

License
-----
Please read the attached license file — the game code is open source but it's not a free open source project. All the source code from this repository should be used only in context of CryoFall modding. Please read [LICENSE.md](LICENSE.md) for details.
