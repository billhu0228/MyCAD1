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
    class HatchPloter
    {


        public static void PlotDZT(Database db,Point2d pos,double refH,DZT relatedDZT,int s)
        {
            Dictionary<string, string> patterdic = new Dictionary<string, string>();
            patterdic.Add("中砂砾", "AR-CONC");
            patterdic.Add("黏土", "ANSI31");
            patterdic.Add("中砂", "DOTS");
            patterdic.Add("全风化硅质板岩", "CORK");
            patterdic.Add("中风化石英闪长岩", "CROSS");
            patterdic.Add("块体", "GRAVEL");
            patterdic.Add("其他", "ANSI38");
            Dictionary<string, int> patterscale = new Dictionary<string, int>();
            patterscale.Add("中砂砾",1);
            patterscale.Add("黏土", 15);
            patterscale.Add("中砂", 30);
            patterscale.Add("全风化硅质板岩", 15);
            patterscale.Add("中风化石英闪长岩", 15);
            patterscale.Add("块体", 15);
            patterscale.Add("其他", 15);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                for(int i = 0; i < relatedDZT.DZBG.Count();i++)
                {
                    Tuple<string, double> tp = relatedDZT.DZBG[i];
                    double topbg;
                    double botbg = tp.Item2;

                    DBText theTitle = new DBText();
                    theTitle.TextString = string.Format("{0:F3}", botbg);
                    theTitle.Height = 2.5 * s;
                    theTitle.Position = pos.Convert3D(0 + 2200 + 500, (botbg - refH) * 1000);
                    theTitle.HorizontalMode = TextHorizontalMode.TextLeft;
                    theTitle.VerticalMode = TextVerticalMode.TextBottom;
                    theTitle.AlignmentPoint = theTitle.Position;
                    theTitle.Layer = "地质";
                    theTitle.TextStyleId = st["fsdb"];
                    theTitle.WidthFactor = 0.85;
                    modelSpace.AppendEntity(theTitle);
                    tr.AddNewlyCreatedDBObject(theTitle, true);

                    if (relatedDZT.DZBG.Count() == 1||i+1==relatedDZT.DZBG.Count())
                    {
                        topbg = relatedDZT.kongkou;

                        theTitle = new DBText();
                        theTitle.TextString = string.Format("{0:F3}",topbg);
                        theTitle.Height = 2.5*s;
                        theTitle.Position = pos.Convert3D(0+2200+500, (topbg - refH) * 1000);
                        theTitle.HorizontalMode = TextHorizontalMode.TextLeft;
                        theTitle.VerticalMode = TextVerticalMode.TextBottom;
                        theTitle.AlignmentPoint = theTitle.Position;
                        theTitle.Layer = "地质";
                        theTitle.TextStyleId = st["fsdb"];
                        theTitle.WidthFactor = 0.85;
                        modelSpace.AppendEntity(theTitle);
                        tr.AddNewlyCreatedDBObject(theTitle, true);
                    }
                    else
                    {                        
                        topbg = relatedDZT.DZBG[i+1].Item2;
                    }


                    Polyline pl = new Polyline();
                    pl.AddVertexAt(0, pos.Convert2D(0+2200, (topbg - refH) * 1000), 0, 0, 0);
                    pl.AddVertexAt(1, pos.Convert2D(500 + 2200, (topbg - refH) * 1000), 0, 0, 0);
                    pl.AddVertexAt(2, pos.Convert2D(500 + 2200, (botbg - refH) * 1000), 0, 0, 0);
                    pl.AddVertexAt(3, pos.Convert2D(0 + 2200, (botbg - refH) * 1000), 0, 0, 0);
                    pl.Closed = true;
                    pl.Layer = "地质";
                    modelSpace.AppendEntity(pl);
                    tr.AddNewlyCreatedDBObject(pl, true);
                    Hatch hat = new Hatch();
                    try
                    {
                        hat.SetHatchPattern(HatchPatternType.PreDefined, patterdic[tp.Item1]);
                        hat.PatternScale = patterscale[tp.Item1];
                    }
                    catch
                    {
                        hat.SetHatchPattern(HatchPatternType.PreDefined, patterdic["其他"]);
                        hat.PatternScale = 15;
                    }
                    modelSpace.AppendEntity(hat);
                    tr.AddNewlyCreatedDBObject(hat, true);
                    hat.Layer = "地质";
                    hat.Associative = true;
                    hat.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { pl.ObjectId });
                    hat.EvaluateHatch(true);

                }
                tr.Commit();
            }
        }











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
