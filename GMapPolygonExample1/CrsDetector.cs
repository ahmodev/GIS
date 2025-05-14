using DotSpatial.Projections;
using System;
using System.IO;

namespace GMapPolygonExample1
{
    public static class CrsDetector
    {
        public static (string CrsInfo, ProjectionInfo RecommendedProjection) DetectAndRecommendProjection(string shapefilePath)
        {
            try
            {
                string prjFile = Path.ChangeExtension(shapefilePath, ".prj");
                if (!File.Exists(prjFile))
                    return ("No .prj file found (CRS information missing)", null);

                string wkt = File.ReadAllText(prjFile);
                ProjectionInfo sourceProjection = ProjectionInfo.FromEsriString(wkt);

                bool isGeographic = IsGeographicCRS(sourceProjection);
                bool isProjected = !isGeographic;

                ProjectionInfo recommendedProjection = GetRecommendedProjection(sourceProjection);

                string crsInfo = $"CRS: {sourceProjection.Name}\n" +
                                $"EPSG Code: {sourceProjection.AuthorityCode}\n" +
                                $"Is Geographic: {isGeographic}\n" +
                                $"Is Projected: {isProjected}\n" +
                                $"WKT: {sourceProjection.ToEsriString()}";

                return (crsInfo, recommendedProjection);
            }
            catch (Exception ex)
            {
                return ($"Error reading CRS: {ex.Message}", null);
            }
        }

        private static bool IsGeographicCRS(ProjectionInfo proj)
        {
            if (proj.AuthorityCode <= 0)
                return proj.Unit.Name.ToLower().Contains("degree");

            return proj.AuthorityCode == 4326 ||  // WGS84
                   proj.AuthorityCode == 4269 ||  // NAD83
                   proj.AuthorityCode == 4258;    // ETRS89
        }

        private static ProjectionInfo GetRecommendedProjection(ProjectionInfo sourceProjection)
        {
            if (sourceProjection.AuthorityCode == 4326)  // WGS84
                return KnownCoordinateSystems.Geographic.World.WGS1984;

            if (sourceProjection.AuthorityCode == 3857)  // Web Mercator
                return KnownCoordinateSystems.Projected.World.WebMercator;
            if(sourceProjection.Name!=null)
            {
                if (sourceProjection.Name.Contains("UTM"))   // UTM
                    return sourceProjection;
            }
           

            // Default: Convert to WGS84
            return KnownCoordinateSystems.Geographic.World.WGS1984;
        }
    }

}
