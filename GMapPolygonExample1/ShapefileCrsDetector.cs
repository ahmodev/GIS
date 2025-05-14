using System.IO;
using DotSpatial.Projections;

namespace GMapPolygonExample1
{


    public class ShapefileCrsDetector
    {
        public static string DetectShapefileCrsDotSpatial(string shapefilePath)
        {
            try
            {
                // Read the shapefile's projection file (.prj)
                string prjFile = Path.ChangeExtension(shapefilePath, ".prj");

                if (!File.Exists(prjFile))
                {
                    return "No .prj file found (CRS information missing)";
                }

                string wkt = File.ReadAllText(prjFile);
                ProjectionInfo proj = ProjectionInfo.FromEsriString(wkt);

                return $"CRS: {proj.Name}\n" +
                       $"EPSG Code: {proj.AuthorityCode}\n" +
                       $"WKT: {proj.ToEsriString()}";
            }
            catch (Exception ex)
            {
                return $"Error reading CRS: {ex.Message}";
            }
        }
    }
}
