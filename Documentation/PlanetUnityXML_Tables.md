[Table of Contents](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/TableOfContents.md)

# Tables

Planet Unity includes several robust table implementations, a welcome addition to anyone generating user interfaces with Unity UI! Each table of the tables are similar in setup and usage, but each are slightly different in their ideal use case.

Here are the pros/cons in summary:

|   | Section Header | Variable Cell Size | Dynamic Cell Resizing | Predictable Cell Ordering | High Performance | Direction
|---|---|---|---|---|---|---|
|  PUTable  | Yes | Yes | Yes | Yes | No | Any |
|  PUGridTable  | Yes | Yes | Yes | No | No | Any |
|  PUSimpleTable  | Yes | No | No | Yes | Yes | Vertical |


## PUTable

![](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/Images/PUTable.gif)

PUTable is the most robust table class in Planet Unity. It allows for cells of multiple sizes and allows for dynamically changing cell sizes at runtime. PUTable uses a simple left-to-right, top-to-bottom approach to laying out cells, supports multiple sections with section headers, and will layout the cells in the order provided. To support all of this, PUTable needs to be able to load and layout all cells of the table at the same time, and as such does not scale to many cells very well.

## PUGridTable

![](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/Images/PUGridTable.gif)

PUGridTable is a subclass of PUTable with a custom layout algorithm, specifically it uses the MAXRECTS algorithm to attempt to tightly pack the cells to minimize wasted space. As such, the vertical ordering of the cells cannot be preserved. PUGridTable is useful when you have a lot of cells of drastically varying sizes and the visible ordering of those cells are not important.

## PUSimpleTable

![](https://github.com/SmallPlanetUnity/PlanetUnity2/blob/master/Documentation/Images/PUSimpleTable.gif)

PUSimpleTable is the ideal table class to use, especially for mobile. It supports lazy loading of cells, it only layouts and loads the cells which are visible, and is highly performant. However, the performance improvements come with more restrictions. PUSimpleTable cells must all be of the same size (not including section headers, which all need to be the same size but can be a different size from other cells), and only supports vertical scrolling.