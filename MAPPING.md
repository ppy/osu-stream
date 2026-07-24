# Mapping for osu!stream

Mapping for osu!stream is quite a bit more involved as there is no built in Editor for Beatmaps to be made, this guide will walk you through how to create maps for osu!stream.

This guide assumes you know how to get osu!stream to build, at least for Desktop.

## Map Structure

Maps in osu!stream are structured a little differently when it comes to spread. There are 4 difficulty levels:
* Easy
* Normal
* Hard
* Extreme

Easy and Extreme are self explanatory, Normal and Hard are both for the **Stream** difficulty, the Lower Stream is the Normal difficulty, this is also the starting difficulty in Stream Mode. Hard difficulty is then the higher stream.

When those switches occur is entirely up to the Mapper to decide and is something we'll touch on later.

Your map also ***has*** to have a audio file named audio.m4a in its map directory.

### Regarding Difficulty Names

Important to mention is that your Beatmap **has** to adhere to those difficulty names, otherwise you will not be able to Package the Beatmap and play it in osu!stream.
So your Beatmap has to have all those difficulties with those exact names. 

### Consequences for not adhering

Not having a difficulty will either lock you out from playing your Beatmap in a certain Difficulty Level (for example, when you're missing the Easy or Expert difficulty, you will not be able to play either of those Modes) or in Stream mode the higher stream won't be accessible.

# Mapping workflow

For the most part, you will be using the osu! Editor you most likely are familiar with, alongside BeatmapCombinator and StreamTester

### BeatmapCombinator

BeatmapCombinator is the program which you use to Package your Beatmaps into .osz2 files which osu!stream deals with

**Usage:**

Drag and Drop your Beatmap folder with all your .osu files which should all have the earlier specified Difficulty names and the audio.m4a file inside. After that it should give you back a .osz2 file which you can use for osu!stream

### StreamTester

StreamTester is a convinient little program for quickly Test-playing maps,

<p align="center">
  <img src="Readme Pictures/StreamTester.png">
</p>