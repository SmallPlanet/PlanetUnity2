[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Overview

By now you've probably guessed it, but the main goal for Planet Unity is to allow developers and designers to create user interfaces using XML.  This might seem contradictory to the purpose of Unity and if you're of that persuasion I will not try to convince you otherwise. Here is a simple list the many benefits we see by using Planet Unity and you can decide if its worth your while.

* **It's kind of like working in HTML/WPF/Insert your XML-like tech here**  
User interfaces being created from XML-ish languages is a well provden path in the industry.  Behemoths like HTML, WPF, even Androids XML-based layouts all make use of the many benefits this medium provides to development. One of the main advantages is that many designers already have some experience and understand of HTML, which allows them to dive in an modify Planet Unity XML with ease.

* **Custom entities will change the way you think of XML driven UI**  
Unlike other XML driven UI technologies, one of the core tennets of Planet Unity development is making it incredibly simple for developers to create new entities, entities which are often tied very specifically to the application being developed. We call this **entitiy driven development** and it may very well change the way you think about XML driving UI.  

 So what is entity driven development? Consider for a moment that most parts of a user interface are actually a combination of smaller pieces of user interface. For example, a button might have an image, a table, an optional icon, and additional custom behaviour based upon some current model state or logic in the app. A typical XML driven UI for this might look something like this in Planet Unity:
 
 ````
 	<ImageButton title="LoginButton" resourcePath="Shared/Images/UI/loginBtn" bounds="@eval(4,-70*2,64,64)" anchor="top,left" color="#FFFFFF33" onTouchUp="UserTappedLoginButton">
		<Image active="false" resourcePath="Shared/Images/UI/twitterIcon" pivot="1,0.5" bounds="0,0,48,48" anchor="middle,right" />
		<Text value="@localization(MainMenu.Login)" fontSize="28" fontColor="#FFFFFFFF" pivot="0.5,0.5" anchor="middle,center" />
	</ImageButton>
	<Code class="LoginButtonController">
		<Notification name="UserTappedLoginButton" />
	</Code>

 ````
 Pretty standard, right?  We have a image button, an icon, a text label title, and some logic controller which listens to when the button is pressed and can access the button to change its state based upon some variable or another.  Under the strictures of entity driven development, we look at this functionally as a whole; instead of it being an image button, with an icon and a text label and some logic, we identify it for what it is, the Login Button. Having identified this combination of UI and logic as the Login Button, we go and create a custom entity for it.  The XML is then changed to use this entity, and the XML becomes:
 
  ````
  <LoginButton bounds="@eval(4,-70*2,64,64)" anchor="top,left" />
  ````
  That's it! This new entity knows how to create itself, how to update itself, and is aware of the model / logical states of the application. By identifying these time saving custom entities, you not only have cleaner, easier to read XML, you're reusing code, reducing bugs, and increasing the power of your applications UI XML. And creating custom entities is a snap!

* **Live updating in the Unity Editor**  
Keep the editor running in Play mode while working on your Planet Unity XML; whenever you save the XML file, Planet Unity will reload the file and update the UI to incorporate your changes. Need to make a few tweaks in the middle of a live run? Just do it. And unlike working with a Unity scene in play mode, your changes will persist outside of that play session.

* **Easy API for generating Unity UI from C# code**  
While the Unity UI game object + component system is very powerful, it is also very verbose! Creating an image button from code, for example, can easily be a 20-30 line affair. Since creating Unity UI from code is Planet Unity's bread-and-butter, all of the entity APIs are available for direct code use and makes generating UI from code super simple.  Here's an image button example done in Planet Unity:

 ````
PUImageButton btn = new PUImageButton();
btn.resourcePath = "play_btn";
btn.SetFrame (0, 0, 200, 64, 0.5f, 0.5f, "middle,center");
btn.LoadIntoPUGameObject (parent);
````

* **@eval(), @dpi() and @localization()**  
Planet Unity provides several time saving functions which can be used anywhere in your Planet Unity XML.  For example, let's say we want to create a button which is half the width and height of its parent, but we don't know the width or the height of the parent?  We can use @eval function to do this easily ```size="@eval(w/2,h/2)"```.  Or if you want to include a localization look up to set the title of a button, you could use ```value=@localization(MainMenu.Play)```. Finally, @dpi is just like @eval but can be used to define your sizes in inches instead of pixels, which is an incredibly simply way to create interfaces with actual, physical dimensions.

* **Full project text searching**  
Unity adds all of your Planet Unity XML files to the generated C# projects, making it trivial to do project wide text searches for anything UI related. Needs to find all instances of a particular button scattered throughout the whole project? Simple, fast, effective.

* **Source control just works**  
If you've worked on multi-person projects with Unity, then you probably already know what we're talking about. When multiple people are modifying the same Unity scene, things can get messy very quickly when your favorite source control tries to glom it all together. Planet Unity XML is a human readable text format which, on the rare occassion your source control throws up a merge conflict, can be modified and corrected manually.



## PlanetUnity.xsd + gaxb

Internally all of the [Standard Entities](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/PlanetUnityXML_StandardEntities.md) supported by Planet Unity are autogenerated using [gaxb](https://github.com/SmallPlanet/gaxb) from a single XML schema file.  This schema file and gaxb tool is located in .Support/Tools. As a user of Planet Unity you shouldn't need to delve into the .Support folder or change the PlanetUnity2.xsd schema file.  However, if you are familiar with XML schema you can peruse the schema yourself and see an overview of all of the entity classes, their attributes, and types.

Where our use of gaxb does come into play for the intermediate Planet Unity developer is when you are practicing **entity driven development**, where you can override many functions in the XML loading pipeline to specify entity behavour as you need it.