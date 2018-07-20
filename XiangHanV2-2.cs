using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [CommandMethod("d2")]
        public static void Draw2(double yy)
        {
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
                Point2d BB = new Point2d(0, yy);
                double delt1 = 1000.0 * (height[0] - height[3]);
                double delt2 = 1000.0 * (height[1] - height[3]);
                double delt3 = 1000.0 * (height[2] - height[3]);
                double L = Layout[0] * Layout[1];
                slop = Layout[3];
                Line axixYY = new Line(Convert3d(BB, 0, -800), Convert3d(BB, 0, sectiont[3] + delt2 + 1200.0));
                axixYY.Layer = "中心线";
                axixYY.LinetypeScale = 100;
                modelSpace.AppendEntity(axixYY);
                tr.AddNewlyCreatedDBObject(axixYY, true);
                //-------------------------------------------------------------------------------------------
                Line L1 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + 0.5 * L * slop, 0.0));
                Line L2 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[3] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[3] + 0.5 * L * slop, 0.0));
                Line L3 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[2] - sectiont[4] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[2] - sectiont[4] + 0.5 * L * slop, 0.0));
                Line L4 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[2] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[2] + 0.5 * L * slop, 0.0));
                foreach (Line aa in new List<Line> { L1, L2, L3, L4 })
                {
                    aa.Layer = "粗线";
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }
                foreach (int ii in Enumerable.Range(0, (int)Layout[0] + 1))
                {
                    double dx = -0.5 * L + ii * Layout[1];
                    double y1 = BB.Y + dx * slop;
                    double y2 = BB.Y + Sect[2] + dx * slop;
                    Line aa = new Line(new Point3d(BB.X + dx, y1, 0), new Point3d(BB.X + dx, y2, 0));
                    aa.Layer = "细线";
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }
                Point2d p0 = Convert2d(L4.StartPoint);
                Point2d p1 = Convert2d(p0, 0, Layout[5]);
                Point2d p2 = Convert2d(p1, Layout[4], 0);
                Point2d p3f = Convert2d(p2, 0, -10000);
                Point2d p3 = new Line2d(p2, p3f).IntersectWith(ConvertLine2d(L4))[0];
                Polyline PL1 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                p0 = Convert2d(L4.EndPoint);
                p1 = Convert2d(p0, 0, Layout[5]);
                p2 = Convert2d(p1, -Layout[4], 0);
                p3f = Convert2d(p2, 0, -10000);
                p3 = new Line2d(p2, p3f).IntersectWith(ConvertLine2d(L4))[0];
                Polyline PL2 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL2.AddVertexAt(0, p0, 0, 0, 0);
                PL2.AddVertexAt(1, p1, 0, 0, 0);
                PL2.AddVertexAt(2, p2, 0, 0, 0);
                PL2.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);
                //-------------------------------------------------------------------------------------------
                p0 = PL1.GetPoint2dAt(1);
                p1 = Convert2d(p0,  -Wall_leftE[0],0);
                p2 = Convert2d(BB, (-0.5 * L - Wall_leftE[2]), 
                    (-0.5 * L - Wall_leftE[2]) * slop + Wall_leftE[3] + Wall_leftE[1]);
                p3 = Convert2d(BB, (-0.5 * L - Wall_leftE[2]),
                    (-0.5 * L - Wall_leftE[2]) * slop + Wall_leftE[3]);
                Point2d p4 = Convert2d(p3, Wall_leftE[2], Wall_leftE[2] * slop);
                Polyline PL3 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL3.AddVertexAt(0, p0, 0, 0, 0);
                PL3.AddVertexAt(1, p1, 0, 0, 0);
                PL3.AddVertexAt(2, p2, 0, 0, 0);
                PL3.AddVertexAt(3, p3, 0, 0, 0);
                PL3.AddVertexAt(4, p4, 0, 0, 0);                
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);
                p0 = PL2.GetPoint2dAt(1);
                p1 = Convert2d(p0, Wall_rightE[0], 0);
                p2 = Convert2d(BB, (0.5 * L + Wall_rightE[2]),
                    (+0.5 * L + Wall_rightE[2]) * slop + Sect[3] + Wall_rightE[1]);
                p3 = Convert2d(BB, (0.5 * L + Wall_rightE[2]),
                    (0.5 * L + Wall_rightE[2]) * slop + Sect[3]);
                p4 = Convert2d(p3, -Wall_rightE[2], -Wall_rightE[2] * slop);
                Polyline PL4 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL4.AddVertexAt(0, p0, 0, 0, 0);
                PL4.AddVertexAt(1, p1, 0, 0, 0);
                PL4.AddVertexAt(2, p2, 0, 0, 0);
                PL4.AddVertexAt(3, p3, 0, 0, 0);
                PL4.AddVertexAt(4, p4, 0, 0, 0);
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);

                
                p0 = PL4.GetPoint2dAt(4);
                p1 = PL4.GetPoint2dAt(3);
                p2 = Convert2d(p1, 0, -Wall_rightE[3]);
                p3 = Convert2d(p2,0, -Wall_rightE[5]);
                p4 = Convert2d(p3, -Wall_rightE[4],0);
                Line2d l2d_1 = new Line2d(p4, Convert2d(p4,0,1));
                Line2d l2d_2 = ConvertLine2d(L1);
                Point2d p5 = l2d_1.IntersectWith(l2d_2)[0];
                p5 = Convert2d(p5, 0, Sect[3]-Wall_rightE[3]);
                Point2d p6 = Convert2d(p0, 0, -Wall_rightE[3]);
                Polyline PL6 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL6.AddVertexAt(0, p0, 0, 0, 0);
                PL6.AddVertexAt(1, p1, 0, 0, 0);
                PL6.AddVertexAt(2, p2, 0, 0, 0);
                PL6.AddVertexAt(3, p3, 0, 0, 0);
                PL6.AddVertexAt(4, p4, 0, 0, 0);
                PL6.AddVertexAt(5, p5, 0, 0, 0);
                PL6.AddVertexAt(6, p6, 0, 0, 0);
                modelSpace.AppendEntity(PL6);
                tr.AddNewlyCreatedDBObject(PL6, true);
                p0 = PL3.GetPoint2dAt(4);
                p1 = PL3.GetPoint2dAt(3);
                p2 = Convert2d(p1, 0, -Wall_leftE[3]);
                p3 = Convert2d(p2, 0, -Wall_leftE[5]);
                p4 = Convert2d(p3, Wall_leftE[4], 0);
                l2d_1 = new Line2d(p4, Convert2d(p4, 0, 1));
                l2d_2 = ConvertLine2d(L1);
                p5 = l2d_1.IntersectWith(l2d_2)[0];
                p5 = Convert2d(p5, 0, Sect[3] - Wall_leftE[3]);
                p6 = Convert2d(p0, 0, -Wall_leftE[3]);

                Polyline PL5 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL5.AddVertexAt(0, p0, 0, 0, 0);
                PL5.AddVertexAt(1, p1, 0, 0, 0);
                PL5.AddVertexAt(2, p2, 0, 0, 0);
                PL5.AddVertexAt(3, p3, 0, 0, 0);
                PL5.AddVertexAt(4, p4, 0, 0, 0);
                PL5.AddVertexAt(5, p5, 0, 0, 0);
                PL5.AddVertexAt(6, p6, 0, 0, 0);
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);
                //-------------------------------------------------------------------------------------------
                p0 = Convert2d(BB, 0, -cLayer[0][1]);
                Line l3d_1 = new Line(Convert3d(p0), Convert3d(p0, 1, 1 * slop));
                Point3dCollection pts = new Point3dCollection();
                PL5.IntersectWith(l3d_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p0 =Convert2d(pts[1]);
                pts = new Point3dCollection();
                PL6.IntersectWith(l3d_1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p1 = Convert2d(pts[1]);
                p2 = PL6.GetPoint2dAt(5);
                p3 = PL5.GetPoint2dAt(5);
                Polyline H1 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                H1.AddVertexAt(0, p0, 0, 0, 0);
                H1.AddVertexAt(1, p1, 0, 0, 0);
                H1.AddVertexAt(2, p2, 0, 0, 0);
                H1.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(H1);
                tr.AddNewlyCreatedDBObject(H1, true);
                p0 = Convert2d(PL5.GetPoint2dAt(3), 0, -cLayer[0][1]);
                p1 = Convert2d(PL5.GetPoint2dAt(4), 0, -cLayer[0][1]);
                p2 = PL5.GetPoint2dAt(4);
                p3 = PL5.GetPoint2dAt(3);
                Polyline H2 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                H2.AddVertexAt(0, p0, 0, 0, 0);
                H2.AddVertexAt(1, p1, 0, 0, 0);
                H2.AddVertexAt(2, p2, 0, 0, 0);
                H2.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(H2);
                tr.AddNewlyCreatedDBObject(H2, true);

                p0 = Convert2d(PL6.GetPoint2dAt(3), 0, -cLayer[0][1]);
                p1 = Convert2d(PL6.GetPoint2dAt(4), 0, -cLayer[0][1]);
                p2 = PL6.GetPoint2dAt(4);
                p3 = PL6.GetPoint2dAt(3);
                Polyline H3 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                H3.AddVertexAt(0, p0, 0, 0, 0);
                H3.AddVertexAt(1, p1, 0, 0, 0);
                H3.AddVertexAt(2, p2, 0, 0, 0);
                H3.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(H3);
                tr.AddNewlyCreatedDBObject(H3, true);

                p0 = H2.GetPoint2dAt(1);
                p1 = H3.GetPoint2dAt(1);
                p2 = H1.GetPoint2dAt(1);
                p3 = H1.GetPoint2dAt(0);
                Polyline H4 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                H4.AddVertexAt(0, p0, 0, 0, 0);
                H4.AddVertexAt(1, p1, 0, 0, 0);
                H4.AddVertexAt(2, p2, 0, 0, 0);
                H4.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(H4);
                tr.AddNewlyCreatedDBObject(H4, true);

                Polyline C1 = new Polyline()
                {
                    Layer = "细线",
                    Closed = false,
                };
                p0 = new Point2d(BB.X - RoadWidth[0] - delt1 / RoadSlop[1] * RoadSlop[0], BB.Y);
                p1 = new Point2d(BB.X - RoadWidth[0], BB.Y + sectiont[3] + delt1);
                p2 = new Point2d(BB.X, BB.Y + sectiont[3] + delt2);
                p3 = new Point2d(BB.X + RoadWidth[1], BB.Y + sectiont[3] + delt3);
                p4 = new Point2d(BB.X + RoadWidth[1] + delt3 / RoadSlop[1] * RoadSlop[0], BB.Y);
                C1.AddVertexAt(0, p0, 0, 0, 0);
                C1.AddVertexAt(1, p1, 0, 0, 0);
                C1.AddVertexAt(2, p2, 0, 0, 0);
                C1.AddVertexAt(3, p3, 0, 0, 0);
                C1.AddVertexAt(4, p4, 0, 0, 0);
                pts = new Point3dCollection();
                C1.IntersectWith(L4, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                DBObjectCollection dbCs = C1.GetSplitCurves(pts);
                C1 = (Polyline)dbCs[1];
                modelSpace.AppendEntity(C1);
                tr.AddNewlyCreatedDBObject(C1, true);
                //-------------------------------------------------------------------------------------------
                // 填充
                //-------------------------------------------------------------------------------------------
                Hatch hatchref1 = new Hatch();
                hatchref1.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                Hatch hatchref2 = new Hatch();
                hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
                Hatch hatchref3 = new Hatch();
                hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
                List<Polyline> plist = new List<Polyline> { H2, H3 };
                foreach (Polyline pl in plist)
                {
                    ObjectIdCollection ids = new ObjectIdCollection { pl.ObjectId };
                    Hatch hatch = new Hatch();
                    modelSpace.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);
                    hatch.AppendLoop(HatchLoopTypes.Default, ids);
                    hatch.PatternScale = 20;
                    hatch.SetHatchPattern(hatchref1.PatternType, hatchref1.PatternName);
                    hatch.EvaluateHatch(true);
                }
                Hatch hatch2 = new Hatch();
                modelSpace.AppendEntity(hatch2);
                tr.AddNewlyCreatedDBObject(hatch2, true);
                hatch2.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { H1.ObjectId });
                hatch2.PatternScale = 1;
                hatch2.SetHatchPattern(hatchref3.PatternType, hatchref3.PatternName);
                hatch2.EvaluateHatch(true);
                Hatch hatch3 = new Hatch();
                modelSpace.AppendEntity(hatch3);
                tr.AddNewlyCreatedDBObject(hatch3, true);
                hatch3.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { H4.ObjectId });
                hatch3.PatternScale = 1;
                hatch3.SetHatchPattern(hatchref2.PatternType, hatchref2.PatternName);
                hatch3.EvaluateHatch(true);
                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------
                BiaoGao(height[3] - 0.001 * slop * 0.5 * L, L2.StartPoint, modelSpace, tr, blockTbl);
                BiaoGao(height[3] + 0.001 * slop * 0.5 * L, L2.EndPoint, modelSpace, tr, blockTbl);
                BiaoGao(height[3],
                    new Point3d((L2.StartPoint.X + L2.EndPoint.X) * 0.5, (L2.StartPoint.Y + L2.EndPoint.Y) * 0.5, 0), modelSpace, tr, blockTbl);
                BiaoGao(height[3] - 0.001 * slop * (0.5 * L + lengthB[0]), PL5.GetPoint3dAt(1), modelSpace, tr, blockTbl);
                BiaoGao(height[3] + 0.001 * slop * (0.5 * L + lengthB[0]), PL6.GetPoint3dAt(1), modelSpace, tr, blockTbl);
                BiaoGao(height[0], C1.GetPoint3dAt(1), modelSpace, tr, blockTbl);
                BiaoGao(height[1], C1.GetPoint3dAt(2), modelSpace, tr, blockTbl);
                BiaoGao(height[2], C1.GetPoint3dAt(3), modelSpace, tr, blockTbl);
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                var dsId = dst["1-100"];
                double ymax = delt2+Sect[3] + 1000;
                Point3d dimref1 = Convert3d(BB, 0, ymax);
                Point3d dimref2 = Convert3d(BB, -0.5 * L - Wall_leftE[2] - 600, 0);
                Point3d dimref3 = Convert3d(H2.GetPoint3dAt(0),0,-600);
                Point3d dimref4 = Convert3d(BB, 0, Sect[2]+800);
                RotatedDimension D1 = new RotatedDimension(0, PL5.GetPoint3dAt(1),PL5.GetPoint3dAt(0), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, L1.StartPoint,C1.GetPoint3dAt(1), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0,  C1.GetPoint3dAt(1),C1.GetPoint3dAt(2), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, C1.GetPoint3dAt(2), C1.GetPoint3dAt(3), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, C1.GetPoint3dAt(3),L1.EndPoint, dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, PL6.GetPoint3dAt(1), PL6.GetPoint3dAt(0), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                D1 = new RotatedDimension(Math.PI/2, PL3.GetPoint3dAt(2), PL3.GetPoint3dAt(3), dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                D1 = new RotatedDimension(Math.PI / 2, PL5.GetPoint3dAt(1), PL5.GetPoint3dAt(2), dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, PL5.GetPoint3dAt(2), PL5.GetPoint3dAt(3), dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                D1 = new RotatedDimension(Math.PI / 2, H2.GetPoint3dAt(2), H2.GetPoint3dAt(0), dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, H2.GetPoint3dAt(0), H2.GetPoint3dAt(1), dimref3, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, L4.StartPoint,L4.EndPoint, dimref4, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);


                LineAngularDimension2 AD1 = new LineAngularDimension2(
                    PL3.GetPoint3dAt(1), PL3.GetPoint3dAt(2),          
                    L4.StartPoint,L1.StartPoint,
                    Convert3d(PL3.GetPoint3dAt(1),-1, -1000), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);
                AD1 = new LineAngularDimension2(
                    PL4.GetPoint3dAt(1), PL4.GetPoint3dAt(2),
                    L4.EndPoint, L1.EndPoint,
                    Convert3d(PL4.GetPoint3dAt(1), 1, -1000), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);
                //-------------------------------------------------------------------------------------------
                double hp1 = 100 * Math.Abs((height[1] - height[0]) * 1000 / RoadWidth[0]);
                double hp2 = 100 * Math.Abs((height[2] - height[1]) * 1000 / RoadWidth[1]);
                HengPo(hp1, Convert3d(BB, -0.5 * RoadWidth[0], Sect[3] + delt2 + 100), height[0] < height[1], modelSpace, tr, blockTbl);
                HengPo(hp2, Convert3d(BB, 0.5 * RoadWidth[1], Sect[3] + delt2 + 100), height[1] < height[2], modelSpace, tr, blockTbl);
                HengPo(Math.Abs(100 * slop), Convert3d(BB, 1.5 * length[1], Sect[3] + 100), slop > 0, modelSpace, tr, blockTbl);
                //-------------------------------------------------------------------------------------------
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;
                double Gy0 = Sect[3]+delt2+2000;
                DBText T1 = new DBText();
                T1.TextString = "COUPE A-A";
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
                T2.AddVertexAt(0, Convert2d(BB, -1625, Gy0), 0, 10, 10);
                T2.AddVertexAt(1, Convert2d(BB, 1625, Gy0), 0, 10, 10);
                T2.Layer = "标注";
                modelSpace.AppendEntity(T2);
                tr.AddNewlyCreatedDBObject(T2, true);
                T2 = (Polyline)T2.Clone();
                T2.TransformBy(Matrix3d.Displacement(new Vector3d(0, -100, 0)));
                T2.GlobalWidth(0);
                modelSpace.AppendEntity(T2);
                tr.AddNewlyCreatedDBObject(T2, true);
                T1 = (DBText)T1.Clone();
                T1.Height = 400;
                T1.TransformBy(Matrix3d.Displacement(new Vector3d(0, -550, 0)));
                T1.TextString = "Ech:1/100";
                modelSpace.AppendEntity(T1);
                tr.AddNewlyCreatedDBObject(T1, true);
                //
                tr.Commit();
            }
        }
        //-------------------------------------------------------------------------------------------
        // END
        //-------------------------------------------------------------------------------------------






    }
}
