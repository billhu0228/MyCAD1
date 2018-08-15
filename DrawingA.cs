using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using MOExcel=Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

[assembly: CommandClass(typeof(MyCAD1.Drawing))]


namespace MyCAD1
{
    public struct DMT
    {
        public double Wz, Wy;
        public double H1, H2, H3;
        public double Ht, Hw;
        public double x0;
        public string pk_string;
        public Polyline dmx, sjx;
    }
    public class Drawing
    {


        public List<DMT> Dmt_list;


        

        




        [CommandMethod("main")]
        public void Main()
        {
            // 基本句柄
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ObjectId paperSpace = db.CreatPaperSpace();
            MOExcel.Application app = new MOExcel.Application();
            MOExcel.Workbook wbook = app.Workbooks.Add(Type.Missing);
            MOExcel.Worksheet wsheet = (MOExcel.Worksheet)wbook.Worksheets[1];

            // 初始化带帽图列表
            string dmtpath = BPublicFunctions.GetXPath("选择带帽图数据","断面图提取文件|*.dat");
            if (dmtpath == "")
            {
                return;
            }
            else
            {
                Dmt_list = IniDMT(dmtpath);
                ed.WriteMessage("\n断面图数据读取成功");
            }



            //读取数据
            Dalot TheDalot;
            System.Data.DataTable Parameters = new System.Data.DataTable();

            string aa = BPublicFunctions.GetXPath("选择设计表", "参数表|*.xls");
            if (aa == "")
            {
                return;
            }
            else
            {
                Parameters = GetDataFromExcelByCom(true, aa);                
                ed.WriteMessage("\n涵洞数据读取成功");
            }

            // 生成绘图id序列           

            var IDtoPolot = Parameters.AsEnumerable().Select(t =>t.Field<int>("id")).ToList();

            //PromptKeywordOptions KO = new PromptKeywordOptions("\n请输入涵洞号");           
            //KO.Keywords.Add("All");
            //KO.Keywords.Add("Single");
            //KO.AllowNone = true;

            //PromptResult pResults = ed.GetKeywords(KO);
            //if (pResults.Status == PromptStatus.OK)
            //{
            //    if (pResults.StringResult == "All")
            //    {
            //        //

            //    }
            //}




            // 实例化并绘图
            foreach(int ii in IDtoPolot)            
            {
                
                // 实例化涵洞
                TheDalot = GetDalotFromDataTable(Parameters, ii);


                // 查询带帽图
                DMT relatedDMT = DmtLookUp(Dmt_list, TheDalot.Pk_string());
                if(relatedDMT.dmx is null)
                {
                    ed.WriteMessage("\n里程"+TheDalot.Pk_string()+"涵洞无断面信息，无法绘制");
                    continue;
                }
                Point3d PointDMT = new Point3d(relatedDMT.x0, 0, 0);
                TheDalot.W1 = relatedDMT.Wz * 1000;
                TheDalot.W2 = relatedDMT.Wy * 1000;
                TheDalot.H1 = relatedDMT.H1;
                TheDalot.H2 = relatedDMT.H2;
                TheDalot.H3 = relatedDMT.H3;
                TheDalot.H0 = relatedDMT.H2 - TheDalot.dH;

                Point3d SJXRefPoint = relatedDMT.sjx.GetClosestPointTo(PointDMT, Vector3d.YAxis, true);
                Point3d DmxRefP = relatedDMT.dmx.GetClosestPointTo(PointDMT, Vector3d.YAxis, true);







                //绘图
                int scaleA = TheDalot.ScaleA;
                int scaleB = TheDalot.ScaleB;
                double[] AreaForTabe = new double[4];

                Extents2d extA;
                extA = TheDalot.PlotA(db, TheDalot.BasePoint);
                double cx = (extA.MaxPoint.X + extA.MinPoint.X) * 0.5;
                double cy = (extA.MaxPoint.Y + extA.MinPoint.Y) * 0.5;
                double halfY = (extA.MaxPoint.Y - extA.MinPoint.Y) * 0.5;
                Point2d centerPoint = Point2d.Origin.Convert2D(cx, extA.MaxPoint.Y - 277 * 0.5 * scaleA + 15 * scaleA);
                double dAB = extA.MinPoint.Y - TheDalot.BasePoint.Y;
                dAB -= 15*scaleA;//图名
                dAB -= 15* scaleA;//顶标注
                dAB -= 15* scaleA; //H2
                dAB -= (TheDalot.H2 - TheDalot.H0) * 1000;
                TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), relatedDMT.sjx, relatedDMT.dmx,
                    SJXRefPoint, DmxRefP);
                AreaForTabe=TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(800* scaleA, dAB));

