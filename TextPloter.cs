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
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using MOExcel = Microsoft.Office.Interop.Excel;

namespace MyCAD1
{
    class TextPloter
    {


        public static void PrintNumTitle(Database db, Point3d PaperOrigenPoint,Dalot curDalot)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord recorder = tr.GetObject(blockTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                for (int i=0; i <= 1; i++)
                {
                    DBText num = new DBText();
                    num.TextString = "1";
                    num.Height = 2.5;
                    if (i == 0)
                    {
                        num.Position = PaperOrigenPoint.Convert3D(394.817, 283.25);
                    }
                    else
                    {
                        num.Position = PaperOrigenPoint.Convert3D(404.939, 283.25);
                    }                    
                    num.HorizontalMode = TextHorizontalMode.TextCenter;
                    num.VerticalMode = TextVerticalMode.TextVerticalMid;
                    num.AlignmentPoint = num.Position;
                    num.Layer = "标注";
                    num.TextStyleId = st["En"];
                    num.WidthFactor = 0.85;
                    recorder.AppendEntity(num);
                    tr.AddNewlyCreatedDBObject(num, true);
                }

                // 标题
                DBText title = new DBText();
                if (curDalot.DalotType == DType.A)
                {
                    title.TextString = "PLAN DE COFFRAGE DU DALOT (1-1.5×1.0m)";
                }
                else if (curDalot.DalotType == DType.B)
                {
                    title.TextString = "PLAN DE COFFRAGE DU DALOT (1-2.0×1.5m)";
                }
                else if(curDalot.DalotType == DType.C)
                {
                    title.TextString = "PLAN DE COFFRAGE DU DALOT (1-4.0×2.0m)";
                }
                else if(curDalot.DalotType == DType.D)
                {
                    title.TextString = "PLAN DE COFFRAGE DU DALOT (1-4.0×3.0m)";
                }
                else
                {
                    return;
                }
                title.Height = 3.5;
                title.Position = PaperOrigenPoint.Convert3D(254.5391, 21.4013);                
                title.HorizontalMode = TextHorizontalMode.TextCenter;
                title.VerticalMode = TextVerticalMode.TextVerticalMid;
                title.AlignmentPoint = title.Position;
                title.Layer = "标注";
                title.TextStyleId = st["En"];
                title.WidthFactor = 0.8;
                recorder.AppendEntity(title);
                tr.AddNewlyCreatedDBObject(title, true);

                title = new DBText();
                title.TextString =curDalot.Pk_string();
                title.Height = 3.5;
                title.Position = PaperOrigenPoint.Convert3D(254.5391,15.2987);
                title.HorizontalMode = TextHorizontalMode.TextCenter;
                title.VerticalMode = TextVerticalMode.TextVerticalMid;
                title.AlignmentPoint = title.Position;
                title.Layer = "标注";
                title.TextStyleId = st["En"];
                title.WidthFactor = 0.8;
                recorder.AppendEntity(title);
                tr.AddNewlyCreatedDBObject(title, true);

                //  图号
                DBText dn = new DBText();
                dn.TextString = curDalot.DesignNo;
                dn.Height = 2.5;
                dn.Position = PaperOrigenPoint.Convert3D(371,14);
                dn.HorizontalMode = TextHorizontalMode.TextCenter;
                dn.VerticalMode = TextVerticalMode.TextVerticalMid;
                dn.AlignmentPoint = dn.Position;
                dn.Layer = "标注";
                dn.TextStyleId = st["En"];
                dn.WidthFactor = 0.8;
                recorder.AppendEntity(dn);
                tr.AddNewlyCreatedDBObject(dn, true);





                tr.Commit();
            }
            return;
        }


