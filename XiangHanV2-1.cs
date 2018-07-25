using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;


[assembly: CommandClass(typeof(MyCAD1.HanDong))]


namespace MyCAD1
{
    public partial class HanDong
    {
        [CommandMethod("fa")]
        public static void Test3()
        {
           //Application excel = new Application();
           WFM.OpenFileDialog dialog = new WFM.OpenFileDialog();
           dialog.InitialDirectory = "d:\\";
           dialog.Filter = "ext files (*.txt)|*.txt|All files(*.*)|*>**";
           dialog.FilterIndex = 2;
           dialog.RestoreDirectory = true;
           if (dialog.ShowDialog() == WFM.DialogResult.OK)
           {
               string file = dialog.FileName;
           }
        }

        [CommandMethod("d1")]
        public static void Draw()
        {
            double res,Gy0;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //-------------------------------------------------------------------------------------------
                // 获取模型空间
                //-------------------------------------------------------------------------------------------
                BlockTable blockTbl = tr.GetObject(
                    db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(
                    blockTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                //-------------------------------------------------------------------------------------------
                // 平面布置图
                //-------------------------------------------------------------------------------------------
                Point2d BB = new Point2d(0, 0);
                Point3dCollection pts = new Point3dCollection();
                double L = Layout[0] * Layout[1];
                double ang = Layout[2] / 180 * Math.PI;
                Vector2d vec = new Vector2d(Math.Cos(ang), Math.Sin(ang));
                Line2d AxisX = new Line2d(BB, vec);                
                Line L1 = new Line(
                    Convert3d(BB,-0.5*L,-0.5*Sect[1]-0.5*L*Math.Tan(ang)),
                    Convert3d(BB,+0.5*L,-0.5*Sect[1]+0.5*L*Math.Tan(ang)));
                Line L2 = new Line(
                    Convert3d(BB, -0.5 * L, -0.5 * Sect[1] + Sect[5] - 0.5 * L * Math.Tan(ang)),
                    Convert3d(BB, +0.5 * L, -0.5 * Sect[1] + Sect[5] + 0.5 * L * Math.Tan(ang)));
                Line L3 = new Line(
                    Convert3d(BB, -0.5 * L, +0.5 * Sect[1] - Sect[5] - 0.5 * L * Math.Tan(ang)),
                    Convert3d(BB, +0.5 * L, +0.5 * Sect[1] - Sect[5] + 0.5 * L * Math.Tan(ang)));
                Line L4 = new Line(
                    Convert3d(BB, -0.5 * L, +0.5 * Sect[1] - 0.5 * L * Math.Tan(ang)),
                    Convert3d(BB, +0.5 * L, +0.5 * Sect[1] + 0.5 * L * Math.Tan(ang)));
                foreach (Line aa in new List<Line> { L1, L2, L3, L4 })
                {
                    aa.Layer = "虚线";
                    aa.LinetypeScale = 500;
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }


                foreach (int ii in Enumerable.Range(0, (int)Layout[0] + 1))
                {
                    double dx = - 0.5 * L + ii * Layout[1];
                    double y1 = BB.Y - 0.5 * Sect[1] + dx * Math.Tan(ang);
                    double y2 = BB.Y + 0.5 * Sect[1] + dx * Math.Tan(ang);
                    Line aa = new Line(new Point3d(BB.X+dx, y1, 0), new Point3d(BB.X + dx, y2, 0))
                    {
                        Layer = "虚线",
                        LinetypeScale = 500,
                    };
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }
                Polyline PL1 = new Polyline();
                PL1.Layer = "粗线";
                PL1.Closed = true;
                double dx1 = 0.5 * L;
                double dx2 = 0.5 * L - Layout[4];
                PL1.AddVertexAt(0, Convert2d(BB, -dx2, -0.5 * Sect[1] - dx2 * Math.Tan(ang)), 0, 0, 0);
                PL1.AddVertexAt(1, Convert2d(BB, -dx2, 0.5 * Sect[1] - dx2 * Math.Tan(ang)), 0, 0, 0);
                PL1.AddVertexAt(2, Convert2d(BB, -dx1, 0.5 * Sect[1] - dx1 * Math.Tan(ang)), 0, 0, 0);
                PL1.AddVertexAt(3, Convert2d(BB, -dx1, -0.5 * Sect[1] - dx1 * Math.Tan(ang)), 0, 0, 0);
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                Polyline PL2 = new Polyline();
                PL2.Layer = "粗线";
                PL2.Closed = true;
                PL2.AddVertexAt(0, Convert2d(BB, dx2, -0.5 * Sect[1] + dx2 * Math.Tan(ang)), 0, 0, 0);
                PL2.AddVertexAt(1, Convert2d(BB, dx2, 0.5 * Sect[1] + dx2 * Math.Tan(ang)), 0, 0, 0);
                PL2.AddVertexAt(2, Convert2d(BB, dx1, 0.5 * Sect[1] + dx1 * Math.Tan(ang)), 0, 0, 0);
                PL2.AddVertexAt(3, Convert2d(BB, dx1, -0.5 * Sect[1] + dx1 * Math.Tan(ang)), 0, 0, 0);
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);

                Point2d p0 = PL1.GetPoint2dAt(3);
                Point2d p1 = Convert2d(PL1.GetPoint2dAt(3), -Wall_leftE[2], 
                    -Wall_leftE[2]*Math.Tan(Wall_leftP[0]/180*Math.PI));
                Point2d p2 = Convert2d(p1, 0, Wall_leftP[2] / Math.Cos(Wall_leftP[0] / 180 * Math.PI));
                Line2d l2d_1 = new Line2d(p2, Convert2d(p2, Math.Cos(Wall_leftP[0] / 180 * Math.PI), 
                    Math.Sin(Wall_leftP[0] / 180 * Math.PI)));
                Line2d l2d_2 = new Line2d(Convert2d(L2.StartPoint), Convert2d(L2.StartPoint, -100, 0));
                Point2d p3= l2d_1.IntersectWith(l2d_2)[0];
                Point2d p4 = Convert2d(L2.StartPoint);
                Polyline PL3 = new Polyline();
                PL3.Layer = "粗线";
                PL3.Closed = true;
                PL3.AddVertexAt(0, p0, 0, 0, 0);
                PL3.AddVertexAt(1, p1, 0, 0, 0);
                PL3.AddVertexAt(2, p2, 0, 0, 0);
                PL3.AddVertexAt(3, p3, 0, 0, 0);
                PL3.AddVertexAt(4, p4, 0, 0, 0);
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);

                p0 = PL1.GetPoint2dAt(2);
                p1 = Convert2d(PL1.GetPoint2dAt(2), -Wall_leftE[2],
                    +Wall_leftE[2] * Math.Tan(Wall_leftP[1] / 180 * Math.PI));
                p2 = Convert2d(p1, 0,-Wall_leftP[2] / Math.Cos(Wall_leftP[1] / 180 * Math.PI));
                l2d_1 = new Line2d(p2, Convert2d(p2, Math.Cos(Wall_leftP[1] / 180 * Math.PI),
                    -Math.Sin(Wall_leftP[1] / 180 * Math.PI)));
                l2d_2 = new Line2d(Convert2d(L3.StartPoint), Convert2d(L3.StartPoint, -100, 0));
                p3 = l2d_1.IntersectWith(l2d_2)[0];
                p4 = Convert2d(L3.StartPoint);
                Polyline PL4 = new Polyline();
                PL4.Layer = "粗线";
                PL4.Closed = true;
                PL4.AddVertexAt(0, p0, 0, 0, 0);
                PL4.AddVertexAt(1, p1, 0, 0, 0);
                PL4.AddVertexAt(2, p2, 0, 0, 0);
                PL4.AddVertexAt(3, p3, 0, 0, 0);
                PL4.AddVertexAt(4, p4, 0, 0, 0);
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);

