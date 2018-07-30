using System.IO;
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
            public double H0, H1, H2;
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

            // 初始化带帽图列表
            string dmtpath = BPublicFunctions.GetXPath("选择带帽图数据");
            Dmt_list = IniDMT(dmtpath);           



            //读取数据
            Dalot TheDalot = new Dalot();
            System.Data.DataTable Parameters = new System.Data.DataTable();
            string aa = BPublicFunctions.GetXPath("选择设计表");
            if (aa == "")
            {
                ed.WriteMessage("\n建模使用默认数据");
                TheDalot =new Dalot();
                Extents2d extA;
                //绘图
                extA = TheDalot.PlotA(db, TheDalot.BasePoint, 100);
                double dAB = extA.MinPoint.Y - TheDalot.BasePoint.Y;
                dAB -= 1500;//图名
                dAB -= 1500;//顶标注
                dAB -= 1500; //H2
                dAB -= (TheDalot.H2 - TheDalot.H0) * 1000;
                TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), 100);
                TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(30000, dAB), 75);
            }
            else
            {
                Parameters = GetDataFromExcelByCom(true, aa);
                
                ed.WriteMessage("\n涵洞数据读取成功");
            }




            // 实例化并绘图
            if (aa != "")
            {
                for (int ii = 0; ii < Parameters.Rows.Count; ii++)
                {
                    // 实例化涵洞
                    TheDalot = GetDalotFromDataTable(Parameters, ii);
                    // 查询带帽图
                    DMT relatedDMT = DmtLookUp(Dmt_list, TheDalot.Pk_string());


    
                    //绘图
                    Extents2d extA;
                    extA = TheDalot.PlotA(db, TheDalot.BasePoint, 100);
                    double dAB = extA.MinPoint.Y - TheDalot.BasePoint.Y;
                    dAB -= 1500;//图名
                    dAB -= 1500;//顶标注
                    dAB -= 1500; //H2
                    dAB -= (TheDalot.H2 - TheDalot.H0) * 1000;
                    TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), 100);
                    TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(30000, dAB), 75);

                    if (ii == 1)
                    {
                        break;
                    }
                }


            }

            




            // 成图
            //TheDalot.CreatPaperSpace(db, ed, new int[] { 100, 100, 75 });



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
            //public AType Amont, Avale;
            //public DType DalotType;
            Point2d BasePoint=new Point2d(0,no*-50000);            
            int LayerNum= int.Parse((string)theDT.Rows[rowIndex]["layerNum"]);
            Dalot res = new Dalot(710, Ang, Slop, Length, SegLength, XMidDist, DType.B, AType.BZQ, AType.BZQ, BasePoint, LayerNum);
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
                    }
                    continue;
                }

                else if (nextLine.StartsWith("Ht"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
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
                    item.H0 = Convert.ToDouble(matches[1].Value);
                }
                else if (nextLine.StartsWith("H1"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.H1 = Convert.ToDouble(matches[1].Value);
                }
                else if (nextLine.StartsWith("H2"))
                {
                    MatchCollection matches = Regex.Matches(nextLine, @"(\d+\.?\d*)");
                    item.H2 = Convert.ToDouble(matches[1].Value);
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
                    if (matches.Count !=2|| matches.Count != 3)
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
