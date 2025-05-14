using System;
using DotSpatial.Projections; // Requires DotSpatial.Projections NuGet package
using DotSpatial.Data; // Requires DotSpatial.Data NuGet package

namespace GMapPolygonExample1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotSpatial.Data;
    using DotSpatial.Projections;

    public static class UtmZoneCalculator
    {
        public static int CalculateUtmZoneFromShapefile(string shapefilePath)
        {
            if (string.IsNullOrWhiteSpace(shapefilePath))
                throw new ArgumentException("Shapefile path cannot be null or empty.", nameof(shapefilePath));

            // Load the shapefile with error handling
            IFeatureSet shapefile;
            try
            {
                shapefile = Shapefile.OpenFile(shapefilePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not open shapefile: {ex.Message}", ex);
            }

            // Check if shapefile has features
            if (shapefile.Features == null || shapefile.Features.Count == 0)
                throw new InvalidOperationException("Shapefile contains no features.");

            // 1. First check if the shapefile is already in a UTM projection
            if (shapefile.Projection != null && !shapefile.Projection.IsLatLon)
            {
                var projString = shapefile.Projection.ToString();
                if (projString.Contains("UTM zone"))
                {
                    int start = projString.IndexOf("UTM zone") + 8;
                    int end = projString.IndexOf('"', start);
                    if (int.TryParse(projString.Substring(start, end - start).Trim(), out int existingZone))
                        return existingZone;
                }
            }

            // 2. Calculate zones for all features
            var zoneCounts = new Dictionary<int, int>();
            int totalFeatures = 0;

            foreach (IFeature feature in shapefile.Features)
            {
                if (feature?.Geometry?.Centroid == null)
                    continue;

                var centroid = feature.Geometry.Centroid;
                int zone = CalculateUtmZoneFromCoordinate(centroid.X, centroid.Y);

                if (zoneCounts.ContainsKey(zone))
                    zoneCounts[zone]++;
                else
                    zoneCounts[zone] = 1;

                totalFeatures++;
            }

            // Handle case where all features were invalid
            if (totalFeatures == 0)
                throw new InvalidOperationException("Shapefile contains no valid features with geometries.");

            // 3. Return the most appropriate zone
            if (zoneCounts.Count == 1)
                return zoneCounts.Keys.First();

            // For multi-zone shapefiles, return the zone covering most features
            if (zoneCounts.Count > 1)
            {
                // Option 1: Return the most common zone
                var mostCommonZone = zoneCounts.OrderByDescending(z => z.Value).First().Key;

                // Option 2: Throw an exception for multi-zone shapefiles
                // throw new InvalidOperationException($"Shapefile spans multiple UTM zones: {string.Join(", ", zoneCounts.Keys.OrderBy(z => z))}");

                return mostCommonZone;
            }

            // 4. Fallback: calculate from extent center
            return CalculateUtmZoneFromExtent(shapefile.Extent);
        }

        private static int CalculateUtmZoneFromExtent(Extent extent)
        {
            if (extent == null)
                throw new ArgumentNullException(nameof(extent));

            double centerLon = (extent.MinX + extent.MaxX) / 2.0;
            double centerLat = (extent.MinY + extent.MaxY) / 2.0;
            return CalculateUtmZoneFromCoordinate(centerLon, centerLat);
        }

        public static int CalculateUtmZoneFromCoordinate(double longitude, double latitude)
        {
            // Normalize longitude to [-180, 180]
            longitude = NormalizeLongitude(longitude);

            // Base zone calculation
            int zone = (int)Math.Floor((longitude + 180) / 6) + 1;
            zone = Math.Max(1, Math.Min(60, zone));

            // Norway/Svalbard exceptions
            if (latitude >= 56.0 && latitude < 64.0 && longitude >= 3.0 && longitude < 12.0)
                return 32;

            if (latitude >= 72.0 && latitude < 84.0)
            {
                if (longitude >= 0.0 && longitude < 9.0) return 31;
                if (longitude >= 9.0 && longitude < 21.0) return 33;
                if (longitude >= 21.0 && longitude < 33.0) return 35;
                if (longitude >= 33.0 && longitude < 42.0) return 37;
            }

            return zone;
        }

        private static double NormalizeLongitude(double longitude)
        {
            while (longitude < -180) longitude += 360;
            while (longitude > 180) longitude -= 360;
            return longitude;
        }
    }
}
