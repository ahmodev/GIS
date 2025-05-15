![De-Map](https://github.com/user-attachments/assets/c041d490-29f5-43a5-a4d7-fb7fbe3f284e)# GIS Shapefile Viewer with UTM Zone Detection
![App_view](https://github.com/user-attachments/assets/c55a1734-966e-461d-94a9-edfac5d5ab89)
![De-Map](https://github.com/user-attachments/assets/78106374-e8f5-4c5f-8145-92691fe3d8e0)
![Tj-De-Map](https://github.com/user-attachments/assets/a7e614b2-4d9e-4db2-ac7d-aa875e45e0d4)
![US1](https://github.com/user-attachments/assets/79d63211-f287-4dd3-9253-9d1ee86bd124)
![US2](https://github.com/user-attachments/assets/a1470ae9-f411-477b-8ce6-f233487faafe)
![World-Map](https://github.com/user-attachments/assets/a2b60846-c44a-4bd4-86e4-e3f2d300c6a9)






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
