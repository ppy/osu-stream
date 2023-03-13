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

# Mapping for osu!stream

Mapping for osu!stream is quite a bit more involved as there is no built-in in editor for beatmaps to be made, this guide will walk you through how to create maps for osu!stream.

This guide assumes you know how to get osu!stream to build, at least for Desktop.

## Map Structure

Maps in osu!stream are structured a little differently when it comes to spread. There are 4 difficulty levels:
* Easy
* Normal
* Hard
* Expert

Difficulties in osu!stream are called "streams," but to make this easier to understand they will be referenced as difficulties.

Accessing both Easy and Expert difficulties are self-explanatory. Normal and Hard are both accessed through **Stream mode**. Stream mode only loads the Normal difficulty, but "streaming up" is when the Hard difficulty is accessed.

When those switches occur is entirely up to the mapper to decide and is something that we'll touch on later.

Your map can have a .mp3 or .m4a audio file, but **must** be named "audio".

### Regarding Difficulty Names

An important thing to mention when making a beatmap is that your map **has** to adhere to those difficulty names, otherwise you will not be able to package the beatmap and play it in osu!stream. (this does mean you're limited to only 4 difficulties)

### Consequences for not adhering

Not having a difficulty will either lock you out from playing your Beatmap in a certain Difficulty Level (for example, when you're missing the Easy or Expert difficulty, you will not be able to play either of those Modes) or in Stream mode the higher stream won't be accessible.

# Mapping workflow

For the most part, you will be using the osu! editor you're most likely familiar with, alongside BeatmapCombinator and StreamTester

### BeatmapCombinator

BeatmapCombinator is the program which you use to package your Beatmaps into .osz2 files which osu!stream uses. This is also used by StreamTester to make the map playable, and package it.

**Usage:**

Drag and drop your beatmap folder with all your .osu files inside. After that it should output a .osz2 file which you can use for osu!stream.

### StreamTester

StreamTester is a convinient little program to quickly play-test and package your maps.

**Usage:**

Drag and drop your beatmap folder into the box saying to do so, and your map will be loaded.

**Play-testing:**
To test your map, click on "Test once." A window will pop up with your map being played in auto mode. To change the difficulty being played, select a difficulty in the "Initial Difficulty " section, and click "Test once" again.

# Mapping

This section will be referencing information already listed below in [peppys doc on osu!stream mapping.](https://docs.google.com/document/d/1FYmHhRX-onR-osgTS6uHSOZuu_0JEbfRZePVySvvr9g/edit?usp=drivesdk)

## Hitobjects

Everyone knows how to place a circle, slider, and a spinner, so we'll only cover osu!stream's special hitobjects.

### Multitouch objects

Multitouch objects are different objects that need to be tapped at the same time. 

**Making multitouch objects:**
Normally the osu! editor won't allow you to place 2 hitobjects at the same time, instead erasing the hitobject and replacing it. To bypass this, place your second hitobject at a different point in time, then drag your second hitobject to your first hitobjects point in time. If done correctly, your second hitobject should be shown stacking ontop of your first hitobject.

### Hold circles

Hold circles are self-explanatory, they're objects that need to be held for a certain amount of time.

**Making a hold circle:**
Make a short slider with more than 4 repeats. To force a hold circle, add a Finish sound to the slider.


## Breaks 

There are no breaks in osu!stream (for whatever reason). Avoid not placing hitobjects for 4 seconds, and instead make a "break section" in your map.


## Difficulty switches

tba

## Extra metadata

Unlike regular beatmaps, osu!stream beatmaps require extra metadata to be included.

Create a file named "metadata.txt" in your beatmap folder, and paste in the following:

	Title: 
	TitleUnicode: 
	
	Artist: 
	ArtistUnicode: 
	ArtistFullName: 
	ArtistUrl: http://
	ArtistTwitter:
	
	Creator: 
	
	Source: 
	SourceUnicode: 
	
	PreviewTime: <ms>
	
	Difficulty: 1-10

What to do next is self-explanatory.

## Backgrounds

osu!stream backgrounds work differently. Resize your background image to both 256x172 and 128x86, and save them as .jpg. Rename both of these files to "thumb-256" and "thumb-128." Afterwards, drop them in your beatmap folder.






