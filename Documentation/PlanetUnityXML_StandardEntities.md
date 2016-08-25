[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Standard Entities

----

## &lt;GameObject/&gt;
### C# Class - PUGameObject

The base class of most standard entities, the GameObject entity will create an empty Unity Game Object. When used directly it is generally used for building hierarchical structures in your UI as a lightweight container to hold other entities.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| position | Vector3 | The position of the RectTranform |
| size | Vector2 | The width and height of the RectTransform |
| bounds | Vector4 | Convenience attribute combines positions and size as x,y,width,height |
| pivot | Vector2 | The pivot of the RectTransform |
| anchor | string | One of the common anchor values (see below) |
| title | string | Sets the name of the Game Object, also allows linking to C# code |
| active | boolean | Whether this Game Object is initialized as active or not |

**Valid values for anchor**  
"top,left", "top,center", "top,right", "top,stretch", "middle,left", "middle,center", "middle,right", "middle,stretch", "bottom,left", "bottom,center", "bottom,right", "bottom,stretch", "stretch,left", "stretch,center", "stretch,right", "stretch,stretch"

----

## &lt;Canvas/&gt;
### C# Class - PUCanvas : PUGameObject

Generates a standard Unity Canvas

| Attribute | Type | Description              |
|----------|--------------|---------------|
| renderMode | CanvasRenderMode | Set the rendermode of the canvas to "ScreenSpaceOverlay", "ScreenSpaceCamera" or "WorldSpace" |
| planeDistance | float | The distance of the canvas plane from the camera when render mode is set to ScreenSpaceCamera |
| pixelPerfect | bool | Convenience attribute combines positions and size as x,y,width,height |

----

## &lt;Color/&gt;
### C# Class - PUColor : PUGameObject

A colored rectangle the size of the RectTransform

| Attribute | Type | Description              |
|----------|--------------|---------------|
| color | Color | The color of the rectangle |

----

## &lt;ColorButton/&gt;
### C# Class - PUColorButton : PUColor

A colored rectangle the size of the RectTransform which can be pressed like a button

| Attribute | Type | Description              |
|----------|--------------|---------------|
| onTouchUp | string | Notification name to be sent when the button is clicked |
| pressedColor | Color | The color of the button in its pressed state |

----

## &lt;ClearButton/&gt;
### C# Class - PUClearButton : PUGameObject

A clear rectangle the size of the RectTransform which can be pressed like a button; note that this is more performant than using a ColorButton with a clear color because ClearButton will have no impact on rendering performance.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| onTouchUp | string | Notification name to be sent when the button is clicked |

----

## &lt;RawImage/&gt;
### C# Class - PURawImage : PUGameObject

An image the size of the RectTransform

| Attribute | Type | Description              |
|----------|--------------|---------------|
| resourcePath | string | The path to the texture in Assets/Resources to load |
| color | Color | The color to tint the raw image |


----

## &lt;Image/&gt;
### C# Class - PUImage : PUGameObject

A sprite set to the size of the RectTransform

| Attribute | Type | Description              |
|----------|--------------|---------------|
| resourcePath | string | The path to the sprite in Assets/Resources to load |
| color | Color | The color to tint the image |
| type | ImageType | "simple", "filled", "sliced", "tiled", "aspectFilled" |

Note: at the time of this writing, you cannot load a single sprite with C# code, you need to load a whole directory of sprites and then search through it for the sprite you want. This can have severe memory and performance consquence, especially on mobile devices. To work around this, by default PUImage will load all resources as textures, not sprites. If you need to use actual sprites (and there are many reasons why you'd want to), there are to paths:

1. You can call LoadSprite() from C# code to force the loading of a sprite on a specific PUImage
2. You can override Planet Unity's default behaviour and have it load sprites for all PUImage by setting **PlanetUnityOverride.ForceActualSprites = true;**  Please note you should set Planet Unity overrides in an Awake method to ensure they are set before Planet Unity loads UI


----

## &lt;ImageButton/&gt;
### C# Class - PUImageButton : PUImage

An image the size of the RectTransform which can be pressed like a button

| Attribute | Type | Description              |
|----------|--------------|---------------|
| pressedResourcePath | string | The path to a texture in Assets/Resources to use when the button is pressed |
| highlightedResourcePath | string | The path to a texture in Assets/Resources to use when the button is highlighted |
| disabledResourcePath | string | The path to a texture in Assets/Resources to use when the button is disabled |
| onTouchUp | string | Notification name to be sent when the button is clicked |
| onTouchDown | string | Notification name to be sent when the button is pressed |

----

## &lt;ScrollRect/&gt;
### C# Class - PUScrollRect : PUGameObject

A container object that allows you to scroll the children placed in it

| Attribute | Type | Description              |
|----------|--------------|---------------|
| inertia | boolean | Should the scroll rect animate smoothly |
| horizontal | boolean | Should you be able to scroll horizontally |
| vertical | boolean | Should you be able to scroll vertically |
| scrollWheelSensitivity | float | The sensitivity to scroll wheel and trackpad events |


----

## &lt;Text/&gt;
### C# Class - PUText : PUGameObject

Text displayed within the bounds of the RectTransform. Please note you might want to consider using [PUTMPro](https://github.com/SmallPlanetUnity/PUTMPro) and [TextMesh Pro](https://www.assetstore.unity3d.com/en/#!/content/17662) instead.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| font | string | The resource path to the font |
| fontSize | int | The point size of the text |
| fontStyle | FontStyle |"normal", "bold", "italic", "boldAndItalic" |
| fontColor | Color | The color of the text |
| lineSpacing | float | Line spacing specified as a factor of the font line height |
| alignment | TextAlignment | "upperLeft", "upperCenter", "upperRight", "middleLeft", "middleCenter", "middleRight", "lowerLeft", "lowerCenter", "lowerRight" |
| value | string | The content of the text |
| sizeToFit | boolean | Resize the font to size the size of the RectTransform |
| maxFontSize | int | Maximum font size to use with the sizeToFit option is specified |
| minFontSize | int | Minimum font size to use with the sizeToFit option is specified |
| vOverflow | boolean | Should the text overflow the RectTransform area vertically |
| hOverflow | boolean | Should the text overflow the RectTransform area horizontally |
| onLinkClick | string | Notification sent when a link is clicked |



----

## &lt;Text/&gt;
### C# Class - PUText : PUGameObject

Text displayed within the bounds of the RectTransform. Please note you might want to consider using [PUTMPro](https://github.com/SmallPlanetUnity/PUTMPro) and [TextMesh Pro](https://www.assetstore.unity3d.com/en/#!/content/17662) instead.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| font | string | The resource path to the font |
| fontSize | int | The point size of the text |
| fontStyle | FontStyle |"normal", "bold", "italic", "boldAndItalic" |
| fontColor | Color | The color of the text |
| lineSpacing | float | Line spacing specified as a factor of the font line height |
| alignment | TextAlignment | "upperLeft", "upperCenter", "upperRight", "middleLeft", "middleCenter", "middleRight", "lowerLeft", "lowerCenter", "lowerRight" |
| value | string | The content of the text |
| sizeToFit | boolean | Resize the font to size the size of the RectTransform |
| maxFontSize | int | Maximum font size to use with the sizeToFit option is specified |
| minFontSize | int | Minimum font size to use with the sizeToFit option is specified |
| vOverflow | boolean | Should the text overflow the RectTransform area vertically |
| hOverflow | boolean | Should the text overflow the RectTransform area horizontally |
| onLinkClick | string | Notification sent when a link is clicked |



----

## &lt;TextButton/&gt;
### C# Class - PUTextButton : PUText

Text displayed within the bounds of the RectTransform which can be clicked like a button

| Attribute | Type | Description              |
|----------|--------------|---------------|
| onTouchUp | string | Notification name to be sent when the button is clicked |


----

## &lt;InputField/&gt;
### C# Class - PUInputField : PUText

Text displayed within the bounds of the RectTransform which can be clicked like a button

| Attribute | Type | Description              |
|----------|--------------|---------------|
| onValueChanged | string | Notification name to be sent when the input field is submitted |
| placeholder | string | Text content to display if the input field is empty |
| limit | int | Number of character to limit the content of the input field to |
| contentType | InputFieldContentType | "standard", "autocorrected", "integer", "number", "alphanumeric", "name", "email", "password", "pin", "custom" |
| lineType | InputFieldLineType | "single", "multiSubmit", "multiNewline" |
| selectionColor | Color | The color of the input field's highlights |

----

## &lt;Slider/&gt;
### C# Class - PUSlider : PUImage

A basic slider control

| Attribute | Type | Description              |
|----------|--------------|---------------|
| handleResourcePath | string | The path to the texture in Assets/Resources to use for the handle of the slider |
| handleSize | Vector2 | The width and height of the handle of the slider |
| fillResourcePath | string | The path to the texture in Assets/Resources to use when filling up the slider |
| onValueChanged | string | Notification name to be sent when the slider value is changed |
| minValue | float | The minimum value of the slider |
| maxValue | float | The maximum value of the slider |
| direction | SliderDirection | "LeftToRight", "RightToLeft", "BottomToTop", "TopToBottom" |

----

## &lt;Table/&gt;
### C# Class - PUTable : PUScrollRect

Present a scrollable list of items to the user. PUTable uses a simple algorithm to place cell from left-to-right from top-to-bottom, and contains a simple API for use. PUTable relies on loading and laying out all of the cells at the same time, so is not recommended for tables with lots of cells. Use PUSimpleTable for tables with lots of content.

| Attribute | Type | Description              |
|----------|--------------|---------------|
|  |  |  |

----

## &lt;GridTable/&gt;
### C# Class - PU GridTable : PUTable

A subclass of PUTable which uses the MAXRECTS algorithm to tighly fit cells of different sizes into the requested scroll area. Very useful for things like displaying multiple images. Just like PUTable, PUGridTable relies on loading and laying out all of the cells at the same time, so is not recommended for tables with lots of cells.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| heuristic | GridTableHeuristic | "RectBestShortSideFit", "RectBestLongSideFit", "RectBestAreaFit", "RectBottomLeftRule", "RectContactPointRule" |
| expandToFill | boolean | Resize the cells to reduce as much empty space as possible |

----

## &lt;GridLayoutGroup/&gt;
### C# Class - PUGridLayoutGroup : PUGameObject

The grid layout group will organize the child entities in a grid based on the parameters supplied

| Attribute | Type | Description              |
|----------|--------------|---------------|
| cellSize | Vector2 | The width and height of each cell |
| spacing | Vector2 | The x and y spacing between each cell |
| startCorner | GridLayoutStartCorner | "upperLeft", "upperRight", "lowerLeft", "lowerRight" |
| startAxis | GridLayoutStartAxis | "horizontal", "vertical" |
| childAlignment | GridLayoutChildAlignment | "upperLeft", "upperCenter", "upperRight", "middleLeft", "middleCenter", "middleRight", "lowerLeft", "lowerCenter", "lowerRight" |
| fixedRows | int | Constrain to the number of rows specified |
| fixedColumns | int | Constrain to the number of columns specified |

----


## &lt;VerticalLayoutGroup/&gt;
### C# Class - PUVerticalLayoutGroup : PUGameObject

A layout group which will organize the child entities vertically

| Attribute | Type | Description              |
|----------|--------------|---------------|
| spacing | float | The amount of spacing between each cell |
| padding | Vector4 | The amount of internal padding for each cell |
| childAlignment | GridLayoutChildAlignment | "upperLeft", "upperCenter", "upperRight", "middleLeft", "middleCenter", "middleRight", "lowerLeft", "lowerCenter", "lowerRight" |

----

## &lt;HorizontalLayoutGroup/&gt;
### C# Class - PUHorizontalLayoutGroup : PUGameObject

A layout group which will organize the child entities horizontally

| Attribute | Type | Description              |
|----------|--------------|---------------|
| spacing | float | The amount of spacing between each cell |
| padding | Vector4 | The amount of internal padding for each cell |
| childAlignment | GridLayoutChildAlignment | "upperLeft", "upperCenter", "upperRight", "middleLeft", "middleCenter", "middleRight", "lowerLeft", "lowerCenter", "lowerRight" |

----

## &lt;AspectFit/&gt;
### C# Class - PUAspectFit : PUGameObject

Use the Unity AspectRatioFitter component to resize children of this entity.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| contentSize | Vector2 | The size of the content to fit |
| mode | AspectFitMode | "None", "WidthControlsHeight", "HeightControlsWidth", "FitInParent", "EnvelopeParent" |

----

## &lt;Code/&gt;
### C# Class - PUCode : PUGameObject

The code entity will instantiate an instance of the MonoBehaviour class specified, automatically fill out any public member variables which are the same type and title of other entities, and subscribe to any notifications specified

| Attribute | Type | Description              |
|----------|--------------|---------------|
| class | string | The name of the MonoBehaviour class |
| singleton | boolean | Should this instance be kept around and treated like a singleton |

----

## &lt;Notification/&gt;
### C# Class - PUNotification : PUObject

Used as a child of the Code entity, provides a list of notifications the code entity should subscribe to

| Attribute | Type | Description              |
|----------|--------------|---------------|
| name | string | The name of the notification the code entity should subscribe to |

----

## &lt;Movie/&gt;
### C# Class - PUMovie : PUGameObject
#### TO BE REMOVED SOON

An entity to load and play a MovieTexture. This entity is marked for possible removal from Planet Unity due to the lack of MovieTexture support across all Unity platforms.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| looping | boolean | Should this movie loop |
| resourcePath | string | The path to the movie to play |
| color | Color | The color to tint the move |

----

## &lt;Prefab/&gt;
### C# Class - PUPrefab : PUGameObject
#### TO BE REMOVED SOON

Load a prefab and set it as my child. This entity is marked for possible removal from Planet Unity. Using prefabs for UI elements is against the purpose of Planet Unity, and while there may be good reasons to use this entity it is by far the exception rather than the rule. In addition, those who have this need usually end up doing this in C# code and/or in some other method than with the PUPrefab entity.

| Attribute | Type | Description              |
|----------|--------------|---------------|
| name | string | Path to the prefab to load |

----
