##MagicUI
#### A framework for rapidly prototyping Unity user interfaces

Gizmocracy only has one coder (hi there!), so I spend a lot of my time developing tools to shift work from my backlog to others'. This is especially important when we're in the rapid-prototyping phase of a game--I found I was spending more time doing UI development than coding actual gameplay. The Unity editor can technically do anything, but it's cluttered with lots of features a designer doesn't need... plus, to be brutally honest, we didn't want to drop three grand on a second seat.

The MagicUI system fills our need by rendering a layout defined in an online UI prototyping (or "wireframing") tool. Originally we were using [Balsamiq](http://balsamiq.com/), but after the first draft of this system was developed, a new service called [NinjaMock](http://ninjamock.com/) came online and we switched to that. Because the service is online, multiple people can access it, and layouts can be fetched by the client directly through an Internet connection.

At runtime, the client fetches the specified NinjaMock project and then renders all the pages as collections of NGUI controls, using a skin (basically an NGUI sprite atlas) set at compile time. Button links defined in the wireframe project are automatically live and functional in the client, and editable controls write their values to a global lookup table that can be accessed by code. All the programmer has to do is write code that checks the lookup table for changes values and reacts appropriately. For example, the music system monitors the "music" value in the table and uses it to set the volume. Any place the designer places a control named "music", that control now controls the volume level--not only can the designer move the control to different screens as the UI is refined, the control type itself can be changed without altering any of the program's functionality.

### Usage

Getting started with MagicUI isn't particularly difficult, but it's got a number of manual steps that I haven't bothered to automate yet. I suggest getting a test of the system working in a blank project, then exporting it as a package so that you can fire up new projects quickly.

1. **Install Prerequisites**. MagicUI depends on some other code, most notably the excellent NGUI.
2. **Copy Code**. Clone the repo and copy its code over to your project.
3. **Create a Skin**. Skins are how MagicUI organizes sprites and fonts. You can have multiple skins and switch between them at compile time, but you have to have at least one.
4. **Set Up the Singleton**. The MagicUI system is driven by a master class, MagicUIManager, that exists as a singleton component in your UI scene.
5. **Set Up the String Table** (optional). MagicUI treats the strings and text you enter into NinjaMock not as literal text but as control names. You can define a string table to translate these names into human-readable text, or for prototyping you can skip that and rely on savvy users to know what you're talking about.
6. **Wire Up Behavior** (optional). Basic functionality--jumping from page to page, remembering the values of controls--is provided automatically. To wire that functionality into your app you'll have to write code, but your designers can test and refine the layout without any involvement from you.

#### Install Prerequisites

1. **[NGUI](http://u3d.as/content/tasharen-entertainment/ngui-next-gen-ui/2vh)**. Doesn't everyone have this already?
2. **[JSONObject](https://www.assetstore.unity3d.com/#/content/710)**. There's several different JSON parsers out there (including one built into NGUI); I happen to like this one. It wouldn't be too onerous to replace it with your own favorite.
3. **My [Singleton](http://stebeegizmo.github.io/Singleton/) framework**. The MagicUI system is a local-scope singleton, and it relies on the DebugManager singleton for error reporting.
4. **A [NinjaMock](http://ninjamock.com/) account**. You can experiment for free, and a pro account (required for continued use) is on sale for $10 a month right now, down from the usual $15/month.

#### Copy Code

Clone this repo, then copy the .cs files to your project. In theory, as of 4.3.4 it's possible to symlink a folder to share code across multiple projects. I haven't had the courage to try that yet, but it's a feature I desperately want, so I'll explore it sometime in the next few weeks and update these instructions if it works.

#### Create a Skin

A MagicUI skin is an NGUI atlas with an additional component. Create the UIAtlas as normal, select the resulting prefab, then hit Add Component and find "MagicUI > Skin" in the popup. You'll then need to fill in some fields on the new component:
* **Definition**: This is a JSON file that tells MagicUI what frames from your atlas map should be used for various controls. More information on this file is under "skin.json" below.
* **Font Parameters**: Font handling is a work in progress in MagicUI, so there's some clutter here. The fields you need to worry about right now are Font, which should be a TTF or OTF font, and Default Color (if black isn't suitable for your backgrounds).
* **Target DPI**: You can specify a DPI value for each skin, and MagicUI will select the skin that best matches your device's resolution. Or you can just ignore this, because the default value will work fine.
* **Background**: Drag in a Texture2D that will be used as the static background for the UI. Right now, MagicUI will just tile or crop this texture as needed. If you want a more sophisticated background than that, leave this field blank and create the background in your scene manually.
* **Primary Color**: All controls will be set to this color. If your source art is in color rather than monochrome, just leave this field at its default value of white.

##### skin.json

A skin-definition JSON file looks like this:

```json
{"skin": {
	"copyrightSafe": true,
	"mapping": {
		"button": { "on": "button_enabled", "margin": {"x": 8, "y": 10} },
		"checkbox": { "checkedOn": "checkbox_checked_enabled", "uncheckedOn": "checkbox_unchecked_enabled" },
		"slider":  { "thumb": "slider_thumb", "bar": "slider_empty", "fg": "slider_full" },
		"textbox": { "on": "textbox_enabled", "fgInvert": true },
		"rectangle": { "on": "rectangle_enabled" },
		"image": { "missing": "missing_image" }
	}
}}
```

The "copyrightSafe" field lets you mark whether a particular skin is redistributable. It defaults to true, but if the skin's copyrightSafe is false, then production builds will terminate with an error message when they load the skin.

The "mapping" dictionary is of course the guts of the file:
* **button**: The "on" field specifies the frame to use as the background of buttons. You can optionally specify "margin" to make the label smaller than the background. If your art has extra detail that spills outside of the button's rectangular frame (such as scrollwork outside the edges of buttons in a steampunk UI), specify the size of that art with "chrome". Both "margin" and "chrome" specify the *total* padding, with left and right or top and bottom added together, and they can be specified for all controls (not just buttons).
* **checkbox**: The "checkedOn" field is for the checked frame, the "uncheckedOn" field for the unchecked one. MagicUI replaces the unchecked frame with the checked one; providing an option here of having the checked frame superimposed over the unchecked frame is in my backlog.
* **slider**: MagicUI draws the "bar" frame, then stretches the "fg" frame to the current value, then draws the "thumb" frame. NGUI doesn't like using sliced sprites for filled sliders, so sliders is an area that needs a lot more work right now.
* **textbox**: MagicUI draws the "on" frame and then puts the label on top of it. The "fgInvert" bool defaults to false; if true, the text label uses the "Alternate Color" set under Font Parameters.
* **rectangle**: Draws the "on" frame wherever you place a rectangle in NinjaMock.
* **image**: Whenever you specify an icon or an image in NinjaMock, MagicUI takes the URL and strips away all the path and file extension information to get a frame name. For example, if you have an icon for `https://cdn3.iconfinder.com/data/icons/free-social-icons/67/facebook_circle_color-64.png`, MagicUI will look in your atlas for a sprite named `facebook_circle_color-64`. If it can't find that frame, *and you're running a debug build*, it fetches the image live from the provided URL. If that fetch fails, or if you're running a production build, then MagicUI falls back to the "missing" frame specified here.

#### Set Up the Singleton

Use the NGUI menu to create a UI, add a MagicUIManager component to it, then fill in the necessary fields:

* **Layouts**: There are four groups of layouts--phone and tablet landscape, and phone and tablet portrait. Within each one, there are fields for aspect ratios ranging from 4x3 to 16x9. *You do not need to fill in every aspect ratio, and you **definitely** don't need to fill in every layout!* MagicUI will fall back as far as it needs to to find a layout. The variety of layout fields is just here to allow you to customize your UI beyond what the anchoring system already provides. For example, you might want to pack more smaller controls onto the screen on tablets, or you might want to add a sidebar when in landscape mode on wide screens.
  As for what actually goes into these fields, it's the URL NinjaMock gives you when you share a project. When you hit the "Share" button in NinjaMock, you get a popup that gives you an URL that looks something like `http://ninjamock.com/s/dpmvcu`. Paste that value into one or more of the aspect fields in one or more of the MagicUIManager's layout groups.
* **Skins**: This is just an array of MagicUISkin objects. Drag and drop the skin prefab here. MagicUI will pick the skin that best matches the device's DPI.
* **String Table**: This is a simple JSON file--see "strings.json" below.
* **API**: Create an empty GameObject and attach a NinjaMockAPI component to it, then drag that GameObject into this field. And yes, this is silly and cumbersome; having the manager automatically create the API is high in my backlog.
* **Background**: Create an NGUI UITexture object, make it a child of your UI Root and drag it into this field. Or if you want to handle the background yourself (either because you want more complex art for it than just a tile, or because you are overlaying this UI atop a 3D environment), just leave this field blank.

##### strings.json

The string table JSON looks like this:

```json
{"languages": {

"English": {
	"page1textblock": "This is a field with a large amount of text. The text has punctuation and will occupy multiple lines, but I'm not sure yet what will happen to the font.",
	"RandomText": "© 2014 Gizmocracy LLC",
	"Account Info": "Account Info",
	"Your Avatar": "Your Avatar",
	"the_text_field": "Say something..."
},

"German": {
	"page1textblock": "Dies ist ein Bereich mit einer großen Textmenge. Der Text hat Zeichensetzung und wird mehrere Zeilen einnehmen, aber ich bin noch nicht sicher, was mit der Schrift geschieht.",
	"RandomText": "© 2014 Gizmocracy LLC",
	"Account Info": "Kundendaten",
	"Your Avatar": "Ihr Avatar",
	"the_text_field": "Sagen Sie etwas..."
}

}}
```

Each key in the "languages" field is just the language name, as supplied by Unity's `Application.systemLanguage` enum. (In a rational world, this would be a standard IETF language tag like en-US or de-DE, but that's not where we live.) Within a language's dictionary, the keys are the literal text from the NinjaMock controls. If MagicUI can't find a matching value for a text key, it just displays the key in quotation marks.

#### Wire Up Behavior

At this point, having performed all the above steps, you can press play and see your UI come to life. If you've created multiple pages in NinjaMock and used links to navigate between them, those links are automatically honored in MagicUI.

Changes made to controls in the UI are written to a global lookup table, which your code then accesses through the MagicUIManager. Sliders set a value from 0 (leftmost) to 1 (rightmost). Checkboxes are true or false. Text fields contain their text. Buttons are true as long as they're pressed, then revert to false. To access a value, call `MagicUIManager.Instance.ReadX(key)`, where `X` is `String`, `Bool`, `Float` or `Int`.

For example, let's say you want a button in your UI that reloads the scene from scratch, so that any changes you made in NinjaMock get reloaded and displayed. (This is a very common scenario during development, so much so that I really ought to just wire it up as default behavior.) To do that, create a button somewhere in your UI with the label "Refresh", then attach this component to an object somewhere in your scene:

```csharp
using UnityEngine;
using System.Collections;

public class MagicRefresh : MonoBehaviour
{
	protected bool _reloading = false;

	void Update ()
	{
		if (!_reloading)
		{
			if (MagicUIManager.Instance.ReadBool("Refresh"))
			{
				_reloading = true;
				Application.LoadLevel(0);
			}
		}
	}
}
```

A future version of the system will allow you to subscribe to update events so that you don't have to poll.

### Next Steps

Ugh, too many to list right now.

