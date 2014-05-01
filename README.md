##MagicUI
#### A framework for rapidly prototyping Unity user interfaces

Gizmocracy only has one coder (hi there!), so I spend a lot of my time developing tools to shift work from my backlog to others'. This is especially important when we're in the rapid-prototyping phase of a game--I found I was spending more time doing UI development than coding actual gameplay. The Unity editor can technically do anything, but it's cluttered with lots of features a designer doesn't need... plus, to be brutally honest, we didn't want to drop three grand on a second seat.

The MagicUI system fills our need by rendering a layout defined in an online UI prototyping (or "wireframing") tool. Originally we were using [Balsamiq](http://balsamiq.com/), but after the first draft of this system was developed, a new service called [NinjaMock](http://ninjamock.com/) came online and we switched to that. Because the service is online, multiple people can access it, and layouts can be fetched by the client directly through an Internet connection.

At runtime, the client fetches the specified NinjaMock project and then renders all the pages as collections of NGUI controls, using a skin (basically a sprite atlas) set at compile time. Button links defined in the wireframe project are automatically live and functional in the client, and editable controls write their values to a global lookup table that can be accessed by code. All the programmer has to do is write code that checks the lookup table for changes values and reacts appropriately. For example, the music system monitors the "music" value in the table and uses it to set the volume. Any place the designer places a control named "music", that control now controls the volume level--not only can the designer move the control to different screens as the UI is refined, the control type itself can be changed without altering any of the program's functionality.

### Usage

Getting started with MagicUI isn't particularly difficult, but it's got a number of manual steps that I haven't bothered to automate yet. I suggest getting a test of the system working in a blank project, then exporting it as a package so that you can fire up new projects quickly.

1. *Install Prerequisites*. MagicUI depends on some other code, most notably the excellent NGUI.
2. *Copy Code*. Clone the repo and copy its code over to your project.
3. *Create a Skin*. Skins are how MagicUI organizes sprites and fonts. You can have multiple skins and switch between them at compile time, but you have to have at least one.
4. *Set Up the Singleton*. The MagicUI system is driven by a master class, MagicUIManager, that exists as a singleton component in your UI scene.
5. *Set Up the String Table* (optional). MagicUI treats the strings and text you enter into NinjaMock not as literal text but as control names. You can define a string table to translate these names into human-readable text, or for prototyping you can skip that and rely on savvy users to know what you're talking about.
6. *Wire Up Behavior* (optional). Basic functionality--jumping from page to page, remembering the values of controls--is provided automatically. To wire that functionality into your app you'll have to write code, but your designers can test and refine the layout without any involvement from you.

#### Install Prerequisites

1. *[NGUI](http://u3d.as/content/tasharen-entertainment/ngui-next-gen-ui/2vh)*. Doesn't everyone have this already?
2. *[JSONObject](https://www.assetstore.unity3d.com/#/content/710)*. There's several different JSON parsers out there (including one built into NGUI); I happen to like this one. It wouldn't be too onerous to replace it with your own favorite.
3. *My [Singleton](http://stebeegizmo.github.io/Singleton/) framework*. The MagicUI system is a local-scope singleton, and it relies on the DebugManager singleton for error reporting.
4. *A [NinjaMock](http://ninjamock.com/) account*. You can experiment for free, and a pro account (required for continued use) is on sale for $10 a month right now, down from the usual $15/month.

#### Copy Code

REST OF THIS FILE IS ALL TODO. WATCH THIS SPACE!

#### Create a Skin

A MagicUI skin is an NGUI atlas with an additionc component.

#### Set Up the Singleton

#### Set Up the String Table

#### Wire Up Behavior


