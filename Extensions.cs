using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;

[assembly: CommandClass(typeof(MyCAD1.Extensions))]
[assembly: CommandClass(typeof(MyCAD1.Commands))]


namespace MyCAD1
{
    public static class Extensions
    {
        public static Point2d Swap(this Point2d pt, bool flip = true)
        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }
        public static Point3d Pad(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }
        public static Point2d Strip(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        /// <summary>
        /// Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>        
        public static ObjectId CreateAndMakeLayoutCurrent(this LayoutManager lm, string name, bool select = true)
        {
            var id = lm.GetLayoutId(name);
            if (!id.IsValid)
            {
                id = lm.CreateLayout(name);
            }
            if (select)
            {
                lm.CurrentLayout = name;
            }
            return id;
        }
        /// <summary>
        /// Applies an action to the specified viewport from this layout.
        /// Creates a new viewport if none is found withthat number.
        /// </summary>
        /// <param name="tr">The transaction to use to open the viewports.</param>
        /// <param name="vpNum">The number of the target viewport.</param>
        /// <param name="f">The action to apply to each of the viewports.</param>
        public static void ApplyToViewport(this Layout lay, Transaction tr, int vpNum, Action<Viewport> f)
        {
            var vpIds = lay.GetViewports();
            Viewport vp = null;
            foreach (ObjectId vpId in vpIds)
            {
                var vp2 = tr.GetObject(vpId, OpenMode.ForWrite) as Viewport;
                if (vp2 != null && vp2.Number == vpNum)
                {                    // We have found our viewport, so call the action
                    vp = vp2;
                    vp.GridOn = false;
                    break;
                }
            }
            if (vp == null)
            {                // We have not found our viewport, so create one
                var btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                vp = new Viewport();
                // Add it to the database
                btr.AppendEntity(vp);
                tr.AddNewlyCreatedDBObject(vp, true);
                // Turn it - and its grid - on
                vp.On = true;
                vp.GridOn = false;
            }
            // Finally we call our function on it
            f(vp);
        }

        /// <summary>
        /// 获得Line中点Point3D.
        /// </summary>
        /// <param name="aline">目标线.</param>       
        
        public static Point3d GetMidPoint3d(this Line aline)
        {
            double x = 0.5 * (aline.StartPoint.X + aline.EndPoint.X);
            double y = 0.5 * (aline.StartPoint.Y + aline.EndPoint.Y);
            return new Point3d(x, y, 0);
        }

        /// <summary>
        /// Apply plot settings to the provided layout.
        /// </summary>
        /// <param name="pageSize">The canonical media name for our page size.</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb).</param>
        /// <param name="devices">The name of the output device.</param>



        public static void SetPlotSettings(this Layout lay, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {
                ps.CopyFrom(lay);
                var psv = PlotSettingsValidator.Current;
                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }
                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }
                psv.SetPlotWindowArea(ps, new Extents2d(new Point2d(0, 0), new Point2d(420, 297)));
                psv.SetPlotType(ps, PlotType.Window);
                psv.SetPlotCentered(ps, true);
                //psv.SetPlotCentered(ps, true);
                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }
                // Copy the PlotSettings data back to the Layout

