﻿using System;
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


    class Dalot
    {
        public enum AType { BZQ, JSJ, None, Other };
        public enum DType { A, B, C, D, E, F, G, H, I, J, K, L }
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

        //public Dalot(string strFileName)
        //{
        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Editor ed = doc.Editor;
        //    object missing = System.Reflection.Missing.Value;
        //    List<string> colnames = new List<string>() { "No.", "H1", "H2", "H3", "t1", "t2", "t3", "f1", "f2",
        //        "f3", "f4", "s1", "a1", "Length", "numSeg", "SegL", "Ang", "Slop", "ShoulderH", "ShoulderW",
        //        "Pad0Des", "Pad0W", "Pad0H", "Pad1Des", "Pad1W", "Pad1H", "WallWidth", "WallT", "cutTop", "cutBot",
        //        "WA1", "WA2", "WA3", "WA4", "RefillS", "RefillDes", "RefillSep", "MouthT","BG0","BG1","BG2","BG3","RW0","RW1" };
        //    MOE.Application excel = new MOE.Application
        //    {
        //        Visible = false,
        //        UserControl = true
        //    };
        //    MOE.Workbook wb = excel.Application.Workbooks.Open(strFileName, missing, true, missing, missing, missing,
        //        missing, missing, missing, true, missing, missing, missing, missing, missing);
        //    MOE.Worksheet ws = new MOE.Worksheet();
        //    for (int i = 1; i <= wb.Worksheets.Count; i++)
        //    {
        //        if (wb.Worksheets[i].Name=="Parameters")
        //        {
        //            ws = (MOE.Worksheet)wb.Worksheets[i];
        //            break;
        //        }
        //        else
        //        {
        //            ed.WriteMessage("Bad");
        //            return;
        //        }
        //    }
        //    MOE.Range rgUsed = ws.UsedRange;
        //    MOE.Range rgFound;
        //    int iRowNum = 2;

        //    foreach(string thename in colnames)
        //    {
        //        rgFound = (MOE.Range)rgUsed.Find(thename, Type.Missing, MOE.XlFindLookIn.xlValues,
        //            MOE.XlLookAt.xlPart, MOE.XlSearchOrder.xlByRows, MOE.XlSearchDirection.xlNext, false, false);
        //        int iColNum = rgFound.Column;
        //        var value = ws.Cells[iRowNum, iColNum].Value2;

        //    }
        //    wb.Close(true, null, null);
        //    excel.Quit();
        //    return;
        //}



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
            Line[] LSets;
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
            Line[] LSets;
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
                
                BiaoGao(H0 + (LSets[1].StartPoint.Y - AnchorPoint.Y) / 1000, LSets[1].StartPoint, modelSpace, tr, blockTbl, s);
                BiaoGao(H0 + (LSets[1].EndPoint.Y - AnchorPoint.Y) / 1000, LSets[1].EndPoint, modelSpace, tr, blockTbl, s);




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


            p0 = AnchorPoint.Convert2D(-W1 - delt1 / 2 * 3, 0);
            p1 = AnchorPoint.Convert2D(-W1, delt1);
            p2 = AnchorPoint.Convert2D(0, delt2);
            p3 = AnchorPoint.Convert2D(W2, delt3);
            p4 = AnchorPoint.Convert2D(W2 + delt3 / 2 * 3, 0);
            Road = PolylinePloter.PlotN(db, new Point2d[] { p0, p1, p2, p3, p4 }, false);
            Road.Layer = "细线";
            BiaoGao(H1, p1.Convert3D(), modelSpace, tr, blockTbl,s);
            BiaoGao(H2, p2.Convert3D(), modelSpace, tr, blockTbl,s);
            BiaoGao(H3, p3.Convert3D(), modelSpace, tr, blockTbl, s);
            BiaoGao(H0, AnchorPoint.Convert3D(), modelSpace, tr, blockTbl, s);

            DimPloter.Dim0(db, Wall_left.GetPoint3dAt(5), Wall_left.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db,  Wall_left.GetPoint3dAt(12),Road.GetPoint3dAt(1), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db,  Road.GetPoint3dAt(1), Road.GetPoint3dAt(2), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Road.GetPoint3dAt(2), Road.GetPoint3dAt(3), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Road.GetPoint3dAt(3), Wall_right.GetPoint3dAt(12), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);
            DimPloter.Dim0(db, Wall_right.GetPoint3dAt(12),Wall_right.GetPoint3dAt(5), AnchorPoint.Convert3D(0, delt2 + 10 * s), DimStyleID);

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


        private static void BiaoGao(double bgdata, Point3d refpt, BlockTableRecord ms, Transaction tr, BlockTable blockTbl,double s=100)
        {
            ObjectId blkRecId = blockTbl["BG"];
            double factor = s / 100;
            using (BlockReference acBlkRef = new BlockReference(refpt, blkRecId))
            {
                //acBlkRef.SetAttributes();
                acBlkRef.ScaleFactors = new Scale3d(factor);
                ms.AppendEntity(acBlkRef);
                tr.AddNewlyCreatedDBObject(acBlkRef, true);
                BlockTableRecord zheshiyuankuai;
                zheshiyuankuai = tr.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId gezhongshuxingID in zheshiyuankuai)
                {
                    DBObject gezhongshuxing = tr.GetObject(gezhongshuxingID, OpenMode.ForRead) as DBObject;
                    if (gezhongshuxing is AttributeDefinition)
                    {
                        AttributeDefinition acAtt = gezhongshuxing as AttributeDefinition;
                        using (AttributeReference acAttRef = new AttributeReference())
                        {
                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);
                            acAttRef.TextString = String.Format("{0:f2}", bgdata);
                            //acAttRef.Height = acAttRef.Height * factor;
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);

                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
            }
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
            
            ed.Command("_.ZOOM", "_E");
            ed.Command("_.ZOOM", ".7X");
            ed.Regen();
        }

    }
}
