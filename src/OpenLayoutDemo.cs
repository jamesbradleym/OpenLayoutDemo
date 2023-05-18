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
            var objects = inputModels["Foo"].Elements.Where(x => x.Value.AdditionalProperties.ContainsKey("Category") && input.Extract.Select(y => y.Category).ToList().Contains(x.Value.AdditionalProperties["Category"])).ToList();

            var model = new Model();
            var warnings = new List<string>();

            foreach (var extract in input.Extract)
            {
                foreach (var obj in objects.Where(e => e.Value.AdditionalProperties["Category"].ToString() == extract.Category))
                {
                    MeshElement modelObject = (MeshElement)obj.Value;
                    modelObject.IsElementDefinition = true;

                    var roomBoundaries = spacePlanningZones.AllElementsOfType<SpaceBoundary>().Where(b => b.Name == extract.Program).ToList();
                    foreach (var room in roomBoundaries)
                    {
                        var (layoutModel, layoutWarnings) = ApplyLayoutStrategy(extract.Layout, room, modelObject, obj, extract);
                        model.AddElements(layoutModel.Elements.Values);
                        warnings.AddRange(layoutWarnings);
                    }
                }
            }

            var output = new OpenLayoutDemoOutputs(0);
            output.Model = model;
            output.Warnings.AddRange(warnings);
            return output;
        }

        private static (Model, List<string>) ApplyLayoutStrategy(string layoutStrategy, SpaceBoundary room, MeshElement modelObject, KeyValuePair<Guid, Element> obj, Extract extract)
        {
            switch (layoutStrategy)
            {
                case "Row":
                    return ApplyRowLayout(room, modelObject, obj, extract);
                    break;
                case "SomeOtherLayout":
                    return ApplySomeOtherLayout(room, modelObject, obj, extract);
                    break;
                default:
                    // Handle unknown layout strategy
                    return ApplyRowLayout(room, modelObject, obj, extract);
                    break;
            }
        }

        private static (Model, List<string>) ApplyRowLayout(SpaceBoundary room, MeshElement modelObject, KeyValuePair<Guid, Element> obj, Extract extract)
        {
            var model = new Model();
            var warnings = new List<string>();

            var totalArea = 0.0;
            var width = extract.Forward == 0.0 ? modelObject.Mesh.BoundingBox.Max.X - modelObject.Mesh.BoundingBox.Min.X : extract.Forward;
            var depth = extract.Backward == 0.0 ? modelObject.Mesh.BoundingBox.Max.Y - modelObject.Mesh.BoundingBox.Min.Y : extract.Backward;
            var gap = extract.Gap;
            var aisle = extract.Aisle;

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
                return (model, warnings);
            }
            var alignment = new Transform(Vector3.Origin, longestEdge.Direction(), Vector3.ZAxis);
            var grid = new Grid2d(inset, alignment);
            grid.U.DivideByPattern(new[] { ("Forward", depth), ("Gap", gap), ("Backward", depth), ("Aisle", aisle) });
            grid.V.DivideByFixedLength(width);

            var floorGrid = new Grid2d(profile.Perimeter, alignment);
            floorGrid.U.DivideByFixedLength(0.6096);
            floorGrid.V.DivideByFixedLength(0.6096);

            var index = 0;
            foreach (var cell in grid.GetCells())
            {
                index++;
                var cellRect = cell.GetCellGeometry() as Polygon;
                if (cell.IsTrimmed() || cell.Type == null || cell.GetTrimmedCellGeometry().Count() == 0)
                {
                    continue;
                }
                if (cell.Type == "Aisle")
                {
                    if (extract.ShowPattern)
                    {
                        var t = new Transform(room.Transform);
                        t.Move(new Vector3(0, 0, 0.001));
                        model.AddElement(new Panel(cellRect, BuiltInMaterials.YAxis, t));
                    }
                }
                else if (cell.Type == "Space")
                {
                    if (extract.ShowPattern)
                    {
                        var t = new Transform(room.Transform);
                        t.Move(new Vector3(0, 0, 0.001));
                        model.AddElement(new Panel(cellRect, BuiltInMaterials.ZAxis, t));
                    }
                }
                else if (cell.Type == "Forward" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                {
                    if (extract.ShowPattern)
                    {
                        var t = new Transform(room.Transform);
                        t.Move(new Vector3(0, 0, 0.001));
                        model.AddElement(new Panel(cellRect, BuiltInMaterials.XAxis, t));
                    }
                    var centroid = cellRect.Centroid();
                    var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), -90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), extract.Category);
                    model.AddElement(instance);
                }
                else if (cell.Type == "Backward" && cellRect.Area().ApproximatelyEquals(width * depth, 0.01))
                {
                    if (extract.ShowPattern)
                    {
                        var t = new Transform(room.Transform);
                        t.Move(new Vector3(0, 0, 0.001));
                        model.AddElement(new Panel(cellRect, BuiltInMaterials.XAxis, t));
                    }
                    var centroid = cellRect.Centroid();
                    var instance = modelObject.CreateInstance(alignment.Concatenated(new Transform(new Vector3(), 90)).Concatenated(new Transform(centroid)).Concatenated(room.Transform), extract.Category);
                    model.AddElement(instance);
                }
            }
            return (model, warnings);
        }

        private static (Model, List<string>) ApplySomeOtherLayout(SpaceBoundary room, MeshElement modelObject, KeyValuePair<Guid, Element> obj, Extract extract)
        {
            var model = new Model();
            var warnings = new List<string>();
            // Implement other layout strategies here
            return (model, warnings);
        }
    }
}