                p0 = PL2.GetPoint2dAt(3);
                p1 = Convert2d(PL2.GetPoint2dAt(3), Wall_rightE[2],
                    -Wall_rightE[2] * Math.Tan(Wall_rightP[0] / 180 * Math.PI));
                p2 = Convert2d(p1, 0, Wall_rightP[2] / Math.Cos(Wall_rightP[0] / 180 * Math.PI));
                l2d_1 = new Line2d(p2, Convert2d(p2, -Math.Cos(Wall_rightP[0] / 180 * Math.PI),
                    Math.Sin(Wall_rightP[0] / 180 * Math.PI)));
                l2d_2 = new Line2d(Convert2d(L2.EndPoint), Convert2d(L2.EndPoint, 100, 0));
                p3 = l2d_1.IntersectWith(l2d_2)[0];
                p4 = Convert2d(L2.EndPoint);

                Polyline PL5 = new Polyline();
                PL5.Layer = "粗线";
                PL5.Closed = true;
                PL5.AddVertexAt(0, p0, 0, 0, 0);
                PL5.AddVertexAt(1, p1, 0, 0, 0);
                PL5.AddVertexAt(2, p2, 0, 0, 0);
                PL5.AddVertexAt(3, p3, 0, 0, 0);
                PL5.AddVertexAt(4, p4, 0, 0, 0);
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);

