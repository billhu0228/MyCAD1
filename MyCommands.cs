using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[assembly: CommandClass(typeof(MyCAD1.MyCommands))]

namespace MyCAD1
{
    public class MyCommands
    {
        //-------------------------------------------------------------------------------------------
        [CommandMethod("ini")]
        public static void CADini()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;            
            // Start a transaction
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Dictionary<string, short> ldic = new Dictionary<string, short>()
                {
                    ["粗线"] = 4,
                    ["细线"] = 2,
                    ["标注"] = 7,
                    ["中心线"] = 1,
                    ["虚线"] = 3,
                    ["图框"] = 8,
                };
                List<string> Lname = new List<string>() { "CENTER", "DASHED" };
                LayerTable acLyrTbl;
                acLyrTbl = tr.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                LinetypeTable acLinTbl;
                acLinTbl = tr.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                foreach(string ltname in Lname)
                {
                    if (!acLinTbl.Has(ltname))
                    {
                        acCurDb.LoadLineTypeFile(ltname, "acad.lin");
                    }
                }
                foreach (string key in ldic.Keys)
                {
                    short cid = ldic[key];
                    LayerTableRecord acLyrTblRec = new LayerTableRecord();
                    if (!acLyrTbl.Has(key))
                    {                        
                        acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, cid);
                        if (cid != 4) { acLyrTblRec.LineWeight = LineWeight.LineWeight013; }
                        else { acLyrTblRec.LineWeight = LineWeight.LineWeight030; }
                        if (cid == 1) { acLyrTblRec.LinetypeObjectId = acLinTbl["CENTER"]; }
                        if (cid == 3) { acLyrTblRec.LinetypeObjectId = acLinTbl["DASHED"]; }
                        if (cid == 8) { acLyrTblRec.IsPlottable = false; }
                        acLyrTblRec.Name = key;
                        if (acLyrTbl.IsWriteEnabled == false) acLyrTbl.UpgradeOpen();
                        acLyrTbl.Add(acLyrTblRec);
                        tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                    }
                    else
                    {
                        acLyrTblRec = tr.GetObject(acLyrTbl[key], OpenMode.ForWrite) as LayerTableRecord;
                        acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, cid);
                        if (cid != 4) { acLyrTblRec.LineWeight = LineWeight.LineWeight013; }
                        else { acLyrTblRec.LineWeight = LineWeight.LineWeight030; }
                        if (cid == 1) { acLyrTblRec.LinetypeObjectId = acLinTbl["CENTER"]; }
                        if (cid == 3) { acLyrTblRec.LinetypeObjectId = acLinTbl["DASHED"]; }
                        if (cid == 8) { acLyrTblRec.IsPlottable = false; }
                    }
                }
                //-------------------------------------------------------------------------------------------
                TextStyleTable st = tr.GetObject(acCurDb.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;
                if(!st.Has("EN"))
                {
                    TextStyleTableRecord str = new TextStyleTableRecord()
                    {
                        Name = "En",
                        FileName = "ARIALNBI",
                    };
                    st.Add(str);
                    tr.AddNewlyCreatedDBObject(str, true);
                }
                else
                {
                    TextStyleTableRecord str = tr.GetObject(st["En"], OpenMode.ForWrite) as TextStyleTableRecord;
                    str.FileName = "ARIALNBI";
                }
                if (!st.Has("fsdb"))
                {
                    TextStyleTableRecord str2 = new TextStyleTableRecord()
                    {
                        Name = "fsdb",
                        FileName = "fsdb_e.shx",
                        BigFontFileName = "fsdb.shx",
                        XScale = 0.75,
                };
                    ObjectId textstyleid = st.Add(str2);
                    tr.AddNewlyCreatedDBObject(str2, true);
                }
                else
                {
                    TextStyleTableRecord str = tr.GetObject(st["fsdb"], OpenMode.ForWrite) as TextStyleTableRecord;
                    str.FileName = "fsdb_e.shx";
                    str.BigFontFileName = "fsdb.shx";
                    str.XScale = 0.75;
                }
                //-------------------------------------------------------------------------------------------
                DimStyleTable dst = (DimStyleTable)tr.GetObject(acCurDb.DimStyleTableId, OpenMode.ForWrite);
                foreach (int thescale in new int[] { 75, 100,125,150,200 })
                {
                    string scname = "1-" + thescale.ToString();
                    DimStyleTableRecord dstr = new DimStyleTableRecord();
                    if (!dst.Has(scname))
                    {
                        dstr.Name = "1-" + thescale.ToString();
                        dstr.Dimscale = thescale;
                        dstr.Dimtxsty = st["fsdb"];
                        dstr.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 6);
                        dstr.Dimclre = Color.FromColorIndex(ColorMethod.ByAci, 6);
                        dstr.Dimdli = 5.0;
                        dstr.Dimexe = 1.0;
                        dstr.Dimexo = 1.0;
                        dstr.DimfxlenOn = true;
                        dstr.Dimfxlen = 4;
                        dstr.Dimtxt = 2;
                        dstr.Dimasz = 1.5;
                        dstr.Dimtix = true;
                        dstr.Dimtmove = 1;
                        dstr.Dimtad = 1;
                        dstr.Dimgap = 0.8;
                        dstr.Dimdec = 0;
                        dstr.Dimtih = false;
                        dstr.Dimtoh = false;
                        dstr.Dimdsep = '.';
                        dstr.Dimlfac = 0.1;
                        dst.Add(dstr);
                        tr.AddNewlyCreatedDBObject(dstr, true);
                    }
                    else
                    {
                        dstr = tr.GetObject(dst[scname], OpenMode.ForWrite) as DimStyleTableRecord;
                        dstr.Name = "1-" + thescale.ToString();
                        dstr.Dimscale = thescale;
                        dstr.Dimtxsty = st["fsdb"];
                        dstr.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, 6);
                        dstr.Dimclre = Color.FromColorIndex(ColorMethod.ByAci, 6);
                        dstr.Dimdli = 5.0;
                        dstr.Dimexe = 1.0;
                        dstr.Dimexo = 1.0;
                        dstr.DimfxlenOn = true;
                        dstr.Dimfxlen = 4;
                        dstr.Dimtxt = 2;
                        dstr.Dimasz = 1.5;
                        dstr.Dimtix = true;
                        dstr.Dimtmove = 1;
                        dstr.Dimtad = 1;
                        dstr.Dimgap = 0.8;
                        dstr.Dimdec = 0;
                        dstr.Dimtih = false;
                        dstr.Dimtoh = false;
                        dstr.Dimdsep = '.';
                        dstr.Dimlfac = 0.1;
                    }

                }
                //-------------------------------------------------------------------------------------------
                // 自定义块
                //-------------------------------------------------------------------------------------------
                BlockTable bt = (BlockTable)tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = new BlockTableRecord();
                if (!bt.Has("BG"))
                {
                    btr.Name = "BG";
                    bt.UpgradeOpen();
                    bt.Add(btr);
                    tr.AddNewlyCreatedDBObject(btr, true);
                    Polyline Paa = new Polyline()
                    {
                        Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                    };
                    Paa.AddVertexAt(0, new Point2d(0, 0), 0, 0, 200);
                    Paa.AddVertexAt(1, new Point2d(0, 200), 0, 0, 0);
                    btr.AppendEntity(Paa);
                    tr.AddNewlyCreatedDBObject(Paa, true);
                    AttributeDefinition curbg = new AttributeDefinition();
                    curbg.Position = new Point3d(120, 200, 0);
                    curbg.Height = 200;
                    curbg.WidthFactor = 0.75;
                    curbg.Tag = "标高";
                    curbg.TextStyleId = st["fsdb"];
                    btr.AppendEntity(curbg);
                    tr.AddNewlyCreatedDBObject(curbg, true);
                }
                //-------------------------------------------------------------------------------------------
                if (!bt.Has("ZP"))
                {
                    BlockTableRecord btr2 = new BlockTableRecord();
                    btr2.Name = "ZP";
                    bt.UpgradeOpen();
                    bt.Add(btr2);
                    tr.AddNewlyCreatedDBObject(btr2, true);
                    Polyline Paa2 = new Polyline()
                    {
                        Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                    };
                    Paa2.AddVertexAt(0, new Point2d(0 - 350, 0), 0, 0, 80);
                    Paa2.AddVertexAt(1, new Point2d(200 - 350, 0), 0, 0, 0);
                    Paa2.AddVertexAt(2, new Point2d(700 - 350, 0), 0, 0, 0);
                    btr2.AppendEntity(Paa2);
                    tr.AddNewlyCreatedDBObject(Paa2, true);
                    AttributeDefinition curzp = new AttributeDefinition();
                    curzp.Position = new Point3d(220 - 350, 0, 0);
                    curzp.Height = 200;
                    curzp.WidthFactor = 0.75;
                    curzp.Tag = "左坡";
                    curzp.TextStyleId = st["fsdb"];
                    btr2.AppendEntity(curzp);
                    tr.AddNewlyCreatedDBObject(curzp, true);
                }

                //-------------------------------------------------------------------------------------------
                if (!bt.Has("YP"))
                {
                    BlockTableRecord btr3 = new BlockTableRecord();
                    btr3.Name = "YP";
                    bt.UpgradeOpen();
                    bt.Add(btr3);
                    tr.AddNewlyCreatedDBObject(btr3, true);
                    Polyline Paa3 = new Polyline()
                    {
                        Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
                    };
                    Paa3.AddVertexAt(0, new Point2d(0 + 350, 0), 0, 0, 80);
                    Paa3.AddVertexAt(1, new Point2d(-200 + 350, 0), 0, 0, 0);
                    Paa3.AddVertexAt(2, new Point2d(-700 + 350, 0), 0, 0, 0);
                    btr3.AppendEntity(Paa3);
                    tr.AddNewlyCreatedDBObject(Paa3, true);
                    AttributeDefinition curyp = new AttributeDefinition();
                    curyp.Position = new Point3d(-220 + 350, 0, 0);
                    curyp.HorizontalMode = TextHorizontalMode.TextRight;
                    curyp.AlignmentPoint = curyp.Position;
                    curyp.Height = 200;
                    curyp.WidthFactor = 0.75;
                    curyp.Tag = "右坡";
                    curyp.TextStyleId = st["fsdb"];
                    btr3.AppendEntity(curyp);
                    tr.AddNewlyCreatedDBObject(curyp, true);
                }
                //-------------------------------------------------------------------------------------------

                //-------------------------------------------------------------------------------------------
                tr.Commit();
            }
        }
    }
}