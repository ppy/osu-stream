
# Mapping for osu!stream

Mapping for osu!stream is quite a bit more involved as there is no built-in editor for beatmaps to be made, this guide will walk you through how to create maps for osu!stream.

This guide assumes you know how to get osu!stream to build, at least for Desktop.

# Mapping workflow

For the most part, you will be using the osu! editor you're most likely familiar with, alongside BeatmapCombinator and StreamTester.

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

## Map Structure

  

Maps in osu!stream are structured a little differently when it comes to spread. There are only 4 difficulty levels:

* Easy
* Normal
* Hard
* Expert

Difficulties in osu!stream are called "streams," but to make this easier to understand they will be referenced as difficulties.

Accessing both Easy and Expert difficulties are self-explanatory. Normal and Hard are both accessed through **Stream mode**. Stream mode only loads the Normal difficulty, but "streaming up" is when the Hard difficulty is accessed. When those switches occur is up to the game or mapper to decide and is something  touched on later.

Your map can have a .mp3 or .m4a audio file, but **must** be named "audio".

osu!stream maps also require an extra metadata file to specify things in the song info section. 

  

### Regarding Difficulty Names

  Your beatmap must follow the difficulty naming rules. They can't be named anything else other than Easy, Normal, Hard, or Expert. Not following these rules leads to issues as:

* Being locked out of certain difficulties (e.g, not being able to access Hard in Stream mode or not being able to play Stream mode at all)
* Being unable to package your map
* Being unable to play-test your map

## Hitobjects

Everyone knows how to place a circle, slider, and a spinner, so we'll only cover osu!stream's special hitobjects.

### Multitouch objects

Multitouch objects are multiple hitobjects that need to be tapped at the same time.

**Making multitouch objects:**
Normally the osu! editor won't allow you to place 2 hitobjects at the same time, instead erasing the first hitobject and replacing it. To bypass this, place your second hitobject at a different point in time, then drag your second hitobject to your first hitobjects point in time. If done correctly, your second hitobject should be shown stacking ontop of your first hitobject. 

### Hold circles

  Hold circles are self-explanatory, they're objects that need to be held for a certain amount of time.

**Making a hold circle:**
Make a short slider with more than 4 repeats. To force a hold circle, add a Finish sound to the slider.  

## Breaks

There are no breaks in osu!stream (for whatever reason). Avoid making sections without hitobjects for 4 seconds, and instead make a cooldown section in your map.  

## Difficulty switches

 Difficulty switches (also known as streaming up/down) occur when the player reaches 100% health or 0% health.
  
**Making difficulty switches:**
Though switches are dynamic, they can be manually done by placing a bookmark at a point in time before the next new combo. The difficulty switching occurs after the bookmark after the next new combo. 

## Difficulty settings  

osu!stream ignores your beatmaps difficulty settings, and instead uses its own for each difficulty. Below are the settings osu!stream uses.

| Mode | CS | AR |
|:-----:|----:|--:|
|Easy|3|3|
|Stream|-|6|
|Expert|-|8|

***OD and HP drain is also ignored.**

## Extra metadata

Unlike osu!, osu!stream requires extra metadata to be included in your beatmap.
In your beatmap's folder, create a file named "metadata.txt" and paste in the following:  

	Title:
	TitleUnicode:

	Artist:
	ArtistUnicode:
	ArtistFullName:
	ArtistUrl: 
	ArtistTwitter:

	Creator:

	Source:
	SourceUnicode:

	PreviewTime: <ms>

	Difficulty: 1-10

## Backgrounds

osu!stream backgrounds work differently. Resize your background image to both 256x172 and 128x86, and save them as .jpg. Rename both of these files to "thumb-256" and "thumb-128." Afterwards, drop them in your beatmap folder.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEyOTM5NzE4NDJdfQ==
-->