                p0 = PL2.GetPoint2dAt(2);
                p1 = Convert2d(PL2.GetPoint2dAt(2), Wall_rightE[2],
                    +Wall_rightE[2] * Math.Tan(Wall_rightP[1] / 180 * Math.PI));
                p2 = Convert2d(p1, 0, -Wall_rightP[2] / Math.Cos(Wall_rightP[1] / 180 * Math.PI));
                l2d_1 = new Line2d(p2, Convert2d(p2, -Math.Cos(Wall_rightP[1] / 180 * Math.PI),
                    -Math.Sin(Wall_rightP[1] / 180 * Math.PI)));
                l2d_2 = new Line2d(Convert2d(L3.EndPoint), Convert2d(L3.EndPoint, -100, 0));
                p3 = l2d_1.IntersectWith(l2d_2)[0];
                p4 = Convert2d(L3.EndPoint);
                Polyline PL6 = new Polyline();
                PL6.Layer = "粗线";
                PL6.Closed = true;
                PL6.AddVertexAt(0, p0, 0, 0, 0);
                PL6.AddVertexAt(1, p1, 0, 0, 0);
                PL6.AddVertexAt(2, p2, 0, 0, 0);
                PL6.AddVertexAt(3, p3, 0, 0, 0);
                PL6.AddVertexAt(4, p4, 0, 0, 0);
                modelSpace.AppendEntity(PL6);
                tr.AddNewlyCreatedDBObject(PL6, true);

