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
using System.Text.RegularExpressions;

namespace MyCAD1
{
    class TextPloter
    {
        public static void PrintTitle(Database db,string textstring,Point2d PositionPoint ,double scale= 100)
        {

            DBText title= new DBText(),scalestring= new DBText();
            Line BLine,SLine;
            Regex regChina = new Regex("^[^\x00-\xFF]");
            Regex regEnglish = new Regex("^[a-zA-Z]");



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                double WdWidth;
                if (regEnglish.IsMatch(textstring[textstring.Length-1].ToString()))
                {
                    WdWidth = 2.5;
                    scalestring.TextString = string.Format("Ech:1/{0:G0}", scale);

                }
                else
                {
                    WdWidth = 5 * 0.7;
                    scalestring.TextString = string.Format("1：{0:G0}", scale);
                }
                double WdLeng = textstring.Length * WdWidth * scale;



                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                BLine = new Line(PositionPoint.Convert3D(-0.5 * WdLeng, 0), PositionPoint.Convert3D(0.5 * WdLeng, 0));
                SLine = new Line(PositionPoint.Convert3D(-0.5 * WdLeng, -1*scale), PositionPoint.Convert3D(0.5 * WdLeng, -1 * scale));
                BLine.Layer = "标注";
                BLine.LineWeight = LineWeight.LineWeight050;
                SLine.Layer = "标注";
                BLine.ColorIndex = 1;


                title = new DBText();
                title.TextString = textstring;
                title.Height = 5 * scale;
                title.Position = PositionPoint.Convert3D();
                title.HorizontalMode = TextHorizontalMode.TextCenter;
                title.VerticalMode = TextVerticalMode.TextBottom;
                title.AlignmentPoint = PositionPoint.Convert3D(0,0.5*scale);       

                scalestring.Position = PositionPoint.Convert3D();
                scalestring.Height = 3.5 * scale;
                scalestring.HorizontalMode = TextHorizontalMode.TextCenter;
                scalestring.VerticalMode = TextVerticalMode.TextTop;
                scalestring.AlignmentPoint = PositionPoint.Convert3D(0,-2*scale);
                title.TextStyleId = st["fsdb"];
                scalestring.TextStyleId = st["fsdb"];
                title.Layer = "标注";
                scalestring.Layer = "标注";
                title.WidthFactor = 0.75;
                scalestring.WidthFactor = 0.75;                

                modelSpace.AppendEntity(BLine);
                tr.AddNewlyCreatedDBObject(BLine, true);
                modelSpace.AppendEntity(SLine);
                tr.AddNewlyCreatedDBObject(SLine, true);
                modelSpace.AppendEntity(title);
                tr.AddNewlyCreatedDBObject(title, true);
                modelSpace.AppendEntity(scalestring);
                tr.AddNewlyCreatedDBObject(scalestring, true);
                tr.Commit();
            }
            return ;

        }



        /// <summary>
        /// 剖面标注
        /// </summary>
        /// <param name="db"></param>
        /// <param name="theName"></param>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="scale"></param>
        public static void DimSection(Database db, char theName, Point2d StartPoint,Point2d EndPoint, double scale = 100)
        {

            
            Line[] Lset = new Line[2];
            Polyline[] PLset = new Polyline[2];
            double ang = StartPoint.X == EndPoint.X ? 0.5 * Math.PI : 0;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                for (int i = 0; i < 2; i++)
                {
                    DBText t = new DBText()
                    {
                        TextString = theName.ToString(),
                        Layer = "标注",
                        Height = 3 * scale,
                        TextStyleId = st["fsdb"],
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextBottom,
                        WidthFactor = 0.75,
                        Rotation = ang,
                    };
                    if (i == 0)
                    {
                        t.AlignmentPoint = StartPoint.Convert3D(0, 0.5 * scale);
                    }
                    else
                    {
                        t.AlignmentPoint = EndPoint.Convert3D(0, 0.5 * scale);
                    }
                    modelSpace.AppendEntity(t);
                    tr.AddNewlyCreatedDBObject(t, true);


                    Line L;
                    Polyline PL=new Polyline();
                    Point2d p0=new Point2d(), p1=new Point2d();
                    if (ang==0)
                    {
                        L = i == 0 ? new Line(StartPoint.Convert3D(-1 * scale, 0), StartPoint.Convert3D(1 * scale, 0)) :
                            new Line(EndPoint.Convert3D(-1 * scale, 0), EndPoint.Convert3D(1 * scale, 0));
                        p0 = i==0?StartPoint:EndPoint;
                        p1 = i==0?StartPoint.Convert2D(0, -1 * scale):EndPoint.Convert2D(0, -1 * scale);
                    }
                    else
                    {
                        L = i == 0 ? new Line(StartPoint.Convert3D(0, -1 * scale), StartPoint.Convert3D(0, 1 * scale)) :
                            new Line(EndPoint.Convert3D(0, -1 * scale), EndPoint.Convert3D(0, 1 * scale));
                        p0 = i == 0 ? StartPoint : EndPoint;
                        p1 = i == 0 ? StartPoint.Convert2D(1 * scale,0): EndPoint.Convert2D(1 * scale, 0);
                    }
                    L.Layer = "标注";
                    L.ColorIndex = 1;
                    modelSpace.AppendEntity(L);
                    tr.AddNewlyCreatedDBObject(L, true);

                    PL.AddVertexAt(0,p0, 0, 0, 1.5*scale);
                    PL.AddVertexAt(1, p1, 0, 0, 0);
                    modelSpace.AppendEntity(PL);
                    tr.AddNewlyCreatedDBObject(PL, true);





                }
                tr.Commit();
            }

        }



    }
}
