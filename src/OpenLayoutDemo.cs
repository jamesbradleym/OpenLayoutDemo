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

            inputModels.TryGetValue("Space Planning Zones", out var spacePlanningZones);
            inputModels.TryGetValue("Flexible Zones", out var flexibleSpacePlanningZones);
            var objects = inputModels["Foo"].Elements.Where(x => x.Value.AdditionalProperties.ContainsKey("Category") && input.Extract.Select(y => y.Category).ToList().Contains(x.Value.AdditionalProperties["Category"])).ToList();
            // var flexibleSpacePlanningZones = inputModels["FlexZones"];

            var model = new Model();
            var warnings = new List<string>();

            foreach (var obj in objects)
            {
                var category = obj.Value.AdditionalProperties["Category"].ToString();
                MeshElement modelObject = (MeshElement)obj.Value;
                modelObject.IsElementDefinition = true;

                var totalArea = 0.0;
                var width = modelObject.Mesh.BoundingBox.Max.X - modelObject.Mesh.BoundingBox.Min.X;
                var depth = modelObject.Mesh.BoundingBox.Max.Y - modelObject.Mesh.BoundingBox.Min.Y;

                var roomBoundaries = spacePlanningZones.AllElementsOfType<SpaceBoundary>().Where(b => b.Name == input.Extract.FirstOrDefault(x => x.Category == category).Program).ToList();
                roomBoundaries.Concat(flexibleSpacePlanningZones.AllElementsOfType<FlexibleZone>().Where(b => b.Name == input.Extract.FirstOrDefault(x => x.Category == category).Program).ToList());
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
                    // grid.U.DivideByPattern(new[] { ("Forward Rack", depth), ("Hot Aisle", 5), ("Backward Rack", depth), ("Cold Aisle", 5) });
                    if (category == "Slot")
                    {
                        grid.U.DivideByPattern(new[] { ("Forward", depth), ("Space", 0.1), ("Backward", depth), ("Aisle", 2) });
                        grid.V.DivideByFixedLength(width);
                        // grid.V.DivideByPattern(new[] { ("A", width), ("B", width), ("", width), ("", width), ("", width), ("", width), ("", width * 3) });
                    }
                    else if (category == "Table")
                    {
                        grid.U.DivideByPattern(new[] { ("Forward", depth), ("Space", depth * 1.5), ("Backward", depth), ("Aisle", depth * 1.25) });
                        grid.V.DivideByFixedLength(width);
                    }
                    // grid.V.DivideByFixedLength(width);
                    var floorGrid = new Grid2d(profile.Perimeter, alignment);
                    floorGrid.U.DivideByFixedLength(0.6096);
                    floorGrid.V.DivideByFixedLength(0.6096);
                    // model.AddElements(floorGrid.ToModelCurves(room.Transform));

                    var index = 0;
                    foreach (var cell in grid.GetCells())
                    {
                        index++;
                        var cellRect = cell.GetCellGeometry() as Polygon;
                        if (category == "Table" && index % 2 == 0)
                        {
                            continue;
                        }
                        if (cell.IsTrimmed() || cell.Type == null || cell.GetTrimmedCellGeometry().Count() == 0)
                        {
                            continue;
                        }
                        if (cell.Type == "Aisle")
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.YAxis, room.Transform));
                        }
                        else if (cell.Type == "Space")
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.ZAxis, room.Transform));
                        }
                        else if (cell.Type == "Forward" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.XAxis, room.Transform));
                            var centroid = cellRect.Centroid();
                            var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), -90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), category);
                            model.AddElement(instance);
                        }
                        else if (cell.Type == "Backward" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                        {
                            model.AddElement(new Panel(cellRect, BuiltInMaterials.XAxis, room.Transform));
                            var centroid = cellRect.Centroid();
                            var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), 90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), category);
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