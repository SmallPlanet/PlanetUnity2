[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2#documentation)

# Adding Planet Unity (PU) to Your Unity Project

If you are using git as source control, then the recommended method for putting Planet Unity into your project is with git submodules.  Using a git submodule will allow you to track, manage and update to changes made to Planet Unity.

[Git Submodule Tutorial](https://chrisjean.com/git-submodules-adding-using-removing-and-updating/)

A simpler method to adding PU to your project, simply [download the project as a zip](https://github.com/SmallPlanetUnity/PlanetUnity2/archive/master.zip) from github, extract the zip, and place the contents in your projects Assets folder.


#Sample XML

Here is some sample XML to get you started with the integration steps below.  Copy the code into **Assets/Resources/PlanetUnityTest/PlanetUnityTest.xml**, you can then put **PlanetUnityTest/PlanetUnityTest** in the Xml Path fields mentioned below.  This XML will generate a canvas which renders using the screen space overlay, and in that canvas will be a red square which will stretch to fill the entire canvas.

~~~~
<?xml version="1.0" encoding="utf-8" ?>
<Canvas renderMode="ScreenSpaceOverlay" xmlns="http://schema.smallplanet.com/PlanetUnity2.xsd" >
	<Color color="#FF0000FF" anchor="stretch,stretch" />
</Canvas>
~~~~


# Standard Integration

The standard way to integrate Planet Unity in an existing scene is by using the **Planet Unity Game Object** component.

1. Create a new Game Object, optionally name it "Planet Unity"
2. Add the "Planet Unity Game Object" component
3. In the Xml Path field, enter the path to your Planet Unity XML (note that the file should be located in your **Assets/Resources** directory, and that the path **should not contain** the .xml extension)

That's it! When the scene is run, the XML file will be loaded and the associated Unity UI components will be added to a dynamically created canvas called "PlanetUnityContainer".

By using the standard integration, you can get access to the loaded components through the PlanetUnityGameObject class.  For example, you can access the main canvas by calling ```PlanetUnityGameObject.MainCanvas()```.

# Embedded Integration

It is possible to load multiple PU XML files in the same scene.  While there can only be one "main" xml (which can be accessed through the PlanetUnityGameObject component), you can optionally "embed" other bits of PU generated UI anywhere you want.

1. Choose a Game Object you want Planet Unity to load UI into
2. Add the "Planet Unity Embed" component
3. Specify the Xml Path to the XML file you'd like to load

There are many reasons why you might want to do this, such as:

1. You would like to have two completely separate canvases (perhaps rendering to two separate cameras)
2. You want to embed PU XML into an already existing UI

# Programmatic Integration

One of the strengths of Planet Unity is that everything can be easily generated through code.  As such, you can generate the whole UI easily though code, or you can dynamically load whole XML UI through code.  To load an entire PU XML by code, simply call ```PlanetUnityGameObject.LoadXML (pathToXmlFile, ParentGameObjectToLoadUIInto);```


