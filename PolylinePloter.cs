using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAD1
{
    class PolylinePloter
    {
        /// <summary>
        /// 八边形绘图
        /// </summary>
        /// <param name="AnchorPoint">锚点，八边形下中心点</param>
        /// <param name="Height">总高</param>
        /// <param name="Width">总宽</param>
        /// <param name="F1x">下倒角，x方向</param>
        /// <param name="F1y">下倒角，y方向</param>
        /// <param name="F2x">上倒角，x方向</param>
        /// <param name="F2y">上倒角，y方向</param>
        public static Polyline Plot8(Database db, Point2d AnchorPoint,double Height,double Width,
            double F1x=50,double F1y=50,double F2x=200,double F2y=100)
        {
            Polyline PL1 = new Polyline() { Closed = true };
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                Line2d AxisY = new Line2d(AnchorPoint, AnchorPoint.Convert2D(0, 1));
                Point2d p0, p1, p2, p3, p4, p5, p6, p7;
                
                p0 = AnchorPoint.Convert2D(-0.5 * Width + F1x, 0);
                p1 = p0.Mirror(AxisY);
                p2 = p1.Convert2D(F1x, F1y);
                p3 = p2.Convert2D(0, Height - F1y - F2y);
                p4 = p3.Convert2D(-F2x, F2y);
                p5 = p4.Mirror(AxisY);
                p6 = p3.Mirror(AxisY);
                p7 = p2.Mirror(AxisY);
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                PL1.AddVertexAt(4, p4, 0, 0, 0);
                PL1.AddVertexAt(5, p5, 0, 0, 0);
                PL1.AddVertexAt(6, p6, 0, 0, 0);
                PL1.AddVertexAt(7, p7, 0, 0, 0);

                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                tr.Commit();
            }

            return PL1;
        }

        /// <summary>
        /// 绘制四边形
        /// </summary>
        /// <param name="db"></param>
        /// <param name="AnchorPoint">四边形中下点</param>
        /// <param name="Height"></param>
        /// <param name="Width"></param>
        /// <returns></returns>
        public static Polyline Plot4(Database db, Point2d AnchorPoint, double Height, double Width)
        {
            Polyline PL1 = new Polyline() { Closed = true };
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                Line2d AxisY = new Line2d(AnchorPoint, AnchorPoint.Convert2D(0, 1));
                Point2d p0, p1, p2, p3;

                p0 = AnchorPoint.Convert2D(-0.5 * Width, 0);
                p1 = p0.Mirror(AxisY);
                p2 = p1.Convert2D(0, Height);
                p3 = p2.Mirror(AxisY);
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                tr.Commit();
            }
            return PL1;
        }


        public static Polyline PlotN(Database db, Point2d[] Vertexs,bool isClosed)
        {
            Polyline PL1 = new Polyline() { Closed = isClosed };
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                int i = 0;
                foreach(Point2d pt in Vertexs)
                {
                    PL1.AddVertexAt(i, pt, 0, 0, 0);
                    i++;
                }     
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                tr.Commit();
            }
            return PL1;
        }




        /// <summary>
        /// 绘制八字墙立面
        /// </summary>
        /// <param name="db"></param>
        /// <param name="AnchorPoint">下缘锚点</param>
        /// <param name="height">总高度=Sect【2】</param>
        /// <param name="rotAngle">转角，弧度</param>
        /// <param name="isLeft">左/右</param>
        /// <returns></returns>
        public static Polyline PlotWall(Database db, Point2d AnchorPoint,double height, double rotAngle,bool isLeft)
        {
            Polyline PL1 = new Polyline() { Closed = true };
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                int direct= isLeft?-1:1;
                Point2d p0, p1, p2, p3,p4,p5,p6,p7,p8,p9,p10,p11,p12;
                p0 = AnchorPoint.Convert2D(0, 0);
                p1 = AnchorPoint.Convert2D(0, 0);
                p2 = AnchorPoint.Convert2D(0, 0);
                p3 = AnchorPoint.Convert2D(0, 0);
                p4 = AnchorPoint.Convert2D(0, 0);
                p5 = AnchorPoint.Convert2D(0, 0);
                p6 = AnchorPoint.Convert2D(0, 0);
                p7 = AnchorPoint.Convert2D(0, 0);
                p8 = AnchorPoint.Convert2D(0, 0);
                p9 = AnchorPoint.Convert2D(0, 0);
                p10 = AnchorPoint.Convert2D(0, 0);
                p11 = AnchorPoint.Convert2D(0, 0);
                p12 = AnchorPoint.Convert2D(0, 0);

                switch (height)
                {
                    case 1400:
                        p1 = p0.Convert2D(direct * 1800, 0);
                        p2 = p1.Convert2D(0, -400);
                        p3 = p2.Convert2D(direct * 200);
                        p4 = p3.Convert2D(0, 400);
                        p5 = p4.Convert2D(0, 200);
                        p6 = p5.Convert2D(0, 300);
                        p7 = p6.Convert2D(-direct * 1800, 1200);
                        p8 = p7.Convert2D(-direct * 200, 0);
                        p9 = p8.Convert2D(-direct * 200, 0);
                        p10 = p9.Convert2D(0,-300);
                        p11 = p10.Convert2D(direct*200,0);
                        p12 = p11.Convert2D(0, -height+200);
                        break;
                    case 2000:
                        p1 = p0.Convert2D(direct * 2575, 0);
                        p2 = p1.Convert2D(0, -400);
                        p3 = p2.Convert2D(direct * 250);
                        p4 = p3.Convert2D(0, 400);
                        p5 = p4.Convert2D(0, 250);
                        p6 = p5.Convert2D(0, 300);
                        p7 = p6.Convert2D(-direct * 2625, 1750);
                        p8 = p7.Convert2D(-direct * 200, 0);
                        p9 = p8.Convert2D(-direct * 200, 0);
                        p10 = p9.Convert2D(0, -300);
                        p11 = p10.Convert2D(direct * 200, 0);
                        p12 = p11.Convert2D(0, -height + 250);
                        break;

                    case 2800:
                        p0 = AnchorPoint.Convert2D(0, 100 / Math.Cos(rotAngle));
                        p1 = p0.Convert2D(direct * 3350, direct * 3350 * Math.Sin(rotAngle));
                        p4 = p0.Convert2D(direct * 3600, direct * 3600 * Math.Sin(rotAngle));
                        p5 = p4.Convert2D(0, 300);
                        p3 = p5.Convert2D(0, -1100);
                        p2 = p3.Convert2D(-direct * 250);
                        p6 = p5.Convert2D(0, 300);
                        p12 = p0.Convert2D(0, 300 / Math.Cos(rotAngle));
                        p11 = p0.Convert2D(0, -100 + height / Math.Cos(rotAngle));
                        p10 = p11.Convert2D(-direct * 300, -direct * 300 * Math.Tan(rotAngle));
                        p7 = p11.Convert2D(0, 300);                        
                        p8 = p7.Convert2D(0, 0);
                        p9 = p8.Convert2D(-direct * 300, 0);
                        p10 = p9.Convert2D(0, -300);   
                        break;

                    case 3800:
                        p0 = AnchorPoint.Convert2D(0, 100 / Math.Cos(rotAngle));
                        p1 = p0.Convert2D(direct * 4850, direct * 4850 * Math.Sin(rotAngle));
                        p4 = p0.Convert2D(direct * 5100, direct * 5100 * Math.Sin(rotAngle));
                        p5 = p4.Convert2D(0, 300);
                        p3 = p5.Convert2D(0, -1100);
                        p2 = p3.Convert2D(-direct * 250);
                        p6 = p5.Convert2D(0, 300);
                        p12 = p0.Convert2D(0, 300 / Math.Cos(rotAngle));
                        p11 = p0.Convert2D(0, -100 + height / Math.Cos(rotAngle));
                        p10 = p11.Convert2D(-direct * 300, -direct * 300 * Math.Tan(rotAngle));
                        p7 = p11.Convert2D(0, 300);
                        p8 = p7.Convert2D(0, 0);
                        p9 = p8.Convert2D(-direct * 300, 0);
                        p10 = p9.Convert2D(0, -300);
                        break;
                }

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
                PL1.AddVertexAt(12, p12, 0, 0, 0);

                if (height < 2800)
                {
                    PL1.TransformBy(Matrix3d.Rotation(rotAngle, Vector3d.ZAxis, AnchorPoint.Convert3D()));
                }
                PL1.Layer = "粗线";

                Line L1, L2;
                L1 = new Line(PL1.GetPoint3dAt(5), PL1.GetPoint3dAt(12));
                L2 = new Line(PL1.GetPoint3dAt(8), PL1.GetPoint3dAt(11));
                L1.Layer = "细线";
                L2.Layer = "细线";

                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                modelSpace.AppendEntity(L1);
                tr.AddNewlyCreatedDBObject(L1, true);
                modelSpace.AppendEntity(L2);
                tr.AddNewlyCreatedDBObject(L2, true);



                tr.Commit();
            }
            return PL1;
        }






        public static Polyline[] PlotWallPlan(Database db, Line[] LSet, double width, bool isLeft,int scale)
        {
            Polyline PL1 = new Polyline() { Closed = true, Layer = "粗线" };    // 外框
            Polyline PL2 = new Polyline() { Closed = false, Layer = "粗线" };   // 上拐角
            Polyline PL3 = new Polyline() { Closed = false, Layer = "粗线" };   // 下拐角
            Polyline PL4 = new Polyline() { Closed = false, Layer = "粗线" };   // 上内侧
            Polyline PL5 = new Polyline() { Closed = false, Layer = "粗线" };  // 下内侧
            Line L1 = new Line() { Layer = "虚线", LinetypeScale = 4.0 };
            Line L4 = new Line() { Layer = "细线" };  // 上短线
            Line L5 = new Line() { Layer = "细线" };  // 下短线
            Line L6 = new Line() { Layer = "粗线" };  // 内墙
            Line temp;
            Point3dCollection pts = new Point3dCollection();
            Point2d UpperPt, LowerPt,UpperPt2,LowerPt2;
            double ang_red = LSet[0].Angle;

            if (isLeft)
            {
                LowerPt = LSet[0].StartPoint.Convert2D();
                UpperPt = LSet[LSet.Length-1].StartPoint.Convert2D();
                UpperPt2= LSet[LSet.Length - 2].StartPoint.Convert2D();
                LowerPt2= LSet[1].StartPoint.Convert2D();
            }
            else
            {
                UpperPt = LSet[LSet.Length - 1].EndPoint.Convert2D();
                UpperPt2 = LSet[LSet.Length - 2].EndPoint.Convert2D();
                LowerPt2 = LSet[1].EndPoint.Convert2D();
                LowerPt = LSet[0].EndPoint.Convert2D();
            }          


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                int direct = isLeft ? -1 : 1;
                double dy = (UpperPt.Y - LowerPt.Y)/Math.Cos(ang_red);
                double dy2 = 0;
                double thick,Wall_width,Wall_thick,Mouth_thick,Plate_thick;

                
                switch (width)
                {
                    default:
                        thick = 300;
                        Wall_width = 1800;
                        Wall_thick = 350;
                        Mouth_thick = 250;
                        Plate_thick = 250;
                        break;
                    case 1900:
                        thick = 200;
                        Wall_width = 1800+200;
                        Wall_thick = 200;
                        Mouth_thick = 200;
                        Plate_thick = 200;
                        break;
                    case 2500:
                        thick = 200;
                        Wall_width = 2575+250;
                        Wall_thick = 250;
                        Mouth_thick = 250;
                        Plate_thick = 250;
                        break;
                    case 4700:
                        thick = 300;
                        Wall_thick = 350;
                        Wall_width = 3600;
                        Mouth_thick = 250;
                        Plate_thick = 300;
                        break;

                }
                // 外框
                Point2d p0, p1, p2, p3, p4, p5;
                dy2 = dy + Wall_width * Math.Tan(30.0 / 180.0 * Math.PI) * 2;
                p0 = LowerPt;
                p1 = p0.Convert2D(-direct * thick, 0);
                p2 = p1.Convert2D(0, dy);
                p3 = p2.Convert2D(direct * thick, 0);
                p4 = p3.Convert2D(direct * Wall_width, Wall_width * Math.Tan(30.0 / 180.0 * Math.PI));
                p5 = p4.Convert2D(0, -dy2);
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                PL1.AddVertexAt(4, p4, 0, 0, 0);
                PL1.AddVertexAt(5, p5, 0, 0, 0);
                PL1.TransformBy(Matrix3d.Rotation(ang_red, Vector3d.ZAxis, LowerPt.Convert3D()));
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                
                
                //  拐角
                temp = new Line(PL1.GetPoint3dAt(4), PL1.GetPoint3dAt(5));
                temp.TransformBy(Matrix3d.Displacement(new Vector3d(-direct * Mouth_thick*Math.Cos(ang_red), 
                    -direct * Mouth_thick * Math.Cos(ang_red)*Math.Sin(ang_red), 0)));
                temp.IntersectWith(PL1, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p0 = PL1.GetPoint2dAt(4);
                p1 = temp.StartPoint.Convert2D();
                p2 = pts[0].Convert2D();
                PL2.AddVertexAt(0, p0, 0, 0, 0);
                PL2.AddVertexAt(1, p1, 0, 0, 0);
                PL2.AddVertexAt(2, p2, 0, 0, 0);               
                modelSpace.AppendEntity(PL2);
                tr.AddNewlyCreatedDBObject(PL2, true);
                p0 = PL1.GetPoint2dAt(5);
                p1 = temp.EndPoint.Convert2D();
                p2 = pts[1].Convert2D();
                PL3.AddVertexAt(0, p0, 0, 0, 0);
                PL3.AddVertexAt(1, p1, 0, 0, 0);
                PL3.AddVertexAt(2, p2, 0, 0, 0);
                modelSpace.AppendEntity(PL3);
                tr.AddNewlyCreatedDBObject(PL3, true);
                L1.StartPoint = pts[0];
                L1.EndPoint= pts[1];
                modelSpace.AppendEntity(L1);
                tr.AddNewlyCreatedDBObject(L1, true);
                // 上内墙
                pts = new Point3dCollection();
                temp = new Line(PL1.GetPoint3dAt(3), PL1.GetPoint3dAt(4));
                temp=(Line)temp.GetOffsetCurves(-Wall_thick*direct)[0];
                temp.IntersectWith(LSet[LSet.Length - 2], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p0 = UpperPt2;
                p1 = pts[0].Convert2D();
                pts.Clear();
                temp.IntersectWith(PL1.GetLine(4), Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p2 = pts[0].Convert2D();                                
                PL4.AddVertexAt(0, p0, 0, 0, 0);
                PL4.AddVertexAt(1, p1, 0, 0, 0);
                PL4.AddVertexAt(2, p2, 0, 0, 0);
                modelSpace.AppendEntity(PL4);
                tr.AddNewlyCreatedDBObject(PL4, true);

                pts.Clear();
                temp = new Line(PL1.GetPoint3dAt(0), PL1.GetPoint3dAt(5));
                temp = (Line)temp.GetOffsetCurves(Wall_thick * direct)[0];
                //temp.TransformBy(Matrix3d.Displacement(new Vector3d(0, Wall_thick / Math.Cos(30.0 / 180.0 * Math.PI), 0)));
                temp.IntersectWith(LSet[1], Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p0 = LowerPt2;
                p1 = pts[0].Convert2D();
                pts.Clear();
                temp.IntersectWith(PL1.GetLine(4), Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                p2 = pts[0].Convert2D();
                PL5.AddVertexAt(0, p0, 0, 0, 0);
                PL5.AddVertexAt(1, p1, 0, 0, 0);
                PL5.AddVertexAt(2, p2, 0, 0, 0);
                modelSpace.AppendEntity(PL5);
                tr.AddNewlyCreatedDBObject(PL5, true);

                L6.StartPoint = PL1.GetPoint3dAt(3);
                L6.EndPoint = PL1.GetPoint3dAt(0);
                modelSpace.AppendEntity(L6);
                tr.AddNewlyCreatedDBObject(L6, true);

                DBText txt = new DBText();
                Circle C1 = new Circle();
                Circle C2 = new Circle();
                Point2d PositionPoint = p0.Convert2D(direct * 10 * scale, 3 * scale);
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;

                
                txt.TextString = (Plate_thick/10).ToString();
                txt.Height = 2 * scale;
                txt.Position = PositionPoint.Convert3D();
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                txt.AlignmentPoint = PositionPoint.Convert3D();
                txt.TextStyleId = st["fsdb"];
                txt.Layer = "标注";
                txt.WidthFactor = 0.75;

                C1 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.3 * scale);
                C2 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.6 * scale);
                C1.Layer = "标注";
                C2.Layer = "标注";
                modelSpace.AppendEntity(txt);
                tr.AddNewlyCreatedDBObject(txt, true);
                modelSpace.AppendEntity(C1);
                tr.AddNewlyCreatedDBObject(C1, true);
                modelSpace.AppendEntity(C2);
                tr.AddNewlyCreatedDBObject(C2, true);

                tr.Commit();
            }
            Polyline[] res = new Polyline[] { PL1, PL2, PL3, PL4, PL5 };
            return res;
        }



















        /// <summary>
        /// 涵洞外框五边形
        /// </summary>
        /// <param name="db">cad数据库</param>
        /// <param name="AnchorPoint">中下点</param>
        /// <param name="Height">高度，不含坡</param>
        /// <param name="Width">总宽</param>
        /// <param name="hp">横坡</param>
        /// <returns></returns>
        public static Polyline Plot5(Database db, Point2d AnchorPoint, double Height, double Width, double hp = 0.01)
        {
            Polyline PL1 = new Polyline() { Closed = true };
            using (Transaction tr = db.TransactionManager.StartTransaction()) 
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                Line2d AxisY = new Line2d(AnchorPoint, AnchorPoint.Convert2D(0, 1));
                Point2d p0, p1, p2, p3, p4;
               

                p0 = AnchorPoint.Convert2D(-0.5 * Width, 0);
                p1 = p0.Mirror(AxisY);
                p2 = p1.Convert2D(0, Height);
                p3 = AnchorPoint.Convert2D(0, Height + 0.5 * Width * hp);
                p4 = p2.Mirror(AxisY);
                PL1.AddVertexAt(0, p0, 0, 0, 0);
                PL1.AddVertexAt(1, p1, 0, 0, 0);
                PL1.AddVertexAt(2, p2, 0, 0, 0);
                PL1.AddVertexAt(3, p3, 0, 0, 0);
                PL1.AddVertexAt(4, p4, 0, 0, 0);

                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                tr.Commit();
            }            
            return PL1;
        }

        public static Polyline Plot11(Database db, Point2d AnchorPoint, Dalot curDalot, double hp = 0.01,double footWidht=400)
        {
            Polyline PL1 = new Polyline() { Closed = true };
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                Line2d AxisY = new Line2d(AnchorPoint, AnchorPoint.Convert2D(0, 1));
                Point2d p0, p1, p2, p3, p4,p5,p6,p7,p8,p9,p10;
                p0 = AnchorPoint.Convert2D(-0.5 * curDalot.Sect[0] - footWidht, 0);
                p1 = p0.Mirror(AxisY);
                p2 = p1.Convert2D(0, curDalot.Sect[4]);
                if (footWidht == 0)
                {
                    p3 = p2.Convert2D(-footWidht +0);
                    p4 = p3.Convert2D(0, curDalot.Sect[7]);
                }
                else
                {
                    p3 = p2.Convert2D(-footWidht + curDalot.Sect[6]);
                    p4 = p3.Convert2D(-curDalot.Sect[6], curDalot.Sect[7]);
                }
                p5 = p1.Convert2D(-footWidht, curDalot.Sect[2]);
                p6 = AnchorPoint.Convert2D(0, curDalot.Sect[2] + 0.5 * curDalot.Sect[2] * hp);
                p7 = p5.Mirror(AxisY);
                p8 = p4.Mirror(AxisY);
                p9 = p3.Mirror(AxisY);
                p10 = p2.Mirror(AxisY);
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
                modelSpace.AppendEntity(PL1);
                tr.AddNewlyCreatedDBObject(PL1, true);
                tr.Commit();
            }
            return PL1;
        }









        public static Polyline PlotWellPlan(Database db, Line[] LSet, double width, bool isLeft)
        {
            Polyline SideWall = new Polyline();
            Polyline inSideWall;
            Polyline Cover, Shoulder;            
            Point2d p0, p1, p2, p3, p4, p5, p6, p7;
            Point2d ap1, ap2;
            double ang_red = LSet[0].Angle;
            Vector2d theDalotDir = new Vector2d(Math.Cos(ang_red), Math.Sin(ang_red));
            Vector2d XtheDalotDir = new Vector2d(Math.Cos(ang_red+0.5*Math.PI), Math.Sin(ang_red + 0.5 * Math.PI));
            int dirx = isLeft ? -1 : 1;
            if (isLeft)
            {
                ap1 = LSet[0].StartPoint.Convert2D();
                ap2 = LSet[LSet.Length - 1].StartPoint.Convert2D();
            }
            else
            {
                ap1 = LSet[0].EndPoint.Convert2D();
                ap2 = LSet[LSet.Length - 1].EndPoint.Convert2D();
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                
                Shoulder = new Polyline();
                p0 = ap1;
                p1 = ap2;
                p2 = p1.MoveDistance(theDalotDir, dirx * 200);
                //p2 = p1.Convert2D(dirx * 200);
                p3 = p0.Convert2D(dirx * 200);
                p3 = p0.MoveDistance(theDalotDir, dirx * 200);
                Shoulder.AddVertexAt(0, p0, 0, 0, 0);
                Shoulder.AddVertexAt(1, p1, 0, 0, 0);
                Shoulder.AddVertexAt(2, p2, 0, 0, 0);
                Shoulder.AddVertexAt(3, p3, 0, 0, 0);
                Shoulder.Layer = "细线";
                Shoulder.Closed = true;
                modelSpace.AppendEntity(Shoulder);
                tr.AddNewlyCreatedDBObject(Shoulder, true);

                if (width == 1900)
                {
                    SideWall = new Polyline();                    
                    p0 = ap1.MoveDistance(theDalotDir, dirx * 200);
                    p1 = ap1.MoveDistance(theDalotDir,dirx * 1850);
                    p2 = ap2.MoveDistance(theDalotDir,dirx * 1850);
                    p3 = ap2.MoveDistance(theDalotDir,dirx * 200);
                    //p0 = ap1.Convert2D(dirx * 200);
                    //p1 = ap1.Convert2D(dirx * 1850);
                    //p2 = ap2.Convert2D(dirx * 1850);
                    //p3 = ap2.Convert2D(dirx * 200);
                    SideWall.AddVertexAt(0, p0, 0, 0, 0);
                    SideWall.AddVertexAt(1, p1, 0, 0, 0);
                    SideWall.AddVertexAt(2, p2, 0, 0, 0);
                    SideWall.AddVertexAt(3, p3, 0, 0, 0);
                    SideWall.Layer = "细线";
                    SideWall.Closed = false;
                    modelSpace.AppendEntity(SideWall);
                    tr.AddNewlyCreatedDBObject(SideWall, true);

                    inSideWall = new Polyline();
                    p0 = p0.MoveDistance(XtheDalotDir, 200);
                    p1 = p0.MoveDistance(theDalotDir, dirx*1450);
                    p2 = p1.MoveDistance(XtheDalotDir ,1500);
                    p3 = p2.MoveDistance(theDalotDir, -dirx * 1450);                    
                    inSideWall.AddVertexAt(0, p0, 0, 0, 0);
                    inSideWall.AddVertexAt(1, p1, 0, 0, 0);
                    inSideWall.AddVertexAt(2, p2, 0, 0, 0);
                    inSideWall.AddVertexAt(3, p3, 0, 0, 0);
                    inSideWall.Layer = "虚线";
                    inSideWall.Closed = false;
                    modelSpace.AppendEntity(inSideWall);
                    tr.AddNewlyCreatedDBObject(inSideWall, true);

                    for(int i = 0; i < 3; i++)
                    {
                        double x0 = dirx * (220 + 1550);
                        double y0 = 180 + i * 520;
                        Cover = new Polyline();
                        Cover.AddVertexAt(0, ap1.Convert2D(x0, y0), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0 - dirx * 1550, y0), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0 - dirx * 1550, y0 + 500), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0, y0 + 500), 0, 0, 0);
                        Cover.Layer = "细线";
                        Cover.Closed = true;
                        Cover.TransformBy(Matrix3d.Rotation(ang_red, Vector3d.ZAxis, ap1.Convert3D()));
                        modelSpace.AppendEntity(Cover);
                        tr.AddNewlyCreatedDBObject(Cover, true);
                        
                    }

                }
                else
                {
                    SideWall = new Polyline();
                    p0 = ap1.MoveDistance(theDalotDir,dirx * 200);
                    p1 = ap1.MoveDistance(theDalotDir,dirx * 1900);
                    p2 = ap2.MoveDistance(theDalotDir,dirx * 1900);
                    p3 = ap2.MoveDistance(theDalotDir,dirx * 200);
                    SideWall.AddVertexAt(0, p0, 0, 0, 0);
                    SideWall.AddVertexAt(1, p1, 0, 0, 0);
                    SideWall.AddVertexAt(2, p2, 0, 0, 0);
                    SideWall.AddVertexAt(3, p3, 0, 0, 0);
                    SideWall.Layer = "细线";
                    SideWall.Closed = false;
                    modelSpace.AppendEntity(SideWall);
                    tr.AddNewlyCreatedDBObject(SideWall, true);

                    inSideWall = new Polyline();
                    p0 = p0.Convert2D(0, 250);
                    p1 = p1.Convert2D(-dirx * 250, 250);
                    p2 = p2.Convert2D(-dirx * 250, -250);
                    p3 = p3.Convert2D(0, -250);
                    inSideWall.AddVertexAt(0, p0, 0, 0, 0);
                    inSideWall.AddVertexAt(1, p1, 0, 0, 0);
                    inSideWall.AddVertexAt(2, p2, 0, 0, 0);
                    inSideWall.AddVertexAt(3, p3, 0, 0, 0);
                    inSideWall.Layer = "虚线";
                    inSideWall.Closed = false;
                    modelSpace.AppendEntity(inSideWall);
                    tr.AddNewlyCreatedDBObject(inSideWall, true);

                    for (int i = 0; i < 4; i++)
                    {
                        double x0 = dirx * (220 + 1550);
                        double y0 = 220 + i * 520;
                        Cover = new Polyline();
                        Cover.AddVertexAt(0, ap1.Convert2D(x0, y0), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0 - dirx * 1550, y0), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0 - dirx * 1550, y0 + 500), 0, 0, 0);
                        Cover.AddVertexAt(0, ap1.Convert2D(x0, y0 + 500), 0, 0, 0);
                        Cover.Layer = "细线";
                        Cover.Closed = true;
                        Cover.TransformBy(Matrix3d.Rotation(ang_red, Vector3d.ZAxis, ap1.Convert3D()));
                        modelSpace.AppendEntity(Cover);
                        tr.AddNewlyCreatedDBObject(Cover, true);
                    }
                }
                

                tr.Commit();
            }
                return SideWall;
        }




        /// <summary>
        /// 绘制集水井立面
        /// </summary>
        /// <param name="db"></param>
        /// <param name="AnchorPoint">最表面点角点（Lsets[1].startpoint）</param>
        /// <param name="height"></param>
        /// <param name="rotAngle"></param>
        /// <param name="isLeft"></param>
        /// <param name="ss">标注比例</param>
        public static Polyline PlotWell(Database db, Point2d AnchorPoint,double slop, double height,bool isLeft,bool isBianGouTri)
        {
            Polyline BottomWall = new Polyline();
            Polyline Cover,Shoulder;
            Polyline BianGou;
            Point2d p0, p1, p2, p3, p4, p5, p6, p7;

            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                double absslop = Math.Abs(slop);
                double redslop = Math.Atan(absslop);
                double L = 0;
                int dirx = isLeft ? -1 : 1;
                
                switch (height)
                {
                    case (2000):
                        {
                            // 底板、边板
                            p0 = AnchorPoint.Convert2D(dirx*250*Math.Sin(redslop),-250 * Math.Cos(redslop));
                            p1 = AnchorPoint.Convert2D(dirx * 1900,-250);
                            p2 = p1.Convert2D(0,2000);
                            p3 = p2.Convert2D(-dirx * 250,0);
                            p4 = p3.Convert2D(0,-1750);
                            p5 = AnchorPoint;
                            BottomWall = new Polyline();
                            BottomWall.AddVertexAt(0, p0, 0, 0, 0);
                            BottomWall.AddVertexAt(1, p1, 0, 0, 0);
                            BottomWall.AddVertexAt(2, p2, 0, 0, 0);
                            BottomWall.AddVertexAt(3, p3, 0, 0, 0);
                            BottomWall.AddVertexAt(4, p4, 0, 0, 0);
                            BottomWall.AddVertexAt(5, p5, 0, 0, 0);
                            BottomWall.Layer = "粗线";
                            BottomWall.Closed = true;
                            modelSpace.AppendEntity(BottomWall);
                            tr.AddNewlyCreatedDBObject(BottomWall, true);
                            
                            // 盖板
                            p0 = p2.Convert2D(-dirx*130, 0);
                            p1 = p0.Convert2D(-dirx * (1420 + 130), 0);
                            p2 = p1.Convert2D(0, 100);
                            p3 = p0.Convert2D(0, 100);
                            Cover = new Polyline();
                            Cover.AddVertexAt(0, p0, 0, 0, 0);
                            Cover.AddVertexAt(1, p1, 0, 0, 0);
                            Cover.AddVertexAt(2, p2, 0, 0, 0);
                            Cover.AddVertexAt(3, p3, 0, 0, 0);
                            Cover.Layer = "粗线";
                            Cover.Closed = true;
                            modelSpace.AppendEntity(Cover);
                            tr.AddNewlyCreatedDBObject(Cover, true);

                            p0 = p1.Convert2D(dirx * (130-height*Math.Sin(absslop)+15), 0);
                            p1 = p0.Convert2D(0, -250);
                            p2 = p1.Convert2D(-dirx * 350);
                            p3 = p2.Convert2D(0, 550);
                            p4 = p3.Convert2D(dirx * 200);
                            p5 = p4.Convert2D(0, -300);
                            Shoulder = new Polyline();
                            Shoulder.AddVertexAt(0, p0, 0, 0, 0);
                            Shoulder.AddVertexAt(1, p1, 0, 0, 0);
                            Shoulder.AddVertexAt(2, p2, 0, 0, 0);
                            Shoulder.AddVertexAt(3, p3, 0, 0, 0);
                            Shoulder.AddVertexAt(4, p4, 0, 0, 0);
                            Shoulder.AddVertexAt(5, p5, 0, 0, 0);
                            Shoulder.Layer = "粗线";
                            Shoulder.Closed = true;
                            modelSpace.AppendEntity(Shoulder);
                            tr.AddNewlyCreatedDBObject(Shoulder, true);

                            //// 边沟
                            //if (isBianGouTri)
                            //{
                            //    BianGou = new Polyline();
                            //    p2 = p0.Convert2D(dirx * 230);
                            //    p1 = p2.Convert2D(dirx * 602.3077, -401.5385);
                            //    p0 = p2.Convert2D(dirx * 870);
                            //    BianGou.AddVertexAt(0, p0, 0, 0, 0);
                            //    BianGou.AddVertexAt(1, p1, 0, 0, 0);
                            //    BianGou.AddVertexAt(2, p2, 0, 0, 0);
                            //    BianGou.Layer = "细线";
                            //    BianGou.Closed = false;
                            //    modelSpace.AppendEntity(BianGou);
                            //    tr.AddNewlyCreatedDBObject(BianGou, true);
                            //}
                            //else
                            //{
                            //    BianGou = new Polyline();
                            //    p3 = p0.Convert2D(dirx * 250);
                            //    p2 = p3.Convert2D(0, -650);
                            //    p1 = p2.Convert2D(dirx * 800);
                            //    p0 = p1.Convert2D(0,650);
                            //    BianGou.AddVertexAt(0, p0, 0, 0, 0);
                            //    BianGou.AddVertexAt(1, p1, 0, 0, 0);
                            //    BianGou.AddVertexAt(2, p2, 0, 0, 0);
                            //    BianGou.AddVertexAt(3, p3, 0, 0, 0);
                            //    BianGou.Layer = "细线";
                            //    BianGou.Closed = false;
                            //    modelSpace.AppendEntity(BianGou);
                            //    tr.AddNewlyCreatedDBObject(BianGou, true);
                            //}




                            break;
                        }
                    case (1400):
                        {
                            p0 = AnchorPoint.Convert2D(dirx * 200 * Math.Sin(redslop), -200 * Math.Cos(redslop));
                            p1 = AnchorPoint.Convert2D(dirx * 1850, -200);
                            p2 = p1.Convert2D(0, 1400);
                            p3 = p2.Convert2D(-dirx * 200, 0);
                            p4 = p3.Convert2D(0, -1200);
                            p5 = AnchorPoint;
                            BottomWall = new Polyline();
                            BottomWall.AddVertexAt(0, p0, 0, 0, 0);
                            BottomWall.AddVertexAt(1, p1, 0, 0, 0);
                            BottomWall.AddVertexAt(2, p2, 0, 0, 0);
                            BottomWall.AddVertexAt(3, p3, 0, 0, 0);
                            BottomWall.AddVertexAt(4, p4, 0, 0, 0);
                            BottomWall.AddVertexAt(5, p5, 0, 0, 0);
                            BottomWall.Layer = "粗线";
                            BottomWall.Closed = true;
                            modelSpace.AppendEntity(BottomWall);
                            tr.AddNewlyCreatedDBObject(BottomWall, true);

                            p0 = p2.Convert2D(-dirx * 80, 0);
                            p1 = p0.Convert2D(-dirx * (1420 + 130), 0);
                            p2 = p1.Convert2D(0, 100);
                            p3 = p0.Convert2D(0, 100);
                            Cover = new Polyline();
                            Cover.AddVertexAt(0, p0, 0, 0, 0);
                            Cover.AddVertexAt(1, p1, 0, 0, 0);
                            Cover.AddVertexAt(2, p2, 0, 0, 0);
                            Cover.AddVertexAt(3, p3, 0, 0, 0);
                            Cover.Layer = "粗线";
                            Cover.Closed = true;
                            modelSpace.AppendEntity(Cover);
                            tr.AddNewlyCreatedDBObject(Cover, true);
                            // 牛腿
                            Shoulder = new Polyline();
                            p0 = p1.Convert2D(dirx * (130 - height * Math.Sin(absslop) + 15), 0);
                            p1 = p0.Convert2D(0, -200);
                            p2 = p1.Convert2D(-dirx * 350);
                            p3 = p2.Convert2D(0, 500);
                            p4 = p3.Convert2D(dirx * 200);
                            p5 = p4.Convert2D(0, -300);             
                            Shoulder.AddVertexAt(0, p0, 0, 0, 0);
                            Shoulder.AddVertexAt(1, p1, 0, 0, 0);
                            Shoulder.AddVertexAt(2, p2, 0, 0, 0);
                            Shoulder.AddVertexAt(3, p3, 0, 0, 0);
                            Shoulder.AddVertexAt(4, p4, 0, 0, 0);
                            Shoulder.AddVertexAt(5, p5, 0, 0, 0);
                            Shoulder.Layer = "粗线";
                            Shoulder.Closed = true;
                            modelSpace.AppendEntity(Shoulder);
                            tr.AddNewlyCreatedDBObject(Shoulder, true);

                            //// 边沟
                            //if (isBianGouTri)
                            //{
                            //    BianGou = new Polyline();
                            //    p2 = p0.Convert2D(dirx * 230);
                            //    p1 = p2.Convert2D(dirx * 602.3077, -401.5385);
                            //    p0 = p2.Convert2D(dirx * 870);
                            //    BianGou.AddVertexAt(0, p0, 0, 0, 0);
                            //    BianGou.AddVertexAt(1, p1, 0, 0, 0);
                            //    BianGou.AddVertexAt(2, p2, 0, 0, 0);
                            //    BianGou.Layer = "细线";
                            //    BianGou.Closed = false;
                            //    modelSpace.AppendEntity(BianGou);
                            //    tr.AddNewlyCreatedDBObject(BianGou, true);
                            //}
                            //else
                            //{
                            //    BianGou = new Polyline();
                            //    p3 = p0.Convert2D(dirx * 250);
                            //    p2 = p3.Convert2D(0, -650);
                            //    p1 = p2.Convert2D(dirx * 800);
                            //    p0 = p1.Convert2D(0, 650);
                            //    BianGou.AddVertexAt(0, p0, 0, 0, 0);
                            //    BianGou.AddVertexAt(1, p1, 0, 0, 0);
                            //    BianGou.AddVertexAt(2, p2, 0, 0, 0);
                            //    BianGou.AddVertexAt(3, p3, 0, 0, 0);
                            //    BianGou.Layer = "细线";
                            //    BianGou.Closed = false;
                            //    modelSpace.AppendEntity(BianGou);
                            //    tr.AddNewlyCreatedDBObject(BianGou, true);
                            //}








                            break;
                        }

                }









                tr.Commit();
            }

            return BottomWall;

        }















    }
}
