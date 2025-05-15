# GIS Shapefile Viewer with UTM Zone Detection

![Screenshot](screenshot.png) <!-- Add a screenshot if available -->

A WPF application for visualizing GIS shapefiles with automatic UTM zone detection and coordinate conversion.

## Features

- Load and display ESRI Shapefiles (.shp) with geometry types:
  - Points
  - Polylines
  - Polygons
  - Multi-geometry collections
- Automatic UTM zone detection from shapefile:
  - Reads existing projection information
  - Calculates optimal UTM zone based on feature locations
- Coordinate conversion between UTM and WGS84 (lat/lng)
- Interactive map with pan/zoom functionality
- Clean visualization with customizable styling

## Technologies Used

- .NET WPF (Windows Presentation Foundation)
- [GMap.NET](https://github.com/radioman/greatmaps) - Interactive map control
- [DotSpatial](https://github.com/DotSpatial/DotSpatial) - GIS data handling and projections
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) - Geometry operations
- [ProjNET](https://github.com/NetTopologySuite/ProjNet4GeoAPI) - Coordinate system transformations

## Installation

1. **Prerequisites**:
   - .NET Framework 4.7.2 or later
   - Windows OS

2. **Build from source**:
   ```bash
   git clone https://github.com/yourusername/GMapPolygonExample1.git
   cd GMapPolygonExample1
   msbuild GMapPolygonExample1.sln
