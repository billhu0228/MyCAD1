using System;
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

[assembly: CommandClass(typeof(MyCAD1.Drawing))]


namespace MyCAD1
{
    public class Drawing
    {

        [CommandMethod("main")]
        public void Main()
        {
            //基本句柄
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //读取数据
            Dalot TheDalot = new Dalot();
            System.Data.DataTable Parameters = new System.Data.DataTable();
            string aa = GetXPath("选择设计表");
            if (aa == "")
            {
                ed.WriteMessage("\n建模使用默认数据。");
            }
            else
            {
                Parameters = GetDataFromExcelByCom(true, aa);
                
                ed.WriteMessage("\n涵洞数据读取成功。");
            }

            if (aa != "")
            {
                for (int ii = 0; ii < Parameters.Rows.Count; ii++)
                {
                    // Parameters.Rows[ii]


                    if (ii == 0)
                    {
                        break;
                    }
                }


            }

            Extents2d extA, extB, extC;

            //绘图
            //extA =TheDalot.PlotA(db, TheDalot.BasePoint, 100);
            //double dAB = extA.MinPoint.Y-TheDalot.BasePoint.Y;
            //dAB -= 1500;//图名
            //dAB -= 1500;//顶标注
            //dAB -= 1500; //H2
            //dAB -= (TheDalot.H2 - TheDalot.H0)*1000;
            //TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), 100);
            //TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(30000, dAB), 75);


            // 成图
            //TheDalot.CreatPaperSpace(db, ed, new int[] { 100, 100, 75 });



        }



        public  Dalot GetDalotFromDataTable(System.Data.DataTable theDT,int rowIndex)
        {
            string Pk=(string)theDT.Rows[rowIndex]["pk"];
            double Ang=90.0-(double)theDT.Rows[rowIndex]["Ang"];
            double Slop= (double)theDT.Rows[rowIndex]["slop"];
            double Length= (double)theDT.Rows[rowIndex]["Length"];
            double SegLength= (double)theDT.Rows[rowIndex]["SegLength"];
            double XMidDist= (double)theDT.Rows[rowIndex]["XMidLength"];
            int no= (int)theDT.Rows[rowIndex]["no"];
            //public AType Amont, Avale;
            //public DType DalotType;
            Point2d BasePoint=new Point2d(0,no*-50000);            
            int LayerNum= (int)theDT.Rows[rowIndex]["layerNum"];

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
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;
                app.Workbooks.Close();
                app.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                app = null;
            }
        }



        private static string GetXPath(string tit)
        {
            string xpath = "";
            WFM.OpenFileDialog dialog = new WFM.OpenFileDialog();
            dialog.Title = tit;
            dialog.InitialDirectory = "G:\\涵洞自动成图程序";
            //dialog.Filter = "ext files (*.xls)|*.xls|All files(*.*)|*>**";
            //dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == WFM.DialogResult.OK)
            {
                xpath = dialog.FileName;
            }
            else
            {
                xpath = "";
            }
            return xpath;
        }


    }
}
