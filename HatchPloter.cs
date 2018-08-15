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
    class HatchPloter
    {
        /// <summary>
        /// 绘制填充
        /// </summary>
        /// <param name="db">CAD数据库</param>
        /// <param name="pl">外轮廓多线</param>
        /// <param name="href">填充参考样式</param>
        /// <param name="scale">填充比例，默认=1</param>
        /// <returns>返回填充对象</returns>
        public static Hatch PlotH(Database db, Polyline pl, Hatch href,double scale= 1)
        {

            Hatch hatch1 = new Hatch();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                hatch1.SetHatchPattern(href.PatternType, href.PatternName);
                modelSpace.AppendEntity(hatch1);
                tr.AddNewlyCreatedDBObject(hatch1, true);
                hatch1.PatternScale = href.PatternScale;
                hatch1.Layer = "填充";
                hatch1.Associative = true;
                hatch1.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { pl.ObjectId });                
                hatch1.EvaluateHatch(true);

                
                hatch1.PatternScale = href.PatternScale;
                hatch1.SetHatchPattern(hatch1.PatternType, hatch1.PatternName);


                tr.Commit();
            }
            return hatch1;

        }


        public static Polyline PlotLayerOne(Database db,Point2d p1,Point2d p2,Hatch href,double offset=0,double tk=100,double scale=1,bool isVetica = false)
        {
            Hatch hat = new Hatch();
            Polyline PL = new Polyline();
            Line ori = new Line(p1.Convert3D(), p2.Convert3D());
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;


                if (isVetica)
                {
                    PL.AddVertexAt(0, p1.Convert2D(0,-offset), 0, 0, 0);
                    PL.AddVertexAt(1, p2.Convert2D(0,-offset), 0, 0, 0);
                    PL.AddVertexAt(2, p2.Convert2D(0, -tk-offset), 0, 0, 0);
                    PL.AddVertexAt(3, p1.Convert2D(0, -tk-offset), 0, 0, 0);
                }
                else
                {
                    Line lineup = (Line)ori.GetOffsetCurves(-offset)[0];
                    PL.AddVertexAt(0, lineup.StartPoint.Convert2D(), 0, 0, 0);
                    PL.AddVertexAt(1, lineup.EndPoint.Convert2D(), 0, 0, 0);
                    Line linetmp = (Line)ori.GetOffsetCurves(-tk-offset)[0];
                    PL.AddVertexAt(2, linetmp.EndPoint.Convert2D(), 0, 0, 0);
                    PL.AddVertexAt(3, linetmp.StartPoint.Convert2D(), 0, 0, 0);
                }
                PL.Closed = true;
                PL.Layer = "细线";
                modelSpace.AppendEntity(PL);
                tr.AddNewlyCreatedDBObject(PL, true);
            

                hat.SetHatchPattern(href.PatternType, href.PatternName);
                modelSpace.AppendEntity(hat);
                tr.AddNewlyCreatedDBObject(hat, true);
                hat.PatternScale = href.PatternScale;
                hat.Layer = "填充";
                hat.Associative = true;
                hat.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { PL.ObjectId });
                hat.EvaluateHatch(true);

                hat.PatternScale = href.PatternScale;
                hat.SetHatchPattern(hat.PatternType, hat.PatternName);
                tr.Commit();
            }


            return PL;

        }
    }
}
