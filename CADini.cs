using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;


[assembly: CommandClass(typeof(MyCAD1.HanDong))]

namespace MyCAD1
{
    public partial class HanDong
    {
        static double[] sectiont = new double[12] { 2500, 2500, 2000, 250, 250, 250, 50, 50, 200, 100, 0.01, 200 };
        static double[] Sect = new double[12] { 2500, 2500, 2000, 250, 250, 250, 50, 50, 200, 100, 0.01, 0 };
        static double[] Layout = new double[6] { 8, 2000, 3, -0.01,200,300};
        static Dictionary<int, double[]> cLayer = new Dictionary<int, double[]>
        {
            { 0,new double[]{2900,100} },
            { 1,new double[]{3500,400} },
        };
        static Dictionary<int, double[]> Refill = new Dictionary<int, double[]>
        {
            { 0,new double[]{1,500} },
        };
        static double[] Wall_leftE = new double[6] { 200, 300, 2820, Sect[3], 250, cLayer[1][1] };
        static double[] Wall_rightE = new double[6] { 200, 300, 2820, Sect[3], 250, cLayer[1][1] };
        static double[] Wall_leftP = new double[] { 30, 30, 250 };
        static double[] Wall_rightP = new double[] { 30, 30, 250 };
        static double[] height = new double[4] { 22.89, 22.83, 22.70, 19.30 };
        static double[] RoadWidth = new double[2] { 5000, 5000 };
        static double[] RoadSlop = new double[2] { 3, 2 };
        //-------------------------------------------------------------------------------------------
        static int[] length = new int[2] { 8, 2200 };
        static double[] lengthB = new double[2] { 2820, 2820 };        
        static double slop = -0.01;
        static int LayerA = 400;
        static int LayerB = 100;
        //-------------------------------------------------------------------------------------------
        private static void BiaoGao(double bgdata, Point3d refpt, BlockTableRecord ms, Transaction tr, BlockTable blockTbl)
        {
            ObjectId blkRecId = blockTbl["BG"];
            using (BlockReference acBlkRef = new BlockReference(refpt, blkRecId))
            {
                //acBlkRef.SetAttributes();
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
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
            }
        }
        private static Point3d Convert3d(Point2d pt)
        {
            Point3d res = new Point3d(pt.X, pt.Y, 0);
            return res;
        }
        //-------------------------------------------------------------------------------------------
        private static Point3d Convert3d(Point2d pt, double x = 0, double y = 0)
        {
            Point3d res = new Point3d(pt.X + x, pt.Y + y, 0);
            return res;
        }
        private static Point3d Convert3d(Point3d pt, double x = 0, double y = 0)
        {
            Point3d res = new Point3d(pt.X + x, pt.Y + y, 0);
            return res;
        }
        public static Point2d Convert2d(Point2d pt, double x = 0, double y = 0)
        {
            Point2d res = new Point2d(pt.X + x, pt.Y + y);
            return res;
        }
        private static Point2d Convert2d(Point3d pt, double x = 0, double y = 0)
        {
            Point2d res = new Point2d(pt.X + x, pt.Y + y);
            return res;
        }

        private static Line2d ConvertLine2d(Line src)
        {
            Line2d output = new Line2d(Convert2d(src.StartPoint), Convert2d(src.EndPoint));
            return output;
        }
        private static Line3d ConvertLine3d(Line2d src)
        {
            Line3d output = new Line3d(Convert3d(src.StartPoint), Convert3d(src.EndPoint));
            return output;
        }
        public static void addTitle(Point2d pos,string theTitle,int theScale,BlockTableRecord rcd,Document doc)
        {
            var db = doc.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;
                DBText mainT = new DBText();
                DBText subT = new DBText();
                mainT.Position = Convert3d(pos);
                mainT.HorizontalMode = TextHorizontalMode.TextMid;
                mainT.VerticalMode = TextVerticalMode.TextBottom;
                subT.HorizontalMode = TextHorizontalMode.TextMid;
                subT.VerticalMode = TextVerticalMode.TextTop;               
                mainT.AlignmentPoint = Convert3d(pos);


                mainT.TextStyleId = st["fsdb"];
                subT.TextStyleId = st["fsdb"];
                mainT.TextString = theTitle;
                rcd.AppendEntity(mainT);
                tr.AddNewlyCreatedDBObject(mainT, true);
                tr.Commit();
            }
            //using (var tr = db.TransactionManager.StartTransaction())
            //{
            //    Polyline L1 = new Polyline();
            //    Polyline L2 = new Polyline();
            //    double dx = mainT.Bounds.Value.MaxPoint.X - mainT.Bounds.Value.MaxPoint.X;
            //    L1.AddVertexAt(0, Convert2d(mainT.Bounds.Value.MinPoint), 0, 0, 0);
            //    L1.AddVertexAt(1, Convert2d(mainT.Bounds.Value.MinPoint, dx, 0), 0, 0, 0);
            //    L1.Thickness = 0.5;
            //    rcd.AppendEntity(L1);
            //    tr.AddNewlyCreatedDBObject(L1, true);
            //    return;
            //    tr.Commit();

