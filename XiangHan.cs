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
        [CommandMethod("w1")]
        public static void Plan()
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
                // 绘图
                //-------------------------------------------------------------------------------------------
                Point2d BB = new Point2d(0, 10000);
                double L = length[0] * length[1];

                Line P1 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y, 0.0), new Point3d(BB.X + 0.5 * L, BB.Y, 0.0));
                Line P2 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[5], 0.0), new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[5], 0.0));
                Line P3 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[1] - sectiont[5], 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[1] - sectiont[5], 0.0));
                Line P4 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[1], 0.0), new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[1], 0.0));
                List<Line> list1 = new List<Line> { P1, P2, P3, P4 };
                foreach (Line aa in list1)
                {
                    aa.Layer = "虚线";
                    aa.LinetypeScale = 500;
                    modelSpace.AppendEntity(aa);
                    tr.AddNewlyCreatedDBObject(aa, true);
                }
                foreach (int ii in Enumerable.Range(0, length[0] + 1))
                {
                    double x0 = BB.X - 0.5 * L + ii * length[1];
                    double y1 = BB.Y;
                    double y2 = BB.Y + sectiont[1];
                    Line L1 = new Line(new Point3d(x0, y1, 0), new Point3d(x0, y2, 0))
                    {
                        Layer = "虚线",
                        LinetypeScale = 500,
                    };
                    modelSpace.AppendEntity(L1);
                    tr.AddNewlyCreatedDBObject(L1, true);
                }

                Polyline PL1 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL1.AddVertexAt(0, new Point2d((BB.X - 0.5 * L + 200.0), BB.Y), 0, 0, 0);
                PL1.AddVertexAt(1, new Point2d((BB.X - 0.5 * L + 200.0), BB.Y + sectiont[1]), 0, 0, 0);
                PL1.AddVertexAt(2, new Point2d((BB.X - 0.5 * L), BB.Y + sectiont[1]), 0, 0, 0);
                PL1.AddVertexAt(3, new Point2d((BB.X - 0.5 * L), BB.Y), 0, 0, 0);
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);

                Polyline PL2 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL2.AddVertexAt(0, new Point2d((BB.X + 0.5 * L - 200.0), BB.Y), 0, 0, 0);
                PL2.AddVertexAt(1, new Point2d((BB.X + 0.5 * L - 200.0), BB.Y + sectiont[1]), 0, 0, 0);
                PL2.AddVertexAt(2, new Point2d((BB.X + 0.5 * L), BB.Y + sectiont[1]), 0, 0, 0);
                PL2.AddVertexAt(3, new Point2d((BB.X + 0.5 * L), BB.Y), 0, 0, 0);
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);

                Line3d axixX = new Line3d(new Point3d(BB.X - 0.5 * L - lengthB[0], BB.Y + 0.5 * sectiont[1], 0),//水平中心线
                    new Point3d(BB.X + 0.5 * L + lengthB[1], BB.Y + 0.5 * sectiont[1], 0));
                Line3d axixY = new Line3d(new Point3d(BB.X, BB.Y - 3000.0, 0), new Point3d(BB.X, BB.Y + sectiont[1] + 3000.0, 0));

                Line axixXX = new Line(new Point3d(BB.X - 0.5 * L - lengthB[0], BB.Y + 0.5 * sectiont[1], 0),//水平中心线
                    new Point3d(BB.X + 0.5 * L + lengthB[1], BB.Y + 0.5 * sectiont[1], 0));
                axixXX.Layer = "中心线";
                axixXX.LinetypeScale = 500;
                modelSpace.AppendEntity(axixXX);
                tr.AddNewlyCreatedDBObject(axixXX, true);
                Line axixYY = new Line(new Point3d(BB.X, BB.Y - 3000.0, 0), new Point3d(BB.X, BB.Y + sectiont[1] + 3000.0, 0));
                axixYY.Layer = "中心线";
                axixYY.LinetypeScale = 500;
                modelSpace.AppendEntity(axixYY);
                tr.AddNewlyCreatedDBObject(axixYY, true);

                Polyline PL3 = new Polyline()//左下八字墙
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL3.AddVertexAt(0, new Point2d(BB.X - 0.5 * L - 67, BB.Y + sectiont[5]), 0, 0, 0);
                PL3.AddVertexAt(1, new Point2d(BB.X - 0.5 * L, BB.Y + sectiont[5]), 0, 0, 0);
                PL3.AddVertexAt(2, new Point2d(BB.X - 0.5 * L, BB.Y), 0, 0, 0);
                PL3.AddVertexAt(3, new Point2d(BB.X - 0.5 * L - lengthB[0], BB.Y - Math.Tan(30.0 * Math.PI / 180) * lengthB[0]), 0, 0, 0);
                PL3.AddVertexAt(4, new Point2d(BB.X - 0.5 * L - lengthB[0],
                    BB.Y - Math.Tan(30.0 * Math.PI / 180) * lengthB[0] + sectiont[5] / Math.Cos(30 * Math.PI / 180)), 0, 0, 0);
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);
                Polyline PL4 = (Polyline)PL3.Clone();
                PL4.TransformBy(Matrix3d.Mirroring(axixX));
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);

                Polyline PL5 = (Polyline)PL4.Clone();
                PL5.TransformBy(Matrix3d.Mirroring(axixY));
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);
                Polyline PL6 = (Polyline)PL3.Clone();
                PL6.TransformBy(Matrix3d.Mirroring(axixY));
                modelSpace.AppendEntity(PL6);
                tr.AddNewlyCreatedDBObject(PL6, true);

                Line L31 = new Line(PL3.GetPoint3dAt(3), PL4.GetPoint3dAt(3));
                L31.Layer = "粗线";
                modelSpace.AppendEntity(L31);
                tr.AddNewlyCreatedDBObject(L31, true);
                Line L32 = new Line(PL5.GetPoint3dAt(3), PL6.GetPoint3dAt(3));
                L32.Layer = "粗线";
                modelSpace.AppendEntity(L32);
                tr.AddNewlyCreatedDBObject(L32, true);

                Line L41 = new Line(PL3.GetPoint3dAt(3), PL3.GetPoint3dAt(3).TransformBy(Matrix3d.Displacement(new Vector3d(250, 0, 0))));
                L41.Layer = "粗线";
                modelSpace.AppendEntity(L41);
                tr.AddNewlyCreatedDBObject(L41, true);
                Line L42 = (Line)L41.Clone();
                L42.TransformBy(Matrix3d.Mirroring(axixX));
                modelSpace.AppendEntity(L42);
                tr.AddNewlyCreatedDBObject(L42, true);
                Line L43 = (Line)L41.Clone();
                L43.TransformBy(Matrix3d.Mirroring(axixY));
                modelSpace.AppendEntity(L43);
                tr.AddNewlyCreatedDBObject(L43, true);
                Line L44 = (Line)L42.Clone();
                L44.TransformBy(Matrix3d.Mirroring(axixY));
                modelSpace.AppendEntity(L44);
                tr.AddNewlyCreatedDBObject(L44, true);

                Line L51 = new Line(L41.EndPoint, L42.EndPoint);
                L51.Layer = "虚线";
                L51.LinetypeScale = 500;
                modelSpace.AppendEntity(L51);
                tr.AddNewlyCreatedDBObject(L51, true);

                Line L52 = new Line(L43.EndPoint, L44.EndPoint);
                L52.Layer = "虚线";
                L52.LinetypeScale = 500;
                modelSpace.AppendEntity(L52);
                tr.AddNewlyCreatedDBObject(L52, true);

                Line L4 = (Line)axixXX.Clone();
                L4.Layer = "虚线";
                L4.TransformBy(Matrix3d.Displacement(new Vector3d(0, -0.5 * sectiont[1] - 3000, 0)));
                modelSpace.AppendEntity(L4);
                tr.AddNewlyCreatedDBObject(L4, true);


                Line L5 = (Line)axixXX.Clone();
                L5.Layer = "虚线";
                L5.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0.5 * sectiont[1] + 3000, 0)));
                modelSpace.AppendEntity(L5);
                tr.AddNewlyCreatedDBObject(L5, true);

                Line L6 = new Line(L4.StartPoint, L5.StartPoint);
                L6.Layer = "细线";
                modelSpace.AppendEntity(L6);
                tr.AddNewlyCreatedDBObject(L6, true);

                Line L7 = new Line(L4.EndPoint, L5.EndPoint);
                L7.Layer = "细线";
                modelSpace.AppendEntity(L7);
                tr.AddNewlyCreatedDBObject(L7, true);


                Line L8 = (Line)axixYY.Clone();
                L8.TransformBy(Matrix3d.Displacement(new Vector3d(-RoadWidth[0], 0, 0)));
                L8.Layer = "细线";
                modelSpace.AppendEntity(L8);
                tr.AddNewlyCreatedDBObject(L8, true);


                Line L9 = (Line)axixYY.Clone();
                L9.TransformBy(Matrix3d.Displacement(new Vector3d(RoadWidth[1], 0, 0)));
                L9.Layer = "细线";
                modelSpace.AppendEntity(L9);
                tr.AddNewlyCreatedDBObject(L9, true);

                double leng = axixYY.Length;
                int npts = (int)leng / 200;
                foreach (int i in Enumerable.Range(1, npts))
                {
                    double[] startA = { BB.X - RoadWidth[0], BB.Y - 3000.0 + 200 * i, 0 };
                    double[] startB = { BB.X + RoadWidth[1], BB.Y - 3000.0 + 200 * i, 0 };
                    double[] endA = { 0, 0, 0 };
                    double[] endB = { 0, 0, 0 };
                    if (i % 2 == 0)
                    {
                        endA = new double[] { BB.X - RoadWidth[0] - 400, BB.Y - 3000 + 200 * i, 0 };
                        endB = new double[] { BB.X + RoadWidth[1] + 400, BB.Y - 3000 + 200 * i, 0 };
                    }
                    else
                    {
                        endA = new double[] { BB.X - RoadWidth[0] - 200, BB.Y - 3000 + 200 * i, 0 };
                        endB = new double[] { BB.X + RoadWidth[1] + 200, BB.Y - 3000 + 200 * i, 0 };
                    }
                    if (startA[1] < BB.Y || startA[1] > BB.Y + sectiont[1])
                    {
                        Line LA = new Line(new Point3d(startA), new Point3d(endA));
                        Line LB = new Line(new Point3d(startB), new Point3d(endB));
                        LA.Layer = "细线";
                        LB.Layer = "细线";
                        modelSpace.AppendEntity(LA);
                        tr.AddNewlyCreatedDBObject(LA, true);
                        modelSpace.AppendEntity(LB);
                        tr.AddNewlyCreatedDBObject(LB, true);
                    }
                }
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                ObjectId dsId = dst["1-100"];
                Point3d dimref = Convert3d(BB, 0, -2200);
                Point3d dimref2 = Convert3d(BB, -0.5 * L - lengthB[0] - 800, 0);
                Point3d dimref3 = Convert3d(BB, -RoadWidth[0] + 500, 0);
                Point3d dimref4 = Convert3d(BB, 0, 500);


                RotatedDimension D1 = new RotatedDimension(0, L41.StartPoint, L41.EndPoint, dimref, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                RotatedDimension D2 = new RotatedDimension(0, L41.EndPoint, PL3.GetPoint3dAt(2), dimref, "", dsId);
                D2.Layer = "标注";
                modelSpace.AppendEntity(D2);
                tr.AddNewlyCreatedDBObject(D2, true);

                RotatedDimension D3 = new RotatedDimension(0, PL3.GetPoint3dAt(2), PL6.GetPoint3dAt(2), dimref, "", dsId);
                D3.Layer = "标注";
                modelSpace.AppendEntity(D3);
                tr.AddNewlyCreatedDBObject(D3, true);

                RotatedDimension D4 = new RotatedDimension(0, PL6.GetPoint3dAt(2), L43.EndPoint, dimref, "", dsId);
                D4.Layer = "标注";
                modelSpace.AppendEntity(D4);
                tr.AddNewlyCreatedDBObject(D4, true);

                RotatedDimension D5 = new RotatedDimension(0, L43.EndPoint, L43.StartPoint, dimref, "", dsId);
                D5.Layer = "标注";
                modelSpace.AppendEntity(D5);
                tr.AddNewlyCreatedDBObject(D5, true);

                RotatedDimension D6 = new RotatedDimension(Math.PI / 2, L31.EndPoint, L31.StartPoint, dimref2, "", dsId);
                D6.Layer = "标注";
                modelSpace.AppendEntity(D6);
                tr.AddNewlyCreatedDBObject(D6, true);

                RotatedDimension D7 = new RotatedDimension(Math.PI / 2, P1.StartPoint, P2.StartPoint, dimref3, "", dsId);
                D7.Layer = "标注";
                modelSpace.AppendEntity(D7);
                tr.AddNewlyCreatedDBObject(D7, true);
                RotatedDimension D8 = new RotatedDimension(Math.PI / 2, P2.StartPoint, P3.StartPoint, dimref3, "", dsId);
                D8.Layer = "标注";
                modelSpace.AppendEntity(D8);
                tr.AddNewlyCreatedDBObject(D8, true);
                RotatedDimension D9 = new RotatedDimension(Math.PI / 2, P3.StartPoint, P4.StartPoint, dimref3, "", dsId);
                D9.Layer = "标注";
                modelSpace.AppendEntity(D9);
                tr.AddNewlyCreatedDBObject(D9, true);

                RotatedDimension D10 = new RotatedDimension(0, PL1.GetPoint3dAt(1), PL1.GetPoint3dAt(2), dimref4, "", dsId);
                D10.Layer = "标注";
                modelSpace.AppendEntity(D10);
                tr.AddNewlyCreatedDBObject(D10, true);

                RotatedDimension D11 = new RotatedDimension(-60.0 / 180.0 * Math.PI, PL3.GetPoint3dAt(0), PL3.GetPoint3dAt(2),
                    Convert3d(PL3.GetPoint2dAt(0), -800, 0), "", dsId);
                D11.Layer = "标注";
                modelSpace.AppendEntity(D11);
                tr.AddNewlyCreatedDBObject(D11, true);

                //
                tr.Commit();
            }
        }































        [CommandMethod("w3")]
        public static void CoupeB()
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
                // 绘图
                //-------------------------------------------------------------------------------------------
                double delt1 = 1000.0 * (height[0] - height[3]);
                double delt2 = 1000.0 * (height[1] - height[3]);
                double delt3 = 1000.0 * (height[2] - height[3]);
                Point2d BB = new Point2d(20000 , 0 );
                Line3d axixY = new Line3d(new Point3d(BB.X + sectiont[0] * 0.5, BB.Y - 3000.0, 0),
                    new Point3d(BB.X + sectiont[0] * 0.5, BB.Y + 3000.0, 0));
                Polyline P1 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                P1.SetDatabaseDefaults();
                P1.AddVertexAt(0, new Point2d((BB.X), (BB.Y) ), 0, 0, 0);
                P1.AddVertexAt(1, new Point2d((BB.X + sectiont[0]), (BB.Y)), 0, 0, 0);
                P1.AddVertexAt(2, new Point2d((BB.X + sectiont[0] + (sectiont[1] - sectiont[0]) * 0.5) ,
                    (BB.Y + sectiont[2])), 0, 0, 0);
                P1.AddVertexAt(3, new Point2d((BB.X - (sectiont[1] - sectiont[0]) * 0.5) ,
                    (BB.Y + sectiont[2]) ), 0, 0, 0);
                modelSpace.AppendEntity(P1);
                tr.AddNewlyCreatedDBObject(P1, true);

                Polyline P2 = new Polyline
                {
                    Layer = "粗线",
                    Closed = true,
                };
                P2.SetDatabaseDefaults();
                P2.AddVertexAt(0, new Point2d((BB.X + sectiont[5] + sectiont[6]) , (BB.Y + sectiont[3]) ), 0, 0, 0);
                P2.AddVertexAt(1, new Point2d((BB.X + sectiont[0] - sectiont[5] - sectiont[6]), (BB.Y + sectiont[3])), 0, 0, 0);
                P2.AddVertexAt(2, new Point2d((BB.X + sectiont[0] - sectiont[5]) , (BB.Y + sectiont[3] + sectiont[7]) ), 0, 0, 0);
                P2.AddVertexAt(3, new Point2d((BB.X + sectiont[0] - sectiont[5]) ,
                    (BB.Y + sectiont[2] - sectiont[4] - sectiont[9]) ), 0, 0, 0);
                P2.AddVertexAt(4, new Point2d((BB.X + sectiont[0] - sectiont[5] - sectiont[8]) ,
                    (BB.Y + sectiont[2] - sectiont[4]) ), 0, 0, 0);
                P2.AddVertexAt(5, new Point2d((BB.X + sectiont[5] + sectiont[8]) ,
                    (BB.Y + sectiont[2] - sectiont[4]) ), 0, 0, 0);
                P2.AddVertexAt(6, new Point2d((BB.X + sectiont[5]) ,
                    (BB.Y + sectiont[2] - sectiont[4] - sectiont[9]) ), 0, 0, 0);
                P2.AddVertexAt(7, new Point2d((BB.X + sectiont[5]) ,
                    (BB.Y + sectiont[3] + sectiont[7])), 0, 0, 0);
                modelSpace.AppendEntity(P2);
                tr.AddNewlyCreatedDBObject(P2, true);

                Polyline P3 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                P3.SetDatabaseDefaults();
                P3.AddVertexAt(0, new Point2d((BB.X - 200.0) , (BB.Y - LayerB) ), 0, 0, 0);
                P3.AddVertexAt(1, new Point2d((BB.X + sectiont[0] + 200) , (BB.Y - LayerB) ), 0, 0, 0);
                P3.AddVertexAt(2, new Point2d((BB.X + sectiont[0] + 200) , (BB.Y) ), 0, 0, 0);
                P3.AddVertexAt(3, new Point2d((BB.X - 200.0) , (BB.Y) ), 0, 0, 0);
                modelSpace.AppendEntity(P3);
                tr.AddNewlyCreatedDBObject(P3, true);

                Polyline P4 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                P4.SetDatabaseDefaults();
                P4.AddVertexAt(0, new Point2d((BB.X - 500.0) , (BB.Y - LayerB - LayerA) ), 0, 0, 0);
                P4.AddVertexAt(1, new Point2d((BB.X + sectiont[0] + 500) , (BB.Y - LayerB - LayerA) ), 0, 0, 0);
                P4.AddVertexAt(2, new Point2d((BB.X + sectiont[0] + 500) , (BB.Y - LayerB) ), 0, 0, 0);
                P4.AddVertexAt(3, new Point2d((BB.X - 500.0) , (BB.Y - LayerB) ), 0, 0, 0);
                modelSpace.AppendEntity(P4);
                tr.AddNewlyCreatedDBObject(P4, true);
                //-------------------------------------------------------------------------------------------
                Polyline P5 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                P5.AddVertexAt(0, P4.GetPoint2dAt(3), 0, 0, 0);
                P5.AddVertexAt(1, Convert2d(P4.GetPoint2dAt(3),  - sectiont[2] - 100, sectiont[2] + 100), 0, 0, 0);
                P5.AddVertexAt(2, P1.GetPoint2dAt(3), 0, 0, 0);
                P5.AddVertexAt(3, P1.GetPoint2dAt(0), 0, 0, 0);
                P5.AddVertexAt(4, P3.GetPoint2dAt(3), 0, 0, 0);
                P5.AddVertexAt(5, P3.GetPoint2dAt(0), 0, 0, 0);
                modelSpace.AppendEntity(P5);
                tr.AddNewlyCreatedDBObject(P5, true);

                Polyline P6 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                P6.AddVertexAt(0, P4.GetPoint2dAt(2), 0, 0, 0);
                P6.AddVertexAt(1, Convert2d(P4.GetPoint2dAt(2), sectiont[2] + 100, sectiont[2] + 100), 0, 0, 0);
                P6.AddVertexAt(2, P1.GetPoint2dAt(2), 0, 0, 0);
                P6.AddVertexAt(3, P1.GetPoint2dAt(1), 0, 0, 0);
                P6.AddVertexAt(4, P3.GetPoint2dAt(2), 0, 0, 0);
                P6.AddVertexAt(5, P3.GetPoint2dAt(1), 0, 0, 0);
                modelSpace.AppendEntity(P6);
                tr.AddNewlyCreatedDBObject(P6, true);
                Polyline P7 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                P7.AddVertexAt(0, P5.GetPoint2dAt(1), 0, 0, 0);
                P7.AddVertexAt(1, Convert2d(P5.GetPoint2dAt(1), -1500, 1500), 0, 0, 0);
                P7.AddVertexAt(2, Convert2d(P6.GetPoint2dAt(1), 1500, 1500), 0, 0, 0);
                P7.AddVertexAt(3, P6.GetPoint2dAt(1), 0, 0, 0);    
                modelSpace.AppendEntity(P7);
                tr.AddNewlyCreatedDBObject(P7, true);
                //-------------------------------------------------------------------------------------------
                // 填充
                //-------------------------------------------------------------------------------------------
                List<Polyline> plist = new List<Polyline> { P4,P5, P6 };
                Hatch hatchref = new Hatch();
                hatchref.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
                foreach (Polyline pl in plist)
                {
                    ObjectIdCollection ids = new ObjectIdCollection { pl.ObjectId };
                    Hatch hatch = new Hatch();
                    modelSpace.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);

                    hatch.AppendLoop(HatchLoopTypes.Default, ids);
                    hatch.PatternScale = 2;
                    hatch.SetHatchPattern(hatchref.PatternType, hatchref.PatternName);
                    hatch.EvaluateHatch(true);
                }
                hatchref.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
                ObjectIdCollection ids2 = new ObjectIdCollection { P3.ObjectId };
                Hatch hatch2 = new Hatch();
                modelSpace.AppendEntity(hatch2);
                tr.AddNewlyCreatedDBObject(hatch2, true);
                hatch2.AppendLoop(HatchLoopTypes.Default, ids2);
                hatch2.PatternScale = 1;
                hatch2.SetHatchPattern(hatchref.PatternType, hatchref.PatternName);
                hatch2.EvaluateHatch(true);



                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                ObjectId dsId = dst["1-75"];
                AlignedDimension ad1 = new AlignedDimension(
                    new Point3d(BB.X , BB.Y , 0.0),
                    new Point3d((BB.X + sectiont[1]) , BB.Y , 0.0),
                    new Point3d((BB.X + sectiont[1]) / 2.0 , BB.Y + sectiont[2] + 200 , 0.0),
                    "",
                    dsId
                    );
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(ad1);
                tr.AddNewlyCreatedDBObject(ad1, true);

                //
                tr.Commit();
            }
        }


        [CommandMethod("w2")]
        public static void CoupeA()
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
                // 绘图
                //-------------------------------------------------------------------------------------------
                double delt1 = 1000.0 * (height[0] - height[3]);
                double delt2 = 1000.0 * (height[1] - height[3]);
                double delt3 = 1000.0 * (height[2] - height[3]);
                Point2d BB = new Point2d(0, 0);
                double L = length[0] * length[1];
                Line P1 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + 0.5 * L * slop, 0.0));
                Line P2 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[3] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[3] + 0.5 * L * slop, 0.0));
                Line P3 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[2] - sectiont[4] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[2] - sectiont[4] + 0.5 * L * slop, 0.0));
                Line P4 = new Line(new Point3d(BB.X - 0.5 * L, BB.Y + sectiont[2] - 0.5 * L * slop, 0.0),
                    new Point3d(BB.X + 0.5 * L, BB.Y + sectiont[2] + 0.5 * L * slop, 0.0));
                P1.Layer = "粗线";
                P2.Layer = "细线";
                P3.Layer = "细线";
                P4.Layer = "粗线";

                Line axixYY = new Line(Convert3d(BB,0,-800), Convert3d(BB,0,sectiont[3]+delt2+ 1200.0));
                axixYY.Layer = "中心线";
                axixYY.LinetypeScale = 500;
                modelSpace.AppendEntity(axixYY);
                tr.AddNewlyCreatedDBObject(axixYY, true);

                modelSpace.AppendEntity(P1);
                modelSpace.AppendEntity(P2);
                modelSpace.AppendEntity(P3);
                modelSpace.AppendEntity(P4);
                tr.AddNewlyCreatedDBObject(P1, true);
                tr.AddNewlyCreatedDBObject(P2, true);
                tr.AddNewlyCreatedDBObject(P3, true);
                tr.AddNewlyCreatedDBObject(P4, true);
                foreach (int ii in Enumerable.Range(0, length[0] + 1))
                {
                    double x0 = BB.X - 0.5 * L + ii * length[1];
                    double y1 = BB.Y + slop * (-0.5 * L + ii * length[1]);
                    double y2 = BB.Y + sectiont[2] + slop * (-0.5 * L + ii * length[1]);
                    Line L1 = new Line(new Point3d(x0, y1, 0), new Point3d(x0, y2, 0));
                    L1.Layer = "细线";
                    modelSpace.AppendEntity(L1);
                    tr.AddNewlyCreatedDBObject(L1, true);
                }
                Polyline PL1 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL1.AddVertexAt(0, new Point2d((BB.X - 0.5 * L), BB.Y + sectiont[3] + slop * (-0.5 * L)), 0, 0, 0);
                PL1.AddVertexAt(1, new Point2d((BB.X - 0.5 * L), BB.Y + sectiont[2] + slop * (-0.5 * L) + 300.0), 0, 0, 0);
                PL1.AddVertexAt(2, new Point2d((BB.X - 0.5 * L - 200.0), BB.Y + sectiont[2] + slop * (-0.5 * L) + 300.0), 0, 0, 0);
                PL1.AddVertexAt(3, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) + 300 + sectiont[3]), 0, 0, 0);
                PL1.AddVertexAt(4, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) + sectiont[3]), 0, 0, 0);
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);

                Polyline PL2 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL2.AddVertexAt(0, new Point2d((BB.X + 0.5 * L), BB.Y + sectiont[3] + slop * (0.5 * L)), 0, 0, 0);
                PL2.AddVertexAt(1, new Point2d((BB.X + 0.5 * L), BB.Y + sectiont[2] + slop * (0.5 * L) + 300.0), 0, 0, 0);
                PL2.AddVertexAt(2, new Point2d((BB.X + 0.5 * L + 200.0), BB.Y + sectiont[2] + slop * (0.5 * L) + 300.0), 0, 0, 0);
                PL2.AddVertexAt(3, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) + 300 + sectiont[3]), 0, 0, 0);
                PL2.AddVertexAt(4, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) + sectiont[3]), 0, 0, 0);
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);

                Polyline PL3 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL3.AddVertexAt(0, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) + sectiont[3]), 0, 0, 0);
                PL3.AddVertexAt(1, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0])), 0, 0, 0);
                PL3.AddVertexAt(2, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) - 400), 0, 0, 0);
                PL3.AddVertexAt(3, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0]) - 400), 0, 0, 0);
                PL3.AddVertexAt(4, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0] + 250.0)), 0, 0, 0);
                PL3.AddVertexAt(5, new Point2d((BB.X - 0.5 * L), BB.Y + slop * (-0.5 * L)), 0, 0, 0);
                PL3.AddVertexAt(6, new Point2d((BB.X - 0.5 * L), BB.Y + slop * (-0.5 * L) + sectiont[3]), 0, 0, 0);
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);

                Polyline PL4 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL4.AddVertexAt(0, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) + sectiont[3]), 0, 0, 0);
                PL4.AddVertexAt(1, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1])), 0, 0, 0);
                PL4.AddVertexAt(2, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) - 400), 0, 0, 0);
                PL4.AddVertexAt(3, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (0.5 * L + lengthB[1]) - 400), 0, 0, 0);
                PL4.AddVertexAt(4, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (0.5 * L + lengthB[1] - 250.0)), 0, 0, 0);
                PL4.AddVertexAt(5, new Point2d((BB.X + 0.5 * L), BB.Y + slop * (0.5 * L)), 0, 0, 0);
                PL4.AddVertexAt(6, new Point2d((BB.X + 0.5 * L), BB.Y + slop * (0.5 * L) + sectiont[3]), 0, 0, 0);
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);

                Polyline PL5 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL5.AddVertexAt(0, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0] + 250.0)), 0, 0, 0);
                PL5.AddVertexAt(1, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0] + 250.0) - 100), 0, 0, 0);
                PL5.AddVertexAt(2, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (+0.5 * L + lengthB[1] - 250.0) - 100), 0, 0, 0);
                PL5.AddVertexAt(3, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (+0.5 * L + lengthB[1] - 250.0)), 0, 0, 0);
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);

                Polyline PL51 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL51.AddVertexAt(0, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0] + 250.0) - 500), 0, 0, 0);
                PL51.AddVertexAt(1, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250.0), BB.Y + slop * (-0.5 * L - lengthB[0] + 250.0) - 100), 0, 0, 0);
                PL51.AddVertexAt(2, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (+0.5 * L + lengthB[1] - 250.0) - 100), 0, 0, 0);
                PL51.AddVertexAt(3, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250.0), BB.Y + slop * (+0.5 * L + lengthB[1] - 250.0) - 500), 0, 0, 0);
                modelSpace.AppendEntity(PL51);
                tr.AddNewlyCreatedDBObject(PL51, true);
                Polyline PL6 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL6.AddVertexAt(0, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) - 400), 0, 0, 0);
                PL6.AddVertexAt(1, new Point2d((BB.X - 0.5 * L - lengthB[0]), BB.Y + slop * (-0.5 * L - lengthB[0]) - 500), 0, 0, 0);
                PL6.AddVertexAt(2, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250), BB.Y + slop * (-0.5 * L - lengthB[0]) - 500), 0, 0, 0);
                PL6.AddVertexAt(3, new Point2d((BB.X - 0.5 * L - lengthB[0] + 250), BB.Y + slop * (-0.5 * L - lengthB[0]) - 400), 0, 0, 0);
                modelSpace.AppendEntity(PL6);
                tr.AddNewlyCreatedDBObject(PL6, true);

                Polyline PL7 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL7.AddVertexAt(0, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) - 400), 0, 0, 0);
                PL7.AddVertexAt(1, new Point2d((BB.X + 0.5 * L + lengthB[1]), BB.Y + slop * (0.5 * L + lengthB[1]) - 500), 0, 0, 0);
                PL7.AddVertexAt(2, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250), BB.Y + slop * (0.5 * L + lengthB[1]) - 500), 0, 0, 0);
                PL7.AddVertexAt(3, new Point2d((BB.X + 0.5 * L + lengthB[1] - 250), BB.Y + slop * (0.5 * L + lengthB[1]) - 400), 0, 0, 0);
                modelSpace.AppendEntity(PL7);
                tr.AddNewlyCreatedDBObject(PL7, true);

                //-------------------------------------------------------------------------------------------
                // 填充
                //-------------------------------------------------------------------------------------------
                Hatch hatchref = new Hatch();
                hatchref.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                Hatch hatchref2 = new Hatch();
                hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
                Hatch hatchref3 = new Hatch();
                hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");

                List<Polyline> plist = new List<Polyline> {PL6,PL7};
                foreach (Polyline pl in plist)
                {
                    ObjectIdCollection ids = new ObjectIdCollection { pl.ObjectId };
                    Hatch hatch = new Hatch();
                    modelSpace.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);           
                    hatch.AppendLoop(HatchLoopTypes.Default, ids);
                    hatch.PatternScale = 20;
                    hatch.SetHatchPattern(hatchref.PatternType, hatchref.PatternName);
                    hatch.EvaluateHatch(true);
                }
                
                Hatch hatch2 = new Hatch();
                modelSpace.AppendEntity(hatch2);
                tr.AddNewlyCreatedDBObject(hatch2, true);
                hatch2.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { PL5.ObjectId });
                hatch2.PatternScale = 1;
                hatch2.SetHatchPattern(hatchref3.PatternType, hatchref3.PatternName);
                hatch2.EvaluateHatch(true);
                Hatch hatch3 = new Hatch();
                modelSpace.AppendEntity(hatch3);
                tr.AddNewlyCreatedDBObject(hatch3, true);
                hatch3.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { PL51.ObjectId });
                hatch3.PatternScale = 1;
                hatch3.SetHatchPattern(hatchref2.PatternType, hatchref2.PatternName);
                hatch3.EvaluateHatch(true);
                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------

                BiaoGao(height[3] - 0.001 * slop * 0.5 * L, P2.StartPoint, modelSpace, tr, blockTbl);
                BiaoGao(height[3] + 0.001 * slop * 0.5 * L, P2.EndPoint, modelSpace, tr, blockTbl);
                BiaoGao(height[3],
                    new Point3d((P2.StartPoint.X + P2.EndPoint.X) * 0.5, (P2.StartPoint.Y + P2.EndPoint.Y) * 0.5, 0), modelSpace, tr, blockTbl);
                BiaoGao(height[3] - 0.001 * slop * (0.5 * L + lengthB[0]), PL3.GetPoint3dAt(0), modelSpace, tr, blockTbl);
                BiaoGao(height[3] + 0.001 * slop * (0.5 * L + lengthB[0]), PL4.GetPoint3dAt(0), modelSpace, tr, blockTbl);

                Polyline C1 = new Polyline()
                {
                    Layer = "细线",
                    Closed = false,
                };

                Point2d Pt0 = new Point2d(BB.X - RoadWidth[0]-delt1/RoadSlop[1]*RoadSlop[0], BB.Y);
                Point2d Pt1 = new Point2d(BB.X - RoadWidth[0], BB.Y + sectiont[3] + delt1);
                Point2d Pt2 = new Point2d(BB.X, BB.Y + sectiont[3] + delt2);
                Point2d Pt3 = new Point2d(BB.X + RoadWidth[1], BB.Y + sectiont[3] + delt3);
                Point2d Pt4 = new Point2d(BB.X + RoadWidth[1] + delt3 / RoadSlop[1] * RoadSlop[0], BB.Y);
                C1.AddVertexAt(0, Pt0, 0, 0, 0);
                C1.AddVertexAt(1, Pt1, 0, 0, 0);
                C1.AddVertexAt(2, Pt2, 0, 0, 0);
                C1.AddVertexAt(3, Pt3, 0, 0, 0);
                C1.AddVertexAt(4, Pt4, 0, 0, 0);
                Point3dCollection pts = new Point3dCollection();
                C1.IntersectWith(P4, Intersect.ExtendBoth, pts,IntPtr.Zero,IntPtr.Zero);
                DBObjectCollection dbCs = C1.GetSplitCurves(pts);
                C1 = (Polyline)dbCs[1];
                modelSpace.AppendEntity(C1);
                tr.AddNewlyCreatedDBObject(C1, true);
                BiaoGao(height[0], Convert3d(Pt1), modelSpace, tr, blockTbl);
                BiaoGao(height[1], Convert3d(Pt2), modelSpace, tr, blockTbl);
                BiaoGao(height[2], Convert3d(Pt3), modelSpace, tr, blockTbl);
                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                ObjectId dsId = dst["1-100"];
                Point3d dimref = Convert3d(BB, 0, delt2 + 1200.0);
                RotatedDimension D1 = new RotatedDimension(0, PL3.GetPoint3dAt(0), PL3.GetPoint3dAt(6), dimref, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                RotatedDimension D2 = new RotatedDimension(0, P4.StartPoint, Convert3d(Pt1), dimref, "", dsId);
                D2.Layer = "标注";
                modelSpace.AppendEntity(D2);
                tr.AddNewlyCreatedDBObject(D2, true);
                RotatedDimension D3 = new RotatedDimension(0, Convert3d(Pt1), Convert3d(Pt2), dimref, "", dsId);
                D3.Layer = "标注";
                modelSpace.AppendEntity(D3);
                tr.AddNewlyCreatedDBObject(D3, true);
                RotatedDimension D4 = new RotatedDimension(0, Convert3d(Pt2), Convert3d(Pt3), dimref, "", dsId);
                D4.Layer = "标注";
                modelSpace.AppendEntity(D4);
                tr.AddNewlyCreatedDBObject(D4, true);
                RotatedDimension D5 = new RotatedDimension(0, Convert3d(Pt3), P4.EndPoint, dimref, "", dsId);
                D5.Layer = "标注";
                modelSpace.AppendEntity(D5);
                tr.AddNewlyCreatedDBObject(D5, true);
                RotatedDimension D6 = new RotatedDimension(0, PL4.GetPoint3dAt(6), PL4.GetPoint3dAt(0), dimref, "", dsId);
                D6.Layer = "标注";
                modelSpace.AppendEntity(D6);
                tr.AddNewlyCreatedDBObject(D6, true);
                //-------------------------------------------------------------------------------------------
                Point3d dimref2 = Convert3d(BB, -0.5*L-lengthB[0]-800,0);
                RotatedDimension D7 = new RotatedDimension(Math.PI / 2, PL1.GetPoint3dAt(3), PL1.GetPoint3dAt(4), dimref2, "", dsId);
                D7.Layer = "标注";
                modelSpace.AppendEntity(D7);
                tr.AddNewlyCreatedDBObject(D7, true);
                RotatedDimension D8 = new RotatedDimension(Math.PI / 2, PL3.GetPoint3dAt(0), PL3.GetPoint3dAt(1), dimref2, "", dsId);
                D8.Layer = "标注";
                modelSpace.AppendEntity(D8);
                tr.AddNewlyCreatedDBObject(D8, true);
                RotatedDimension D9 = new RotatedDimension(Math.PI / 2, PL3.GetPoint3dAt(1), PL3.GetPoint3dAt(2), dimref2, "", dsId);
                D9.Layer = "标注";
                modelSpace.AppendEntity(D9);
                tr.AddNewlyCreatedDBObject(D9, true);
                RotatedDimension D10 = new RotatedDimension(Math.PI / 2, PL6.GetPoint3dAt(0), PL6.GetPoint3dAt(1), dimref2, "", dsId);
                D10.Layer = "标注";
                modelSpace.AppendEntity(D10);
                tr.AddNewlyCreatedDBObject(D10, true);
                RotatedDimension D11 = new RotatedDimension(0, P4.StartPoint,P4.EndPoint, Convert3d(BB, 0,sectiont[2]+800), "", dsId);
                D11.Layer = "标注";
                modelSpace.AppendEntity(D11);
                tr.AddNewlyCreatedDBObject(D11, true);
                //-------------------------------------------------------------------------------------------                
                LineAngularDimension2 AD1 = new LineAngularDimension2(PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1),
                    PL1.GetPoint3dAt(2), PL1.GetPoint3dAt(3), Convert3d(PL1.GetPoint2dAt(1), -100, -800), "", dsId);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                LineAngularDimension2 AD2 = new LineAngularDimension2(PL2.GetPoint3dAt(2), PL2.GetPoint3dAt(3),
                    PL2.GetPoint3dAt(1), PL2.GetPoint3dAt(0), Convert3d(PL2.GetPoint2dAt(1),100, -800), "", dsId);
                AD2.Layer = "标注";
                modelSpace.AppendEntity(AD2);
                tr.AddNewlyCreatedDBObject(AD2, true);







                //-------------------------------------------------------------------------------------------
                double hp1 = 100 * Math.Abs((height[1] - height[0]) * 1000 / RoadWidth[0]);
                double hp2 = 100 * Math.Abs((height[2] - height[1]) * 1000 / RoadWidth[1]);
                HengPo(hp1, Convert3d(BB, -0.5 * RoadWidth[0], sectiont[3] + delt2 + 100), height[0] < height[1], modelSpace, tr, blockTbl);
                HengPo(hp2, Convert3d(BB, 0.5 * RoadWidth[1], sectiont[3] + delt2 + 100), height[1] < height[2], modelSpace, tr, blockTbl);
                HengPo(Math.Abs(100*slop), Convert3d(BB, 1.5 * length[1], sectiont[3] + 100), slop > 0, modelSpace, tr, blockTbl);
                //-------------------------------------------------------------------------------------------
                tr.Commit();
            }
        }











    }
}
