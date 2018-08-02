﻿using System;
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
    class MulitlinePloter
    {


        /// <summary>
        /// 横向平行多线
        /// </summary>
        /// <param name="db"></param>
        /// <param name="BotStartPoint">左下起点</param>
        /// <param name="BotEndPoint">右下终点</param>
        /// <param name="Seps">相对下线间距列表</param>
        /// <param name="Layers">图层列表</param>
        /// <param name="isVertical">是否竖直修剪</param>
        /// <returns>多线集合</returns>
        public static Line[] PlotN(Database db, Point3d BotStartPoint,Point3d BotEndPoint, double[] Seps,string[] Layers,bool isVertical=false)
        {
            int size = Layers.Length;
            System.Diagnostics.Debug.Assert(size==Seps.Length);

            Line[] LineSet = new Line[size];
            Line leftside, rightside;
            Line tmp;
            Point3dCollection startPts, endPts;
            leftside = new Line(BotStartPoint, BotStartPoint.Convert3D(0, 1));
            rightside = new Line(BotEndPoint, BotEndPoint.Convert3D(0, 1));

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                for (int ii=0; ii<size; ii++)
                {
                    startPts = new Point3dCollection();
                    endPts = new Point3dCollection();
                    tmp = new Line(BotStartPoint, BotEndPoint) {Layer=Layers[ii] ,LinetypeScale=4.0};
                    tmp = (Line)tmp.GetOffsetCurves(Seps[ii])[0];
                    if (isVertical & Seps[ii]!=0)
                    {
                        tmp.IntersectWith(leftside, Intersect.ExtendBoth, startPts, IntPtr.Zero, IntPtr.Zero);
                        tmp.IntersectWith(rightside, Intersect.ExtendBoth, endPts, IntPtr.Zero, IntPtr.Zero);
                        tmp.StartPoint = startPts[0];
                        tmp.EndPoint = endPts[0];                        
                    }
                    LineSet[ii] = tmp;
                    modelSpace.AppendEntity(LineSet[ii]);
                    tr.AddNewlyCreatedDBObject(LineSet[ii], true);
                }


                tr.Commit();
            }

            return LineSet;
        }




        /// <summary>
        /// 竖向分割线
        /// </summary>
        /// <param name="db"></param>
        /// <param name="LowerLine">下线</param>
        /// <param name="UperLine">上线</param>
        /// <param name="Seps">偏移距离</param>
        /// <param name="LyName">图层名称</param>
        /// <param name="isVertical">是否竖直分割</param>
        /// <returns></returns>
        public static Line[] PlotCutLine(Database db, Line LowerLine, Line UperLine,double [] Seps,string LyName,bool isVertical=false)
        {
            int size = Seps.Length;
            Line baseLine;
            if (isVertical)
            {
                baseLine = new Line(LowerLine.StartPoint, LowerLine.StartPoint.Convert3D(0, 1, 0));
            }
            else
            {
                baseLine = (Line)LowerLine.Clone();
                baseLine.TransformBy(Matrix3d.Rotation(0.5 * Math.PI, new Vector3d(0, 0, 1), LowerLine.StartPoint));
            }
            
            Line[] LineSet = new Line[size];
            Line tmp;
            Point3dCollection startPts, endPts;         
            

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                for (int ii = 0; ii < size; ii++)
                {
                    startPts = new Point3dCollection();
                    endPts = new Point3dCollection();
                    tmp =  (Line)baseLine.GetOffsetCurves(-Seps[ii])[0];
                    tmp.IntersectWith(LowerLine, Intersect.ExtendBoth, startPts, IntPtr.Zero, IntPtr.Zero);
                    tmp.IntersectWith(UperLine, Intersect.ExtendBoth, endPts, IntPtr.Zero, IntPtr.Zero);
                    tmp.StartPoint = startPts[0];
                    tmp.EndPoint = endPts[0];
                    tmp.Layer = LyName;
                    LineSet[ii] = tmp;
                    modelSpace.AppendEntity(LineSet[ii]);
                    tr.AddNewlyCreatedDBObject(LineSet[ii], true);
                }


                tr.Commit();
            }

            return LineSet;
        }




        public static void PlotSideLine(Database db, Line AxisLine, Line StartLine, Line EndLine,double scale, bool isLeft = false)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                Point3dCollection pts = new Point3dCollection();
                Point3d LimitA, LimitB;
                StartLine.IntersectWith(AxisLine, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                LimitA = pts[0];
                pts.Clear();
                EndLine.IntersectWith(AxisLine, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                LimitB = pts[0];
                Point3d st, ed;
                double distA = Math.Abs(LimitA.DistanceTo(AxisLine.StartPoint));                
                double distB = Math.Abs(LimitB.DistanceTo(AxisLine.EndPoint));
                int dir = LimitA.Y > AxisLine.StartPoint.Y ? 1 : -1;
                int dirx = isLeft ? -1 : 1;
                double kk = AxisLine.GetK();
                for (int i=0;i<distA/2/scale;i++)
                {
                    double dy = i * 2 * scale * dir;
                    st = AxisLine.StartPoint.Convert3D(dy/ kk, dy);
                    if (i % 2 == 0)
                    {
                        ed = st.Convert3D(dirx * 2 * scale, 0, 0);
                    }
                    else
                    {
                        ed= st.Convert3D(dirx * 4 * scale, 0, 0);
                    }
                    Line tmp = new Line(st, ed);
                    modelSpace.AppendEntity(tmp);
                    tr.AddNewlyCreatedDBObject(tmp, true);                    
                }
                for (int i = 0; i < distB / 2 / scale; i++)
                {
                    double dy = -i * 2 * scale * dir;
                    st = AxisLine.EndPoint.Convert3D(dy / kk, dy);
                    if (i % 2 == 0)
                    {
                        ed = st.Convert3D(dirx * 2 * scale, 0, 0);
                    }
                    else
                    {
                        ed = st.Convert3D(dirx * 4 * scale, 0, 0);
                    }
                    Line tmp = new Line(st, ed);
                    modelSpace.AppendEntity(tmp);
                    tr.AddNewlyCreatedDBObject(tmp, true);
                }





                tr.Commit();
            }
        }


        




    }
}