            //}





        }
        //-------------------------------------------------------------------------------------------
        private static void HengPo(double hpdata, Point3d refpt,bool isLeft, BlockTableRecord ms, Transaction tr, BlockTable blockTbl)
        {
            ObjectId blkRecId;
            if (isLeft) {  blkRecId = blockTbl["ZP"]; }else
            {  blkRecId = blockTbl["YP"]; }           
            
            using (BlockReference acBlkRef = new BlockReference(refpt, blkRecId))
            {
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
                            acAttRef.TextString = String.Format("{0:f2}%", hpdata);
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------------------------
        [CommandMethod("bini")]
        public static void CADini()
        {
            Dictionary<string, short> ldic = new Dictionary<string, short>();
            ldic["粗线"] = 4;
            ldic["细线"] = 2;
            ldic["标注"] = 7;
            ldic["中心线"] = 1;
            ldic["虚线"] = 3;
            ldic["图框"] = 8;

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            acCurDb.LoadLineTypeFile("CENTER", "acad.lin");
            acCurDb.LoadLineTypeFile("DASHED", "acad.lin");
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                LinetypeTable acLinTbl;
                acLinTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                foreach (string key in ldic.Keys)
                {
                    short cid = ldic[key];
                    using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                    {
                        acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, cid);
                        if (cid != 4) { acLyrTblRec.LineWeight = LineWeight.LineWeight013; }
                        else { acLyrTblRec.LineWeight = LineWeight.LineWeight030; }
                        if (cid == 1) { acLyrTblRec.LinetypeObjectId = acLinTbl["CENTER"]; }
                        if (cid == 3) { acLyrTblRec.LinetypeObjectId = acLinTbl["DASHED"]; }
                        if (cid == 8) { acLyrTblRec.IsPlottable = false; }
                        acLyrTblRec.Name = key;
                        acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForWrite);
                        acLyrTbl.Add(acLyrTblRec);
                    }
                }

                TextStyleTable st = acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;
                TextStyleTableRecord str = new TextStyleTableRecord()
                {
                    Name = "En",
                    FileName = "ARIALNBI",
                };
                st.Add(str);
                acTrans.AddNewlyCreatedDBObject(str, true);
                TextStyleTableRecord str2 = new TextStyleTableRecord()
                {
                    Name = "fsdb",
                    FileName = "fsdb_e.shx",
                    BigFontFileName="fsdb.shx",
                    XScale=0.75,
                };
                ObjectId textstyleid = st.Add(str2);
                acTrans.AddNewlyCreatedDBObject(str2, true);


                DimStyleTable dst = (DimStyleTable)acTrans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForWrite);

