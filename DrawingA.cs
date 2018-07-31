﻿using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Drawing
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
        public List<DMT> Dmt_list;


        

        




        [CommandMethod("main")]
        public void Main()
        {
            // 基本句柄
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ObjectId paperSpace = db.CreatPaperSpace();
            //Transaction tr = db.TransactionManager.StartTransaction();

            // 初始化带帽图列表
            string dmtpath = BPublicFunctions.GetXPath("选择带帽图数据","断面图提取文件|*.dat");
            Dmt_list = IniDMT(dmtpath);           



            //读取数据
            Dalot TheDalot = new Dalot();
            System.Data.DataTable Parameters = new System.Data.DataTable();
            test:
            string aa = BPublicFunctions.GetXPath("选择设计表");
            if (aa == "")
            {
                goto test;
            }
            else
            {
                Parameters = GetDataFromExcelByCom(true, aa);
                
                ed.WriteMessage("\n涵洞数据读取成功");
            }




            // 实例化并绘图

            for (int ii = 0; ii < Parameters.Rows.Count; ii++)
            {
                // 实例化涵洞
                TheDalot = GetDalotFromDataTable(Parameters, ii);
                // 查询带帽图
                DMT relatedDMT = DmtLookUp(Dmt_list, TheDalot.Pk_string());
                Point3d PointDMT = new Point3d(relatedDMT.x0, 0, 0);
                TheDalot.W1 = relatedDMT.Wy * 1000;
                TheDalot.W2 = relatedDMT.Wz * 1000;
                TheDalot.H1 = relatedDMT.H1;
                TheDalot.H2 = relatedDMT.H2;
                TheDalot.H3 = relatedDMT.H3;
                if (TheDalot.H0 == 0)
                {
                    if (relatedDMT.Hw != 0)
                    {
                        TheDalot.H0 = TheDalot.H2 - 2 * TheDalot.Sect[2] / 1000;
                    }
                    else
                    {
                        TheDalot.H0 = TheDalot.H2 - relatedDMT.Ht - TheDalot.Sect[2] / 1000;
                    }
                }

                Point3d SJXRefPoint = relatedDMT.sjx.GetClosestPointTo(PointDMT, Vector3d.YAxis, true);
                Point3d DmxRefP = relatedDMT.dmx.GetClosestPointTo(PointDMT, Vector3d.YAxis, true);



                //绘图
                int scaleA = TheDalot.ScaleA;
                int scaleB = TheDalot.ScaleB;

                Extents2d extA;
                extA = TheDalot.PlotA(db, TheDalot.BasePoint);
                double cx = (extA.MaxPoint.X + extA.MinPoint.X) * 0.5;
                double cy = (extA.MaxPoint.Y + extA.MinPoint.Y) * 0.5;
                double halfY = (extA.MaxPoint.Y - extA.MinPoint.Y) * 0.5;
                Point2d centerPoint = Point2d.Origin.Convert2D(cx, extA.MaxPoint.Y - 277 * 0.5 * scaleA + 15 * scaleA);
                double dAB = extA.MinPoint.Y - TheDalot.BasePoint.Y;
                dAB -= 1500;//图名
                dAB -= 1500;//顶标注
                dAB -= 1500; //H2
                dAB -= (TheDalot.H2 - TheDalot.H0) * 1000;
                TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), relatedDMT.sjx, relatedDMT.dmx,
                    SJXRefPoint, DmxRefP);
                TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(30000, dAB));


                // 成图

                db.XrefAttachAndInsert(@"G:\涵洞自动成图程序\TK.dwg", paperSpace, Point3d.Origin.Convert3D(0, 24 - (1 + ii) * 297, 0));

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Layout lay = (Layout)tr.GetObject(paperSpace, OpenMode.ForWrite);
                    //lay.ApplyToViewport(tr, 1 + ii, vp => { vp.DrawMyViewport(1, Point3d.Origin.Convert3D(0, -ii * 297, 0), Point2d.Origin, 100); vp.Locked = true; });
                    //lay.ApplyToViewport(tr, 1 + ii, vp => { vp.DrawMyViewport(2, Point3d.Origin.Convert3D(0, -ii * 297, 0), Point2d.Origin, 75); vp.Locked = true; });
                    var vpIds = lay.GetViewports();
                    Viewport vpA, vpB;
                    var btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                    vpA = new Viewport();
                    btr.AppendEntity(vpA);
                    tr.AddNewlyCreatedDBObject(vpA, true);
                    vpA.On = true;
                    vpA.GridOn = false;
                    vpA.DrawMyViewport(1, Point3d.Origin.Convert3D(0, -(1 + ii) * 297, 0), centerPoint, 100);
                    vpB = new Viewport();
                    btr.AppendEntity(vpB);
                    tr.AddNewlyCreatedDBObject(vpB, true);
                    vpB.On = true;
                    vpB.GridOn = false;
                    vpB.DrawMyViewport(2, Point3d.Origin.Convert3D(0, -(1 + ii) * 297, 0), Point2d.Origin, 100);


                    MText theNote = new MText();
                    theNote.Contents = "Note:" +
                        "1.L'unité de dimension est en centimètre sauf que les cotes. \\P" +
                        "2.Le corps du dalot est préfabriqué en segment et la tête d'ouvrage est coulés sur place. Le joint est  mis en place entre différents parties. \\P" +
                        "3.Explication pour joint et couche d'étanchéité : \\P" +
                        "   Joint: Le joint est rempli par bitume et filasse ou par autre matériaux d'étanchéité élastique. Le pied-droit et tablier adoptent deux couches " +
                        "de feutre bitumé coller dessus le joint par bitume, la largeur de feutre bitumé est de 40cm. Le feutre bitumé est collé dessus le gâchis hydrofuge si le tableau est couvert par le gâchis hydrofuge.\\P" +
                        "   Couche d'étanchéité: Pour hauteur de remblai plus de 0.5m, il faut enduire bitume avec épaisseur de 1~1.5mm deux  fois pour les partie enterrés du dalot; Pour H.remblai moins de 0.5m, " +
                        "il faut d'abord mettre 2cm de  mortier hydrofuge sur tablier et puis enduire bitume avec épaisseur de 1~1.5mm deux fois pour les  partie enterrés du dalot. \\P" +
                        "4.Vérifier la conformabilité entre les plans et terrain réel avant l'exécution du travaux. Il faut informer le bureau d'étude à réviser la cote s'il existe différence évidente.\\P" +
                        "5.Annuler l'enrochement d'exutoire du dalot si sa fondation est en roche. Après l'achèvement du dalot, il faut remplir ou creuser l'amont et l'aval du dalot pour assurer l'évacuation d'eau.\\P";
                    theNote.Location = Point3d.Origin.Convert3D(40, 40 - (1 + ii) * 297, 0);

                    btr.AppendEntity(theNote);
                    tr.AddNewlyCreatedDBObject(theNote, true);

                    tr.Commit();
                }





                //break;





            }












            //ObjectId paperSpace=TheDalot.CreatPaperSpace(db, ed, new int[] { 100, 100, 75 });


        }



        public  Dalot GetDalotFromDataTable(System.Data.DataTable theDT,int rowIndex)
        {
            string Pk=(string)theDT.Rows[rowIndex]["pk"];
            
            double Ang=90.0-double.Parse((string)theDT.Rows[rowIndex]["Ang"]);
            double Slop = double.Parse((string)theDT.Rows[rowIndex]["slop"]);
            double Length = double.Parse((string)theDT.Rows[rowIndex]["Length"])*1000.0;
            double SegLength = double.Parse((string)theDT.Rows[rowIndex]["SegLength"])*1000.0;
            double XMidDist = double.Parse((string)theDT.Rows[rowIndex]["XMidLength"])*1000.0;
            int no= int.Parse((string)theDT.Rows[rowIndex]["id"]);
            double H0 = double.Parse((string)theDT.Rows[rowIndex]["H0"]);
            int c= int.Parse((string)theDT.Rows[rowIndex]["c"]);
            double w= double.Parse((string)theDT.Rows[rowIndex]["w"]);
            double h = double.Parse((string)theDT.Rows[rowIndex]["h"]);
            DType theType=DType.A;
            if (c==1)
            {
                if (w == 1.5)
                {
                    theType = DType.A;
                }
                else if(w==2.0)
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
            else if(c==2)
            {
                //两孔
                if (w == 2.0)
                {
                    theType = DType.F;
                }
                else if(w==3.0)
                {
                    theType = DType.G;
                }                
            }
            else
            {
                theType = DType.A;
            }
            //public AType Amont, Avale;
            //public DType DalotType;
            Point2d BasePoint=new Point2d(0,no*-50000);            
            int LayerNum= int.Parse((string)theDT.Rows[rowIndex]["layerNum"]);

            int sA= int.Parse((string)theDT.Rows[rowIndex]["ScaleA"]);
            int sB = int.Parse((string)theDT.Rows[rowIndex]["ScaleB"]);
            Dalot res = new Dalot(PkString2Double(Pk), Ang, Slop, Length, SegLength, XMidDist, theType, AType.JSJ, AType.BZQ, BasePoint, LayerNum,H0,sA,sB);
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
                    dt.Columns.Add(new System.Data.DataColumn(name, typeof(string)));
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
            catch { return null; }
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
            double pk_double = PkString2Double(pk);
            foreach (DMT item in repo)
            {
                double target = PkString2Double(item.pk_string);
                if (PkString2Double(item.pk_string) == PkString2Double(pk))
                {
                    res = item;
                }
            }
            return res;
        }


        private double PkString2Double(string pks)
        {
            if (pks == null)
            {
                return 0;
            }            

            MatchCollection matches = Regex.Matches(pks, @"(\d+\.?\d*)");
            if (matches.Count == 1)
            {
                return Convert.ToDouble(matches[0].Value);
            }
            else if (matches.Count == 2)
            {
                return Convert.ToDouble(matches[0].Value)*1000+ Convert.ToDouble(matches[1].Value); 
            }
            else
            {
                return 0;
            }                
        }
    }
}
