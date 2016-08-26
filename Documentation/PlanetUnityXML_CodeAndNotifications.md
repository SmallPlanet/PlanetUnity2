[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Code and Notifications

## Controllers for your Views

Being able to create user interfaces quickly and easily is great, but a user interface is nothing without controller logic to make it do what its supposed to do! Planet Unity provides several mechanisms for making communication to and from your entities easy.

### Adding your MonoBehaviour to your Planet Unity XML

As you might imagine, your controller code will be a C# class that inherits from MonoBehaviour. Since Planet Unity user interfaces are generated at runtime by XML, it makes sense that your MonoBehaviour will also get instantiated at runtime. To do this is trivial; let's suppose have a C# class called **MyControllerClass**.  We will focus on what this class wants to do with our UI in a bit. For now we just want to relate this controller to the entities in our XML. To do that, you would use the **Code** entity and place it at the end of your XML.

````
<Image title="MyImage" resourcePath="image" />
<Button title="MyButton" onTouchUp="UserPressedButton" />

<Code class="MyControllerClass" />
````
Easy enough. Let's suppose this class will need to respond to the MyButton button when it is pressed and then change the color of the MyImage image.

### Responding to user interactions

Planet Unity uses a publish/subscribe class very similar to NotificationCenter on iOS; it resides in the NotificationCenter.cs class. Entities such as buttons will post notifications to the NotificationCenter, and anyone can listen and respond to those notifications. Unless overridden, notifications sent by Planet Unity entities will be scoped to the root element of the XML (meaning if you have two separate XML's loaded side-by-side, notifications from one loaded XML will not interfere with notifications in the other XML).

You can have your controller code subscribe to notifications using one of two methods.  The first method would be to add a **Notification** entity inside of your **Code** entity.

````
<Code class="MyControllerClass">
	<Notification name="UserPressedButton" />
</Code>
````

The name of the notification must be exact; in our case, the button will send the "UserPressedButton" notification onTouchUp (when the user presses and then releases the button).  In our C# class, we then define a method with the exact same name as the notification. Please note that some entities will pass arguments to the method using a Hashtable; the exact arguments depend on the entity in question.

````
public void UserPressedButton(Hashtable args) {
	// Do something when the user pressed the button
}
````

The second method for listening to notifications would be to subscribe to the notification directly in the controller code.

````
public PUImage MyImage;

public void Start() {
	NotificationCenter.addObserver(this, "UserPressedButton", MyImage.Scope(), (args, name) => {
		// Do something when the user pressed the button
	});
}
````

Note that the third parameter to addObserver() is the scope of the notification; we need to call Scope() on a valid PU entity in that XML to get the scope the notifications will send in.  More specifics on that are in the next section.

In certain circumstances you might want to publish and subscribe to notifications in a global scope; in our example from earlier, if you have two XMLs loaded side-by-side you might want XML in one to respond to a notification sent in the other.  To do this all you need to do is prepend "GLOBAL::" to the name of the notification (Note that the name of the method being called in your controller class with still be the same as before).  So in our XML example, that would be:

````
<Image title="MyImage" resourcePath="image" />
<Button title="MyButton" onTouchUp="GLOBAL::UserPressedButton" />

<Code class="MyControllerClass">
	<Notification name="GLOBAL::UserPressedButton" />
</Code>
````

### Accessing PU entities from your Controller

Gaining access to entities loaded from XML is easy; simply add a public variable to your class of the same type and the same name as the title of your entity.  In our example, that would be the PUImage with the title **MyImage**.

````
<Image title="MyImage" resourcePath="image" />
````

In our class, we simply have a public member variable.

````
public class MyControllerClass : MonoBehaviour {
	public PUImage MyImage;
}
````

Its as simple as that! To complete our example, when the user presses the button we want to change the color of the image.

````
public class MyControllerClass : MonoBehaviour {
	public PUImage MyImage;
	
	public void UserPressedButton(Hashtable args) {
		// set the color of MyImage to red
		MyImage.image.color = Color.red;
	}
}
````

Please note that although the PUImage MyImage has a color property (ie MyImage.color), we do not set that; we drill down to the Image component and set it there. This is very important, as all Planet Unity entities follow the [Rubber Stamp Design](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/PlanetUnityAPI_RubberStampDesign.md) methodology and setting the color on the "rubber stamp" has no affect on the stamp already created.