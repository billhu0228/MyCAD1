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
    }
}
