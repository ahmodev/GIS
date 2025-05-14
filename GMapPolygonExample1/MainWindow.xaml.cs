using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using NetTopologySuite.Geometries;

using NetTopologySuite.IO.ShapeFile.Extended;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Win32;
using DotSpatial.Projections;
using NetTopologySuite.IO;
using DotSpatial.Data;
using GeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using Point = NetTopologySuite.Geometries.Point;
using System.Windows.Shapes;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace GMapPolygonExample1
{
    public partial class MainWindow : Window
    {
        private bool isDragging;
        private PointLatLng initialPosition;
        private System.Windows.Point initialMousePosition;

        public MainWindow()
        {
            InitializeComponent();
            //Initialize the map
            InitialMap();

        }
        public static void ReprojectShapefile(IFeatureSet shapefile, ProjectionInfo targetProjection)
        {
            ProjectionInfo sourceProjection = shapefile.Projection;
            if (sourceProjection == null)
                throw new InvalidOperationException("Shapefile has no defined projection.");

            foreach (IFeature feature in shapefile.Features)
            {
                if (feature.Geometry == null)
                    continue;

                // Get coordinates correctly (call Coordinates() as a method)
                var coordinates = feature.Geometry.Coordinates;
                int numPoints = coordinates.Count();

                double[] xy = new double[numPoints * 2];  // Now works: numPoints is an int
                double[] z = new double[numPoints];

                for (int i = 0; i < numPoints; i++)
                {
                    xy[i * 2] = coordinates[i].X;
                    xy[i * 2 + 1] = coordinates[i].Y;
                }

                // Reproject
                Reproject.ReprojectPoints(xy, z, sourceProjection, targetProjection, 0, numPoints);

                // Update coordinates
                for (int i = 0; i < numPoints; i++)
                {
                    coordinates[i].X = xy[i * 2];
                    coordinates[i].Y = xy[i * 2 + 1];
                }
            }

            shapefile.Projection = targetProjection;
        }

        private void LoadAndDrawShapefile(string shapefilePath)
        {
            var result = CrsDetector.DetectAndRecommendProjection(shapefilePath);
            Console.WriteLine(result.CrsInfo);

            string finalShapefilePath = shapefilePath; // Store original path

            if (result.RecommendedProjection != null)
            {
                // Reproject to the recommended CRS (if needed)
                var shapefile = DotSpatial.Data.Shapefile.OpenFile(shapefilePath);
                ReprojectShapefile(shapefile, result.RecommendedProjection);

                // Update the path to use the reprojected shapefile if needed
                // (Assuming ReprojectShapefile saves to a new file or modifies in place)
                finalShapefilePath = shapefile.Filename; // Or whatever path points to reprojected file
            }

            var geometryFactory = new GeometryFactory();

            using (var shapeFileReader = new ShapefileDataReader(finalShapefilePath, geometryFactory))
            {
                while (shapeFileReader.Read())
                {
                    var geometry = shapeFileReader.Geometry;

                    if (geometry is Point point)
                    {
                        DrawPoint(point);
                    }
                    else if (geometry is MultiPoint multiPoint)
                    {
                        foreach (var pt in multiPoint.Geometries.Cast<Point>())
                        {
                            DrawPoint(pt);
                        }
                    }
                    else if (geometry is LineString lineString)
                    {
                        DrawPolyline(lineString);
                    }
                    else if (geometry is MultiLineString multiLineString)
                    {
                        foreach (var singleLineString in multiLineString.Geometries.Cast<LineString>())
                        {
                            DrawPolyline(singleLineString);
                        }
                    }
                    else if (geometry is Polygon polygon)
                    {
                        DrawPolygon(polygon);
                    }
                    else if (geometry is MultiPolygon multiPolygon)
                    {
                        foreach (var poly in multiPolygon.Geometries.Cast<Polygon>())
                        {
                            DrawPolygon(poly);
                        }
                    }
                    else if (geometry is GeometryCollection geometryCollection)
                    {
                        foreach (var geom in geometryCollection.Geometries)
                        {
                            // Recursively handle nested geometries
                            if (geom is Point p) DrawPoint(p);
                            else if (geom is LineString ls) DrawPolyline(ls);
                            else if (geom is Polygon pg) DrawPolygon(pg);
                            // Add other types as needed
                        }
                    }
                }
            }
        }
        private void DrawPoint(Point point)
        {
            if (point == null) return;

            int UTMZoneValue = int.Parse(UtmZoneTextBox.Text);
            var latLng = ConvertUtmToLatLng(point.X, point.Y, UTMZoneValue, true);

            // Create a standard marker with custom styling
            var marker = new GMapMarker(new PointLatLng(latLng.Latitude, latLng.Longitude))
            {
                Shape = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1.5,
                    Fill = Brushes.Red,
                    ToolTip = $"Point: {latLng.Latitude:F6}, {latLng.Longitude:F6}"
                }
            };

            gmap.Markers.Add(marker);

            // Center map on first point
            if (gmap.Position == default)
            {
                gmap.Position = new PointLatLng(latLng.Latitude, latLng.Longitude);
                gmap.MinZoom = 2;
                gmap.MaxZoom = 17;
                gmap.Zoom = 14;
            }
        }
        //private void DrawPolygon(Polygon polygon)
        //{
        //    if (polygon == null) return;

        //    // Draw exterior ring
        //    DrawPolygonRing(polygon.ExteriorRing, Brushes.Red);

        //    // Draw interior rings (holes)
        //    foreach (var interiorRing in polygon.InteriorRings)
        //    {
        //        DrawPolygonRing(interiorRing, Brushes.White);
        //    }
        //}
        private void DrawPolygon( Polygon polygon)
{
    // Create a list of points for the GMapPolygon
    List<GMap.NET.PointLatLng> points = new List<GMap.NET.PointLatLng>();
    
    // Get the coordinates from the NetTopologySuite polygon
    var coordinates = polygon.ExteriorRing.Coordinates;
    
    // Convert each coordinate to GMap.NET PointLatLng
    foreach (var coord in coordinates)
    {
        points.Add(new GMap.NET.PointLatLng(coord.Y, coord.X));
    }
    
    // Create the GMapPolygon
   

            var gmapPolygon = new GMapPolygon(points)
            {
                Shape = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0)),
                   
                }
            };
            // Add the polygon to the map
            gmap.Markers.Add(gmapPolygon);
    
    // Optional: Zoom to fit the polygon
    if (points.Count > 0)
    {
        gmap.ZoomAndCenterMarkers(null);
    }
}
        private void DrawPolygonRing(LineString ring, Brush fillColor)
        {
            if (ring == null) return;

            var points = new List<PointLatLng>();
            int UTMZoneValue = int.Parse(UtmZoneTextBox.Text);

            foreach (var coordinate in ring.Coordinates)
            {
               // var latLng = ConvertUtmToLatLng(coordinate.X, coordinate.Y, UTMZoneValue, true);
                points.Add(new PointLatLng(coordinate.X, coordinate.Y));
            }

            var polygon = new GMapPolygon(points)
            {
                Shape = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = fillColor,
                    Opacity = 0.6
                }
            };
            gmap.Markers.Add(polygon);
        }
        private (double Latitude, double Longitude) ConvertUtmToLatLng(double easting, double northing, int zone, bool isNorthernHemisphere)
        {
            var wgs84 = GeographicCoordinateSystem.WGS84;
            var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, isNorthernHemisphere);
            var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(utm, wgs84);
            double[] utmCoords = new double[] { easting, northing };
            double[] latLngCoords = transform.MathTransform.Transform(utmCoords);
            return (latLngCoords[1], latLngCoords[0]);
        }

        private void DrawPolyline(LineString lineString)
        {
            if (lineString == null) return;

            var points = new List<PointLatLng>();
            int UTMZoneValue = int.Parse(UtmZoneTextBox.Text);
            int counter = 1;
            foreach (var coordinate in lineString.Coordinates)
            {

                var latLng = ConvertUtmToLatLng(coordinate.X, coordinate.Y, UTMZoneValue, true); // Assuming UTM Zone 32N
                points.Add(new PointLatLng(latLng.Latitude, latLng.Longitude));
                if (counter == 1)
                {
                    gmap.Position = new PointLatLng(latLng.Latitude, latLng.Longitude);
                    gmap.MinZoom = 2;
                    gmap.MaxZoom = 17;
                    gmap.Zoom = 14;
                }
                counter++;
            }

            // Create and add a polyline to the map
            GMapRoute polyline = new GMapRoute(points)
            {
                Shape = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2.0
                }
            };
            gmap.Markers.Add(polyline);
        }

        private void Gmap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                initialMousePosition = e.GetPosition(gmap);
                initialPosition = gmap.Position;
                // gmap.Cursor = Cursors.Hand;
                gmap.IgnoreMarkerOnMouseWheel = true; // Optional: ignore markers when dragging
            }
        }


        private void UtmZoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regular expression to match only numeric input
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void UtmZoneTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!int.TryParse(textBox.Text, out _))
            {
                MessageBox.Show("Please enter a valid integer value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Text = string.Empty;
            }
        }

        private void loadShapeFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Shapefiles (*.shp)|*.shp",
                Title = "Open Shapefile"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string shapefilePath = openFileDialog.FileName;
                LoadAndDrawShapefile(shapefilePath);
            }
        }

        private void refreshMapButton_Click(object sender, RoutedEventArgs e)
        {
            gmap.Markers.Clear();
            InitialMap();
        }


        
        private void InitialMap()
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gmap.Position = new PointLatLng(51.1657, 10.4515);//Germany coordinates
            gmap.MinZoom = 2;
            gmap.MaxZoom = 17;
            gmap.Zoom = 4;
            gmap.DragButton = MouseButton.Left;
        }
    }
}
