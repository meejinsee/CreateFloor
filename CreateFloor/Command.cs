using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;

namespace CreateFloor
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Room sroom = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element).ElementId) as Room;

            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> bs = sroom.GetBoundarySegments(opt);

            List<CurveLoop> lps = new List<CurveLoop>();
            foreach (IList<BoundarySegment> item in bs)
            {
                CurveLoop cl = new CurveLoop();
                foreach (BoundarySegment item1 in item)
                {
                    cl.Append(item1.GetCurve());

                }
                lps.Add(cl);
            }

            Level level = doc.ActiveView.GenLevel;
            FilteredElementCollector col = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).OfClass(typeof(FloorType));
            FloorType ft = col.FirstElement() as FloorType;

            using(Transaction trans = new Transaction(doc, "df"))
            {
                trans.Start();
                Floor floor = Floor.Create(doc, lps, ft.Id, level.Id);
                trans.Commit();
            }
            
            return Result.Succeeded;
        }
    }
}