                double tmpwidth = (TheDalot.FooterW == 0) ? 500 : 2000;
                double dBC = 0.5 * TheDalot.Sect[0]+ TheDalot.FooterW+tmpwidth+ TheDalot.Sect[2] + 1100+5*scaleB;
                double offC = scaleB * 240 * 0.5-73*scaleB;// - dBC;

                Point2d centerPointC = TheDalot.BasePoint.Convert2D(800* scaleA - offC, dAB-22.5*scaleB+TheDalot.Sect[2]+25*scaleB);



                // 成图
                string fsd=Path.Combine(Path.GetDirectoryName(dmtpath), "TK.dwg");
                
                db.XrefAttachAndInsert(fsd, paperSpace, Point3d.Origin.Convert3D(0, 24 - (1 + ii) * 297, 0));
                
                // 注释
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Layout lay = (Layout)tr.GetObject(paperSpace, OpenMode.ForWrite);

                    var vpIds = lay.GetViewports();
                    Viewport vpA, vpB;
                    var btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                    vpA = new Viewport();
                    btr.AppendEntity(vpA);
                    tr.AddNewlyCreatedDBObject(vpA, true);
                    vpA.On = true;
                    vpA.GridOn = false;
                    vpA.DrawMyViewport(1, Point3d.Origin.Convert3D(0, -(1 + ii) * 297, 0), centerPoint, TheDalot.ScaleA);
                    vpB = new Viewport();
                    btr.AppendEntity(vpB);
                    tr.AddNewlyCreatedDBObject(vpB, true);
                    vpB.On = true;
                    vpB.GridOn = false;
                    vpB.DrawMyViewport(2, Point3d.Origin.Convert3D(0, -(1 + ii) * 297, 0), centerPointC, TheDalot.ScaleB);

