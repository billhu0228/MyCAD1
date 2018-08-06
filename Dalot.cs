using System;
using System.Collections.Generic;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MOE = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MyCAD1
{
    public enum AType { BZQ, JSJ, None, Other };
    public enum DType { A, B, C, D, E, F, G, H, I, J, K, L }



    public class Dalot
    {
        public double Pk;
        public double Ang, Slop;
        public double Length, SegLength;
        public double XMidDist;
        public AType Amont;
        public DType DalotType;
        public Point2d BasePoint;
        public double[] Sect;
        public double LayerT,LayerW;
        public double H1, H2, H3, H0;
        public double dH;
        public double W1, W2;
        public int ScaleA, ScaleB;
        public double MouthT, PlateT;//伸出嘴厚度，底板厚度
        public double FooterW;

               

        public Dalot(double cPk, double cAng, double cSlop, double cLength, double cSegLength, double cXMidDist,
            DType cDalotType, AType cAmont, Point2d cBasePoint, double cLayerT,double cLayerW, double cdH, int cscaleA, int cscaleB, bool isTri)            
        {
            Pk = cPk;
            Ang = cAng;
            Slop = cSlop;
            DalotType = cDalotType;
            Length = cLength;
            SegLength = cSegLength;
            XMidDist = cXMidDist;
            Amont = cAmont;
            LayerT = cLayerT;
            LayerW = cLayerW;
            BasePoint = cBasePoint;
            ScaleA = cscaleA;
            ScaleB = cscaleB;
            FooterW = 0;
            switch (DalotType)
            {
                case DType.A:
                    Sect = new double[] { 1900, 1900, 1400, 200, 200, 200, 50, 50, 200, 100 };
                    MouthT = 200;
                    PlateT = 200;
                    break;
                case DType.B:
                    Sect = new double[] { 2500, 2500, 2000, 250, 250, 250, 50, 50, 200, 100 };
                    MouthT = 250;
                    PlateT = 250;
                    break;
                case DType.C:
                    Sect = new double[] { 4700, 4700, 2800, 400, 400, 350, 50, 50, 500, 250 };
                    MouthT = 250;
                    PlateT = 300;
                    FooterW = 400;
                    break;
                case DType.D:
                    Sect = new double[] { 4700, 4700, 3800, 400, 400, 350, 50, 50, 500, 250 };
                    MouthT = 250;
                    PlateT = 300;
                    FooterW = 400;
                    break;
                case DType.F:
                    Sect = new double[] { 4900, 4900, 2700, 350, 350, 300, 50, 50, 200, 100 };
                    break;
                case DType.G:
                    Sect = new double[] { 6900, 6900, 2700, 350, 350, 300, 50, 50, 500, 250 };
                    break;
                default:
                    Sect = new double[] { 1900, 1900, 1400, 200, 200, 200, 50, 50, 200, 100 };
                    break;
            }
            dH = cdH;
            
        }


        /// <summary>
        /// 绘制平面图
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="AnchorPoint">锚点</param>
        /// <param name="s">制图比例</param>
        public Extents2d PlotA(Database db, Point2d AnchorPoint)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            int s = ScaleA;
            var DimStyleID = dst["1-" + s.ToString()];
            Hatch hatchref4 = new Hatch();
            hatchref4.SetHatchPattern(HatchPatternType.PreDefined, "GRAVEL");
            hatchref4.PatternScale = 1;
            Point3dCollection pts = new Point3dCollection();  // 交点获取
            double ang_in_rad = 0 / 180 * Math.PI;
            double road_ang = (90-Ang) / 180 * Math.PI;
            Line[] LSets=new Line[4];
            Polyline[] LeftPolylineSet=new Polyline[3], RightPolylineSet=new Polyline[3];
            Polyline LeftPolyline=new Polyline(),RightPolyline=new Polyline();
            Extents2d CurExt;
            Point2d minPoint = AnchorPoint, maxPoint= AnchorPoint;



            if (DalotType <= DType.E)
            {
                //一孔 

                // 涵身
                double x0, x1, y0, y1;
                double dx, dy;
                x0 = -XMidDist * Math.Cos(Math.Atan(Slop)) * Math.Cos(ang_in_rad);
                x1 = (Length - XMidDist) * Math.Cos(Math.Atan(Slop)) * Math.Cos(ang_in_rad);
                y0 = +x0 * Math.Tan(ang_in_rad);
                y1 = +x1 * Math.Tan(ang_in_rad);
                double factor = Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)) / Length;
                LSets = MulitlinePloter.PlotN(db, AnchorPoint.Convert3D(x0, y0), AnchorPoint.Convert3D(x1, y1),  // 涵身
                    new double[] { -0.5 * Sect[0], -0.5 * Sect[0] + Sect[5], 0.5 * Sect[0] - Sect[5], 0.5 * Sect[0] }, new string[] { "虚线", "虚线", "虚线", "虚线" }, false);



                MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1], GetSeps(factor), "虚线");
                TextPloter.PrintCirText(db, (int)Sect[4] / 10, AnchorPoint.Convert2D(3*s, -3*s),
                    LSets[1].StartPoint.Convert2D(), LSets[LSets.Length - 2].EndPoint.Convert2D(),s);
                TextPloter.PrintLineText(db, AnchorPoint, AnchorPoint.Convert2D(-10 * s, -17.3 * s), new string[] { Pk_string(), }, true, s);
                dx = Math.Sin(ang_in_rad) * (0.5 * Sect[0] + 5 * s);
                dy = Math.Cos(ang_in_rad) * (0.5 * Sect[0] + 5 * s);
                TextPloter.DimSection(db, 'B', AnchorPoint.Convert2D(dx, -dy), AnchorPoint.Convert2D(-dx, dy), s);
                DimPloter.Dim0(db, LSets[0].StartPoint, LSets[1].StartPoint, LSets[0].StartPoint.Convert3D(15 * s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);
                DimPloter.Dim0(db, LSets[1].StartPoint, LSets[2].StartPoint, LSets[0].StartPoint.Convert3D(15 * s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);
                DimPloter.Dim0(db, LSets[2].StartPoint, LSets[3].StartPoint, LSets[0].StartPoint.Convert3D(15 * s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);


                // 全八字墙
                if (Amont == AType.BZQ)
                {
                    LeftPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], true,s);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(0), LeftPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, LSets[0].StartPoint, LSets[0].EndPoint, LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[0].GetPoint3dAt(4), LeftPolylineSet[0].GetPoint3dAt(5),
                        LeftPolylineSet[0].GetPoint3dAt(5).Convert3D(-500, 0), DimStyleID, 0.5 * Math.PI+ ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[4].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LeftPolylineSet[0].GetPoint3dAt(0).Convert3D(-15 * s, 0), DimStyleID, 120.0 / 180.0 * Math.PI + ang_in_rad);

                    DimPloter.DimAng(db, LSets[2], LeftPolylineSet[3].GetLine(1), LSets[2].StartPoint.Convert3D(-10*s, 1*s - 10*s * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[1], LeftPolylineSet[4].GetLine(1), LSets[1].StartPoint.Convert3D(-10*s, -1*s- 10*s * Math.Sin(ang_in_rad)), DimStyleID);
                            
                    //TextPloter.PrintCirText(db, 25, LeftPolylineSet[0].GetPoint2dAt(0).Convert2D(-10 * s, 0.25 * Sect[0]), s);

                    // 出口
                    RightPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], false,s);
                    DimPloter.Dim0(db, LSets[0].EndPoint, RightPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[2].GetPoint3dAt(1), RightPolylineSet[2].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[4].GetPoint3dAt(1), RightPolylineSet[0].GetPoint3dAt(0),
                        RightPolylineSet[0].GetPoint3dAt(0).Convert3D(1500, 0), DimStyleID, -120.0 / 180.0 * Math.PI+ ang_in_rad);

                    DimPloter.DimAng(db, LSets[1], RightPolylineSet[4].GetLine(1), LSets[1].EndPoint.Convert3D(10*s, -s + 10*s * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[2], RightPolylineSet[3].GetLine(1), LSets[2].EndPoint.Convert3D(10*s, s + 10*s * Math.Sin(ang_in_rad)), DimStyleID);


                    // 防冲卵石
                    Point2d[] Verts;
                    if (Slop < 0)
                    {
                        Verts = new Point2d[]                        {
                            RightPolylineSet[0].GetPoint2dAt(4),
                            RightPolylineSet[0].GetPoint2dAt(4).Convert2D(1500),
                            RightPolylineSet[0].GetPoint2dAt(5).Convert2D(1500),
                            RightPolylineSet[0].GetPoint2dAt(5),       };
                    }
                    else
                    {
                        Verts = new Point2d[]                        {
                            LeftPolylineSet[0].GetPoint2dAt(4),
                            LeftPolylineSet[0].GetPoint2dAt(4).Convert2D(-1500),
                            LeftPolylineSet[0].GetPoint2dAt(5).Convert2D(-1500),
                            LeftPolylineSet[0].GetPoint2dAt(5),       };
                    }
                    Polyline Hat32 = PolylinePloter.PlotN(db, Verts, true);
                    Hat32.Layer = "细线";
                    Hatch tst = HatchPloter.PlotH(db, Hat32, hatchref4, 10);


                    dx = ang_in_rad < 0 ? LeftPolylineSet[0].GetPoint2dAt(4).X - AnchorPoint.X
                        : LeftPolylineSet[0].GetPoint2dAt(5).X - AnchorPoint.X;
                    dy = Math.Min(LeftPolylineSet[0].GetPoint2dAt(5).Y, RightPolylineSet[0].GetPoint2dAt(5).Y) - 10 * s - AnchorPoint.Y;
                    minPoint = minPoint.Convert2D(dx, dy);
                    dx = ang_in_rad < 0 ? RightPolylineSet[0].GetPoint2dAt(5).Convert2D(0, 20 * s).X - AnchorPoint.X
                        : RightPolylineSet[0].GetPoint2dAt(4).Convert2D(0, 20 * s).X - AnchorPoint.X;
                    dy = Math.Max(LeftPolylineSet[0].GetPoint2dAt(4).Y, RightPolylineSet[0].GetPoint2dAt(4).Y) + 10 * s - AnchorPoint.Y;
                    maxPoint = AnchorPoint.Convert2D(dx, dy);

                }


                // 左入口、集水井
                else if (Amont == AType.JSJ && Slop < 0)
                {
                    LeftPolyline = PolylinePloter.PlotWellPlan(db, LSets, Sect[0], true);
                    DimPloter.Dim0(db, LeftPolyline.GetPoint3dAt(1), LeftPolyline.GetPoint3dAt(0).Convert3D(200),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LSets[0].StartPoint, LSets[0].EndPoint, LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);


                    // 出口
                    RightPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], false,s  );
                    DimPloter.Dim0(db, LSets[0].EndPoint, RightPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[2].GetPoint3dAt(1), RightPolylineSet[2].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.DimAng(db, LSets[1], RightPolylineSet[4].GetLine(1), LSets[1].EndPoint.Convert3D(1000, -10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[2], RightPolylineSet[3].GetLine(1), LSets[2].EndPoint.Convert3D(1000, 10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.Dim0(db, RightPolylineSet[4].GetPoint3dAt(1), RightPolylineSet[0].GetPoint3dAt(0),
                        RightPolylineSet[0].GetPoint3dAt(0).Convert3D(1500, 0), DimStyleID, -120.0 / 180.0 * Math.PI+ang_in_rad);

                    // 防冲卵石
                    Point2d[] Verts;
                    Verts = new Point2d[]           
                    {
                        RightPolylineSet[0].GetPoint2dAt(4),
                        RightPolylineSet[0].GetPoint2dAt(4).Convert2D(1500),
                        RightPolylineSet[0].GetPoint2dAt(5).Convert2D(1500),
                        RightPolylineSet[0].GetPoint2dAt(5),
                    };
                    Polyline Hat3 = PolylinePloter.PlotN(db, Verts, true);
                    Hat3.Layer = "细线";
                    HatchPloter.PlotH(db, Hat3, hatchref4, 10);


                    dx = ang_in_rad < 0 ? RightPolylineSet[0].GetPoint2dAt(5).Convert2D(0, 20 * s).X - AnchorPoint.X
                        : RightPolylineSet[0].GetPoint2dAt(4).Convert2D(0, 20 * s).X - AnchorPoint.X;
                    dy = Math.Max(RightPolylineSet[0].GetPoint2dAt(4).Y,LeftPolyline.GetPoint2dAt(2).Y) +10*s- AnchorPoint.Y;
                    maxPoint = AnchorPoint.Convert2D(dx, dy);
                    dx = ang_in_rad < 0 ? LeftPolyline.GetPoint2dAt(1).X - AnchorPoint.X : LeftPolyline.GetPoint2dAt(2).X - AnchorPoint.X;
                    dy = Math.Min(RightPolylineSet[0].GetPoint2dAt(5).Y, LeftPolyline.GetPoint2dAt(1).Y)-10*s - AnchorPoint.Y;
                    minPoint = minPoint.Convert2D(dx, dy);


                }


                // 右集水井
                else if (Amont == AType.JSJ && Slop > 0)
                {
                    RightPolyline = PolylinePloter.PlotWellPlan(db, LSets, Sect[0], false);
                    DimPloter.Dim0(db, RightPolyline.GetPoint3dAt(1), RightPolyline.GetPoint3dAt(0).Convert3D(-200),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    // 出口
                    LeftPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], true,s);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(0), LeftPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LSets[0].StartPoint, LSets[0].EndPoint, LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[0].GetPoint3dAt(4), LeftPolylineSet[0].GetPoint3dAt(5),
                        LeftPolylineSet[0].GetPoint3dAt(5).Convert3D(-500, 0), DimStyleID, 0.5 * Math.PI+ang_in_rad);
                    DimPloter.DimAng(db, LSets[2], LeftPolylineSet[3].GetLine(1), 
                        LSets[2].StartPoint.Convert3D(-1000, 10 - 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[1], LeftPolylineSet[4].GetLine(1),
                        LSets[1].StartPoint.Convert3D(-1000, -10 - 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.Dim0(db, LeftPolylineSet[4].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LeftPolylineSet[0].GetPoint3dAt(0).Convert3D(-15 * s, 0), DimStyleID, 120.0 / 180.0 * Math.PI+ang_in_rad);


                    // 防冲卵石
                    Point2d[] Verts;
                    Verts = new Point2d[]
                    {
                        LeftPolylineSet[0].GetPoint2dAt(4),
                        LeftPolylineSet[0].GetPoint2dAt(4).Convert2D(-1500),
                        LeftPolylineSet[0].GetPoint2dAt(5).Convert2D(-1500),
                        LeftPolylineSet[0].GetPoint2dAt(5),
                    };
                    Polyline Hat3 = PolylinePloter.PlotN(db, Verts, true);
                    Hat3.Layer = "细线";
                    HatchPloter.PlotH(db, Hat3, hatchref4, 10);

                    dx = ang_in_rad < 0 ? LeftPolylineSet[0].GetPoint2dAt(4).X - AnchorPoint.X
                        : LeftPolylineSet[0].GetPoint2dAt(5).X - AnchorPoint.X;
                    dy = Math.Min(LeftPolylineSet[0].GetPoint2dAt(5).Y, RightPolyline.GetPoint2dAt(1).Y) - 10 * s - AnchorPoint.Y;
                    minPoint = minPoint.Convert2D(dx, dy);

                    dx = ang_in_rad < 0 ? RightPolyline.GetPoint2dAt(2).X - AnchorPoint.X: RightPolyline.GetPoint2dAt(1).X - AnchorPoint.X;
                    dy = Math.Max(LeftPolylineSet[0].GetPoint2dAt(4).Y, RightPolyline.GetPoint2dAt(2).Y) + 10 * s - AnchorPoint.Y;
                    maxPoint = maxPoint.Convert2D(dx, dy);



                }


            }
            else if ((int)DalotType <= 6)
            {
                //两孔
            }
            else if ((int)DalotType <= 10)
            {
                //三孔

            }
            else
            {
                //四孔

            }




            //路基
            //pts.Clear();
            //Line[] RoadSets = MulitlinePloter.PlotN(db, minPoint.Convert3D(), minPoint.Convert3D(maxPoint.X - minPoint.X, 0),
            //    new double[] { 0, maxPoint.Y - minPoint.Y }, new string[] { "虚线", "虚线" }, true);
            //double SideDistLeft = AnchorPoint.X - minPoint.X - W1;
            //double SiedDistRight = maxPoint.X - AnchorPoint.X - W2;
            //Line[] SideSets = MulitlinePloter.PlotCutLine(db, RoadSets[0], RoadSets[1],
            //    new double[] { 0, SideDistLeft, SideDistLeft + W1 + W2, maxPoint.X - minPoint.X }, "细线", true);                        
            //MulitlinePloter.PlotSideLine(db, SideSets[1], LSets[0], LSets[LSets.Length - 1], s, true);
            //MulitlinePloter.PlotSideLine(db, SideSets[2], LSets[0], LSets[LSets.Length - 1], s, false);


            // 中心线  和 路基

            Line RoadCenter = new Line(AnchorPoint.Convert3D(), AnchorPoint.Convert3D(1));
            RoadCenter.TransformBy(Matrix3d.Rotation(road_ang,Vector3d.ZAxis,AnchorPoint.Convert3D()));
            RoadCenter=RoadCenter.CutByDoubleLine(minPoint, maxPoint);
            RoadCenter.Layer = "中心线";
            modelSpace.AppendEntity(RoadCenter);
            tr.AddNewlyCreatedDBObject(RoadCenter, true);
            Line DalotCenter = new Line(AnchorPoint.Convert3D(), AnchorPoint.Convert3D(1));
            DalotCenter = DalotCenter.CutByRect(minPoint, maxPoint);
            DalotCenter.Layer = "中心线";
            modelSpace.AppendEntity(DalotCenter);
            tr.AddNewlyCreatedDBObject(DalotCenter, true);
            DimPloter.DimAng(db, DalotCenter, RoadCenter, AnchorPoint.Convert3D(10 * s, 1 * s),DimStyleID);

            // 上下界限 和 左右坡顶

            Line LeftShoulder =(Line)RoadCenter.GetOffsetCurves(W1)[0];
            LeftShoulder = LeftShoulder.CutByDoubleLine(minPoint, maxPoint);
            LeftShoulder.Layer = "细线";
            modelSpace.AppendEntity(LeftShoulder);
            tr.AddNewlyCreatedDBObject(LeftShoulder, true);
            Line RightShoulder = (Line)RoadCenter.GetOffsetCurves(-W2)[0];
            RightShoulder = RightShoulder.CutByDoubleLine(minPoint, maxPoint);
            RightShoulder.Layer = "细线";
            modelSpace.AppendEntity(RightShoulder);
            tr.AddNewlyCreatedDBObject(RightShoulder, true);
            MulitlinePloter.PlotSideLine(db, LeftShoulder, LSets[0], LSets[LSets.Length - 1], s, true);
            MulitlinePloter.PlotSideLine(db, RightShoulder, LSets[0], LSets[LSets.Length - 1], s, false);
            Line LeftSide, RightSide;
            if (LeftPolyline.Id.IsNull)
            {
                // 左八字墙
                double tmp1 = RoadCenter.GetGeCurve().GetDistanceTo(LeftPolylineSet[0].GetPoint3dAt(5));
                double tmp2 = RoadCenter.GetGeCurve().GetDistanceTo(LeftPolylineSet[0].GetPoint3dAt(4));
                double dist = Math.Min(tmp1, tmp2);        
                LeftSide = (Line)RoadCenter.GetOffsetCurves(dist)[0];
                LeftSide = LeftSide.CutByDoubleLine(minPoint, maxPoint);
                LeftSide.Layer = "细线";
                modelSpace.AppendEntity(LeftSide);
                tr.AddNewlyCreatedDBObject(LeftSide, true);
            }
            else
            {
                // 左集水井
                double tmp1 = RoadCenter.GetGeCurve().GetDistanceTo(LeftPolyline.GetPoint3dAt(1));
                double tmp2 = RoadCenter.GetGeCurve().GetDistanceTo(LeftPolyline.GetPoint3dAt(2));
                double dist = Math.Max(tmp1, tmp2)+10*s;
                LeftSide = (Line)RoadCenter.GetOffsetCurves(dist)[0];
                LeftSide = LeftSide.CutByDoubleLine(minPoint, maxPoint);
                LeftSide.Layer = "细线";
                modelSpace.AppendEntity(LeftSide);
                tr.AddNewlyCreatedDBObject(LeftSide, true);
            }

            if (RightPolyline.Id.IsNull)
            {
                // 右八字墙
                double tmp1 = RoadCenter.GetGeCurve().GetDistanceTo(RightPolylineSet[0].GetPoint3dAt(5));
                double tmp2 = RoadCenter.GetGeCurve().GetDistanceTo(RightPolylineSet[0].GetPoint3dAt(4));
                double dist = Math.Min(tmp1, tmp2);
                RightSide = (Line)RoadCenter.GetOffsetCurves(-dist)[0];
                RightSide = RightSide.CutByDoubleLine(minPoint, maxPoint);
                RightSide.Layer = "细线";
                modelSpace.AppendEntity(RightSide);
                tr.AddNewlyCreatedDBObject(RightSide, true);
            }
            else
            {
                // 右集水井
                double tmp1 = RoadCenter.GetGeCurve().GetDistanceTo(RightPolyline.GetPoint3dAt(1));
                double tmp2 = RoadCenter.GetGeCurve().GetDistanceTo(RightPolyline.GetPoint3dAt(2));
                double dist = Math.Max(tmp1, tmp2)+10*s;
                RightSide = (Line)RoadCenter.GetOffsetCurves(-dist)[0];
                RightSide = RightSide.CutByDoubleLine(minPoint, maxPoint);
                RightSide.Layer = "细线";
                modelSpace.AppendEntity(RightSide);
                tr.AddNewlyCreatedDBObject(RightSide, true);
            }
            Line LowerSide = new Line(LeftSide.StartPoint,RightSide.StartPoint);
            LowerSide.Layer = "虚线";
            modelSpace.AppendEntity(LowerSide);
            tr.AddNewlyCreatedDBObject(LowerSide, true);
            Line UpperSide = new Line(LeftSide.EndPoint,RightSide.EndPoint);
            UpperSide.Layer = "虚线";
            modelSpace.AppendEntity(UpperSide);
            tr.AddNewlyCreatedDBObject(UpperSide, true);

            // 方向
            TextPloter.DimRoadDirect(db, BasePoint.Convert2D(0.5*W2, 30 * s), s, true);
            TextPloter.DimRoadDirect(db, BasePoint.Convert2D(-0.5*W1, -30 * s), s, false);
            //pts.Clear();
            //Line CenterX = new Line(AnchorPoint.Convert3D(-1, Math.Tan(ang_in_rad) * -1), AnchorPoint.Convert3D(1, Math.Tan(ang_in_rad)));
            //CenterX.IntersectWith(SideSets[0], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            //CenterX.StartPoint = pts[0];
            //pts.Clear();
            //CenterX.IntersectWith(SideSets[SideSets.Length-1], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            //CenterX.EndPoint = pts[0];
            //CenterX.Layer = "中心线";
            //modelSpace.AppendEntity(CenterX);
            //tr.AddNewlyCreatedDBObject(CenterX, true);
            //Line CenterY = new Line(AnchorPoint.Convert3D(0, -1), AnchorPoint.Convert3D(0, 1));
            //pts.Clear();
            //CenterY.IntersectWith(RoadSets[0], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            //CenterY.StartPoint = pts[0];
            //pts.Clear();
            //CenterY.IntersectWith(RoadSets[RoadSets.Length - 1], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            //CenterY.EndPoint = pts[0];
            //CenterY.Layer = "中心线";
            //modelSpace.AppendEntity(CenterY);
            //tr.AddNewlyCreatedDBObject(CenterY, true);




            //标题
            maxPoint = maxPoint.Convert2D(0, 15 * s);
            TextPloter.DimSection(db, 'A', AnchorPoint.Convert2D((minPoint.X - 10 * s)*Math.Cos(ang_in_rad),( minPoint.X - 10 * s)*Math.Sin(ang_in_rad))
                , AnchorPoint.Convert2D((maxPoint.X + 5 * s)*Math.Cos(ang_in_rad),( maxPoint.X + 5 * s)*Math.Sin(ang_in_rad)), s);
            double dymax = maxPoint.Y - AnchorPoint.Y;
            double dymin = minPoint.Y - AnchorPoint.Y;
            TextPloter.PrintTitle(db, "VUE EN PLAN", AnchorPoint.Convert2D(0, dymax - 5*s), s);


            // 显示边框
            double dyy = maxPoint.Y - minPoint.Y;
            double dxx = maxPoint.X - minPoint.X;
            PolylinePloter.Plot4(db, minPoint.Convert2D(0.5 * dxx), dyy, dxx);

            

            tr.Commit();
            tr.Dispose();
            CurExt = new Extents2d(minPoint, maxPoint);
            return CurExt;
        }




        

        public void PlotB(Database db, Point2d AnchorPoint, Polyline csjx,Polyline cdmx,Point3d psjx,Point3d pdmx)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            int s = ScaleA;
            var DimStyleID = dst["1-" + s.ToString()];

            Point3dCollection pts = new Point3dCollection();  // 交点获取
            Line[] LSets=new Line[4];
            Line[] CutLineSets;
            Polyline Wall_left=new Polyline(), Wall_right=new Polyline();
            double slop_rad = Math.Atan(Slop);
            Polyline hat1, hat2=new Polyline(), hat3=new Polyline();
            Point2d p0, p1, p2, p3, p4;
            // 填充
            Hatch hatchref1 = new Hatch();
            hatchref1.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            Hatch hatchref2 = new Hatch();
            hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
            Hatch hatchref3 = new Hatch();
            hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
            Hatch hatchref4 = new Hatch();
            hatchref4.SetHatchPattern(HatchPatternType.PreDefined, "GRAVEL");
            hatchref4.PatternScale = 1;
            double delt1 = (H1 - H0) * 1000;
            double delt2 = (H2 - H0) * 1000;
            double delt3 = (H3 - H0) * 1000;



            if (DalotType <= DType.E)
            {
                //一孔、涵身
                double x0, x1, y0, y1;
                x0 = -XMidDist * Math.Cos(slop_rad);
                x1 = (Length - XMidDist) * Math.Cos(slop_rad);
                y0 = -XMidDist * Math.Sin(slop_rad);
                y1 = (Length - XMidDist) * Math.Sin(slop_rad);
                bool tmp_vet = false;
                if (Sect[2] >= 2800)
                {
                    tmp_vet = true;
                }

                LSets = MulitlinePloter.PlotN(db, AnchorPoint.Convert3D(x0, y0), AnchorPoint.Convert3D(x1, y1),  // 涵身
                    new double[] { 0-Sect[3],0, Sect[2] - Sect[4] - Sect[3], Sect[2] - Sect[3] }, new string[] { "粗线", "粗线", "粗线", "粗线" }, tmp_vet);
                CutLineSets=MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1], GetSeps(), "粗线", tmp_vet);

                HatchPloter.PlotLayerOne(db, LSets[0].StartPoint.Convert2D(), LSets[0].EndPoint.Convert2D(), hatchref3,0, 100,1, tmp_vet);
                HatchPloter.PlotLayerOne(db, LSets[0].StartPoint.Convert2D(), LSets[0].EndPoint.Convert2D(), hatchref2,100,LayerT, 1, tmp_vet);
                DimPloter.DimAli(db, LSets[3].StartPoint, LSets[3].EndPoint, LSets[3].EndPoint.Convert3D(0, 1000), DimStyleID);
                DimPloter.HengPo(db, Slop * 100, AnchorPoint.Convert3D(2000, 2000 * Slop + 1 * s), (Slop > 0), s);
                DimPloter.BiaoGao(H0 + (LSets[1].StartPoint.Y - AnchorPoint.Y) / 1000, LSets[1].StartPoint, modelSpace, tr, blockTbl, s);
                DimPloter.BiaoGao(H0 + (LSets[1].EndPoint.Y - AnchorPoint.Y) / 1000, LSets[1].EndPoint, modelSpace, tr, blockTbl, s);
                TextPloter.PrintCirText(db, (int)Sect[5] / 10, AnchorPoint.Convert2D(-5*s, 3*s),
                    LSets[1].StartPoint.Convert2D(), LSets[LSets.Length - 2].EndPoint.Convert2D(), s);

                if (CutLineSets.Length == 2)
                {
                    TextPloter.PrintLineText(db, CutLineSets[0].GetXPoint2d(0.3), CutLineSets[0].GetXPoint2d(0.3,2 * s, 3 * s), 
                        new string[] { "Joint,e=2.0cm" }, false, s);
                    TextPloter.PrintLineText(db, CutLineSets[1].GetXPoint2d(0.3), CutLineSets[1].GetXPoint2d(0.3,-2 * s, 3 * s),
                        new string[] { "Joint,e=2.0cm" }, true, s);
                }
                else
                {
                    TextPloter.PrintLineText(db, CutLineSets[0].GetXPoint2d(0.3), CutLineSets[0].GetXPoint2d(0.3,2 * s, 3 * s),
                        new string[] { "Joint,e=1.5cm" }, false, s);
                    TextPloter.PrintLineText(db, CutLineSets[CutLineSets.Length - 1].GetXPoint2d(0.3),
                        CutLineSets[CutLineSets.Length - 1].GetXPoint2d(0.3,-2 * s, 3 * s), new string[] { "Joint,e=1.5cm" }, true, s);
                    int num = CutLineSets.Length / 2;
                    TextPloter.PrintLineText(db, CutLineSets[num].GetXPoint2d(0.3), CutLineSets[num].GetXPoint2d(0.3,2 * s, 3 * s),
                        new string[] { "Joint,e=1.5cm" }, false, s);
                }

                //出入口
                Point2d[] Verts;
                if (Amont == AType.BZQ)
                {

                    // 左八字墙
                    Wall_left = PolylinePloter.PlotWall(db, LSets[0].StartPoint.Convert2D(), Sect[2], slop_rad, true);


                    // 垫层
                    Verts = new Point2d[]
                    {
                        Wall_left.GetPoint2dAt(2),
                        Wall_left.GetPoint2dAt(2).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_left.GetPoint2dAt(3).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_left.GetPoint2dAt(3),
                    };
                    Polyline Hat1 = PolylinePloter.PlotN(db, Verts, true);
                    Hat1.Layer = "细线";
                    HatchPloter.PlotH(db, Hat1, hatchref1, 1);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1), Wall_left.GetPoint2dAt(0), hatchref3,0, 100, 1, tmp_vet);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1), Wall_left.GetPoint2dAt(0), hatchref2,100, LayerT,1, tmp_vet);

                    // 标注
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(6), Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4).Convert3D(-5 * s, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(4).Convert3D(-5 * s, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(4).Convert3D(-5 * s, 0), DimStyleID);
                    DimPloter.DimAli(db, Hat1.GetPoint3dAt(3), Hat1.GetPoint3dAt(2), Wall_left.GetPoint3dAt(4).Convert3D(-5 * s, 0), DimStyleID);

                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(2), Wall_left.GetPoint3dAt(2).Convert3D(0, -5*s), DimStyleID);
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_left.GetPoint2dAt(0).Convert2D(-10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_left.GetPoint2dAt(5).Y-AnchorPoint.Y)/1000+H0, Wall_left.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

                    // 右八字墙
                    Wall_right = PolylinePloter.PlotWall(db, LSets[0].EndPoint.Convert2D(), Sect[2], slop_rad, false);

                    
                    // 垫层
                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1), hatchref3, 0,100, 1, tmp_vet);
                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1), hatchref2,100, LayerT, 1, tmp_vet);
                    Verts = new Point2d[]
                    {
                        Wall_right.GetPoint2dAt(2),
                        Wall_right.GetPoint2dAt(2).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_right.GetPoint2dAt(3).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_right.GetPoint2dAt(3),
                    };
                    Polyline H2 = PolylinePloter.PlotN(db, Verts, true);
                    H2.Layer = "细线";
                    HatchPloter.PlotH(db, H2, hatchref1, 1);

                    TextPloter.PrintCirText(db, (int)Sect[5] / 10, Wall_right.GetPoint2dAt(0).Convert2D(10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_right.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_right.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_right.GetPoint3dAt(5), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

                    // 防冲卵石
                    if (Slop < 0)
                    {
                        Verts = new Point2d[]                        {                            
                            Wall_right.GetPoint2dAt(5),
                            Wall_right.GetPoint2dAt(5).Convert2D(1500*Math.Cos(slop_rad),1500*Math.Sin(slop_rad)),
                            Wall_right.GetPoint2dAt(5).Convert2D(0+1000*Math.Cos(slop_rad), -500+1000*Math.Sin(slop_rad)),
                            Wall_right.GetPoint2dAt(5).Convert2D(0, -500),                        };
                    }
                    else
                    {
                        Verts = new Point2d[]
                        {
                            Wall_left.GetPoint2dAt(5),
                            Wall_left.GetPoint2dAt(5).Convert2D(-1500*Math.Cos(slop_rad),-1500*Math.Sin(slop_rad)),
                            Wall_left.GetPoint2dAt(5).Convert2D(0-1000*Math.Cos(slop_rad), -500-1000*Math.Sin(slop_rad)),
                            Wall_left.GetPoint2dAt(5).Convert2D(0, -500),
                        };
                    }
                    Polyline Hat3 = PolylinePloter.PlotN(db, Verts, true);
                    Hat3.Layer = "细线";
                    HatchPloter.PlotH(db, Hat3, hatchref4, 1);
                }


                // 左集水井
                else if (Amont==AType.JSJ && Slop<0)
                {
                    Wall_left = PolylinePloter.PlotWell(db, LSets[1].StartPoint.Convert2D(), Slop, Sect[2], true,true);
                    DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

                    Wall_right = PolylinePloter.PlotWall(db, LSets[0].EndPoint.Convert2D(), Sect[2], slop_rad, false);
                    Verts = new Point2d[]
                    {
                        Wall_right.GetPoint2dAt(2),
                        Wall_right.GetPoint2dAt(2).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_right.GetPoint2dAt(3).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_right.GetPoint2dAt(3),
                    };
                    Polyline H2 = PolylinePloter.PlotN(db, Verts, true);
                    H2.Layer = "细线";
                    HatchPloter.PlotH(db, H2, hatchref1, 1);
                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1), hatchref3, 0,100, 1, tmp_vet);
                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1), hatchref3, 100, LayerT, 1, tmp_vet);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1).Convert2D(-200), Wall_left.GetPoint2dAt(0), hatchref3);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1).Convert2D(-200), Wall_left.GetPoint2dAt(0), hatchref2,100,LayerT);
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_right.GetPoint2dAt(0).Convert2D(10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_right.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_right.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_right.GetPoint3dAt(5), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

                    // 卵石
                    Verts = new Point2d[]  {
                            Wall_right.GetPoint2dAt(5),
                            Wall_right.GetPoint2dAt(5).Convert2D(1500*Math.Cos(slop_rad),1500*Math.Sin(slop_rad)),
                            Wall_right.GetPoint2dAt(5).Convert2D(0+1000*Math.Cos(slop_rad), -500+1000*Math.Sin(slop_rad)),
                            Wall_right.GetPoint2dAt(5).Convert2D(0, -500),   };
                    Polyline Hat3 = PolylinePloter.PlotN(db, Verts, true);
                    Hat3.Layer = "细线";
                    HatchPloter.PlotH(db, Hat3, hatchref4, 1);
                }


                // 右集水井
                else if (Amont == AType.JSJ && Slop > 0)
                {
                    Wall_right = PolylinePloter.PlotWell(db, LSets[1].EndPoint.Convert2D(), Slop, Sect[2], false, true);
                    DimPloter.Dim0(db, Wall_right.GetPoint3dAt(5), Wall_right.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);


                    Wall_left = PolylinePloter.PlotWall(db, LSets[0].StartPoint.Convert2D(), Sect[2], slop_rad, true);
                    Verts = new Point2d[]
                    {
                        Wall_left.GetPoint2dAt(2),
                        Wall_left.GetPoint2dAt(2).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_left.GetPoint2dAt(3).Convert2D(100*Math.Sin(slop_rad),-100*Math.Cos(slop_rad)),
                        Wall_left.GetPoint2dAt(3),
                    };
                    Polyline H1 = PolylinePloter.PlotN(db, Verts, true);
                    H1.Layer = "细线";
                    HatchPloter.PlotH(db, H1, hatchref1, 1);

                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1).Convert2D(200),  hatchref3);
                    HatchPloter.PlotLayerOne(db, Wall_right.GetPoint2dAt(0), Wall_right.GetPoint2dAt(1).Convert2D(200), hatchref2,100,LayerT);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1), Wall_left.GetPoint2dAt(0), hatchref3,0, 100, 1, tmp_vet);
                    HatchPloter.PlotLayerOne(db, Wall_left.GetPoint2dAt(1), Wall_left.GetPoint2dAt(0), hatchref2,  100, LayerT,1, tmp_vet);

                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(6), Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, H1.GetPoint3dAt(3), H1.GetPoint3dAt(2), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(2), Wall_left.GetPoint3dAt(2).Convert3D(0, -500), DimStyleID);
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_left.GetPoint2dAt(0).Convert2D(-10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_left.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_left.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);


                    // 卵石
                    Verts = new Point2d[]
                    {
                            Wall_left.GetPoint2dAt(5),
                            Wall_left.GetPoint2dAt(5).Convert2D(-1500*Math.Cos(slop_rad),-1500*Math.Sin(slop_rad)),
                            Wall_left.GetPoint2dAt(5).Convert2D(0-1000*Math.Cos(slop_rad), -500-1000*Math.Sin(slop_rad)),
                            Wall_left.GetPoint2dAt(5).Convert2D(0, -500),
                    };
                    Polyline Hat3 = PolylinePloter.PlotN(db, Verts, true);
                    Hat3.Layer = "细线";
                    HatchPloter.PlotH(db, Hat3, hatchref4, 1);
                }






                // 对齐标注






            }
            else if ((int)DalotType <= 6)
            {
                //两孔
            }
            else if ((int)DalotType <= 10)
            {
                //三孔

            }
            else
            {
                //四孔

            }

            // 绘制帽子

            Polyline Road = (Polyline)csjx.Clone();                
            Road.TransformBy(Matrix3d.Scaling(1000, psjx));            
            Road.TransformBy(Matrix3d.Displacement(psjx.GetVectorTo(AnchorPoint.Convert3D())));
            Road.TransformBy(Matrix3d.Displacement(new Vector3d(0,delt2, 0)));
            Road.Layer = "sjx";
            modelSpace.AppendEntity(Road);
            tr.AddNewlyCreatedDBObject(Road, true);

            Polyline Land = (Polyline)cdmx.Clone();
            Land.TransformBy(Matrix3d.Scaling(1000, psjx));
            Land.TransformBy(Matrix3d.Displacement(psjx.GetVectorTo(AnchorPoint.Convert3D())));
            Land.TransformBy(Matrix3d.Displacement(new Vector3d(0, delt2, 0)));
            Land.Layer = "dmx";
            Point3d cutpoint = Land.GetClosestPointTo(AnchorPoint.Convert3D(20000), Vector3d.YAxis, true);
            Line cutline = new Line(cutpoint, cutpoint.Convert3D(0, 100, 0));
            pts.Clear();
            Land.IntersectWith(cutline, Intersect.ExtendArgument, pts, IntPtr.Zero, IntPtr.Zero);
            var tmp=Land.GetSplitCurves(pts);
            Land = (Polyline)tmp[0];            
            modelSpace.AppendEntity(Land);
            tr.AddNewlyCreatedDBObject(Land, true);

            Point3d shoulder1 = Road.GetClosestPointTo(AnchorPoint.Convert3D(-W1, 0), Vector3d.YAxis, true);
            Point3d shoulder2 = Road.GetClosestPointTo(AnchorPoint.Convert3D(0, 0), Vector3d.YAxis, true);
            Point3d shoulder3 = Road.GetClosestPointTo(AnchorPoint.Convert3D(W2, 0), Vector3d.YAxis.Negate(), true);
            Point3d DalotTop1 = LSets[LSets.Length - 1].GetClosestPointTo(shoulder1, true);
            Point3d DalotTop2 = LSets[LSets.Length - 1].GetClosestPointTo(shoulder2, true);
            Point3d DalotTop3 = LSets[LSets.Length - 1].GetClosestPointTo(shoulder3, true);


            DimPloter.BiaoGao(H1, shoulder1, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H2, shoulder2, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H3, shoulder3, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H0, AnchorPoint.Convert3D(), modelSpace, tr, blockTbl, s);

            // 路肩水平标注
            DimPloter.Dim0(db, LSets[1].StartPoint, shoulder1, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder1, shoulder2, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder2, shoulder3, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder3, LSets[1].EndPoint, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            // 路肩竖直标注
            DimPloter.Dim0(db, shoulder1, DalotTop1, shoulder1, DimStyleID,0.5*Math.PI);
            DimPloter.Dim0(db, shoulder2, DalotTop2, shoulder2, DimStyleID, 0.5 * Math.PI);
            DimPloter.Dim0(db, shoulder3, DalotTop3, shoulder3, DimStyleID, 0.5 * Math.PI);




            //DimPloter.Dim0(db,  Wall_left.GetPoint3dAt(12),Road.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db,  Road.GetPoint3dAt(1), Road.GetPoint3dAt(2), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db, Road.GetPoint3dAt(2), Road.GetPoint3dAt(3), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db, Road.GetPoint3dAt(3), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

            //DimPloter.Dim0(db, Wall_right.GetPoint3dAt(12),Wall_right.GetPoint3dAt(5), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.DimAng(db, Wall_left.GetLine(6), Wall_left.GetLine(11), Wall_left.GetPoint3dAt(7).Convert3D(0, -10*s),DimStyleID);
            //DimPloter.DimAng(db, Wall_right.GetLine(6), Wall_right.GetLine(11), Wall_right.GetPoint3dAt(7).Convert3D(0, -10*s), DimStyleID);


            // 中心线
            pts.Clear();
            Line CenterX = new Line(AnchorPoint.Convert3D(),shoulder2.Convert3D(0,11*s));
            CenterX.Layer = "中心线";
            modelSpace.AppendEntity(CenterX);
            tr.AddNewlyCreatedDBObject(CenterX, true);


            if (Slop < 0)
            {
                TextPloter.PrintText(db, "AMONT", 3, LSets[LSets.Length - 1].StartPoint.Convert2D(-1000, 500), s);
                TextPloter.PrintText(db, "AVALE", 3, LSets[LSets.Length - 1].EndPoint.Convert2D(1000, 500), s);
            }
            else
            {
                TextPloter.PrintText(db, "AVALE", 3, LSets[LSets.Length - 1].StartPoint.Convert2D(-1000, 500), s);
                TextPloter.PrintText(db, "AMONT", 3, LSets[LSets.Length - 1].EndPoint.Convert2D(1000, 500), s);
            }

            TextPloter.PrintTitle(db, "COUPE A-A", AnchorPoint.Convert2D(0, delt2 + 20 * s),s);
            if (LayerT != 0)
            {
                TextPloter.PrintLineText(db, LSets[0].GetMidPoint2d(-1000, -1000 * Slop), LSets[0].GetMidPoint2d(-1000, -1000 * Slop - 1000),
                    new string[] { "C12/15 B.P e=10cm", string.Format("Graveleux lateritique e={0}cm", (LayerT / 10).ToString()) }, false, s);
            }
            else
            {
                TextPloter.PrintLineText(db, LSets[0].GetMidPoint2d(-1000, -1000 * Slop), LSets[0].GetMidPoint2d(-1000, -1000 * Slop - 1000),
                    new string[] { "C12/15 B.P e=10cm"  }, false, s);
            }




            tr.Commit();
            tr.Dispose();
        }






        /// <summary>
        /// 绘制剖面图
        /// </summary>
        /// <param name="s">绘图比例，默认1:75</param>
        /// <returns>回填面积、素混凝土面积、红土粒料面积</returns>
        public double[] PlotC(Database db, Point2d AnchorPoint)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            int s = ScaleB;
            DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            var DimStyleID = dst["1-" + s.ToString()];
            Point2d BB = AnchorPoint;
            // 填充
            Hatch hatchref1 = new Hatch();
            hatchref1.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            Hatch hatchref2 = new Hatch();
            hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
            Hatch hatchref3 = new Hatch();
            hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
            Hatch hatch1, hatch2, hatch3, hatch4;
            Polyline PL4 = new Polyline();
            double[] AreaS = new double[6];

            if ((int)DalotType <= 4)
            {
                Polyline PL1 = new Polyline();
                if (DalotType >= DType.C)
                {
                    PL1 = PolylinePloter.Plot11(db, AnchorPoint, this, 0.01, 400);   // 外框
                }
                else
                {
                    PL1 = PolylinePloter.Plot11(db, AnchorPoint, this, 0.01, 0);   // 外框
                }                
                PL1.Layer = "粗线";
                AreaS[5] = PL1.Area / 1000000;
                AreaS[3] = (PL1.GetLineSegmentAt(1).Length + PL1.GetLineSegmentAt(2).Length + PL1.GetLineSegmentAt(3).Length + PL1.GetLineSegmentAt(4).Length) / 1000;
                AreaS[4] = (PL1.GetLineSegmentAt(5).Length + PL1.GetLineSegmentAt(6).Length) / 1000;
                Polyline PL2 = PolylinePloter.Plot8(db, BB.Convert2D(0, Sect[3]),
                    Sect[2] - Sect[3] - Sect[4], Sect[0] - 2 * Sect[5]);
                PL2.Layer = "粗线";
                Polyline PL3 = PolylinePloter.Plot4(db, BB.Convert2D(0, -100), 100, Sect[0]+FooterW*2 + 2 * 200);   // 外框
                PL3.Layer = "细线";
                hatch1 = HatchPloter.PlotH(db, PL3, hatchref3, 0.2);
                AreaS[1] = PL3.Area/1000000;
                if (LayerT != 0)
                {
                    PL4 = PolylinePloter.Plot4(db, BB.Convert2D(0, -100-LayerT), LayerT, Sect[0] + 2 * LayerW);   // 外框
                    PL4.Layer = "细线";
                    hatch2 = HatchPloter.PlotH(db, PL4, hatchref2, 0.2);
                    AreaS[2] = PL4.Area/1000000;
                }


                // H1
                double disttmp = 0;
                if(DalotType<=DType.B)
                {
                    disttmp = 300;
                }
                else
                {
                    disttmp = 1800;
                }
                Point2d[] pts = new Point2d[]
                {
                    PL3.GetPoint2dAt(0).Convert2D(-disttmp,0),
                    PL3.GetPoint2dAt(0).Convert2D(-disttmp-100-Sect[2],100+Sect[2]),
                    PL1.GetPoint2dAt(7),
                    PL1.GetPoint2dAt(8),
                    PL1.GetPoint2dAt(9),
                    PL1.GetPoint2dAt(10),
                    PL1.GetPoint2dAt(0),
                    PL3.GetPoint2dAt(3),
                    PL3.GetPoint2dAt(0)
                };
                Polyline Hat1 = PolylinePloter.PlotN(db, pts, true);
                Hat1.Layer = "细线";
                HatchPloter.PlotH(db, Hat1, hatchref2, 0.2);
                              
                pts = new Point2d[]
                {
                    PL3.GetPoint2dAt(1).Convert2D(disttmp,0),
                    PL3.GetPoint2dAt(1).Convert2D(disttmp+100+Sect[2],100+Sect[2]),
                    PL1.GetPoint2dAt(5),
                    PL1.GetPoint2dAt(4),
                    PL1.GetPoint2dAt(3),
                    PL1.GetPoint2dAt(2),
                    PL1.GetPoint2dAt(1),
                    PL3.GetPoint2dAt(2),
                    PL3.GetPoint2dAt(1)
                };
                Polyline Hat2 = PolylinePloter.PlotN(db, pts, true);
                Hat2.Layer = "细线";
                HatchPloter.PlotH(db, Hat2, hatchref2, 0.2);
                double tiantu = ((H2 - H0) * 1000 - Sect[2]) >= 1000 ? 1000 : ((H2 - H0) * 1000 - Sect[2]);
                pts = new Point2d[]
                {
                    Hat1.GetPoint2dAt(1),
                    Hat1.GetPoint2dAt(1).Convert2D(-tiantu,tiantu),
                    Hat2.GetPoint2dAt(1).Convert2D(tiantu,tiantu),
                    Hat2.GetPoint2dAt(1),
                };
                Polyline PL5 = PolylinePloter.PlotN(db, pts, false);
                PL5.Layer = "细线";
                double tmp1 = Math.Abs(PL5.GetPoint2dAt(3).X - PL5.GetPoint2dAt(0).X);
                double tmp2 = tmp1 + tiantu*2;

                AreaS[0] = (Hat1.Area * 2 + (tmp1 + tmp2) * tiantu * 0.5) / 1000000;

                // 标注
                DimPloter.Dim0(db, PL1.GetPoint3dAt(7), PL2.GetPoint3dAt(7), PL1.GetPoint2dAt(7).Convert3D(0, 5*s), DimStyleID);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(7), PL2.GetPoint3dAt(3), PL1.GetPoint2dAt(7).Convert3D(0, 5*s), DimStyleID);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(3), PL1.GetPoint3dAt(5), PL1.GetPoint2dAt(7).Convert3D(0, 5*s), DimStyleID);

                DimPloter.Dim0(db, PL1.GetPoint3dAt(5), PL2.GetPoint3dAt(4), Hat2.GetPoint2dAt(0).Convert3D(8*s, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(4), PL2.GetPoint3dAt(1), Hat2.GetPoint2dAt(0).Convert3D(8*s, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(1), PL1.GetPoint3dAt(1), Hat2.GetPoint2dAt(0).Convert3D(8*s, 0), DimStyleID, 0.5 * Math.PI);

                DimPloter.Dim0(db, PL1.GetPoint3dAt(5), PL1.GetPoint3dAt(1), Hat2.GetPoint2dAt(0).Convert3D(11*s, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL3.GetPoint3dAt(2), PL3.GetPoint3dAt(1), Hat2.GetPoint2dAt(0).Convert3D(11*s, 0), DimStyleID, 0.5 * Math.PI);
                if (LayerT!=0)
                {
                    DimPloter.Dim0(db, PL4.GetPoint3dAt(2), PL4.GetPoint3dAt(1), Hat2.GetPoint2dAt(0).Convert3D(11*s, 0), DimStyleID, 0.5 * Math.PI);
                    //DimPloter.Dim0(db, PL4.GetPoint3dAt(0), PL1.GetPoint3dAt(7), PL4.GetPoint2dAt(0).Convert3D(0, -5*s), DimStyleID);
                    //DimPloter.Dim0(db, PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1), PL4.GetPoint2dAt(0).Convert3D(0, -5 * s), DimStyleID);
                    //DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL4.GetPoint3dAt(1), PL4.GetPoint2dAt(0).Convert3D(0, -5 * s), DimStyleID);

                    //DimPloter.Dim0(db, PL4.GetPoint3dAt(3), PL3.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                    //DimPloter.Dim0(db, PL3.GetPoint3dAt(1), PL4.GetPoint3dAt(2), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                }
                else
                {
                    //DimPloter.Dim0(db, H1.GetPoint3dAt(0), PL3.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    //DimPloter.Dim0(db, PL3.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    //DimPloter.Dim0(db, PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    //DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL3.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    //DimPloter.Dim0(db, PL3.GetPoint3dAt(1), H2.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                }
                DimPloter.Dim0(db, Hat1.GetPoint3dAt(0), PL3.GetPoint3dAt(0), PL1.GetPoint2dAt(10).Convert3D(0, 3*s), DimStyleID);
                DimPloter.Dim0(db, PL3.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL1.GetPoint2dAt(10).Convert3D(0, 3 * s), DimStyleID);
                DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL3.GetPoint3dAt(1), PL1.GetPoint2dAt(10).Convert3D(0, 3 * s), DimStyleID);
                DimPloter.Dim0(db, PL3.GetPoint3dAt(1), Hat2.GetPoint3dAt(0), PL1.GetPoint2dAt(10).Convert3D(0, 3 * s), DimStyleID);

                //DimPloter.Dim0(db, H1.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL1.GetPoint2dAt(0).Convert3D(0, -100-LayerT-5 * s), DimStyleID);
                //DimPloter.Dim0(db, PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1), PL1.GetPoint2dAt(0).Convert3D(0, -100 - LayerT - 5 * s), DimStyleID);
                //DimPloter.Dim0(db, PL1.GetPoint3dAt(1), H2.GetPoint3dAt(0), PL1.GetPoint2dAt(0).Convert3D(0, -100 - LayerT - 5 * s), DimStyleID);

                DimPloter.HengPo(db, 1.0, PL1.GetPoint3dAt(6).Convert3D(-0.25 * Sect[0], 100), true, s);
                DimPloter.HengPo(db, 1.0, PL1.GetPoint3dAt(6).Convert3D(+0.25 * Sect[0], 100), false, s);

                TextPloter.PrintLineText(db, PL2.GetLine(5).GetMidPoint2d(), PL2.GetLine(5).GetMidPoint2d(3 * s, -3 * s), 
                    new string[] { string.Format("{0}*{1}",Sect[8]/10,Sect[9]/10) }, false, s);
                TextPloter.PrintLineText(db, PL2.GetLine(7).GetMidPoint2d(), PL2.GetLine(7).GetMidPoint2d(3 * s, 3 * s),
                    new string[] { string.Format("{0}*{1}", Sect[6] / 10, Sect[7] / 10) }, false, s);

                DBText temp = TextPloter.PrintText(db, "1/1", 2.5, Hat1.GetPoint2dAt(1).Convert2D(8*s, -8 * s), s);                
                temp.Rotation=-0.25 * Math.PI;
                DBText temp2 = TextPloter.PrintText(db, "1/1", 2.5, Hat2.GetPoint2dAt(1).Convert2D(-8 * s, -8 * s), s);
                temp2.Rotation = 0.25 * Math.PI;
                TextPloter.PrintText(db, "REMBLAI.G.L", 3, Hat1.GetPoint2dAt(2).Convert2D(-10*s ,-10*s), s);
                //-------------------------------------------------------------------------------------------





            }
            else if ((int)DalotType <= 6)
            {
                //两孔
            }
            else if ((int)DalotType <= 10)
            {
                //三孔

            }
            else
            {
                //四孔

            }
            if (LayerT == 0)
            {
                TextPloter.PrintLineText(db, AnchorPoint.Convert2D(-1000, -50), AnchorPoint.Convert2D(-1000, -LayerT-5*s), new string[]
                { "C12/15 B.P e=10cm" }, false, s);
            }
            else
            {
                TextPloter.PrintLineText(db, AnchorPoint.Convert2D(-1000, -50), AnchorPoint.Convert2D(-1000, -LayerT-5*s), new string[]
                { "C12/15 B.P e=10cm", string.Format("Graveleux lateritique e={0}cm",(LayerT/10).ToString()) }, false, s);
            }
      
                   
            TextPloter.PrintTitle(db, "COUPE B-B", AnchorPoint.Convert2D(0, Sect[2]+21.5*s), s);


            tr.Commit();
            tr.Dispose();


            


            return AreaS;
        }


        /// <summary>
        /// 返回3m2m节段数
        /// </summary>
        /// <returns></returns>
        public int[] GetSegNum()
        {
            int[] res = new int[2];
            int numSeg;
            double sideLength;            
            int numThree=0, numTwo=0;

            if (DalotType == DType.A)
            {
                double reds = Length % 3000;
                if (reds == 1000)
                {
                    numThree = (int)Length / 3000;
                    numThree--;
                }
                else
                {
                    numThree = (int)Length / 3000;
                }


                sideLength = Length - numThree * 3000;
                numTwo = (int)sideLength / 2000;
                res[1] = numThree;
                res[0] = numTwo;
            }
            else
            {
                numSeg = (int)(Length / SegLength);
                res[1] = numSeg;
                res[0] = numSeg;
            }

            return res;

        }

        public double[] GetSeps(double factor=1.0)
        {
            double[] res;
            int numSeg;
            double sideLength;            
            int numThree=0, numTwo=0;
            if (DalotType == DType.A)
            {
                double reds = Length % 3000;
                if (reds == 1000)
                {
                    numThree = (int)Length / 3000;
                    numThree--;
                }
                else
                {
                    numThree = (int)Length / 3000;
                }

                sideLength = Length - numThree * 3000;
                numTwo = (int)sideLength / 2000;
                res = new double[numThree + numTwo+1];

                if (numTwo == 0)
                {                    
                    for(int i = 0; i < res.Length; i++)
                    {
                        res[i] = i * 3000;
                    }
                }
                else
                {
                    res[0] = 2000;
                    for (int i = 1; i <= numThree; i++)
                    {
                        res[i] = i * 3000+2000;
                    }
                    for (int i = 1; i < numTwo; i++)
                    {
                        res[i] = numThree * 3000 + 2000+i*2000;
                    }
                }
            }
            else if(DalotType==DType.B)
            {
                numSeg = (int)(Length / SegLength);
                res = new double[numSeg + 1];
                for (int i = 0; i <= numSeg; i++)
                {
                    res[i] = i * SegLength;
                }

            }
            else
            {
                numSeg = (int)(Length / SegLength);
                res = new double[numSeg + 1];
                for (int i = 0; i <= numSeg; i++)
                {
                    res[i] = i * SegLength;
                }
            }

            int j = 0;
            foreach(double tmp in res)
            {
                res[j] = tmp * factor;
                j++;
            }

            return res;
            
        }




        public static double PkString2Double(string pks)
        {
            if (pks == null)
            {
                return 0;
            }

            MatchCollection matches = Regex.Matches(pks, @"(\d+\.?\d*)");
            if (matches.Count == 1)
            {
                return Convert.ToDouble(matches[0].Value);
            }
            else if (matches.Count == 2)
            {
                return Convert.ToDouble(matches[0].Value) * 1000 + Convert.ToDouble(matches[1].Value);
            }
            else
            {
                return 0;
            }
        }

        public string Pk_string()
        {
            string res;
            int kilo=(int)Pk / 1000;
            double meter = Pk % 1000;
            res = string.Format("PK{0}+{1:0.000}", kilo, meter);

            return res;
        }


        public void ReadData()
        {
            BPublicFunctions.GetXPath("请选择dat文件");
            
            return;
        }


    }
}