        public static void PrintNote(Database db, ObjectId recderID, Point3d InsertPoint)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord recorder = tr.GetObject(recderID, OpenMode.ForWrite) as BlockTableRecord;
                MText theNote=new MText();
                theNote.Contents = "Some text in the default colour...\\P" +
                    "{\\C1;Something red}\\P" +
                    "{\\C2;Something yellow}\\P" +
                    "{\\C3;And} {\\C4;something} " +
                    "{\\C5;multi-}{\\C6;coloured}\\P";
                theNote.Location = InsertPoint;
                recorder.AppendEntity(theNote);
                tr.AddNewlyCreatedDBObject(theNote, true);
                tr.Commit();
            }
            return;
        }

        public static void PrintTable(Database db,  Point3d PaperOrigenPoint,Dalot curDatlotObj,DMT curDMT,System.Data.DataTable curParas,double [] AreaOfSection,
            MOExcel.Worksheet worksheet,int LineIdx)
        {
            double x0 = 284;
            double y_anchor = 267;

            double y0=0;
            double x1 = x0+106;
            double t1 = x0+24;
            double t2 = t1+38;
            double t3 = t2+17;
            double t4 = t3 +16;

            
            int[] num23 = curDatlotObj.GetSegNum();
            var results = from myRow in curParas.AsEnumerable()
                          where Dalot.PkString2Double(myRow.Field<string>("pk")) ==  curDatlotObj.Pk
                          select myRow;
            var theData = results.First();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord recorder = tr.GetObject(blockTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                DBText theTitle = new DBText();
                theTitle.TextString = "TABLEAU QUANTITATIF DES DALOT "+curDatlotObj.Pk_string();
                theTitle.Height = 3.5;
                theTitle.Position = PaperOrigenPoint.Convert3D((x0+x1)*0.5, y_anchor);
                theTitle.HorizontalMode =TextHorizontalMode.TextCenter;
                theTitle.VerticalMode = TextVerticalMode.TextBottom;                
                theTitle.AlignmentPoint = theTitle.Position;
                theTitle.Layer = "标注";
                theTitle.TextStyleId = st["En"];
                theTitle.WidthFactor = 0.85;
                recorder.AppendEntity(theTitle);
                tr.AddNewlyCreatedDBObject(theTitle, true);

                DBText theTitleZh = new DBText();
                theTitleZh.TextString = "涵洞材料数量表";
                theTitleZh.Height = 5;
                theTitleZh.Position = PaperOrigenPoint.Convert3D((x0 + x1) * 0.5, y_anchor+6);
                theTitleZh.HorizontalMode = TextHorizontalMode.TextCenter;
                theTitleZh.VerticalMode = TextVerticalMode.TextBottom;
                theTitleZh.AlignmentPoint = theTitleZh.Position;
                theTitleZh.Layer = "标注";
                theTitleZh.TextStyleId = st["fsdb"];
                theTitleZh.WidthFactor = 0.85;
                // 取消中文
                //recorder.AppendEntity(theTitleZh);
                //tr.AddNewlyCreatedDBObject(theTitleZh, true);

                Dictionary<int,string[]> table = new Dictionary<int, string[]>();

                double Conc = 0;
                double Steel = 0;
                int totalSegNum = 0;
                double bzqL=0, jsjL=0;
                double SectA = 0;
                double YiGeXiaoSanJiao = 0;
                double BZQfangshuiceng = 0;
                double JSJfangshuiceng = 0;


                if (curDatlotObj.DalotType == DType.A)
                {
                    Conc=2.0*num23[0]*1.18915+3.0*num23[1]*1.18915;
                    Steel = num23[0] *(double)theData["2m钢筋量"] + num23[1] * (double)theData["3m钢筋量"];
                    totalSegNum = num23[0] + num23[1];
                    bzqL = 2.000;
                    jsjL = 1.850;
                    SectA = 1.18915;
                    YiGeXiaoSanJiao = 2 * 2 * Math.Tan(30.0 / 180.0 * Math.PI)*0.5;
                    BZQfangshuiceng = 3.8;
                    JSJfangshuiceng = 7.84;
                }
                else if (curDatlotObj.DalotType == DType.B)
                {
                    Conc = curDatlotObj.Length/1000 * 2.035;
                    Steel = (double)theData["节段钢筋量"] * num23[0];
                    totalSegNum = num23[0];
                    bzqL = 2.825;
                    jsjL = 1.900;
                    SectA = 2.035;
                    YiGeXiaoSanJiao = 2.82 * 2.82 * Math.Tan(30.0 / 180.0 * Math.PI) * 0.5;
                    BZQfangshuiceng = 7;
                    JSJfangshuiceng = 12.4;
                }
                else if(curDatlotObj.DalotType==DType.C)
                {
                    Conc = curDatlotObj.Length / 1000 * 5.5379;
                    Steel = (double)theData["现浇钢筋总量"];
                    totalSegNum = num23[0];
                    bzqL = 3.6;
                    jsjL = 0;
                    SectA = 5.5379;
                    YiGeXiaoSanJiao = 3.6 * 3.6 * Math.Tan(30.0 / 180.0 * Math.PI) * 0.5;
                    BZQfangshuiceng = 14.965;
                }
                else if (curDatlotObj.DalotType == DType.D)
                {
                    Conc = curDatlotObj.Length / 1000 * 6.24965;
                    Steel = (double)theData["现浇钢筋总量"];
                    totalSegNum = num23[0];
                    bzqL = 5.1;
                    jsjL = 0;
                    SectA = 6.24965;
                    YiGeXiaoSanJiao = 5.1 * 5.1 * Math.Tan(30.0 / 180.0 * Math.PI) * 0.5;
                    BZQfangshuiceng = 27.089;
                }

                string JSJConc,BZQConc;
                double ASteel = 0;
                double totalLength;
                string C15 = "";
                string Gra = "";
                string Bad = "";
                if (curDatlotObj.Amont == AType.BZQ)
                {
                    JSJConc = "-";
                    BZQConc = string.Format("{0:F1}", (double)theData["八字墙混凝土"] * 2.0);
                    ASteel = (double)theData["八字墙钢筋"] * 2;
                    totalLength = curDatlotObj.Length / 1000 + 2 * bzqL;
                    C15 = string.Format("{0:F1}", (AreaOfSection[1] * totalLength + YiGeXiaoSanJiao * 4 * 0.1) * 1.05);
                    Gra = AreaOfSection[2] == 0 ? "-" : string.Format("{0:F1}", (AreaOfSection[2] * (totalLength - curDatlotObj.MouthT / 1000 * 2)+YiGeXiaoSanJiao*4*curDatlotObj.LayerT/1000) * 1.05);
                    Bad = string.Format("{0:F1}", (AreaOfSection[3] * 2 + AreaOfSection[4]) * curDatlotObj.Length / 1000+BZQfangshuiceng*2.0);
                }
                else
                {
                    JSJConc = string.Format("{0:F1}", (double)theData["八字墙混凝土"] * 1.0);
                    BZQConc = string.Format("{0:F1}", (double)theData["八字墙混凝土"] * 1.0);
                    ASteel = (double)theData["八字墙钢筋"] + (double)theData["集水井钢筋"];
                    totalLength = curDatlotObj.Length / 1000 +  bzqL+jsjL;
                    C15 = string.Format("{0:F1}", (AreaOfSection[1] * totalLength + YiGeXiaoSanJiao * 2* 0.1) * 1.05);
                    Gra = AreaOfSection[2] == 0 ? "-" : string.Format("{0:F1}", (AreaOfSection[2] * (totalLength - curDatlotObj.MouthT / 1000 * 2) + YiGeXiaoSanJiao * 2 * curDatlotObj.LayerT / 1000) * 1.05);
                    Bad = string.Format("{0:F1}", (AreaOfSection[3] * 2 + AreaOfSection[4]) * curDatlotObj.Length / 1000 + BZQfangshuiceng * 2.0+BZQfangshuiceng+JSJfangshuiceng);
                }
                
                
                string Mot = "-";
                
                if (curDatlotObj.H2 - curDatlotObj.H0 - (curDatlotObj.Sect[2]-curDatlotObj.Sect[3])/ 1000 <= 0.5)
                {
                    Mot = string.Format("{0:F1}", AreaOfSection[4] * curDatlotObj.Length / 1000);
                }
                string Joint = string.Format("{0:F1}", (SectA+ AreaOfSection[1]) * (totalSegNum+1));

                string Rem = string.Format("{0:F1}", AreaOfSection[0] * totalLength);
                string Enr=string.Format("{0:F1}",(bzqL / Math.Sqrt(3) * 2 + curDatlotObj.Sect[0]/1000)*0.625);
                string Deb = string.Format("{0:F1}",( AreaOfSection[0]+AreaOfSection[5]+AreaOfSection[1]+AreaOfSection[2])* totalLength);

                table.Add(1, new string[] { "Béton", "C25/30", "m3", string.Format("{0:F1}",Conc)});
                table.Add(2, new string[] { "Armature", "FeE400", "kg", string.Format("{0:F1}",Steel)});
                table.Add(3, new string[] { "Quantité de segment", "-", "bloc", string.Format("{0:G}", totalSegNum) });
                table.Add(4, new string[] { "Puit d'eau", "C25/30", "m3", JSJConc });
                table.Add(5, new string[] { "Mur en aile", "C25/30", "m3", BZQConc });
                table.Add(6, new string[] { "Guide roue", "C25/30", "m3", "-" });
                table.Add(7, new string[] { "Armature", "FeE400", "kg", string.Format("{0:F1}", ASteel) });
                table.Add(8, new string[] { "Béton", "C12/15", "m3", C15});
                table.Add(9, new string[] { "Substitution Radier", "-", "m3", Gra });
                table.Add(10, new string[] { "Badigeonnage des parements", "-", "m2", Bad });
                table.Add(11, new string[] { "Motier hydro", "-", "m2", Mot });
                table.Add(12, new string[] { "Joint", "-", "m2", Joint });
                table.Add(13, new string[] { "Déblai", "-", "m3", Deb });
                table.Add(14, new string[] { "Remblaiement au dos de dalot", "-", "m3", Rem });
                table.Add(15, new string[] { "Enrochement", "-", "m3", Enr });


                Dictionary<int, string> columnName = new Dictionary<int, string>();
                columnName.Add(2, "Crops");
                columnName.Add(5, "Entrée et sortie");
                columnName.Add(8, "Foundation");
                columnName.Add(11, "Etanchéité");
                columnName.Add(14, "Terrassement");

                // 输出表格数据
                worksheet.Cells[LineIdx, 1] = " ";
                worksheet.Cells[LineIdx, 2] =curDatlotObj.Pk_string();
                worksheet.Cells[LineIdx, 3] ="dalot";
                if (curDatlotObj.DalotType == DType.A)
                {
                    worksheet.Cells[LineIdx, 4] = "1-1.5*1.0";
                }
                else if (curDatlotObj.DalotType == DType.B)
                {
                    worksheet.Cells[LineIdx, 4] = "1-2.0*1.5";
                }
                else if (curDatlotObj.DalotType == DType.C)
                {
                    worksheet.Cells[LineIdx, 4] = "1-4.0*2.0";
                }
                else if (curDatlotObj.DalotType == DType.D)
                {
                    worksheet.Cells[LineIdx, 4] = "1-4.0*3.0";
                }
                worksheet.Cells[LineIdx, 5] = 90-curDatlotObj.Ang;
                worksheet.Cells[LineIdx, 6] = curDatlotObj.Length/1000;
                if (curDatlotObj.Amont == AType.BZQ)
                {
                    worksheet.Cells[LineIdx, 7] = "Mur en aile";
                }
                else
                {
                    worksheet.Cells[LineIdx, 7] = "Puit d'eau";
                }
                worksheet.Cells[LineIdx, 8] = "Mur en aile";
                worksheet.Cells[LineIdx, 9] = string.Format("{0:F1}", Conc);
                worksheet.Cells[LineIdx, 10] = string.Format("{0:F1}", Steel);
                worksheet.Cells[LineIdx, 11] = string.Format("{0:G}", totalSegNum);
                worksheet.Cells[LineIdx, 12] = JSJConc;
                worksheet.Cells[LineIdx, 13] = BZQConc;
                worksheet.Cells[LineIdx, 14] = "-";
                worksheet.Cells[LineIdx, 15] = string.Format("{0:F1}", ASteel);
                worksheet.Cells[LineIdx, 16] = C15;
                worksheet.Cells[LineIdx, 17] = Gra;
                worksheet.Cells[LineIdx, 18] = Bad;
                worksheet.Cells[LineIdx, 19] = Mot;
                worksheet.Cells[LineIdx, 20] = Joint;
                worksheet.Cells[LineIdx, 21] = Deb;
                worksheet.Cells[LineIdx, 22] = Rem;
                worksheet.Cells[LineIdx, 23] = Enr;
                worksheet.Cells[LineIdx, 24] = curDatlotObj.H0.ToString(); 


                





                for (int i = 0; i < 16; i++)
                {
                    y0 = y_anchor - i * 6;

                    if (new List<int> { 1, 4, 8, 10, 13 }.Contains(i))
                    {
                        Line hengxian = new Line(PaperOrigenPoint.Convert3D(x0, y0, 0), PaperOrigenPoint.Convert3D(x1, y0, 0));
                        hengxian.Layer = "标注";
                        recorder.AppendEntity(hengxian);
                        tr.AddNewlyCreatedDBObject(hengxian, true);
                    }
                    else
                    {
                        Line hengxian = new Line(PaperOrigenPoint.Convert3D(t1, y0, 0), PaperOrigenPoint.Convert3D(x1, y0, 0));
                        hengxian.Layer = "标注";
                        recorder.AppendEntity(hengxian);
                        tr.AddNewlyCreatedDBObject(hengxian, true);
                    }

                    // 列名
                    if (new List<int> {2,5, 8, 11, 14 }.Contains(i))
                    {
                        DBText txt = new DBText();
                        txt.TextString = columnName[i];
                        txt.TextStyleId = st["En"];
                        txt.Height = 2.5;
                        if (i == 5 || i == 8)
                        {
                            txt.Position = PaperOrigenPoint.Convert3D((x0 + t1) * 0.5, y0);
                            txt.HorizontalMode = TextHorizontalMode.TextCenter;
                            txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                            txt.AlignmentPoint = PaperOrigenPoint.Convert3D((x0 + t1) * 0.5, y0 - 6);
                        }
                        else
                        {
                            txt.Position = PaperOrigenPoint.Convert3D((x0 + t1) * 0.5, y0);
                            txt.HorizontalMode = TextHorizontalMode.TextCenter;
                            txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                            txt.AlignmentPoint = PaperOrigenPoint.Convert3D((x0 + t1) * 0.5, y0 - 3);
                        }

                        txt.Layer = "标注";
                        txt.WidthFactor = 0.85;
                        recorder.AppendEntity(txt);
                        tr.AddNewlyCreatedDBObject(txt, true);
                    }


                    // 内容
                    if (i != 0)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            double tx = 0;
                            if (j == 0)
                            {
                                tx = (t1 + t2) * 0.5;
                            }
                            else if (j == 1)
                            {
                                tx = (t3 + t2) * 0.5;
                            }
                            else if (j == 2)
                            {
                                tx = (t3 + t4) * 0.5;
                            }
                            else
                            {
                                tx = (x1 + t4) * 0.5;
                            }
                            DBText txt = new DBText()
                            {
                                TextString = table[i][j],
                                TextStyleId = st["En"],
                                Height = 2.5,
                                Position = PaperOrigenPoint.Convert3D(tx, y0 - 3),
                                HorizontalMode = TextHorizontalMode.TextCenter,
                                VerticalMode = TextVerticalMode.TextVerticalMid,
                                AlignmentPoint = PaperOrigenPoint.Convert3D(tx, y0 - 3),
                                Layer = "标注",
                                WidthFactor = 0.85,
                            };
                            recorder.AppendEntity(txt);
                            tr.AddNewlyCreatedDBObject(txt, true);
                        }
                    }
                }

                foreach (double x_shuxian in new List<double> { t1, t2, t3, t4 })
                {
                    Line ShuXin = new Line();
                    if (x_shuxian == t1)
                    {
                        ShuXin = new Line(PaperOrigenPoint.Convert3D(x_shuxian, y_anchor-6), PaperOrigenPoint.Convert3D(x_shuxian, y_anchor-96));
                    }
                    else
                    {
                        ShuXin = new Line(PaperOrigenPoint.Convert3D(x_shuxian, y_anchor), PaperOrigenPoint.Convert3D(x_shuxian, y_anchor-96));
                    }
                    ShuXin.Layer = "标注";
                    recorder.AppendEntity(ShuXin);
                    tr.AddNewlyCreatedDBObject(ShuXin, true);
                }



                DBText rowName = new DBText()
                {
                    TextString = "Aspect d'ouvrage",
                    TextStyleId = st["En"],
                    Height = 2.5,
                    Position = PaperOrigenPoint.Convert3D((x0 + t2) * 0.5, y_anchor - 3),
                    HorizontalMode = TextHorizontalMode.TextCenter,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = PaperOrigenPoint.Convert3D((x0 + t2) * 0.5, y_anchor - 3),
                    Layer = "标注",
                    WidthFactor = 0.85,
                };
                recorder.AppendEntity(rowName);
                tr.AddNewlyCreatedDBObject(rowName, true);
                rowName = new DBText()
                {
                    TextString = "Matériaux",
                    TextStyleId = st["En"],
                    Height = 2.5,
                    Position = PaperOrigenPoint.Convert3D((t2 + t3) * 0.5, y_anchor - 3),
                    HorizontalMode = TextHorizontalMode.TextCenter,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = PaperOrigenPoint.Convert3D((t3 + t2) * 0.5, y_anchor - 3),
                    Layer = "标注",
                    WidthFactor = 0.85,
                };
                recorder.AppendEntity(rowName);
                tr.AddNewlyCreatedDBObject(rowName, true);
                rowName = new DBText()
                {
                    TextString = "Unité",
                    TextStyleId = st["En"],
                    Height = 2.5,
                    Position = PaperOrigenPoint.Convert3D((t4 + t3) * 0.5, y_anchor - 3),
                    HorizontalMode = TextHorizontalMode.TextCenter,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = PaperOrigenPoint.Convert3D((t3 + t4) * 0.5, y_anchor - 3),
                    Layer = "标注",
                    WidthFactor = 0.85,
                };
                recorder.AppendEntity(rowName);
                tr.AddNewlyCreatedDBObject(rowName, true);
                rowName = new DBText()
                {
                    TextString = "Quantité",
                    TextStyleId = st["En"],
                    Height = 2.5,
                    Position = PaperOrigenPoint.Convert3D((x1 + t4) * 0.5, y_anchor - 3),
                    HorizontalMode = TextHorizontalMode.TextCenter,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = PaperOrigenPoint.Convert3D((x1 + t4) * 0.5, y_anchor - 3),
                    Layer = "标注",
                    WidthFactor = 0.85,
                };
                recorder.AppendEntity(rowName);
                tr.AddNewlyCreatedDBObject(rowName, true);

                Polyline box = new Polyline();
                box.AddVertexAt(0, PaperOrigenPoint.Convert2D(x0, y0 + 15 * 6), 0, 0.3, 0.3);
                box.AddVertexAt(1, PaperOrigenPoint.Convert2D(x1, y0 + 15 * 6), 0, 0.3, 0.3);
                box.AddVertexAt(2, PaperOrigenPoint.Convert2D(x1, y0-6), 0, 0.3, 0.3);
                box.AddVertexAt(3, PaperOrigenPoint.Convert2D(x0, y0 -6), 0, 0.3, 0.3);
                box.Layer = "标注";
                box.Closed = true;
                recorder.AppendEntity(box);
                tr.AddNewlyCreatedDBObject(box, true);
                tr.Commit();
            }
            return;
        }






        public static void PrintLineText(Database db,Point2d StartPoint, Point2d EndPoint,string[] textList,bool isLeft=true,double scale = 100)
        {
            int number = textList.Count();
            double wdh = 2.5;
            double WdWidth=1.25;
            double LineWidth=0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                Line L1 = new Line();
                Line side = new Line();
                DBText txt = new DBText();

                foreach (string cnt in textList)
                {
                    double w = WdWidth * cnt.Length * scale;
                    if (w > LineWidth) { LineWidth = w; }
                }

                
                double xdir = isLeft ? -1 : 1;
                double ydir;
                if (EndPoint.Y > StartPoint.Y)
                {
                    L1 = new Line(StartPoint.Convert3D(), EndPoint.Convert3D(0, +(wdh + 0.5) * scale * (number - 1)));
                    ydir = 1;
                }
                else
                {
                    L1 = new Line(StartPoint.Convert3D(), EndPoint.Convert3D(0, -(wdh + 0.5) * scale * (number - 1)));
                    ydir = -1;
                }
                L1.Layer = "标注";
                modelSpace.AppendEntity(L1);
                tr.AddNewlyCreatedDBObject(L1, true);

                int i = 0;
                foreach (string cnt in textList)
                {

                    side = new Line(EndPoint.Convert3D(0, i *( wdh+0.5) * scale * ydir), EndPoint.Convert3D(xdir * LineWidth, i * (wdh+0.5) * scale * ydir));
                    side.Layer = "标注";
                    modelSpace.AppendEntity(side);
                    tr.AddNewlyCreatedDBObject(side, true);

                    if (cnt.StartsWith("Joint"))
                    {
                        break;
                    }
                    txt = new DBText();
                    txt.TextString = cnt;
                    txt.Height = wdh * scale;    
                    txt.Position = side.StartPoint;
                    txt.HorizontalMode = isLeft ? TextHorizontalMode.TextRight : TextHorizontalMode.TextLeft;
                    txt.VerticalMode = TextVerticalMode.TextBottom;
                    //txt.Justify = AttachmentPoint.BaseLeft;
                    txt.AlignmentPoint =txt.Position;
                    txt.Layer = "标注";
                    txt.TextStyleId = st["fsdb"];
                    txt.WidthFactor = 0.75;
                    modelSpace.AppendEntity(txt);
                    tr.AddNewlyCreatedDBObject(txt, true);

                    i++;
                }


                if (textList[0].StartsWith("Joint"))
                {
                    MatchCollection matches = Regex.Matches(textList[0], @"(\d+\.?\d*)");


                    txt = new DBText();
                    txt.TextString = "Joint";
                    txt.Height = wdh * scale;
                    txt.Position = side.StartPoint;
                    txt.HorizontalMode = isLeft ? TextHorizontalMode.TextRight : TextHorizontalMode.TextLeft;
                    txt.VerticalMode = TextVerticalMode.TextBottom;
                    //txt.Justify = AttachmentPoint.BaseLeft;
                    txt.AlignmentPoint = txt.Position;
                    txt.Layer = "标注";
                    txt.TextStyleId = st["fsdb"];
                    txt.WidthFactor = 0.75;
                    modelSpace.AppendEntity(txt);
                    tr.AddNewlyCreatedDBObject(txt, true);


                    txt = new DBText();
                    txt.TextString =string.Format ("e={0}cm", matches[0]);
                    txt.Height = wdh * scale;
                    txt.Position = side.StartPoint;
                    txt.HorizontalMode = isLeft ? TextHorizontalMode.TextRight : TextHorizontalMode.TextLeft;
                    txt.VerticalMode = TextVerticalMode.TextTop;
                    //txt.Justify = AttachmentPoint.BaseLeft;
                    txt.AlignmentPoint = txt.Position;
                    txt.Layer = "标注";
                    txt.TextStyleId = st["fsdb"];
                    txt.WidthFactor = 0.75;
                    modelSpace.AppendEntity(txt);
                    tr.AddNewlyCreatedDBObject(txt, true);
                    side.EndPoint = side.StartPoint.Convert3D(xdir*0.7 * side.Length);

                }


                tr.Commit();
            }
        }




        public static void PrintCirText(Database db, int textstring,  Point2d PositionPoint, double scale = 100)
        {
            DBText txt = new DBText();
            Circle C1 = new Circle();            
            Circle C2 = new Circle();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;


                txt.TextString = textstring.ToString();
                txt.Height = 2.5 * scale;
                txt.Position = PositionPoint.Convert3D();
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                txt.AlignmentPoint = PositionPoint.Convert3D();
                txt.TextStyleId = st["fsdb"];
                txt.Layer = "标注";
                txt.WidthFactor = 0.75;

                C1 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.5 * scale);
                C2 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.8 * scale);
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
            return;
        }

        public static void PrintCirText(Database db, int textstring, Point2d PositionPoint,Point2d startP,Point2d endP, double scale = 100)
        {
            DBText txt = new DBText();
            Circle C1 = new Circle();
            Circle C2 = new Circle();
            Line L1, L2;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;


                txt.TextString = textstring.ToString();
                txt.Height = 2.5 * scale;
                txt.Position = PositionPoint.Convert3D();
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                txt.AlignmentPoint = PositionPoint.Convert3D();
                txt.TextStyleId = st["fsdb"];
                txt.Layer = "标注";
                txt.WidthFactor = 0.75;

                C1 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.5 * scale);
                C2 = new Circle(PositionPoint.Convert3D(), Vector3d.ZAxis, 1.8 * scale);
                C1.Layer = "标注";
                C2.Layer = "标注";

                L1 = new Line(PositionPoint.Convert3D(0, 1.8 * scale), endP.Convert3D());
                L2 = new Line(PositionPoint.Convert3D(0, -1.8 * scale), startP.Convert3D());
                L1.Layer = "标注";
                L2.Layer = "标注";

                modelSpace.AppendEntity(txt);
                tr.AddNewlyCreatedDBObject(txt, true);
                modelSpace.AppendEntity(C1);
                tr.AddNewlyCreatedDBObject(C1, true);
                modelSpace.AppendEntity(C2);
                tr.AddNewlyCreatedDBObject(C2, true);
                modelSpace.AppendEntity(L1);
                tr.AddNewlyCreatedDBObject(L1, true);
                modelSpace.AppendEntity(L2);
                tr.AddNewlyCreatedDBObject(L2, true);
                tr.Commit();
            }
            return;
        }



        public static DBText PrintText(Database db, string textstring,double H, Point2d PositionPoint, double scale = 100,bool isCover=false)
        {
            DBText txt=new DBText();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                txt.TextString = textstring;
                txt.Height = H * scale;
                txt.Position = PositionPoint.Convert3D();
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextBottom;
                txt.AlignmentPoint = PositionPoint.Convert3D(0, 0.5 * scale);
                txt.TextStyleId = st["fsdb"];
                txt.Layer = "标注";
                txt.WidthFactor = 0.75;
                if (isCover)
                {
                    // 遮罩
                }

                modelSpace.AppendEntity(txt);
                tr.AddNewlyCreatedDBObject(txt, true);
                tr.Commit();
            }
            return txt;
        }

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
            double ang = new Line2d(StartPoint, EndPoint).Direction.Angle;               
                
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
                        Height = 4 * scale,
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

                    L = i == 0 ? new Line(StartPoint.Convert3D(-1.5 * scale, 0), StartPoint.Convert3D(1.5 * scale, 0)) :
                        new Line(EndPoint.Convert3D(-1.5 * scale, 0), EndPoint.Convert3D(1.5 * scale, 0));
                    p0 = i == 0 ? StartPoint : EndPoint;
                    p1 = i == 0 ? StartPoint.Convert2D(0, -2 * scale) : EndPoint.Convert2D(0, -2 * scale);

                    L.Layer = "标注";
                    L.ColorIndex = 1;                    
                    L.TransformBy(Matrix3d.Rotation(ang,Vector3d.ZAxis,p0.Convert3D()));
                    modelSpace.AppendEntity(L);
                    tr.AddNewlyCreatedDBObject(L, true);

                    PL.AddVertexAt(0,p0, 0, 0, 2*scale);
                    PL.AddVertexAt(1, p1, 0, 0, 0);
                    PL.TransformBy(Matrix3d.Rotation(ang, Vector3d.ZAxis, p0.Convert3D()));
                    modelSpace.AppendEntity(PL);
                    tr.AddNewlyCreatedDBObject(PL, true);
                }
                tr.Commit();
            }

        }

        public static void DimRoadDirect(Database db, Point2d AnchorPoint, double scale,bool isUp)
        {
            Polyline Arrow=new Polyline();
            DBText txt=new DBText();
            int diry = isUp ? 1 : -1;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                TextStyleTable st = tr.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                txt.TextString =isUp?"DABOLA":"COYAH";
                txt.Layer = "标注";
                txt.Height = 3 * scale;
                txt.TextStyleId = st["fsdb"];
                txt.Position = AnchorPoint.Convert3D();
                txt.HorizontalMode = TextHorizontalMode.TextMid;
                txt.VerticalMode = isUp?TextVerticalMode.TextBottom:TextVerticalMode.TextTop;
                txt.AlignmentPoint = AnchorPoint.Convert3D(0,1.2*scale*diry);
                txt.WidthFactor = 0.75;
                modelSpace.AppendEntity(txt);
                tr.AddNewlyCreatedDBObject(txt, true);      
                Point2d p0 = AnchorPoint;
                Point2d p1 = p0.Convert2D(0, -diry * 7.5 * scale);
                Point2d p2 = p1.Convert2D(-1 * scale);
                Point2d p3 = p2.Convert2D(0, diry * 3.5 * scale);
                Point2d p4 = p3.Convert2D(-1.5 * scale);
                Arrow.AddVertexAt(0, p0, 0, 0, 0);
                Arrow.AddVertexAt(1, p1, 0, 0, 0);
                Arrow.AddVertexAt(2, p2, 0, 0, 0);
                Arrow.AddVertexAt(3, p3, 0, 0, 0);
                Arrow.AddVertexAt(4, p4, 0, 0, 0);
                Arrow.Layer = "标注";
                Arrow.Closed = true;
                modelSpace.AppendEntity(Arrow);
                tr.AddNewlyCreatedDBObject(Arrow, true);
                tr.Commit();
            }

        }





    }
}