                    TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                    MText theNote = new MText();
                    MText theNoteZH = new MText();
                    if (TheDalot.DalotType <= DType.B)
                    {
                        theNote.Contents =
                            "Note:\\P" +
                            "1.L'unité de dimension est en centimètre sauf que les cotes. \\P" +
                            "2.Le corps du dalot est préfabriqué en segment et la tête d'ouvrage est coulés sur place. Le joint est  mis en place entre différents parties. \\P" +
                            "3.Explication pour joint et couche d'étanchéité : \\P" +
                            "   Joint: Le joint est rempli par bitume et filasse ou par autre matériaux d'étanchéité élastique. Le pied-droit et tablier adoptent deux couches " +
                            "de feutre bitumé coller dessus le joint par bitume, la largeur de feutre bitumé est de 40cm. Le feutre bitumé est collé dessus le gâchis hydrofuge si le tableau est couvert par le gâchis hydrofuge.\\P" +
                            "   Couche d'étanchéité: Pour hauteur de remblai plus de 0.5m, il faut enduire bitume avec épaisseur de 1~1.5mm deux  fois pour les partie enterrés du dalot; Pour H.remblai moins de 0.5m, " +
                            "il faut d'abord mettre 2cm de  mortier hydrofuge sur tablier et puis enduire bitume avec épaisseur de 1~1.5mm deux fois pour les  partie enterrés du dalot. \\P" +
                            "4.Vérifier la conformabilité entre les plans et terrain réel avant l'exécution du travaux. Il faut informer le bureau d'étude à réviser la cote s'il existe différence évidente.\\P" +
                            "5.Annuler l'enrochement d'exutoire du dalot si sa fondation est en roche. Après l'achèvement du dalot, il faut remplir ou creuser l'amont et l'aval du dalot pour assurer l'évacuation d'eau.\\P";
                        theNote.Location = Point3d.Origin.Convert3D(260, 100 - (1 + ii) * 297, 0);

                        theNoteZH.Contents =
                            "注：\\P" +
                            "1.本图尺寸除高程以米计外，其余均以厘米为单位。\\P" +
                            "2.涵身采用节段预制施工，进出水口构造采用现浇施工，涵身之间及涵身与进出口构造之间设置接缝。\\P" +
                            "3.涵洞接缝及防水层规定：\\P" +
                            "  接缝：涵身接缝及涵身与进出水口构造接缝采用沥青麻絮或其他有弹性的防水材料填塞,侧面及顶面采用两层油毛毡以沥青粘贴于涵身接缝位置，油毛毡宽度40cm。对于涵顶有防水砂浆罩面的情况，油毛毡黏贴于防水砂浆之上。\\P" +
                            "  防水层：当填土高度大于等于0.5m时，涵洞与填土接触部分涂抹热沥青两道，每道厚度约1~1.5mm；当填土高度小于0.5m时，涵洞顶部先采用2cm防水砂浆罩面，然后在涵洞与填土接触部分涂抹热沥青两道，每道厚度约1~1.5mm。\\P" +
                            "4.施工前认真核查设计图纸与实际地形是否吻合，如有较大差别及时与设计单位联系，调整涵洞标高。\\P" +
                            "5.如涵洞出口处地质为岩石，可取消出口处防冲碎石；涵洞施工完后，需对进出水口附近地面做适当挖填，以保证涵洞排水通畅。\\P";
                        theNoteZH.Location = Point3d.Origin.Convert3D(43, 68 - (1 + ii) * 297, 0);
                    }
                    else
                    {
                        theNote.Contents =
                            "Note:\\P" +
                            "1.L'unité de dimensions est en mètre pour altitude et en centimètre pour les autres. \\P" +
                            "2.Le dalot est fabriqué en béton coulé sur place sans joint dilatation pour le corps. \\P" +
                            "3.Enduire bitume avec  épaisseur de 1~1.5mm deux fois entre dalot et terre de remblai. \\P" +
                            "4.Vérifier l'identité entre les plans et terrain réel avant l'exécution de travaux. Il faut contacter  les bureaux d'étude à réviser l'altitude s'il existe différence évidente. \\P" +
                            "5.Après finir l'exécution de travaux, il faut remplir ou creuser l'amont et l'aval de dalot pour  assurer libre évacuation d'eau. \\P";
                        theNote.Location = Point3d.Origin.Convert3D(260, 80 - (1 + ii) * 297, 0);


                        theNoteZH.Contents =
                            "注：\\P" +
                            "1.本图尺寸除高程以米计外，其余均以厘米为单位。\\P" +
                            "2.涵洞采用现浇施工，基础岩石裸露，涵身不设置沉降缝。\\P" +
                            "3.在涵洞与填土接触部分涂抹热沥青两道，每道厚度约1~1.5mm。\\P" +
                            "4.施工前认真核查设计图纸与实际地形是否吻合，如有较大差别及时与设计单位联系，调整涵洞标高。\\P" +
                            "5.涵洞施工完后，需对进出水口附近地面做适当挖填，以保证涵洞排水通畅。\\P";
                        theNoteZH.Location = Point3d.Origin.Convert3D(43, 68 - (1 + ii) * 297, 0);
                    }
                    
                    theNote.TextStyleId = st["En"];
                    theNote.Width = 145;
                    btr.AppendEntity(theNote);
                    tr.AddNewlyCreatedDBObject(theNote, true);
                    theNoteZH.TextStyleId = st["fsdb"];
                    theNoteZH.Width = 245;
                    btr.AppendEntity(theNoteZH);
                    tr.AddNewlyCreatedDBObject(theNoteZH, true);


