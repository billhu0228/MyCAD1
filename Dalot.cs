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
        public AType Amont, Avale;
        public DType DalotType;
        public Point2d BasePoint;
        public double[] Sect;
        public int LayerNum;
        public double H1, H2, H3, H0;
        public double W1, W2;



        public Dalot()
        {
            Pk = 520;
            Ang = 0; // 偏角，从(1,0)方向逆时针，角度
            Slop = -0.01;
            DalotType = DType.B;
            Length = 16000;
            SegLength = 2000;
            XMidDist = 8000;
            Amont = AType.BZQ;
            Avale = AType.BZQ;
            LayerNum = 2;
            BasePoint = new Point2d(0, 0);
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
            H1 = 22.89;
            H2 = 22.83;
            H3 = 22.70;
            H0 = 19.30;
            W1 = 5000;
            W2 = 5000;

        }

        public Dalot(double cPk,double cAng,double cSlop,double cLength,double cSegLength,double cXMidDist,
            DType cDalotType, AType cAmont,AType cAvale,Point2d cBasePoint,int cLayerNum,
            double cH1 = 22.89, double cH2 = 22.83, double cH3 = 22.70, double cH0 = 19.30, double cW1 = 5000, double cW2 = 5000)
        {
            Pk = cPk;
            Ang = cAng;
            Slop = cSlop;
            DalotType = cDalotType;
            Length = cLength;
            SegLength = cSegLength;
            XMidDist = cXMidDist;
            Amont = cAmont;
            Avale = cAvale;
            LayerNum = cLayerNum;
            BasePoint = cBasePoint;

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
            H1 = cH1;
            H2 = cH2;
            H3 = cH3;
            H0 = cH0;
            W1 = cW1;
            W2 = cW2;            
        }


        /// <summary>
        /// 绘制平面图
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="AnchorPoint">锚点</param>
        /// <param name="s">制图比例</param>
        public Extents2d PlotA(Database db, Point2d AnchorPoint, int s = 100)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            var DimStyleID = dst["1-" + s.ToString()];
            Point2d BB = AnchorPoint;
            Point3dCollection pts = new Point3dCollection();  // 交点获取
            double ang_in_rad = Ang / 180 * Math.PI;
            Line[] LSets=new Line[4];
            Polyline[] LeftPolylineSet, RightPolylineSet;
            Extents2d CurExt;
            Point2d minPoint = BasePoint, maxPoint= BasePoint;

            if (DalotType <= DType.E)
            {
                //一孔
                double x0, x1, y0, y1;
                x0 = -XMidDist * Math.Cos(Math.Atan(Slop));
                x1 = (Length - XMidDist) * Math.Cos(Math.Atan(Slop));
                y0 = -0.5 * Sect[0] + x0 * Math.Tan(ang_in_rad);
                y1 = -0.5 * Sect[0] + x1 * Math.Tan(ang_in_rad);
                double factor = Math.Sqrt(Math.Pow(x1 - x0,2) + Math.Pow(y1 - y0,2))/Length;
                LSets = MulitlinePloter.PlotN(db, BB.Convert3D(x0, y0), BB.Convert3D(x1, y1),  // 涵身
                    new double[] { 0, Sect[5], Sect[0] - Sect[5], Sect[0] }, new string[] { "虚线", "虚线", "虚线", "虚线" }, true);
                MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1], GetSeps(factor), "虚线");
                
                if (Amont == AType.BZQ)
                {
                    LeftPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], true);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(0), LeftPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20*s), DimStyleID);
                    DimPloter.Dim0(db, LeftPolylineSet[2].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20 * s), DimStyleID);
                    DimPloter.Dim0(db, LSets[0].StartPoint, LSets[0].EndPoint, LSets[0].EndPoint.Convert3D(0, -20*s), DimStyleID);
                    DimPloter.Dim0(db, LeftPolylineSet[0].GetPoint3dAt(4), LeftPolylineSet[0].GetPoint3dAt(5),
                        LeftPolylineSet[0].GetPoint3dAt(5).Convert3D(-500, 0), DimStyleID, 0.5 * Math.PI);
                    DimPloter.DimAng(db, LSets[2], LeftPolylineSet[3].GetLine(1), LSets[2].StartPoint.Convert3D(-1000, 10), DimStyleID);
                    DimPloter.DimAng(db, LSets[1], LeftPolylineSet[4].GetLine(1), LSets[1].StartPoint.Convert3D(-1000, -10), DimStyleID);
                    DimPloter.Dim0(db, LeftPolylineSet[4].GetPoint3dAt(1), LeftPolylineSet[0].GetPoint3dAt(0),
                        LeftPolylineSet[0].GetPoint3dAt(0).Convert3D(-15*s, 0), DimStyleID, 120.0 / 180.0 * Math.PI);
                    minPoint = minPoint.Convert2D(LeftPolylineSet[0].GetPoint2dAt(5).X,LSets[0].StartPoint.Y-20*s);
                    TextPloter.PrintCirText(db, 25, LeftPolylineSet[0].GetPoint2dAt(0).Convert2D(-10 * s, 0.25 * Sect[0]), s);
                    
                }
                if (Avale == AType.BZQ)
                {
                    RightPolylineSet = PolylinePloter.PlotWallPlan(db, LSets, Sect[0], false);
                    DimPloter.Dim0(db, LSets[0].EndPoint, RightPolylineSet[2].GetPoint3dAt(1),
                        LSets[0].EndPoint.Convert3D(0, -20*s), DimStyleID);
                    DimPloter.Dim0(db, RightPolylineSet[2].GetPoint3dAt(1), RightPolylineSet[2].GetPoint3dAt(0),
                        LSets[0].EndPoint.Convert3D(0, -20*s), DimStyleID);
                    DimPloter.DimAng(db, LSets[1], RightPolylineSet[4].GetLine(1), LSets[1].EndPoint.Convert3D(1000, -10), DimStyleID);
                    DimPloter.DimAng(db, LSets[2], RightPolylineSet[3].GetLine(1), LSets[2].EndPoint.Convert3D(1000, 10), DimStyleID);
                    DimPloter.Dim0(db, RightPolylineSet[4].GetPoint3dAt(1), RightPolylineSet[0].GetPoint3dAt(0),
                        RightPolylineSet[0].GetPoint3dAt(0).Convert3D(1500, 0), DimStyleID, -120.0 / 180.0 * Math.PI);
                    maxPoint = RightPolylineSet[0].GetPoint2dAt(4).Convert2D(10*s, 20*s);
                    TextPloter.PrintCirText(db, 25, RightPolylineSet[0].GetPoint2dAt(0).Convert2D(10 * s, 0.25 * Sect[0]), s);
                }               

                DimPloter.Dim0(db, LSets[0].StartPoint, LSets[1].StartPoint, LSets[0].StartPoint.Convert3D(15*s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);
                DimPloter.Dim0(db, LSets[1].StartPoint, LSets[2].StartPoint, LSets[0].StartPoint.Convert3D(15 * s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);
                DimPloter.Dim0(db, LSets[2].StartPoint, LSets[3].StartPoint, LSets[0].StartPoint.Convert3D(15 * s, 0), DimStyleID, 0.5 * Math.PI + ang_in_rad);
                
                TextPloter.PrintTitle(db, "平面图", BasePoint.Convert2D(0, maxPoint.Y + 10 * s),s);
                maxPoint = maxPoint.Convert2D(0, 10 * s + 5 * s);
                TextPloter.DimSection(db, 'A', BasePoint.Convert2D(minPoint.X-5*s, 0), BasePoint.Convert2D(maxPoint.X, 0),s);
                minPoint = minPoint.Convert2D(-5*s, 0);
                TextPloter.DimSection(db, 'B', BasePoint.Convert2D(0, minPoint.Y-5*s), BasePoint.Convert2D(0,LSets[LSets.Length-1].StartPoint.Y+500), s);
                minPoint = minPoint.Convert2D(0, -5 * s);


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

            //
            TextPloter.PrintCirText(db, (int)Sect[4] / 10, AnchorPoint.Convert2D(0.5 * SegLength, -0.2 * Sect[1]), 
                LSets[1].StartPoint.Convert2D(), LSets[LSets.Length - 2].EndPoint.Convert2D());

            TextPloter.PrintLineText(db, AnchorPoint, AnchorPoint.Convert2D(-10 * s, -17.3 * s), new string[] { Pk_string(), }, true,s);
            tr.Commit();
            tr.Dispose();
            CurExt = new Extents2d(minPoint, maxPoint);
            return CurExt;
        }













        public void PlotB(Database db, Point2d AnchorPoint, int s = 100)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DimStyleTable dst = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            var DimStyleID = dst["1-" + s.ToString()];
            Point2d BB = AnchorPoint;
            Point3dCollection pts = new Point3dCollection();  // 交点获取
            Line[] LSets=new Line[4];
            Polyline Wall_left=new Polyline(), Wall_right=new Polyline();
            double slop_rad = Math.Atan(Slop);
            Polyline Road;
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
                //一孔
                double x0, x1, y0, y1;
                x0 = -XMidDist * Math.Cos(slop_rad);
                x1 = (Length - XMidDist) * Math.Cos(slop_rad);

                y0 = -XMidDist * Math.Sin(slop_rad);
                y1 = (Length - XMidDist) * Math.Sin(slop_rad);

                LSets = MulitlinePloter.PlotN(db, BB.Convert3D(x0, y0), BB.Convert3D(x1, y1),  // 涵身
                    new double[] { 0-Sect[3],0, Sect[2] - Sect[4] - Sect[3], Sect[2] - Sect[3] }, new string[] { "粗线", "细线", "细线", "粗线" }, false);
                MulitlinePloter.PlotCutLine(db, LSets[0], LSets[LSets.Length - 1], GetSeps(), "细线");
                
                
                //出入口
                Point2d[] Verts;
                if (Amont == AType.BZQ)
                {
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
                    
                }


                // 出口
                if (Avale == AType.BZQ)
                {
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
                }




                // 对齐标注
                DimPloter.DimAli(db, LSets[3].StartPoint, LSets[3].EndPoint, LSets[3].EndPoint.Convert3D(0, 500), DimStyleID);
                
                DimPloter.BiaoGao(H0 + (LSets[1].StartPoint.Y - AnchorPoint.Y) / 1000, LSets[1].StartPoint, modelSpace, tr, blockTbl, s);
                DimPloter.BiaoGao(H0 + (LSets[1].EndPoint.Y - AnchorPoint.Y) / 1000, LSets[1].EndPoint, modelSpace, tr, blockTbl, s);
                if (SegLength == Length)
                {
                    DimPloter.HengPo(db, Slop * 100, AnchorPoint.Convert3D(2000, 2000 * Slop + 10 * s), (Slop > 0), s);
                }
                else
                {
                    DimPloter.HengPo(db, Slop * 100, AnchorPoint.Convert3D(SegLength*1.5, SegLength*1.5 * Slop + 1 * s), (Slop > 0), s);
                }

                TextPloter.PrintLineText(db, LSets[0].GetMidPoint2d(-1000,-1000*Slop), LSets[0].GetMidPoint2d(-1000,-1000*Slop -1000), new string[]
                { "C12/15 B.P e=10cm", "Graveleux lateritique e=40cm" }, false, s);


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

            p0 = AnchorPoint.Convert2D(-W1 - delt1 / 2 * 3, 0);
            p1 = AnchorPoint.Convert2D(-W1, delt1);
            p2 = AnchorPoint.Convert2D(0, delt2);
            p3 = AnchorPoint.Convert2D(W2, delt3);
            p4 = AnchorPoint.Convert2D(W2 + delt3 / 2 * 3, 0);
            Road = PolylinePloter.PlotN(db, new Point2d[] { p0, p1, p2, p3, p4 }, false);
            Road.Layer = "细线";
            pts = new Point3dCollection();
            Road.GetLine(0).IntersectWith(LSets[LSets.Length - 1], Intersect.ExtendThis, pts, IntPtr.Zero, IntPtr.Zero);            
            if (pts.Count == 0) { }
            else{ Road.SetPointAt(0, pts[0].Convert2D()); }
            pts = new Point3dCollection();
            Road.GetLine(3).IntersectWith(LSets[LSets.Length - 1], Intersect.ExtendThis, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count == 0) { }
            else { Road.SetPointAt(4,pts[0].Convert2D()); }

            // 绘制垫层


            //TextPloter.PrintLineText(db, AnchorPoint);





            DimPloter.BiaoGao(H1, p1.Convert3D(), modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H2, p2.Convert3D(), modelSpace, tr, blockTbl,s);
            DimPloter.BiaoGao(H3, p3.Convert3D(), modelSpace, tr, blockTbl, s);
            DimPloter.BiaoGao(H0, AnchorPoint.Convert3D(), modelSpace, tr, blockTbl, s);
            DimPloter.BiaoGao((Wall_left.GetPoint2dAt(5).Y-AnchorPoint.Y)/1000+H0, Wall_left.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);
            DimPloter.BiaoGao((Wall_right.GetPoint2dAt(5).Y - AnchorPoint.Y) / 1000 + H0, Wall_right.GetPoint2dAt(5).Convert3D(), modelSpace, tr, blockTbl, s);

            DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db,  Wall_left.GetPoint3dAt(12),Road.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db,  Road.GetPoint3dAt(1), Road.GetPoint3dAt(2), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Road.GetPoint3dAt(2), Road.GetPoint3dAt(3), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Road.GetPoint3dAt(3), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Wall_right.GetPoint3dAt(12),Wall_right.GetPoint3dAt(5), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.DimAng(db, Wall_left.GetLine(6), Wall_left.GetLine(11), Wall_left.GetPoint3dAt(7).Convert3D(0, -10*s),DimStyleID);
            DimPloter.DimAng(db, Wall_right.GetLine(6), Wall_right.GetLine(11), Wall_right.GetPoint3dAt(7).Convert3D(0, -10*s), DimStyleID);
            TextPloter.PrintText(db, "入口", 3, Wall_left.GetPoint2dAt(6).Convert2D(0, 10 * s), s);
            TextPloter.PrintText(db, "出口", 3, Wall_right.GetPoint2dAt(6).Convert2D(0, 10 * s), s);
            TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_left.GetPoint2dAt(0).Convert2D(-10 * s, 10 * s), s);
            TextPloter.PrintCirText(db, (int)Sect[5]/10, Wall_right.GetPoint2dAt(0).Convert2D(10 * s, 10 * s), s);
            TextPloter.PrintCirText(db, (int)Sect[5] / 10, AnchorPoint.Convert2D(-0.5*SegLength, 0.5*Sect[2]),
                LSets[1].StartPoint.Convert2D(),LSets[LSets.Length-2].EndPoint.Convert2D(), s);

            TextPloter.PrintTitle(db, "A-A剖面", AnchorPoint.Convert2D(0, delt2 + 20 * s),s);
            

            tr.Commit();
            tr.Dispose();


        }






        /// <summary>
        /// 绘制剖面图
        /// </summary>
        /// <param name="s">绘图比例，默认1:75</param>
        public void PlotC(Database db, Point2d AnchorPoint, int s = 75)
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
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
                hatch1 = HatchPloter.PlotH(db, PL3, hatchref3, 1);
                if (LayerNum == 2)
                {
                    PL4 = PolylinePloter.Plot4(db, BB.Convert2D(0, -500), 400, Sect[0] + 2 * 500);   // 外框
                    PL4.Layer = "细线";
                    hatch2 = HatchPloter.PlotH(db, PL4, hatchref2, 1);
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
                HatchPloter.PlotH(db, H1, hatchref2, 1);
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
                HatchPloter.PlotH(db, H2, hatchref2, 1);
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
                if (LayerNum == 2)
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
            TextPloter.PrintLineText(db, AnchorPoint.Convert2D(-1000, -50),AnchorPoint.Convert2D(-1000, -1000), new string[]
                { "C12/15 B.P e=10cm", "Graveleux lateritique e=40cm" }, false, s);
           
            TextPloter.PrintText(db, "REMBLAI.G.L", 3 , AnchorPoint.Convert2D(-0.5 * Sect[0] - 1000, 1000), s);
            
            TextPloter.PrintTitle(db, "B-B剖面", AnchorPoint.Convert2D(0, Sect[2]+25*s), s);


            tr.Commit();
            tr.Dispose();
        }


        private double[] GetSeps(double factor=1.0)
        {
            double[] res;
            int numSeg;
            double sideLength;
            double deltLength =0;
            if (Amont == AType.JSJ)
            {
                // 集水井不对称分割
                sideLength = (Length % SegLength);
                if (sideLength == 0)
                {
                    // 无残余
                    numSeg = (int)(Length / SegLength);
                    res = new double[numSeg];
                    for (int j = 0; j < numSeg; j++)
                    {
                        res[j] = SegLength;
                        if (j == 0)
                        {
                            res[j] += deltLength;
                        }
                        else if (j == numSeg - 1)
                        {
                            res[j] -= deltLength;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    // 有残余
                    numSeg = (int)(Length / SegLength) + 1;
                    res = new double[numSeg];
                    for (int j = 0; j < numSeg; j++)
                    {
                        res[j] = SegLength;
                        if (j == numSeg - 1)
                        {
                            res[j] = sideLength - deltLength;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            else
            {
                // 八字墙对称分割
                sideLength = (Length % SegLength) * 0.5;
                if (sideLength == 0)
                {
                    // 整节段数
                    numSeg = (int)(Length / SegLength);
                    res = new double[numSeg];
                    for (int j = 0; j < numSeg; j++)
                    {
                        res[j] = SegLength;
                        if (j == 0)
                        {
                            res[j] += deltLength;
                        }
                        else if (j == numSeg - 1)
                        {
                            res[j] -= deltLength;
                        }
                        else
                        {
                            continue;
                        }

                    }
                }
                else
                {
                    // 有残余节段
                    numSeg = (int)(Length / SegLength) + 2;
                    res = new double[numSeg];
                    for (int j = 0; j < numSeg; j++)
                    {
                        if (j == 0)
                        {
                            res[j] = sideLength + deltLength;
                        }
                        else if (j == numSeg - 1)
                        {
                            res[j] = sideLength - deltLength;
                        }
                        else
                        {
                            res[j] = SegLength;
                        }
                    }
                }
            }
            double[] offsetList = new double[res.Length - 1];
            for (int jj = 0; jj < offsetList.Length; jj++)
            {
                offsetList[jj] = 0;
                for (int ii = 0; ii <= jj; ii++)
                {
                    offsetList[jj] += res[ii]*factor;
                }

            }
            return offsetList;

        }




        public void CreatPaperSpace(Database db,Editor ed, int[] ScaleList )
        {
            // 基本句柄
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            DBDictionary lays = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
            int name = 1;
            foreach (DBDictionaryEntry item in lays)
            {
                if (item.Key == name.ToString())
                {
                    name++;                    
                }                
            }
            var id = LayoutManager.Current.CreateAndMakeLayoutCurrent(name.ToString());
            var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);
            lay.SetPlotSettings("A3", "monochrome.ctb", "Adobe PDF");
            lay.ApplyToViewport(tr, 2, vp => { vp.DrawMyViewport(1, BasePoint.Convert3D(), Point2d.Origin, ScaleList[0]); vp.Locked = true; });
            lay.ApplyToViewport(tr, 3, vp => { vp.DrawMyViewport(2, BasePoint.Convert3D(), Point2d.Origin, ScaleList[ScaleList.Length - 1]); vp.Locked = true; });
            tr.Commit();
            tr.Dispose();



#if CAD2018
            ed.Command("_.ZOOM", "_E");
            ed.Command("_.ZOOM", ".7X");
            ed.Regen();
#endif

        }

        public string Pk_string()
        {
            string res;
            int kilo=(int)Pk / 1000;
            double meter = Pk % 1000;
            res = string.Format("PK{0}+{1:0.000}", kilo, meter);

            return res;
        }

    }
}
