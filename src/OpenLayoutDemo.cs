using Elements;
using Elements.Geometry;
using Elements.Spatial;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenLayoutDemo
{
    public static class OpenLayoutDemo
    {
        /// <summary>
        /// The OpenLayoutDemo function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A OpenLayoutDemoOutputs instance containing computed results and the model with any new elements.</returns>
        public static OpenLayoutDemoOutputs Execute(Dictionary<string, Model> inputModels, OpenLayoutDemoInputs input)
        {
            // var watch = new System.Diagnostics.Stopwatch();
            // watch.Start();

            var spacePlanningZones = inputModels["Space Planning Zones"];
            var roomBoundaries = spacePlanningZones.AllElementsOfType<SpaceBoundary>().Where(b => b.Name == "Data Hall").ToList();
            var objects = inputModels["Foo"].Elements.Where(x => x.Value.AdditionalProperties.ContainsKey("Category") && input.Extract.Select(y => y.Category).ToList().Contains(x.Value.AdditionalProperties["Category"])).ToList();

            var model = new Model();
            var warnings = new List<string>();

            foreach (var obj in objects)
            {
                MeshElement modelObject = (MeshElement)obj.Value;
                modelObject.IsElementDefinition = true;

                var totalArea = 0.0;
                var width = modelObject.Mesh.BoundingBox.Max.X - modelObject.Mesh.BoundingBox.Min.X;
                var depth = modelObject.Mesh.BoundingBox.Max.Y - modelObject.Mesh.BoundingBox.Min.Y;
                foreach (var room in roomBoundaries)
                {
                    var profile = room.Boundary;
                    totalArea += profile.Area();
                    //inset from walls
                    var inset = profile.Perimeter.Offset(-1.2);
                    Line longestEdge = null;
                    try
                    {
                        longestEdge = inset.SelectMany(s => s.Segments()).OrderBy(l => l.Length()).Last();
                    }
                    catch
                    {
                        warnings.Add($"One space was too small for a {obj.Value.AdditionalProperties["Category"]}.");
                        continue;
                    }
                    var alignment = new Transform(Vector3.Origin, longestEdge.Direction(), Vector3.ZAxis);
                    var grid = new Grid2d(inset, alignment);
                    grid.U.DivideByPattern(new[] { ("Forward Rack", depth), ("Hot Aisle", 5), ("Backward Rack", depth), ("Cold Aisle", 5) });
                    grid.V.DivideByFixedLength(width);
                    var floorGrid = new Grid2d(profile.Perimeter, alignment);
                    floorGrid.U.DivideByFixedLength(0.6096);
                    floorGrid.V.DivideByFixedLength(0.6096);
                    // model.AddElements(floorGrid.ToModelCurves(room.Transform));

                    foreach (var cell in grid.GetCells())
                    {
                        var cellRect = cell.GetCellGeometry() as Polygon;
                        // if (cell.IsTrimmed() || cell.Type == null || cell.GetTrimmedCellGeometry().Count() == 0)
                        // {
                        //     continue;
                        // }
                        if (cell.Type == "Hot Aisle")
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.XAxis, room.Transform));
                        }
                        else if (cell.Type == "Cold Aisle")
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.ZAxis, room.Transform));
                        }
                        else if (cell.Type == "Forward Rack" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                        {
                            var centroid = cellRect.Centroid();
                            var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), -90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), "Rack");
                            model.AddElement(instance);
                        }
                        else if (cell.Type == "Backward Rack" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                        {
                            var centroid = cellRect.Centroid();
                            var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), 90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), "Rack");
                            model.AddElement(instance);
                        }
                    }
                }
            }

            // var instances = new List<Element>();
            // instances.Add(objects);

            var output = new OpenLayoutDemoOutputs(0);
            output.Model = model;
            output.Warnings.AddRange(warnings);

            // serialize JSON directly to a file
            // using (StreamWriter file = File.CreateText(@"/Users/jamesbradleym/Desktop/model.json"))
            // {
            //     JsonSerializer serializer = new JsonSerializer();
            //     serializer.Serialize(file, model);
            // }

            // watch.Stop();
            // var time = watch.ElapsedMilliseconds;
            return output;
        }
    }
}