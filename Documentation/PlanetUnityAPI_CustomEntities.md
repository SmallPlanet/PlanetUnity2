[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Custom Entities

![](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/Images/entitydrivendevelopment.png)

## Entity Driven Development

By far the coolest feature of Planet Unity for the advanced user is the ability to quickly and easily create custom entities for your XML.  This allows us to embrace **entitiy driven development** as a model to make our application cleaner and easier to develop.

Take the example above: in this example, we have a simple twitter login button which, if the user is already logged in, shows a play button instead. This is pretty straight forward to accomplish without custom entities; a couple of image buttons (complete with labels and icons) in a switcher and a code controller to drive it. However, it is possible that this is not the only place in our app we need this exact twitter button (perhaps we allow you to play on a guest account first and show the twitter loging in the settings screen). While we could copy & paste this whole XML elsewhere, by embracing **entitiy driven development** we can recognize that this "twitter/play" button is a whole and unique entity by itself, and we can create a custom entity just for it.

## Creating a custom entity

Creating a custom entity is simple; just create a new C# class, subclass of PUGameObject (you can use another Planet Unity entity if you want), and prefix the name of the class with "PU". So in our example above, we would make a class named **PUTwitterLogin**, and in the XML we would use the entity **&lt;TwitterLogin /&gt;**.

Once we have our class properly named, we can override any of several methods to perform our UI generation. These methods are inherently tied to the XML loading process and as such are all gaxb methods.

| gaxb method |  reason to override  |
|----|----|
| gaxb_load |  Called right after the object is instantiated, before gaxb_init is called. Generally you do not need to override this method.  |
| gaxb_init |  Responsible for creating the base Unity Game Object. Override this method to set any default values you want prior to the XML attributes being loaded (for example, title = "TwitterLogin"; will ensure your entity always has a title even if the XML does not set one). |
| gaxb_final | XML attributes get parsed and values set, this is the final step in the creation of the Planet Unity entity. Override this method to gain access to the XmlReader object, allowing your custom entity to accept custom XML attributes. |
| gaxb_complete | Called when the Planet Unity entity, and all of its children, have been created and loaded. Override this method to create your custom user interface for this entity. Doing so here ensures that all of this entity, and all of its children, have already been loaded and are available for your to access |
| gaxb_unload | Called when your entity is being removed. Override this method to perform any cleanup your entity may need to perform |

When overriding the gaxb methods, be certain to call their base methods.

Let's look at a few examples.

### PURedColor.cs
In this example, we want to provide a simple mechanism for drawing a red square. For this we subclass PUColor, and override gaxb_init() to set the default of the color attribute to red.

````
using UnityEngine;

public class PURedColor : PUColor {
	public override void gaxb_init() {
		color = Color.red;
		base.gaxb_init();
	}
}
````

````
<RedColor />
````

### PUTwitterLogin.cs
In a more complex example, let's do the twitter login button we theorized above.

````
using UnityEngine;

public class PUTwitterLogin : PUImageButton {
	public override void gaxb_init() {
		title="<TwitterLogin/>";
		resourcePath = "Images/TwitterButtonBackground";
		base.gaxb_init();
	}

	public override void gaxb_complete() {
		base.gaxb_complete ();

		if (IsLoggedInAlready ()) {
			SetAsTwitterButton ();
		} else {
			SetAsPlayButton ();
		}
	}

	private bool _isLoggedIntoTwitter = false;
	private bool IsLoggedInAlready() {
		// TODO: Perform real twitter logic here
		return _isLoggedIntoTwitter;
	}

	public void SetAsTwitterButton() {

		unloadAllChildren ();

		PUText label = new PUText ();
		label = new PUTextButton ();
		label.font = "Fonts/PressStart2P";
		label.value = "LOGIN";
		label.SetFrame (0, 0, 0, 0, 0, 0, "stretch,stretch");
		label.LoadIntoPUGameObject (this);
		label.SetStretchStretch (10, 10, 10, 100);

		PUImage twitterIcon = new PUImage ();
		twitterIcon.resourcePath = "Images/TwitterIcon";
		twitterIcon.SetFrame (-10, 0, 80, 80, 1.0f, 0.5f, "right,center");
		twitterIcon.LoadIntoPUGameObject (this);

		button.onClick.AddListener (() => {

			// TODO: Perform real twitter login here
			_isLoggedIntoTwitter = true;

			SetAsPlayButton();
		});
	}

	public void SetAsPlayButton() {

		unloadAllChildren ();

		PUText label = new PUText ();
		label = new PUTextButton ();
		label.font = "Fonts/PressStart2P";
		label.value = "PLAY";
		label.SetFrame (0, 0, 378, 100, 0.5f, 0.5f, "stretch,stretch");
		label.LoadIntoPUGameObject (this);
		label.SetStretchStretch (10, 10, 10, 10);
	}
}
````

````
<TwitterLogin bounds="@eval(0,100,300,80)" pivot="0.5,0" anchor="bottom,center" />
````