                Line L5 = new Line(PL3.GetPoint3dAt(1), PL4.GetPoint3dAt(1));
                Line L6 = new Line(PL3.GetPoint3dAt(1), Convert3d(PL3.GetPoint2dAt(1),Wall_leftE[4],0));
                Line L7 = new Line(PL4.GetPoint3dAt(1), Convert3d(PL4.GetPoint2dAt(1), Wall_leftE[4], 0));
                pts = new Point3dCollection();
                Line l_1 = new Line(L6.EndPoint,L7.EndPoint);
                PL4.IntersectWith(l_1, Intersect.ExtendBoth,pts,IntPtr.Zero,IntPtr.Zero);
                Line L8 = new Line(L7.EndPoint, pts[0]);
                pts = new Point3dCollection();
                PL3.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line L10 = new Line(L6.EndPoint, pts[0]);
                Line L9 = new Line(L8.EndPoint, L10.EndPoint);
                l_1 = new Line(Convert3d(PL1.GetPoint2dAt(2), -Wall_leftE[0]),
                    Convert3d(PL1.GetPoint2dAt(3), -Wall_leftE[0]));
                pts = new Point3dCollection();
                PL3.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line L12 = new Line(pts[0], pts[1]);
                pts = new Point3dCollection();
                PL4.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line L11 = new Line(pts[0], pts[1]);
                Line R5 = new Line(PL5.GetPoint3dAt(1), PL6.GetPoint3dAt(1));
                Line R6 = new Line(PL5.GetPoint3dAt(1), Convert3d(PL5.GetPoint2dAt(1), -Wall_rightE[4], 0));
                Line R7 = new Line(PL6.GetPoint3dAt(1), Convert3d(PL6.GetPoint2dAt(1), -Wall_rightE[4], 0));
                pts = new Point3dCollection();
                l_1 = new Line(R6.EndPoint, R7.EndPoint);
                PL6.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line R8 = new Line(R7.EndPoint, pts[0]);
                pts = new Point3dCollection();
                PL5.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line R10 = new Line(R6.EndPoint, pts[0]);
                Line R9 = new Line(R8.EndPoint, R10.EndPoint);
                l_1 = new Line(Convert3d(PL2.GetPoint2dAt(2), Wall_rightE[0]),
                    Convert3d(PL2.GetPoint2dAt(3), Wall_rightE[0]));
                pts = new Point3dCollection();
                PL5.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line R12 = new Line(pts[0], pts[1]);
                pts = new Point3dCollection();
                PL6.IntersectWith(l_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                Line R11 = new Line(pts[0], pts[1]);

                foreach (Line aa in new List<Line> { L5, L6, L7, L8,L10,L11,L12, R5, R6, R7, R8, R10, R11, R12 })
                {
                    aa.Layer = "粗线";
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }

                L9.Layer = "虚线";
                L9.LinetypeScale = 5;
                modelSpace.AppendEntity(L9);
                tr.AddNewlyCreatedDBObject(L9, true);
                R9.Layer = "虚线";
                R9.LinetypeScale = 5;
                modelSpace.AppendEntity(R9);
                tr.AddNewlyCreatedDBObject(R9, true);

                double dy=Math.Max(L5.Length,R5.Length)+3000;

                Line B1 =new Line();
                B1.StartPoint = Convert3d(L5.GetMidPoint3d(), 0, 0.5 * dy);
                B1.EndPoint = Convert3d(L5.GetMidPoint3d(), 0, -0.5 * dy);
                B1.Layer = "细线";
                B1.LinetypeScale = 5;
                modelSpace.AppendEntity(B1);
                tr.AddNewlyCreatedDBObject(B1, true);

                Line B2 = new Line();
                B2.StartPoint = Convert3d(BB, 0, 0.5 * dy);
                B2.EndPoint = Convert3d(BB, 0, -0.5 * dy);
                B2.Layer = "中心线";
                B2.LinetypeScale = 5;
                modelSpace.AppendEntity(B2);
                tr.AddNewlyCreatedDBObject(B2, true);

                Line B3 = new Line();
                B3.StartPoint = Convert3d(R5.GetMidPoint3d(), 0, 0.5 * dy);
                B3.EndPoint = Convert3d(R5.GetMidPoint3d(), 0, -0.5 * dy);
                B3.Layer = "细线";
                B3.LinetypeScale = 5;
                modelSpace.AppendEntity(B3);
                tr.AddNewlyCreatedDBObject(B3, true);

                Line B4 = B2.Clone() as Line;
                B4.TransformBy(Matrix3d.Displacement(new Vector3d(-RoadWidth[0], 0, 0)));
                pts = new Point3dCollection();
                B4.IntersectWith(L4, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p2 = Convert2d(pts[0]);
                pts = new Point3dCollection();
                B4.IntersectWith(L1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p1 = Convert2d(pts[0]);
                B4.StartPoint = Convert3d(p1);
                B4.EndPoint = Convert3d(p2);
                Point3d pt2 = B4.GetMidPoint3d();
                p3 = Convert2d(pt2, 0, 0.5 * dy);
                p0 = Convert2d(pt2, 0, -0.5 * dy);
                Polyline BL4 = new Polyline();
                BL4.AddVertexAt(0, p0, 0, 0, 0);
                BL4.AddVertexAt(1, p1, 0, 0, 0);
                BL4.AddVertexAt(2, p2, 0, 0, 0);
                BL4.AddVertexAt(3, p3, 0, 0, 0);
                BL4.Layer = "细线";
                modelSpace.AppendEntity(BL4);
                tr.AddNewlyCreatedDBObject(BL4, true);

                B4 = B2.Clone() as Line;
                B4.TransformBy(Matrix3d.Displacement(new Vector3d(RoadWidth[1], 0, 0)));
                pts = new Point3dCollection();
                B4.IntersectWith(L4, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p2 = Convert2d(pts[0]);
                pts = new Point3dCollection();
                B4.IntersectWith(L1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p1 = Convert2d(pts[0]);
                B4.StartPoint = Convert3d(p1);
                B4.EndPoint = Convert3d(p2);
                pt2 = B4.GetMidPoint3d();
                p3 = Convert2d(pt2, 0, 0.5 * dy);
                p0 = Convert2d(pt2, 0, -0.5 * dy);
                Polyline BL5 = new Polyline();
                BL5.AddVertexAt(0, p0, 0, 0, 0);
                BL5.AddVertexAt(1, p1, 0, 0, 0);
                BL5.AddVertexAt(2, p2, 0, 0, 0);
                BL5.AddVertexAt(3, p3, 0, 0, 0);
                BL5.Layer = "细线";
                modelSpace.AppendEntity(BL5);
                tr.AddNewlyCreatedDBObject(BL5, true);

                foreach (Polyline aa in new List<Polyline>() { BL4, BL5 })
                {
                    int npts = (int)aa.Length / 200;
                    foreach (int i in Enumerable.Range(1, npts))
                    {
                        int sig = (aa == BL4 ? -1 : 1);
                        double[] startA = { aa.GetPoint2dAt(0).X, aa.GetPoint2dAt(0).Y + 200 * i, 0 };
                        double[] endA = { 0, 0, 0 };
                        if (i % 2 == 0)
                        {
                            endA = new double[] { aa.GetPoint2dAt(0).X + sig * 400, aa.GetPoint2dAt(0).Y + 200 * i, 0 };
                        }
                        else
                        {
                            endA = new double[] { aa.GetPoint2dAt(0).X + sig * 200, aa.GetPoint2dAt(0).Y + 200 * i, 0 };
                        }
                        if (startA[1] < aa.GetPoint2dAt(1).Y || startA[1] > aa.GetPoint2dAt(2).Y)
                        {
                            Line LA = new Line(new Point3d(startA), new Point3d(endA));
                            LA.Layer = "细线";
                            modelSpace.AppendEntity(LA);
                            tr.AddNewlyCreatedDBObject(LA, true);
                        }
                    }
                }
                Polyline BL6 = new Polyline();
                BL6.AddVertexAt(0, Convert2d(B1.EndPoint), 0, 0, 0);
                BL6.AddVertexAt(1, BL4.GetPoint2dAt(0), 0, 0, 0);
                BL6.AddVertexAt(2, BL5.GetPoint2dAt(0), 0, 0, 0);
                BL6.AddVertexAt(3, Convert2d(B3.EndPoint), 0, 0, 0);
                BL6.Layer = "虚线";
                BL6.LinetypeScale = 5;
                modelSpace.AppendEntity(BL6);
                tr.AddNewlyCreatedDBObject(BL6, true);

                Polyline BL7 = new Polyline();
                BL7.AddVertexAt(0, Convert2d(B1.StartPoint), 0, 0, 0);
                BL7.AddVertexAt(1, BL4.GetPoint2dAt(3), 0, 0, 0);
                BL7.AddVertexAt(2, BL5.GetPoint2dAt(3), 0, 0, 0);
                BL7.AddVertexAt(3, Convert2d(B3.StartPoint), 0, 0, 0);
                BL7.Layer = "虚线";
                BL7.LinetypeScale = 5;
                modelSpace.AppendEntity(BL7);
                tr.AddNewlyCreatedDBObject(BL7, true);

                Line CenterX = (Line)L1.Clone();
                CenterX.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0.5 * Sect[1], 0)));
                pts = new Point3dCollection();
                CenterX.IntersectWith(L5, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                CenterX.StartPoint = pts[0];
                pts = new Point3dCollection();
                CenterX.IntersectWith(R5, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                CenterX.EndPoint = pts[0];
                CenterX.Layer = "中心线";
                CenterX.LinetypeScale = 5;
                modelSpace.AppendEntity(CenterX);
                tr.AddNewlyCreatedDBObject(CenterX, true);
                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                var dsId = dst["1-100"];
                double ymin = Math.Min(L6.StartPoint.Y, R6.StartPoint.Y) - 500;
                Point3d dimref1 = Convert3d(BB, 0, ymin);
                Point3d dimref2 = Convert3d(BB, -0.5 * L - Wall_leftE[2] - 600, 0);
                Point3d dimref3 = Convert3d(BB, -0.5 * L + 600, 0);
                Point3d dimref4 = Convert3d(BB, 0, 500);
                RotatedDimension D1 = new RotatedDimension(0, L6.StartPoint, L6.EndPoint, dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, L6.EndPoint, PL3.GetPoint3dAt(0), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, PL3.GetPoint3dAt(0), PL5.GetPoint3dAt(0), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, PL5.GetPoint3dAt(0),R6.EndPoint, dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, R6.EndPoint, R6.StartPoint, dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, L5.EndPoint, L5.StartPoint, dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, L1.StartPoint, L2.StartPoint, dimref3, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, L2.StartPoint, L3.StartPoint, dimref3, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, L3.StartPoint, L4.StartPoint, dimref3, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                LineAngularDimension2 AD1 = new LineAngularDimension2(CenterX.StartPoint,CenterX.EndPoint,
                    B2.StartPoint,B2.EndPoint, Convert3d(BB,200,200), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                AD1 = new LineAngularDimension2()
                {
                    XLine2Start=PL3.GetPoint3dAt(3),
                    XLine2End=Convert3d(PL3.GetPoint3dAt(3), -500, 0),
                    XLine1Start= PL3.GetPoint3dAt(3),
                    XLine1End=PL3.GetPoint3dAt(2),
                    ArcPoint = Convert3d(PL3.GetPoint3dAt(3), -2000, -1), 
                    Layer = "标注",
                    DimensionStyle = dsId,
                };               
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                AD1 = new LineAngularDimension2(PL4.GetPoint3dAt(3), Convert3d(PL4.GetPoint3dAt(3) , -100, 0),
                    PL4.GetPoint3dAt(3), PL4.GetPoint3dAt(2),Convert3d(PL4.GetPoint3dAt(3), -2000, 1), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                AD1 = new LineAngularDimension2(PL5.GetPoint3dAt(3), Convert3d(PL5.GetPoint3dAt(3), 100, 0),
                    PL5.GetPoint3dAt(3), PL5.GetPoint3dAt(2),  Convert3d(PL5.GetPoint3dAt(3),2000, -1), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);
                AD1 = new LineAngularDimension2(
                    PL6.GetPoint3dAt(3), PL6.GetPoint3dAt(2),
                    PL6.GetPoint3dAt(3), Convert3d(PL6.GetPoint3dAt(3), 100, 0),                    
                    Convert3d(PL6.GetPoint3dAt(3), 2000, 1), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);


 

                Gy0 = Math.Max(B1.StartPoint.Y, B3.StartPoint.Y) + 800;
                double Gy1= Math.Min(B1.EndPoint.Y, B3.EndPoint.Y) -1500;
                res = Gy1 - 2000 - (height[1] - height[3]) * 1000.0 - Sect[3];
                
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;
                DBText T1 = new DBText();
                T1.TextString = "VUE EN PLAN";
                T1.Position = Convert3d(BB, 0, Gy0+50);
                T1.HorizontalMode = TextHorizontalMode.TextCenter;
                T1.VerticalMode = TextVerticalMode.TextBottom;
                T1.AlignmentPoint = T1.Position;
                T1.Height = 500;
                T1.TextStyleId = st["fsdb"];
                T1.Layer = "标注";
                modelSpace.AppendEntity(T1);
                tr.AddNewlyCreatedDBObject(T1, true);
                Polyline T2 = new Polyline();
                T2.AddVertexAt(0, Convert2d(BB, -1800, Gy0), 0, 10, 10);
                T2.AddVertexAt(1, Convert2d(BB, 1800, Gy0), 0, 10, 10);
                T2.Layer = "标注";                
                modelSpace.AppendEntity(T2);
                tr.AddNewlyCreatedDBObject(T2, true);
                T2 = (Polyline)T2.Clone();
                T2.TransformBy(Matrix3d.Displacement(new Vector3d(0, -100, 0)));
                T2.GlobalWidth(0);
                modelSpace.AppendEntity(T2);
                tr.AddNewlyCreatedDBObject(T2, true);
                T1 =(DBText) T1.Clone();
                T1.Height = 400;
                T1.TransformBy(Matrix3d.Displacement(new Vector3d(0, -550, 0)));
                T1.TextString = "Ech:1/100";
                modelSpace.AppendEntity(T1);
                tr.AddNewlyCreatedDBObject(T1, true);
                

                //
                tr.Commit();
            }
            Draw2(res);
           // Commands.CreateLayout(Gy0);
        }
        //-------------------------------------------------------------------------------------------
        // END
        //-------------------------------------------------------------------------------------------




    }
}
