using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Autodesk.Windows;

namespace FamilyApi
{
    [Transaction(TransactionMode.Manual)]
    public class CmdLoadArrayFamily : IExternalCommand
    {
        #region CurveSelectionFilter
        class CurveSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                //Autodesk.Revit.DB.DetailCurve
                //  Autodesk.Revit.DB.DetailArc
                //  Autodesk.Revit.DB.DetailEllipse
                //  Autodesk.Revit.DB.DetailLine
                //  Autodesk.Revit.DB.DetailNurbSpline

                //Autodesk.Revit.DB.ModelCurve
                //  Autodesk.Revit.DB.ModelArc
                //  Autodesk.Revit.DB.ModelEllipse
                //  Autodesk.Revit.DB.ModelHermiteSpline
                //  Autodesk.Revit.DB.ModelLine
                //  Autodesk.Revit.DB.ModelNurbSpline

                return e is DetailLine
                  || e is ModelLine
                  || e is DetailArc
                  || e is ModelArc
                  || e is DetailNurbSpline
                  || e is ModelNurbSpline;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return false;
            }
        }
        #endregion // CurveSelectionFilter


        public Result Execute(
  ExternalCommandData commandData,
  ref string message,
  ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference r = null;

            try
            {
                r = uidoc.Selection.PickObject(
                  ObjectType.Element,
                  new CurveSelectionFilter(),
                  "Выберите дугу или сплайн");
            }
            catch (Autodesk.Revit.Exceptions
              .OperationCanceledException)
            {
                return Result.Cancelled;
            }

            if (null == r)
            {
                message = "Не выбрана кривая";
                return Result.Failed;
            }

            Element e = doc.GetElement(r);

            if (null == e || !(e is CurveElement))
            {
                message = "Выбранный элемент не является кривой";
                return Result.Failed;
            }

            System.Windows.Forms.IWin32Window revit_window = new JtWindowHandle(ComponentManager.ApplicationWindow);
            LoadFamilyForm form = new LoadFamilyForm(doc);
            if (form.ShowDialog(revit_window) != System.Windows.Forms.DialogResult.OK)
            {
                return Result.Cancelled;
            }

            // Извлекаем данные из выбранной кривой.

            Curve curve = (e as CurveElement).GeometryCurve;

            IList<XYZ> tessellation = curve.Tessellate();

            // Создаем список равноудаленных точек.

            List<XYZ> pts = new List<XYZ>(1);

            double stepsize = form.StepSize;//5.0;
            double dist = 0.0;

            XYZ p = curve.GetEndPoint(0);

            foreach (XYZ q in tessellation)
            {
                if (0 == pts.Count)
                {
                    pts.Add(p);
                    dist = 0.0;
                }
                else
                {
                    dist += p.DistanceTo(q);

                    if (dist >= stepsize)
                    {
                        pts.Add(q);
                        dist = 0;
                    }
                    p = q;
                }
            }

            // Помечаем кружком точку на линии.

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Рисуем круги на линии");
                string points = "";
                foreach (XYZ pt in pts)
                {
                    //points += pt.ToString() + ";";
                    AddFamily(doc, pt, form.Symbol);
                    //CreateCircle(doc, pt, 1);
                }
                //Util.ErrorMsg(points);
                tx.Commit();
            }
            
            return Result.Succeeded;
        }

        FamilyInstance AddFamily(
            Document doc,
            XYZ location, 
            FamilySymbol symbol)
        {
            /*XYZ norm = XYZ.BasisY;
            Plane plane = new Plane(norm, location);
            XYZ l = TransformPoint(location, Transform.CreateTranslation(XYZ.BasisZ));*/

            //XYZ l = location.Negate();

            StructuralType st
                      = StructuralType.UnknownFraming;
            return doc.Create.NewFamilyInstance(location, symbol, st);
        }

        DetailArc CreateCircle(
    Document doc,
    XYZ location,
    double radius)
        {
            XYZ norm = XYZ.BasisY;

            double startAngle = 0;
            double endAngle = 2 * Math.PI;

            Plane plane = new Plane(norm, location);

            Arc arc = Arc.Create(plane,
              radius, startAngle, endAngle);

            return doc.Create.NewDetailCurve(
              doc.ActiveView, arc) as DetailArc;
        }

        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }

        /*public Result Execute(
              ExternalCommandData commandData,
              ref string message,
              ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //IWin32Window revit_window = new JtWindowHandle(ComponentManager.ApplicationWindow);
            LoadFamilyForm form = new LoadFamilyForm(doc);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Place Instances");

                    Autodesk.Revit.Creation.Document
                      creation_doc = doc.Create;

                    StructuralType st
                      = StructuralType.NonStructural;

                    Selection sel = uidoc.Selection;
                    Reference refer = sel.PickObject(ObjectType.Element, "Выберите кривую");
                    Element elem = doc.GetElement(refer.ElementId);
                    LocationCurve locationCurve1 = elem.Location as LocationCurve;
                    //Line line0 = locationCurve1.Curve as Line;


                    Debug.Assert(null != locationCurve1, "locationCurve1");

                    FamilyInstance inst = doc.FamilyCreate.NewFamilyInstance(
                        line0, form.Symbol, doc.ActiveView);

                    creation_doc.NewFamilyInstance(
                          locationCurve1.Curve, form.Symbol, elem.Level, st);

                    foreach (XYZ p in f.Points)
                    {
                        creation_doc.NewFamilyInstance(
                          p, f.Type, st);
                    }

                    t.Commit();
                }
            }

            return Result.Succeeded;
        }*/
    }
}