                foreach (int thescale in new int[] { 75,100})
                {
                    DimStyleTableRecord dstr = new DimStyleTableRecord()
                    {
                        Name = "1-" + thescale.ToString(),
                        Dimscale = thescale,
                        Dimtxsty = textstyleid,
                        Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 6),
                        Dimclre = Color.FromColorIndex(ColorMethod.ByAci, 6),
                        Dimdli = 5.0,
                        Dimexe = 1.0,
                        Dimexo = 1.0,
                        DimfxlenOn = true,
                        Dimfxlen = 4,
                        Dimtxt = 2,
                        Dimasz = 1.5,
                        Dimtix = true,
                        Dimtmove = 1,
                        Dimtad = 1,
                        Dimgap = 0.8,
                        Dimdec = 0,
                        Dimtih = false,
                        Dimtoh = false,
                        Dimdsep = '.',
                        Dimlfac=0.1,
                    };
                    ObjectId dsId = dst.Add(dstr);
                    acTrans.AddNewlyCreatedDBObject(dstr, true);
                }

                //-------------------------------------------------------------------------------------------
                // 自定义块
                //-------------------------------------------------------------------------------------------
                BlockTable bt = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = new BlockTableRecord();
                btr.Name = "BG";
                bt.UpgradeOpen();
                ObjectId btrId = bt.Add(btr);
                acTrans.AddNewlyCreatedDBObject(btr, true);
                Polyline Paa = new Polyline()
                {
                    Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                };
                Paa.AddVertexAt(0, new Point2d(0,0), 0, 0, 200);
                Paa.AddVertexAt(1, new Point2d(0,200), 0, 0, 0);
                btr.AppendEntity(Paa);
                acTrans.AddNewlyCreatedDBObject(Paa, true);
                AttributeDefinition curbg = new AttributeDefinition();
                curbg.Position = new Point3d(120, 200,0);
                curbg.Height = 200;
                curbg.WidthFactor = 0.75;
                curbg.Tag = "标高";
                curbg.TextStyleId = textstyleid;
                btr.AppendEntity(curbg);
                acTrans.AddNewlyCreatedDBObject(curbg, true);
                //-------------------------------------------------------------------------------------------
                BlockTableRecord btr2 = new BlockTableRecord();
                btr2.Name = "ZP";
                bt.UpgradeOpen();
                bt.Add(btr2);
                acTrans.AddNewlyCreatedDBObject(btr2, true);
                Polyline Paa2 = new Polyline()
                {
                    Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                };
                Paa2.AddVertexAt(0, new Point2d(0-350, 0), 0, 0, 80);
                Paa2.AddVertexAt(1, new Point2d(200 - 350, 0), 0, 0, 0);
                Paa2.AddVertexAt(2, new Point2d(700 - 350, 0), 0, 0, 0);
                btr2.AppendEntity(Paa2);
                acTrans.AddNewlyCreatedDBObject(Paa2, true);
                AttributeDefinition curzp = new AttributeDefinition();
                curzp.Position = new Point3d(220 - 350, 0, 0);
                curzp.Height = 200;
                curzp.WidthFactor = 0.75;
                curzp.Tag = "左坡";
                curzp.TextStyleId = textstyleid;
                btr2.AppendEntity(curzp);
                acTrans.AddNewlyCreatedDBObject(curzp, true);
                //-------------------------------------------------------------------------------------------
                BlockTableRecord btr3 = new BlockTableRecord();
                btr3.Name = "YP";
                bt.UpgradeOpen();
                bt.Add(btr3);
                acTrans.AddNewlyCreatedDBObject(btr3, true);
                Polyline Paa3 = new Polyline()
                {
                    Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                };
                Paa3.AddVertexAt(0, new Point2d(0+350, 0), 0, 0, 80);
                Paa3.AddVertexAt(1, new Point2d(-200 + 350, 0), 0, 0, 0);
                Paa3.AddVertexAt(2, new Point2d(-700 + 350, 0), 0, 0, 0);
                btr3.AppendEntity(Paa3);
                acTrans.AddNewlyCreatedDBObject(Paa3, true);
                AttributeDefinition curyp = new AttributeDefinition();
                curyp.Position = new Point3d(-220 + 350, 0, 0);
                curyp.HorizontalMode=TextHorizontalMode.TextRight;
                curyp.AlignmentPoint = curyp.Position;
                curyp.Height = 200;
                curyp.WidthFactor = 0.75;
                curyp.Tag = "右坡";
                curyp.TextStyleId = textstyleid;
                btr3.AppendEntity(curyp);
                acTrans.AddNewlyCreatedDBObject(curyp, true);
                //-------------------------------------------------------------------------------------------
                            
                //-------------------------------------------------------------------------------------------
                acTrans.Commit();
            }
        }
    }
}
