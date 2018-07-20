using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
        [CommandMethod("d3")]
        public static void Draw3()
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
                Point2d BB = new Point2d(25000, -10000);
                Line2d AxisY = new Line2d(BB, Convert2d(BB, 0, 1));
                Point2d p0; Point2d p1; Point2d p2; Point2d p3; Point2d p4; Point2d p5;
                Point2d p6; Point2d p7; Point2d p8; Point2d p9; Point2d p10; Point2d p11;
                Polyline PL1 = new Polyline();
                if (Sect[11] == 0)
                {
                    p0 = Convert2d(BB, -0.5 * Sect[0], 0);
                    p1 = p0.Mirror(AxisY);
                    p2 = Convert2d(p1, 0, Sect[2]);
                    p3 = Convert2d(BB, 0, Sect[2] + 0.5 * Sect[1] * Sect[10]);
                    p4 = p2.Mirror(AxisY);
                    PL1 = new Polyline()
                    {
                        Layer = "粗线",
                        Closed = true,
                    };
                    PL1.AddVertexAt(0, p0, 0, 0, 0);
                    PL1.AddVertexAt(1, p1, 0, 0, 0);
                    PL1.AddVertexAt(2, p2, 0, 0, 0);
                    PL1.AddVertexAt(3, p3, 0, 0, 0);
                    PL1.AddVertexAt(4, p4, 0, 0, 0);
                    modelSpace.AppendEntity(PL1);
                    tr.AddNewlyCreatedDBObject(PL1, true);
                }
                else
                {
                    p0 = Convert2d(BB, -0.5 * Sect[0] - Sect[11], 0);
                    p1 = Convert2d(BB, -0.5 * Sect[0], 0);
                    p2 = p1.Mirror(AxisY);
                    p3 = p0.Mirror(AxisY);
                    p4 = Convert2d(p3, 0, Sect[3]);
                    p5 = Convert2d(p4, -Sect[11] + Sect[6], 0);
                    p6 = Convert2d(p5, -Sect[6], Sect[7]);
                    p7 = Convert2d(BB, 0.5 * Sect[1], Sect[2]);
                    p8 = p7.Mirror(AxisY);
                    p9 = p6.Mirror(AxisY);
                    p10 = p5.Mirror(AxisY);
                    p11 = p4.Mirror(AxisY);
                    PL1 = new Polyline()
                    {
                        Layer = "粗线",
                        Closed = true,
                    };
                    PL1.AddVertexAt(0, p0, 0, 0, 0);
                    PL1.AddVertexAt(1, p1, 0, 0, 0);
                    PL1.AddVertexAt(2, p2, 0, 0, 0);
                    PL1.AddVertexAt(3, p3, 0, 0, 0);
                    PL1.AddVertexAt(4, p4, 0, 0, 0);
                    PL1.AddVertexAt(5, p5, 0, 0, 0);
                    PL1.AddVertexAt(6, p6, 0, 0, 0);
                    PL1.AddVertexAt(7, p7, 0, 0, 0);
                    PL1.AddVertexAt(8, p8, 0, 0, 0);
                    PL1.AddVertexAt(9, p9, 0, 0, 0);
                    PL1.AddVertexAt(10, p10, 0, 0, 0);
                    PL1.AddVertexAt(11, p11, 0, 0, 0);
                    modelSpace.AppendEntity(PL1);
                    tr.AddNewlyCreatedDBObject(PL1, true);
                }

                p0 = Convert2d(BB, -0.5 * Sect[0] +Sect[5]+ Sect[6],Sect[3]);
                p1 = p0.Mirror(AxisY);
                p2 = Convert2d(p1, Sect[6], Sect[7]);
                p3 = Convert2d(p2, 0, -Sect[7] - Sect[3] + Sect[2] - Sect[4] - Sect[9]);
                p4 = Convert2d(p3, -Sect[8], Sect[9]);
                p5 = p4.Mirror(AxisY);
                p6 = p3.Mirror(AxisY);
                p7 = p2.Mirror(AxisY);
                Polyline PL2 = new Polyline()
                {
                    Layer = "粗线",
                    Closed = true,
                };
                PL2.AddVertexAt(0, p0, 0, 0, 0);
                PL2.AddVertexAt(1, p1, 0, 0, 0);
                PL2.AddVertexAt(2, p2, 0, 0, 0);
                PL2.AddVertexAt(3, p3, 0, 0, 0);
                PL2.AddVertexAt(4, p4, 0, 0, 0);
                PL2.AddVertexAt(5, p5, 0, 0, 0);
                PL2.AddVertexAt(6, p6, 0, 0, 0);
                PL2.AddVertexAt(7, p7, 0, 0, 0);
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);

                p0 = Convert2d(BB, -0.5 * cLayer[0][0], 0);
                p1 = Convert2d(BB, 0.5 * cLayer[0][0], 0);
                p2 = Convert2d(p1, 0, -cLayer[0][1]);
                p3 = Convert2d(p2, -cLayer[0][0], 0);
                Polyline PL3 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL3.AddVertexAt(0, p0, 0, 0, 0);
                PL3.AddVertexAt(1, p1, 0, 0, 0);
                PL3.AddVertexAt(2, p2, 0, 0, 0);
                PL3.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);

                p0 = Convert2d(BB, -0.5 * cLayer[1][0], -cLayer[0][1]);
                p1 = Convert2d(BB, 0.5 * cLayer[1][0], -cLayer[0][1]);
                p2 = Convert2d(p1, 0, -cLayer[1][1] );
                p3 = Convert2d(p2, -cLayer[1][0],0);
                Polyline PL4 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                PL4.AddVertexAt(0, p0, 0, 0, 0);
                PL4.AddVertexAt(1, p1, 0, 0, 0);
                PL4.AddVertexAt(2, p2, 0, 0, 0);
                PL4.AddVertexAt(3, p3, 0, 0, 0);
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);
                //-------------------------------------------------------------------------------------------
                // 填充
                //-------------------------------------------------------------------------------------------
                Hatch hatchref1 = new Hatch();
                hatchref1.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                Hatch hatchref2 = new Hatch();
                hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
                Hatch hatchref3 = new Hatch();
                hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");

                Hatch hatch1 = new Hatch();
                modelSpace.AppendEntity(hatch1);
                tr.AddNewlyCreatedDBObject(hatch1, true);
                hatch1.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { PL3.ObjectId });
                hatch1.PatternScale = 1;
                hatch1.SetHatchPattern(hatchref3.PatternType, hatchref3.PatternName);
                hatch1.EvaluateHatch(true);

                Hatch hatch2 = new Hatch();
                modelSpace.AppendEntity(hatch2);
                tr.AddNewlyCreatedDBObject(hatch2, true);
                hatch2.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { PL4.ObjectId });
                hatch2.PatternScale = 1;
                hatch2.SetHatchPattern(hatchref2.PatternType, hatchref2.PatternName);
                hatch2.EvaluateHatch(true);
                Polyline H1 = new Polyline()
                {
                    Layer = "细线",
                    Closed = true,
                };
                if (Sect[11]==0)
                {
                    p0 = Convert2d(BB, -0.5 * Sect[0] - Refill[0][1], -cLayer[0][1]);
                    p1 = Convert2d(p0, -Refill[0][0] * (cLayer[0][1] + Sect[2]), cLayer[0][1] + Sect[2]);
                    p2 = PL1.GetPoint2dAt(4);
                    p3 = PL1.GetPoint2dAt(0);
                    p4 = PL3.GetPoint2dAt(0);
                    p5 = PL3.GetPoint2dAt(3);
                    H1.AddVertexAt(0, p0, 0, 0, 0);
                    H1.AddVertexAt(1, p1, 0, 0, 0);
                    H1.AddVertexAt(2, p2, 0, 0, 0);
                    H1.AddVertexAt(3, p3, 0, 0, 0);
                    H1.AddVertexAt(4, p4, 0, 0, 0);
                    H1.AddVertexAt(5, p5, 0, 0, 0);
                    modelSpace.AppendEntity(H1);
                    tr.AddNewlyCreatedDBObject(H1, true);
                }
                else
                {
                    p0 = Convert2d(BB, -0.5 * Sect[0] - Refill[0][1], -cLayer[0][1]);
                    p1 = Convert2d(p0, -Refill[0][0] * (cLayer[0][1] + Sect[2]), cLayer[0][1] + Sect[2]);
                    p2 = PL1.GetPoint2dAt(8);
                    p3 = PL1.GetPoint2dAt(9);
                    p4 = PL1.GetPoint2dAt(10);
                    p5 = PL1.GetPoint2dAt(11);
                    p6 = PL1.GetPoint2dAt(0);
                    p7 = PL3.GetPoint2dAt(0);
                    p8 = PL3.GetPoint2dAt(3);
                    H1.AddVertexAt(0, p0, 0, 0, 0);
                    H1.AddVertexAt(1, p1, 0, 0, 0);
                    H1.AddVertexAt(2, p2, 0, 0, 0);
                    H1.AddVertexAt(3, p3, 0, 0, 0);
                    H1.AddVertexAt(4, p4, 0, 0, 0);
                    H1.AddVertexAt(5, p5, 0, 0, 0);
                    H1.AddVertexAt(6, p6, 0, 0, 0);
                    H1.AddVertexAt(7, p7, 0, 0, 0);
                    H1.AddVertexAt(8, p8, 0, 0, 0);
                    modelSpace.AppendEntity(H1);
                    tr.AddNewlyCreatedDBObject(H1, true);
                }
                Polyline H2 = new Polyline();
                H2 = (Polyline) H1.Clone();
                Line3d aa = new Line3d(Convert3d(BB), Convert3d(BB, 0, 100));
                H2.TransformBy(Matrix3d.Mirroring(aa));
                modelSpace.AppendEntity(H2);
                tr.AddNewlyCreatedDBObject(H2, true);
                List<Polyline> plist = new List<Polyline> { H1, H2 };
                foreach (Polyline pl in plist)
                {
                    ObjectIdCollection ids = new ObjectIdCollection { pl.ObjectId };
                    Hatch hatch = new Hatch();
                    modelSpace.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);
                    hatch.AppendLoop(HatchLoopTypes.Default, ids);
                    hatch.PatternScale = 1;
                    hatch.SetHatchPattern(hatchref2.PatternType, hatchref2.PatternName);
                    hatch.EvaluateHatch(true);
                }
                Polyline PL5 = new Polyline();
                PL5.AddVertexAt(0, H1.GetPoint2dAt(1), 0, 0, 0);
                PL5.AddVertexAt(1, Convert2d(H1.GetPoint2dAt(1), -1000, 1000), 0, 0, 0);
                PL5.AddVertexAt(2, Convert2d(H1.GetPoint2dAt(1), -1000, 1000).Mirror(AxisY), 0, 0, 0);
                PL5.AddVertexAt(3, H1.GetPoint2dAt(1).Mirror(AxisY), 0, 0, 0);
                PL5.Layer = "细线";
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);
                //-------------------------------------------------------------------------------------------
                // 标注
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                var dsId = dst["1-75"];
                double ymin =Sect[2]+500;
                Point3d dimref1 = Convert3d(BB, 0, ymin);
                Point3d dimref2 = Convert3d(BB, 0.5*Sect[0]+1000, 0);
                Point3d dimref3 = Convert3d(BB, 0.5 * Sect[0] + 1300, 0);

                RotatedDimension D1 = new RotatedDimension(0, PL2.GetPoint3dAt(7),Convert3d(PL2.GetPoint3dAt(7),-Sect[5]),
                    dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, PL2.GetPoint3dAt(7), PL2.GetPoint3dAt(2), dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(0, PL2.GetPoint3dAt(2), Convert3d(PL2.GetPoint3dAt(2), Sect[5]),
                    dimref1, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI/2, PL1.GetPoint3dAt(0), Convert3d(PL1.GetPoint3dAt(0), 0,Sect[3]),
                    dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, PL2.GetPoint3dAt(0), PL2.GetPoint3dAt(5), dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                D1 = new RotatedDimension(Math.PI / 2, PL2.GetPoint3dAt(5), Convert3d(PL2.GetPoint3dAt(5),0,Sect[4]),
                    dimref2, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                D1 = new RotatedDimension(Math.PI / 2, PL1.GetPoint3dAt(0), Convert3d(PL1.GetPoint3dAt(0), 0, Sect[2]),
                    dimref3, "", dsId);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);

                foreach (int key in cLayer.Keys)
                {
                    double tt = cLayer[key][1];
                    if (key != 0)
                    {
                        D1 = new RotatedDimension(Math.PI / 2,
                            Convert3d(PL1.GetPoint3dAt(0), 0, -cLayer[key-1][1]),
                            Convert3d(PL1.GetPoint3dAt(0), 0, -cLayer[key - 1][1]-tt),
                            dimref3, "", dsId);
                        D1.Layer = "标注";
                        modelSpace.AppendEntity(D1);
                        tr.AddNewlyCreatedDBObject(D1, true);
                    }
                    else
                    {
                        D1 = new RotatedDimension(Math.PI / 2,
                            Convert3d(PL1.GetPoint3dAt(0), 0,0),
                            Convert3d(PL1.GetPoint3dAt(0), 0, - tt),
                            dimref3, "", dsId);
                        D1.Layer = "标注";
                        modelSpace.AppendEntity(D1);
                        tr.AddNewlyCreatedDBObject(D1, true);
                    }
                }










                tr.Commit();
            }
        }
        //-------------------------------------------------------------------------------------------
        // END
        //-------------------------------------------------------------------------------------------






    }
}
