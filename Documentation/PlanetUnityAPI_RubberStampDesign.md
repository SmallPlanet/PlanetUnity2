[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Rubber Stamp Design

Planet Unity entities are only loosely coupled with the generated Unity Game Objects and Components; setting a value on an entity **after** the Unity Game Object has been generated will not affect the generated object.


![](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/Images/rubber_stamps.png)

## icon.color or icon.image.color?

When you are using the Planet Unity API to create UI from code, it is important to keep in mind that Planet Unity follows a "rubber stamp" design. What this means is that there is a two step process when creating the UI. The first step is generating the "rubber stamp" hierarchy of Planet Unity entities (PUImage, PUColor, etc) and the second step is telling that stamp to generate the Unity Game Objects and associated components.

If you want to set attributes on the "rubber stamp" prior to generating the UI, then you should call the accessor variables on the stamp ( ie `icon.color = Color.red;` ). If you want to change the color of the generated UI (ie the "stamp"), then you call into the generated UI component of that stamp (ie `icon.image.color = Color.red;`).

When creating the UI from code, calling `LoadIntoPUGameObject()` or `LoadIntoGameObject()` "stamps" out the UI; as a general rule of thumb, prior to that call you want to change the attributes of the "rubber stamp" and after that call you want to change the attributes of the generated UI components.