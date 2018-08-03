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
        public bool isTriAmont;

               

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

            switch (DalotType)
            {
                case DType.A:
                    Sect = new double[] { 1900, 1900, 1400, 200, 200, 200, 50, 50, 200, 100 };
                    break;
                case DType.B:
                    Sect = new double[] { 2500, 2500, 2000, 250, 250, 250, 50, 50, 200, 100 };
                    break;
                case DType.C:
                    Sect = new double[] { 4700, 4700, 2800, 400, 400, 350, 50, 50, 200, 100 };
                    break;
                case DType.D:
                    Sect = new double[] { 4700, 4700, 3800, 400, 400, 350, 50, 50, 200, 100 };
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
            
            Point3dCollection pts = new Point3dCollection();  // 交点获取
            double ang_in_rad = Ang / 180 * Math.PI;
            Line[] LSets=new Line[4];
            Polyline[] LeftPolylineSet=new Polyline[3], RightPolylineSet=new Polyline[3];
            Polyline LeftPolyline,RightPolyline;
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
                TextPloter.PrintCirText(db, (int)Sect[4] / 10, AnchorPoint.Convert2D(300, -300),
                    LSets[1].StartPoint.Convert2D(), LSets[LSets.Length - 2].EndPoint.Convert2D());
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
                    LeftPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], true);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(0), LeftPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, LSets[0].StartPoint, LSets[0].EndPoint, LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[0].GetPoint3dAt(4), LeftPolylineSet[0].GetPoint3dAt(5),
                        LeftPolylineSet[0].GetPoint3dAt(5).Convert3D(-500, 0), DimStyleID, 0.5 * Math.PI+ ang_in_rad);
                    DimPloter.Dim0(db, LeftPolylineSet[4].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LeftPolylineSet[0].GetPoint3dAt(0).Convert3D(-15 * s, 0), DimStyleID, 120.0 / 180.0 * Math.PI + ang_in_rad);

                    DimPloter.DimAng(db, LSets[2], LeftPolylineSet[3].GetLine(1), LSets[2].StartPoint.Convert3D(-1000, 10 - 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[1], LeftPolylineSet[4].GetLine(1), LSets[1].StartPoint.Convert3D(-1000, -10 - 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                            
                    TextPloter.PrintCirText(db, 25, LeftPolylineSet[0].GetPoint2dAt(0).Convert2D(-10 * s, 0.25 * Sect[0]), s);

                    // 出口
                    RightPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], false);
                    DimPloter.Dim0(db, LSets[0].EndPoint, RightPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[2].GetPoint3dAt(1), RightPolylineSet[2].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID, ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[4].GetPoint3dAt(1), RightPolylineSet[0].GetPoint3dAt(0),
                        RightPolylineSet[0].GetPoint3dAt(0).Convert3D(1500, 0), DimStyleID, -120.0 / 180.0 * Math.PI+ ang_in_rad);

                    DimPloter.DimAng(db, LSets[1], RightPolylineSet[4].GetLine(1), LSets[1].EndPoint.Convert3D(1000, -10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[2], RightPolylineSet[3].GetLine(1), LSets[2].EndPoint.Convert3D(1000, 10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);

                    TextPloter.PrintCirText(db, 25, RightPolylineSet[0].GetPoint2dAt(0).Convert2D(10 * s, 0.25 * Sect[0]), s);


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
                    RightPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], false);
                    DimPloter.Dim0(db, LSets[0].EndPoint, RightPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.Dim0(db, RightPolylineSet[2].GetPoint3dAt(1), RightPolylineSet[2].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID,ang_in_rad);
                    DimPloter.DimAng(db, LSets[1], RightPolylineSet[4].GetLine(1), LSets[1].EndPoint.Convert3D(1000, -10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.DimAng(db, LSets[2], RightPolylineSet[3].GetLine(1), LSets[2].EndPoint.Convert3D(1000, 10 + 1000 * Math.Sin(ang_in_rad)), DimStyleID);
                    DimPloter.Dim0(db, RightPolylineSet[4].GetPoint3dAt(1), RightPolylineSet[0].GetPoint3dAt(0),
                        RightPolylineSet[0].GetPoint3dAt(0).Convert3D(1500, 0), DimStyleID, -120.0 / 180.0 * Math.PI+ang_in_rad);

                    TextPloter.PrintCirText(db, 25, RightPolylineSet[0].GetPoint2dAt(0).Convert2D(10 * s, 0.25 * Sect[0]), s);


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
                    LeftPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], true);
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
                    TextPloter.PrintCirText(db, 25, LeftPolylineSet[0].GetPoint2dAt(0).Convert2D(-10 * s, 0.25 * Sect[0]), s);

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
            pts.Clear();
            Line[] RoadSets = MulitlinePloter.PlotN(db, minPoint.Convert3D(), minPoint.Convert3D(maxPoint.X - minPoint.X, 0),
                new double[] { 0, maxPoint.Y - minPoint.Y }, new string[] { "虚线", "虚线" }, true);
            double SideDistLeft = AnchorPoint.X - minPoint.X - W1;
            double SiedDistRight = maxPoint.X - AnchorPoint.X - W2;
            Line[] SideSets = MulitlinePloter.PlotCutLine(db, RoadSets[0], RoadSets[1],
                new double[] { 0, SideDistLeft, SideDistLeft + W1 + W2, maxPoint.X - minPoint.X }, "细线", true);                        
            MulitlinePloter.PlotSideLine(db, SideSets[1], LSets[0], LSets[LSets.Length - 1], s, true);
            MulitlinePloter.PlotSideLine(db, SideSets[2], LSets[0], LSets[LSets.Length - 1], s, false);


            // 中心线
            pts.Clear();
            Line CenterX = new Line(AnchorPoint.Convert3D(-1, Math.Tan(ang_in_rad) * -1), AnchorPoint.Convert3D(1, Math.Tan(ang_in_rad)));
            CenterX.IntersectWith(SideSets[0], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            CenterX.StartPoint = pts[0];
            pts.Clear();
            CenterX.IntersectWith(SideSets[SideSets.Length-1], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            CenterX.EndPoint = pts[0];
            CenterX.Layer = "中心线";
            modelSpace.AppendEntity(CenterX);
            tr.AddNewlyCreatedDBObject(CenterX, true);
            Line CenterY = new Line(AnchorPoint.Convert3D(0, -1), AnchorPoint.Convert3D(0, 1));
            pts.Clear();
            CenterY.IntersectWith(RoadSets[0], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            CenterY.StartPoint = pts[0];
            pts.Clear();
            CenterY.IntersectWith(RoadSets[RoadSets.Length - 1], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
            CenterY.EndPoint = pts[0];
            CenterY.Layer = "中心线";
            modelSpace.AppendEntity(CenterY);
            tr.AddNewlyCreatedDBObject(CenterY, true);

            // 方向
            TextPloter.DimRoadDirect(db, BasePoint.Convert2D(2100,3000), s, true);
            TextPloter.DimRoadDirect(db, BasePoint.Convert2D(-2100,-3000), s, false);


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
            Polyline Wall_left=new Polyline(), Wall_right=new Polyline();
            double slop_rad = Math.Atan(Slop);
            Point2d p0, p1, p2, p3, p4;
            // 填充
            Hatch hatchref1 = new Hatch();
            hatchref1.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            Hatch hatchref2 = new Hatch();
            hatchref2.SetHatchPattern(HatchPatternType.PreDefined, "AR-SAND");
            Hatch hatchref3 = new Hatch();
            hatchref3.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
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
                    new double[] { 0-Sect[3],0, Sect[2] - Sect[4] - Sect[3], Sect[2] - Sect[3] }, new string[] { "粗线", "细线", "细线", "粗线" }, tmp_vet);

                MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1], GetSeps(), "细线",tmp_vet);
                MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1],new  double[]{0,LSets[0].Length}, "细线", tmp_vet);
                DimPloter.DimAli(db, LSets[3].StartPoint, LSets[3].EndPoint, LSets[3].EndPoint.Convert3D(0, 1000), DimStyleID);
                DimPloter.HengPo(db, Slop * 100, AnchorPoint.Convert3D(2000, 2000 * Slop + 1 * s), (Slop > 0), s);
                DimPloter.BiaoGao(H0 + (LSets[1].StartPoint.Y - AnchorPoint.Y) / 1000, LSets[1].StartPoint, modelSpace, tr, blockTbl, s);
                DimPloter.BiaoGao(H0 + (LSets[1].EndPoint.Y - AnchorPoint.Y) / 1000, LSets[1].EndPoint, modelSpace, tr, blockTbl, s);


                //出入口
                Point2d[] Verts;
                if (Amont == AType.BZQ)
                {

                    // 左八字墙
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
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(6), Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, H1.GetPoint3dAt(3), H1.GetPoint3dAt(2), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(2), Wall_left.GetPoint3dAt(2).Convert3D(0, -500), DimStyleID);
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_left.GetPoint2dAt(0).Convert2D(-10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_left.GetPoint2dAt(5).Y-AnchorPoint.Y)/1000+H0, Wall_left.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

                    // 右八字墙
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
                    TextPloter.PrintCirText(db, (int)Sect[5] / 10, Wall_right.GetPoint2dAt(0).Convert2D(10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_right.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_right.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_right.GetPoint3dAt(5), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
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
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_right.GetPoint2dAt(0).Convert2D(10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_right.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_right.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_right.GetPoint3dAt(5), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
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
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(6), Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(4), Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, H1.GetPoint3dAt(3), H1.GetPoint3dAt(2), Wall_left.GetPoint3dAt(4).Convert3D(-500, 0), DimStyleID);
                    DimPloter.DimAli(db, Wall_left.GetPoint3dAt(3), Wall_left.GetPoint3dAt(2), Wall_left.GetPoint3dAt(2).Convert3D(0, -500), DimStyleID);
                    TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_left.GetPoint2dAt(0).Convert2D(-10 * s, 8 * s), s);
                    DimPloter.BiaoGao((Wall_left.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_left.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
                    DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
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
            Point3d shoulder3 = Road.GetClosestPointTo(AnchorPoint.Convert3D(W2, 0), Vector3d.YAxis, true);
            DimPloter.BiaoGao(H1, shoulder1, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H2, shoulder2, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H3, shoulder3, modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H0, AnchorPoint.Convert3D(), modelSpace, tr, blockTbl, s);

            DimPloter.Dim0(db, LSets[1].StartPoint, shoulder1, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder1, shoulder2, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder2, shoulder3, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, shoulder3, LSets[1].EndPoint, AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);





            //DimPloter.Dim0(db,  Wall_left.GetPoint3dAt(12),Road.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db,  Road.GetPoint3dAt(1), Road.GetPoint3dAt(2), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db, Road.GetPoint3dAt(2), Road.GetPoint3dAt(3), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.Dim0(db, Road.GetPoint3dAt(3), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

            //DimPloter.Dim0(db, Wall_right.GetPoint3dAt(12),Wall_right.GetPoint3dAt(5), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            //DimPloter.DimAng(db, Wall_left.GetLine(6), Wall_left.GetLine(11), Wall_left.GetPoint3dAt(7).Convert3D(0, -10*s),DimStyleID);
            //DimPloter.DimAng(db, Wall_right.GetLine(6), Wall_right.GetLine(11), Wall_right.GetPoint3dAt(7).Convert3D(0, -10*s), DimStyleID);





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
            TextPloter.PrintCirText(db, (int)Sect[5] / 10, AnchorPoint.Convert2D(-500,300),
                LSets[1].StartPoint.Convert2D(),LSets[LSets.Length-2].EndPoint.Convert2D(), s);
            TextPloter.PrintTitle(db, "COUPE A-A", AnchorPoint.Convert2D(0, delt2 + 20 * s),s);
            TextPloter.PrintLineText(db, LSets[0].GetMidPoint2d(-1000, -1000 * Slop), LSets[0].GetMidPoint2d(-1000, -1000 * Slop - 1000),
                new string[] { "C12/15 B.P e=10cm",string.Format( "Graveleux lateritique e={0}cm",(LayerT/10).ToString()) }, false, s);




            tr.Commit();
            tr.Dispose();
        }






        /// <summary>
        /// 绘制剖面图
        /// </summary>
        /// <param name="s">绘图比例，默认1:75</param>
        public void PlotC(Database db, Point2d AnchorPoint)
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

            if ((int)DalotType <= 4)
            {
                Polyline PL1 = PolylinePloter.Plot5(db, BB, Sect[2], Sect[0]);   // 外框
                PL1.Layer = "粗线";
                Polyline PL2 = PolylinePloter.Plot8(db, BB.Convert2D(0, Sect[3]),
                    Sect[2] - Sect[3] - Sect[4], Sect[0] - 2 * Sect[5]);
                PL2.Layer = "粗线";
                Polyline PL3 = PolylinePloter.Plot4(db, BB.Convert2D(0, -100), 100, Sect[0] + 2 * 200);   // 外框
                PL3.Layer = "细线";
                hatch1 = HatchPloter.PlotH(db, PL3, hatchref3, 0.2);
                if (LayerT != 0)
                {
                    PL4 = PolylinePloter.Plot4(db, BB.Convert2D(0, -100-LayerT), LayerT, Sect[0] + 2 * LayerW);   // 外框
                    PL4.Layer = "细线";
                    hatch2 = HatchPloter.PlotH(db, PL4, hatchref2, 0.2);
                }
                Point2d[] pts = new Point2d[]
                {
                    PL3.GetPoint2dAt(0).Convert2D(-300,0),
                    PL3.GetPoint2dAt(0).Convert2D(-300-100-Sect[2],100+Sect[2]),
                    PL1.GetPoint2dAt(4),
                    PL1.GetPoint2dAt(0),
                    PL3.GetPoint2dAt(3),
                    PL3.GetPoint2dAt(0)
                };
                Polyline H1 = PolylinePloter.PlotN(db, pts, true);
                H1.Layer = "细线";
                HatchPloter.PlotH(db, H1, hatchref2, 0.2);
                pts = new Point2d[]
                {
                    PL3.GetPoint2dAt(1).Convert2D(300,0),
                    PL3.GetPoint2dAt(1).Convert2D(300+100+Sect[2],100+Sect[2]),
                    PL1.GetPoint2dAt(2),
                    PL1.GetPoint2dAt(1),
                    PL3.GetPoint2dAt(2),
                    PL3.GetPoint2dAt(1)
                };
                Polyline H2 = PolylinePloter.PlotN(db, pts, true);
                H2.Layer = "细线";
                HatchPloter.PlotH(db, H2, hatchref2, 0.2);
                pts = new Point2d[]
                {
                    H1.GetPoint2dAt(1),
                    H1.GetPoint2dAt(1).Convert2D(-1000,1000),
                    H2.GetPoint2dAt(1).Convert2D(1000,1000),
                    H2.GetPoint2dAt(1),
                };
                Polyline PL5 = PolylinePloter.PlotN(db, pts, false);


                // 标注
                DimPloter.Dim0(db, PL1.GetPoint3dAt(4), PL2.GetPoint3dAt(7), PL1.GetPoint2dAt(4).Convert3D(0, 500), DimStyleID);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(7), PL2.GetPoint3dAt(3), PL1.GetPoint2dAt(4).Convert3D(0, 500), DimStyleID);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(3), PL1.GetPoint3dAt(2), PL1.GetPoint2dAt(4).Convert3D(0, 500), DimStyleID);

                DimPloter.Dim0(db, PL1.GetPoint3dAt(2), PL2.GetPoint3dAt(4), PL1.GetPoint2dAt(1).Convert3D(800, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(4), PL2.GetPoint3dAt(1), PL1.GetPoint2dAt(1).Convert3D(800, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL2.GetPoint3dAt(1), PL1.GetPoint3dAt(1), PL1.GetPoint2dAt(1).Convert3D(800, 0), DimStyleID, 0.5 * Math.PI);

                DimPloter.Dim0(db, PL1.GetPoint3dAt(2), PL1.GetPoint3dAt(1), PL1.GetPoint2dAt(1).Convert3D(1100, 0), DimStyleID, 0.5 * Math.PI);
                DimPloter.Dim0(db, PL3.GetPoint3dAt(2), PL3.GetPoint3dAt(1), PL1.GetPoint2dAt(1).Convert3D(1100, 0), DimStyleID, 0.5 * Math.PI);
                if (LayerT!=0)
                {
                    DimPloter.Dim0(db, PL4.GetPoint3dAt(2), PL4.GetPoint3dAt(1), PL1.GetPoint2dAt(1).Convert3D(800, 0), DimStyleID, 0.5 * Math.PI);
                    DimPloter.Dim0(db, PL4.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL4.GetPoint2dAt(0).Convert3D(0, -1000), DimStyleID);
                    DimPloter.Dim0(db, PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1), PL4.GetPoint2dAt(0).Convert3D(0, -1000), DimStyleID);
                    DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL4.GetPoint3dAt(1), PL4.GetPoint2dAt(0).Convert3D(0, -1000), DimStyleID);
                    DimPloter.Dim0(db, PL4.GetPoint3dAt(3), PL3.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                    DimPloter.Dim0(db, PL3.GetPoint3dAt(1), PL4.GetPoint3dAt(2), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                }
                else
                {
                    DimPloter.Dim0(db, H1.GetPoint3dAt(0), PL3.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    DimPloter.Dim0(db, PL3.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    DimPloter.Dim0(db, PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL3.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                    DimPloter.Dim0(db, PL3.GetPoint3dAt(1), H2.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, -500), DimStyleID);
                }
                DimPloter.Dim0(db, PL3.GetPoint3dAt(0), PL1.GetPoint3dAt(0), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                DimPloter.Dim0(db, PL1.GetPoint3dAt(1), PL3.GetPoint3dAt(1), PL3.GetPoint2dAt(0).Convert3D(0, 500), DimStyleID);
                DimPloter.HengPo(db, 1.0, PL1.GetPoint3dAt(3).Convert3D(-0.25 * Sect[0], 100), true, s);
                DimPloter.HengPo(db, 1.0, PL1.GetPoint3dAt(3).Convert3D(+0.25 * Sect[0], 100), false, s);
                TextPloter.PrintLineText(db, PL2.GetLine(5).GetMidPoint2d(), PL2.GetLine(5).GetMidPoint2d(3 * s, -3 * s), new string[] { "20*10" }, false, s);
                TextPloter.PrintLineText(db, PL2.GetLine(7).GetMidPoint2d(), PL2.GetLine(7).GetMidPoint2d(3 * s, 3 * s), new string[] { "5*5" }, false, s);
                DBText temp = TextPloter.PrintText(db, "1/1", 2.5, H1.GetPoint2dAt(1).Convert2D(500, -500), s);                
                temp.Rotation=-0.25 * Math.PI;
                DBText temp2 = TextPloter.PrintText(db, "1/1", 2.5, H2.GetPoint2dAt(1).Convert2D(-500, -500), s);
                temp2.Rotation = 0.25 * Math.PI;
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
                TextPloter.PrintLineText(db, AnchorPoint.Convert2D(-1000, -50), AnchorPoint.Convert2D(-1000, -1000), new string[]
                { "C12/15 B.P e=10cm" }, false, s);
            }
            else
            {
                TextPloter.PrintLineText(db, AnchorPoint.Convert2D(-1000, -50), AnchorPoint.Convert2D(-1000, -1000), new string[]
                { "C12/15 B.P e=10cm", string.Format("Graveleux lateritique e={0}cm",(LayerT/10).ToString()) }, false, s);
            }
      
            TextPloter.PrintText(db, "REMBLAI.G.L", 3 , AnchorPoint.Convert2D(-0.5 * Sect[0] - 1000, 1000), s);            
            TextPloter.PrintTitle(db, "COUPE B-B", AnchorPoint.Convert2D(0, Sect[2]+25*s), s);


            tr.Commit();
            tr.Dispose();
        }


        private double[] GetSeps(double factor=1.0)
        {
            double[] res;
            int numSeg;
            double sideLength;
            double deltLength =0;
            int numThree, numTwo;
            if (DalotType == DType.A)
            {
                numThree = (int)Length / 3000;
                sideLength = Length - numThree * 3000;
                numTwo = (int)sideLength / 2000;
                res = new double[numThree + 1];
                if (numTwo == 0)
                {                    
                    for(int i = 0; i <= numThree; i++)
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
                res = new double[] { 0,Length};
            }

            int j = 0;
            foreach(double tmp in res)
            {
                res[j] = tmp * factor;
            }

            return res;

            //if (Amont == AType.JSJ)
            //{
            //    // 集水井不对称分割
            //    sideLength = (Length % SegLength);
            //    if (sideLength == 0)
            //    {
            //        // 无残余
            //        numSeg = (int)(Length / SegLength);
            //        res = new double[numSeg];
            //        for (int j = 0; j < numSeg; j++)
            //        {
            //            res[j] = SegLength;
            //            if (j == 0)
            //            {
            //                res[j] += deltLength;
            //            }
            //            else if (j == numSeg - 1)
            //            {
            //                res[j] -= deltLength;
            //            }
            //            else
            //            {
            //                continue;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // 有残余
            //        numSeg = (int)(Length / SegLength) + 1;
            //        res = new double[numSeg];
            //        for (int j = 0; j < numSeg; j++)
            //        {
            //            res[j] = SegLength;
            //            if (j == numSeg - 1)
            //            {
            //                res[j] = sideLength - deltLength;
            //            }
            //            else
            //            {
            //                continue;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    // 八字墙对称分割
            //    sideLength = (Length % SegLength) * 0.5;
            //    if (sideLength == 0)
            //    {
            //        // 整节段数
            //        numSeg = (int)(Length / SegLength);
            //        res = new double[numSeg];
            //        for (int j = 0; j < numSeg; j++)
            //        {
            //            res[j] = SegLength;
            //            if (j == 0)
            //            {
            //                res[j] += deltLength;
            //            }
            //            else if (j == numSeg - 1)
            //            {
            //                res[j] -= deltLength;
            //            }
            //            else
            //            {
            //                continue;
            //            }

            //        }
            //    }
            //    else
            //    {
            //        // 有残余节段
            //        numSeg = (int)(Length / SegLength) + 2;
            //        res = new double[numSeg];
            //        for (int j = 0; j < numSeg; j++)
            //        {
            //            if (j == 0)
            //            {
            //                res[j] = sideLength + deltLength;
            //            }
            //            else if (j == numSeg - 1)
            //            {
            //                res[j] = sideLength - deltLength;
            //            }
            //            else
            //            {
            //                res[j] = SegLength;
            //            }
            //        }
            //    }
            //}
            //double[] offsetList = new double[res.Length - 1];
            //for (int jj = 0; jj < offsetList.Length; jj++)
            //{
            //    offsetList[jj] = 0;
            //    for (int ii = 0; ii <= jj; ii++)
            //    {
            //        offsetList[jj] += res[ii]*factor;
            //    }

            //}
            //return offsetList;

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
