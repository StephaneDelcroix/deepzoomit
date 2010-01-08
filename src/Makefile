all: deepzoomit.exe

deepzoomit.exe: App.cs Image.cs TileKey.cs
	gmcs -debug -pkg:gtk-sharp-2.0 App.cs Image.cs TileKey.cs -out:deepzoomit.exe