                var upgraded = false;
                if (!lay.IsWriteEnabled)
                {
                    lay.UpgradeOpen();
                    upgraded = true;
                }
                lay.CopyFrom(ps);
                if (upgraded)
                {
                    lay.DowngradeOpen();
                }
            }
        }



        /// <summary>
        /// Determine the maximum possible size for this layout.
        /// </summary>
        /// <returns>The maximum extents of the viewport on this layout.</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)
        {
            // If the drawing template is imperial, we need to divide by            
            // 1" in mm (25.4)
            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;
            // We need to flip the axes if the plot is rotated by 90 or 270 deg
            var doIt =
              lay.PlotRotation == PlotRotation.Degrees090 ||
              lay.PlotRotation == PlotRotation.Degrees270;

            // Get the extents in the correct units and orientation
            var min = lay.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max = (lay.PlotPaperSize.Swap(doIt) -
               lay.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;
            return new Extents2d(min, max);
        }



        /// <summary>
        /// Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="ext">The extents of the viewport on the page.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void ResizeViewport(this Viewport vp, Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;
            vp.CenterPoint = (Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }

        /// <summary>
        /// 改写Polylin 的 globewidth.
        /// </summary>
        /// <param name="thePline">多线名称。</param>       
        /// <param name="width">全局宽度。</param>       
        public static void GlobalWidth(this Polyline thePline, double width=0)
        {
            for(int i=0; i<thePline.NumberOfVertices; i++)
            {
                thePline.SetStartWidthAt(i, width);
                thePline.SetEndWidthAt(i, width);
            }

        }


        /// <summary>
        /// 获得对称
        /// </summary>
        /// <param name="thePline">多线名称</param>       
        /// <param name="axis">对称轴</param>       
        public static Polyline GetMirror(this Polyline thePline, Line2d axis)
        {
            Polyline res = (Polyline)thePline.Clone();
            res.TransformBy(Matrix3d.Mirroring(axis.Convert3D()));
            return res;
        }


        /// <summary>
        /// 根据多线线段编号获取对应直线
        /// </summary>
        /// <param name="thePline"></param>
        /// <param name="SegID">线段编号</param>
        /// <returns>对应直线</returns>
        public static Line GetLine(this Polyline thePline, int SegID)
        {
            var seg = thePline.GetLineSegmentAt(SegID);
            Point3d p1 = seg.StartPoint;
            Point3d p2 = seg.EndPoint;
            Line res = new Line(p1, p2);            
            return res;
        }


        public static Line3d Convert3D (this Line2d theL2d)
        {
            return new Line3d(theL2d.StartPoint.Convert3D(), theL2d.EndPoint.Convert3D());
        }













        public static Point2d Convert2D(this Point3d theP3d, double x = 0, double y = 0)
        {
            return new Point2d(theP3d.X + x, theP3d.Y + y);
        }


        public static Point3d Convert3D(this Point3d theP3d, double x = 0, double y = 0,double z=0)
        {
            return new Point3d(theP3d.X + x, theP3d.Y + y,theP3d.Z+z);
        }


        public static Point3d Convert3D(this Point2d theP2d, double x = 0, double y = 0)
        {
            return new Point3d(theP2d.X + x, theP2d.Y + y,0);
        }

        public static Point2d Convert2D(this Point2d theP2d,double x=0,double y=0)
        {
            return new Point2d(theP2d.X + x, theP2d.Y + y);
        }

        
        
        /// <summary>
        /// 平面\立面视口.
        /// </summary>
        /// <param name="vpNum">1=平面，2=剖面.</param>        
        /// <param name="ytop">1=平面，2=剖面.</param> 
        public static void DrawMyViewport(this Viewport vp, int vpNum,double ytop)
        {
            if (vpNum == 1)
            {
                vp.CenterPoint = new Point3d(150,148.5,0);
                vp.Width = 240;
                vp.Height = 277;                
                vp.ViewCenter = new Point2d(0, ytop+1000-13850);                
                vp.StandardScale = StandardScaleType.Scale1To100;
            }
            else if (vpNum==2)
            {
                vp.CenterPoint = new Point3d(340, 148.5, 0);
                vp.Width = 140;
                vp.Height = 277;
                vp.ViewCenter = new Point2d(25000, -10000);
                vp.CustomScale = 1.0 / 75.0;
                //vp.StandardScale = StandardScaleType.Scale1To100;
            }
            vp.Layer = "图框";
        }



        /// <summary>
        /// Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitContentToViewport(this Viewport vp, Extents3d ext, double fac = 1.0)
        {
            // Let's zoom to just larger than the extents
            vp.ViewCenter = (ext.MinPoint + ((ext.MaxPoint - ext.MinPoint) * 0.5)).Strip();
            // Get the dimensions of our view from the database extents
            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;
            var wid = ext.MaxPoint.X - ext.MinPoint.X;
            // We'll compare with the aspect ratio of the viewport itself

            // (which is derived from the page size)
            var aspect = vp.Width / vp.Height;
            // If our content is wider than the aspect ratio, make sure we
            // set the proposed height to be larger to accommodate the
            // content        
            if (wid / hgt > aspect)
            {
                hgt = wid / aspect;
            }
            // Set the height so we're exactly at the extents
            vp.ViewHeight = hgt;
            // Set a custom scale to zoom out slightly (could also
            // vp.ViewHeight *= 1.1, for instance)
            vp.CustomScale *= fac;
        }
    }




    public class Commands
    {
        [CommandMethod("goo")]
        public static void CreateLayout(double ytop)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            if (doc == null)
                return;
            var db = doc.Database;
            var ed = doc.Editor;
            var ext = new Extents2d();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Create and select a new layout tab
                var id = LayoutManager.Current.CreateAndMakeLayoutCurrent("1-1");
                // Open the created layout
                var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);

                // Make some settings on the layout and get its extents
                lay.SetPlotSettings("A3", "monochrome.ctb", "Adobe PDF");
                ext = lay.GetMaximumExtents();
                lay.ApplyToViewport(tr, 2,
                    vp => 
                    {
                        vp.DrawMyViewport(1,ytop);
                        //vp.ResizeViewport(ext, 0.8);
                        vp.Locked = true;
                    }
                    );
                lay.ApplyToViewport(tr, 3,    
                    vp =>  {
                        vp.DrawMyViewport(2,ytop);
                        vp.Locked = true;
                    }
                    );
                //-----------------------------------------------------------------------------
                var blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var ps = tr.GetObject(blockTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;

                Point2d p0 = new Point2d(0, 0);
                Point2d p1 = HanDong.Convert2d(p0, 420, 0);
                Point2d p2 = HanDong.Convert2d(p1, 0,297);
                Point2d p3 = HanDong.Convert2d(p2,-420,0);
                Polyline PL1 = new Polyline();
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                PL1.Layer = "图框";
                PL1.Closed = true;
                ps.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                p0 = new Point2d(30, 10);
                p1 = HanDong.Convert2d(p0,420-40,0);
                p2 = HanDong.Convert2d(p1, 0,297-20);
                p3= HanDong.Convert2d(p0, 0, 297 - 20);
                Polyline PL2 = new Polyline();
                PL2.AddVertexAt(0, p0, 0, 0, 0);
                PL2.AddVertexAt(1, p1, 0, 0, 0);
                PL2.AddVertexAt(2, p2, 0, 0, 0);
                PL2.AddVertexAt(3, p3, 0, 0, 0);
                PL2.Layer = "图框";
                PL2.ColorIndex = 1;
                PL2.Closed = true;
                ps.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);               

                tr.Commit();
            }
            
            // Zoom so that we can see our new layout, again with a little padding

            ed.Command("_.ZOOM", "_E");
            ed.Command("_.ZOOM", ".7X");
            ed.Regen();
        }



        // Returns whether the provided DB extents - retrieved from

        // Database.Extmin/max - are "valid" or whether they are the default

        // invalid values (where the min's coordinates are positive and the

        // max coordinates are negative)



        private bool ValidDbExtents(Point3d min, Point3d max)
        {
            return
              !(min.X > 0 && min.Y > 0 && min.Z > 0 && max.X < 0 && max.Y < 0 && max.Z < 0);
        }
    }
}