                    tr.Commit();
                }


                //  表格

                TextPloter.PrintTable(db, Point3d.Origin.Convert3D(0,-(ii+1)*297),TheDalot,relatedDMT, Parameters, AreaForTabe,wsheet,ii+4);
                // 图名图号
                TextPloter.PrintNumTitle(db, Point3d.Origin.Convert3D(0, -(ii + 1) * 297),TheDalot );

            }


            // 制作表头 储存汇总数量表

            wsheet.Cells[1, 1] = "No";
            MOExcel.Range range = wsheet.get_Range("A1", "A4"); 
            range.Merge(0);
            wsheet.Cells[1, 2] = "PK";
            range = wsheet.get_Range("B1", "B4");
            range.Merge(0);
            wsheet.Cells[1, 3] = "Type\nd'ouvrage";
            range = wsheet.get_Range("C1", "C4");
            range.Merge(0);
            wsheet.Cells[1, 4] = "Dimension";
            range = wsheet.get_Range("D1", "D3");
            range.Merge(0);
            wsheet.Cells[4, 4] = "m";
            wsheet.Cells[1, 5] = "Biais";
            range = wsheet.get_Range("E1", "E3");
            range.Merge(0);
            wsheet.Cells[4, 5] = "°";
            wsheet.Cells[1, 6] = "L.";
            range = wsheet.get_Range("F1", "F3");
            range.Merge(0);
            wsheet.Cells[4, 6] = "m";
            wsheet.Cells[1, 7] = "Type d'éntrée et de\nsortie";
            range = wsheet.get_Range("G1", "H2");
            range.Merge(0);
            wsheet.Cells[3, 7] = "Amont";
            range = wsheet.get_Range("G3", "G4");
            range.Merge(0);
            wsheet.Cells[3, 8] = "Aval";
            range = wsheet.get_Range("H3", "H4");
            range.Merge(0);
            wsheet.Cells[2, 9] = "Corps";
            range = wsheet.get_Range("I2", "K2");
            range.Merge(0);
            wsheet.Cells[3,9] = "Béton\n(C25/30)";
            wsheet.Cells[4, 9] = "m3";
            wsheet.Cells[4, 9].Characters[2,1].Font.Superscript = true;
            wsheet.Cells[3, 10] = "Armature\n(FeE 400)";
            wsheet.Cells[4, 10] = "kg";
            wsheet.Cells[3, 11] = "Quantité de segment de\nprefabriquation,transport et\nlevage";
            wsheet.Cells[4, 11] = "bloc";
            wsheet.Cells[2, 12] = "Entrée et sortie";
            range = wsheet.get_Range("L2", "O2");
            range.Merge(0);
            wsheet.Cells[3, 12] = "Puit déau\n(C25/30)";
            wsheet.Cells[4, 12] = "m3";
            wsheet.Cells[4, 12].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 13] = "Mur en aile\n(C25/30)";
            wsheet.Cells[4, 13] = "m3";
            wsheet.Cells[4, 13].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 14] = "Guide roue\n(C25/30)";
            wsheet.Cells[4, 14] = "m3";
            wsheet.Cells[4, 14].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 15] = "Armature\n(FeE 400)";
            wsheet.Cells[4, 15] = "kg";
            wsheet.Cells[2, 16] = "Foundation";
            range = wsheet.get_Range("P2", "Q2");
            range.Merge(0);
            wsheet.Cells[3, 16] = "Béton\n(C25/30)";
            wsheet.Cells[4, 16] = "m3";
            wsheet.Cells[4, 16].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 17] = "Graveleux\nlatérique";
            wsheet.Cells[4, 17] = "m3";
            wsheet.Cells[4, 17].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[2, 18] = "Etanchéité";
            range = wsheet.get_Range("R2", "T2");
            range.Merge(0);
            wsheet.Cells[3, 18] = "Badigeonnage\ndes parements";
            wsheet.Cells[4, 18] = "m2";
            wsheet.Cells[4, 18].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 19] = "Motier\nhydro";
            wsheet.Cells[4, 19] = "m2";
            wsheet.Cells[4, 19].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 20] = "Joint";
            wsheet.Cells[4, 20] = "m2";
            wsheet.Cells[4, 20].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[2, 21] = "Terrassement";
            range = wsheet.get_Range("U2", "W2");
            range.Merge(0);
            wsheet.Cells[3, 21] = "Déblai";
            wsheet.Cells[4, 21] = "m3";
            wsheet.Cells[4, 21].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 22] = "Remblaiement au\ndos des ponceaux";
            wsheet.Cells[4, 22] = "m3";
            wsheet.Cells[4, 22].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[3, 23] = "Enrochement";
            wsheet.Cells[4, 23] = "m3";
            wsheet.Cells[4, 23].Characters[2, 1].Font.Superscript = true;
            wsheet.Cells[1, 9] = "Quantité des travaux";
            range = wsheet.get_Range("I1", "W1");
            range.Merge(0);

            wsheet.Columns.EntireColumn.AutoFit();
            wsheet.Cells.HorizontalAlignment = MOExcel.XlHAlign.xlHAlignCenter;//水平居中  
            wsheet.Cells.VerticalAlignment = MOExcel.XlVAlign.xlVAlignCenter;//垂直居中  

            string bb =Path.Combine(Path.GetDirectoryName(aa), "数量汇总表");          
            wsheet.SaveAs(bb, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            wbook.Close(false, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(wbook);
            wbook = null;
            app.Workbooks.Close();
            app.KillExcelApp();
        }








        public Dalot GetDalotFromDataTable(System.Data.DataTable theDT, int DalotId)
        {
            var results = from myRow in theDT.AsEnumerable()
                          where myRow.Field<int>("id") == DalotId
                          select myRow;
            var theData = results.First();
            string Pk = (string)theData["pk"];
            double Ang = 90.0 - (double)theData["Ang"];
            double Slop = (double)theData["slop"];
            double Length = (double)theData["Length"] * 1000.0;
            double SegLength = (double)theData["SegLength"] * 1000.0;
            double XMidDist = (double)theData["XMidLength"] * 1000.0;
            int no = DalotId;
            double H0 = (double)theData["H0"];
            double c = (double)theData["c"];
            double w = (double)theData["w"];
            double h = (double)theData["h"];
            string design = (string)theData["DesignNo"];
            DType theType = DType.A;
            if (c == 1)
            {
                if (w == 1.5)
                {
                    theType = DType.A;
                }
                else if (w == 2.0)
                {
                    theType = DType.B;
                }
                else if (w == 4.0)
                {
                    if (h == 2.0)
                    {
                        theType = DType.C;
                    }
                    else
                    {
                        theType = DType.D;
                    }
                }
            }
            else if (c == 2)
            {
                //两孔
                if (w == 2.0)
                {
                    theType = DType.F;
                }
                else if (w == 3.0)
                {
                    theType = DType.G;
                }
            }
            else
            {
                theType = DType.A;
            }

            AType cAtype = AType.BZQ;
            string amonttype = (string)theData["Amont"];
            if (amonttype == "八字墙")
            {
                cAtype = AType.BZQ;
            }
            else if (amonttype == "集水井")
            {
                cAtype = AType.JSJ;
            }

            bool cistri = true;

            //public AType Amont, Avale;
            //public DType DalotType;
            Point2d BasePoint = new Point2d(0, no * -50000);
            double LayerThick = (double)theData["LayerThick"];
            double LayerWidth = (double)theData["LayerWidth"];
            int sA = (int)(double)theData["ScaleA"];
            int sB = (int)(double)theData["ScaleB"];

            Dalot res = new Dalot(Dalot.PkString2Double(Pk), Ang, Slop, Length, SegLength, XMidDist, theType, cAtype, BasePoint, LayerThick, LayerWidth, H0, sA, sB, cistri, design);
            return res;
        }


        public static System.Data.DataTable GetDataFromExcelByCom(bool hasTitle, string fileName)
        {
            MOExcel.Application app = new MOExcel.Application();
            MOExcel.Sheets sheets;
            object oMissiong = System.Reflection.Missing.Value;
            MOExcel.Workbook workbook = null;
            System.Data.DataTable dt = new System.Data.DataTable();

            try
            {
                if (app == null) return null;
                workbook = app.Workbooks.Open(fileName, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong,
                    oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong);
                sheets = workbook.Worksheets;

                //将数据读入到DataTable中
                MOExcel.Worksheet worksheet = (MOExcel.Worksheet)sheets.get_Item(1);//读取第一张表 
                if (worksheet == null) return null;

                int iRowCount = worksheet.UsedRange.Rows.Count;
                int iColCount = worksheet.UsedRange.Columns.Count;
                //生成列头
                for (int i = 0; i < iColCount; i++)
                {
                    var name = "column" + i;
                    if (hasTitle)
                    {
                        var txt = ((MOExcel.Range)worksheet.Cells[1, i + 1]).Text.ToString();
                        if (!string.IsNullOrEmpty(txt)) name = txt;
                    }
                    while (dt.Columns.Contains(name)) name = name + "_1";//重复行名称会报错。
                    if (name == "pk" || name == "Amont"||name=="DesignNo")
                    {
                        dt.Columns.Add(new System.Data.DataColumn(name, typeof(string)));
                    }
                    else if (name == "id")
                    {
                        dt.Columns.Add(new System.Data.DataColumn(name, typeof(int)));
                    }
                    else
                    {
                        dt.Columns.Add(new System.Data.DataColumn(name, typeof(double)));
                    }
                    
                    
                }
                //生成行数据
                MOExcel.Range range;
                int rowIdx = hasTitle ? 2 : 1;
                for (int iRow = rowIdx; iRow <= iRowCount; iRow++)
                {
                    DataRow dr = dt.NewRow();
                    for (int iCol = 1; iCol <= iColCount; iCol++)
                    {
                        range = (MOExcel.Range)worksheet.Cells[iRow, iCol];
                        dr[iCol - 1] = (range.Value2 == null) ? "" : range.Text.ToString();
                    }
                    dt.Rows.Add(dr);
                }



                return dt;
            }
            catch { return dt; }
            finally
            {
                workbook.Close(false, oMissiong, oMissiong);
                Marshal.ReleaseComObject(workbook);
                workbook = null;
                app.Workbooks.Close();                
                app.KillExcelApp();
            }
        }



        private List<DMT> IniDMT(string filepath)
        {
            List<DMT> result = new List<DMT>();



            StreamReader sR = File.OpenText(filepath);
            string nextLine;
            DMT item = new DMT();
            int i = 0;
            while (true)
            {
                i++;
                nextLine = sR.ReadLine();
                if (nextLine == null)
                {
                    result.Add(item);
                    break;
                }
                else if (nextLine.StartsWith("#"))
                {
                    if (i != 1)
                    {
                        result.Add(item);
                        item = new DMT();
                        item.Ht = 0;
                        item.Hw = 0;
                    }
                    continue;
                }
                else if (nextLine.StartsWith("Hw"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.Hw = Convert.ToDouble(matches[0].Value);
                    item.Wz = Convert.ToDouble(matches[1].Value);
                    item.Wy = Convert.ToDouble(matches[2].Value);
                }

                else if (nextLine.StartsWith("Ht"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.Ht = Convert.ToDouble(matches[0].Value);
                    item.Wz = Convert.ToDouble(matches[1].Value);
                    item.Wy = Convert.ToDouble(matches[2].Value);
                }
                else if (nextLine.StartsWith("PK") || nextLine.StartsWith("K"))
                {
                    item.pk_string = nextLine;
                }
                else if (nextLine.StartsWith("H0"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.H1 = Convert.ToDouble(matches[1].Value);
                }
                else if (nextLine.StartsWith("H1"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.H2 = Convert.ToDouble(matches[1].Value);
                }
                else if (nextLine.StartsWith("H2"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.H3 = Convert.ToDouble(matches[1].Value);
                }
                else if (nextLine.StartsWith("sjx"))
                {
                    item.sjx = GetPLFromFile(filepath, i);
                }
                else if (nextLine.StartsWith("dmx"))
                {
                    item.dmx = GetPLFromFile(filepath, i);
                }
                else if (nextLine.StartsWith("X"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.x0 = Convert.ToDouble(matches[0].Value);
                }
            }
            sR.Close();
            return result;
        }

        private Polyline GetPLFromFile(string filePath,int lineInx)
        {
            StreamReader sr = File.OpenText(filePath);
            Polyline res = new Polyline();
            Point2d tmp;
            string nextLine;            
            int i = 0;
            int jj = 0;
            while ((nextLine = sr.ReadLine()) != null)
            {
                i++;
                if (i <= lineInx)
                {
                    continue;
                }
                else
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    if (matches.Count !=2 && matches.Count != 3)
                    {                        
                        break;
                    }
                    else
                    {
                        double x = Convert.ToDouble(matches[0].Value);
                        double y = Convert.ToDouble(matches[1].Value);
                        tmp = new Point2d(x, y);
                        res.AddVertexAt(jj, tmp, 0, 0, 0);
                        jj++;
                    }
                }
            }
            sr.Close();
            return res;
        }


        private DMT DmtLookUp(List<DMT> repo,string pk)
        {
            DMT res = new DMT();
            double pk_double = Dalot.PkString2Double(pk);
            foreach (DMT item in repo)
            {
                double target = Dalot.PkString2Double(item.pk_string);
                if (Dalot.PkString2Double(item.pk_string) == Dalot.PkString2Double(pk))
                {
                    res = item;
                }
            }
            return res;
        }



    }
}
