using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using SD= System.Data;

[assembly: CommandClass(typeof(MyCAD1.BPublicFunctions))]

namespace MyCAD1
{
    public class BPublicFunctions
    {
        /// <summary>
        /// 提示对话框
        /// </summary>
        /// <param name="PromptOpts">对话框标题</param>
        /// <returns></returns>
        public static string GetXPath(string PromptOpts,string FilterStr="Excel文件|*.xls;*.xlsx")
        {
            string xpath = "";
            WFM.OpenFileDialog dialog = new WFM.OpenFileDialog();
            dialog.Title = PromptOpts;

            dialog.InitialDirectory = "G:\\涵洞自动成图程序";
            dialog.Filter = FilterStr;
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





        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static SD.DataTable OpenCSV(string filePath)
        {
            Encoding encoding = Encoding.UTF8;//
            SD.DataTable dt = new SD.DataTable();
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(fs, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] TableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                //strLine = Common.ConvertStringUTF8(strLine, encoding);
                //strLine = Common.ConvertStringUTF8(strLine);

                if (IsFirst == true)
                {
                    TableHead = strLine.Split(',');
                    IsFirst = false;
                    columnCount = TableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        SD.DataColumn dc = new SD.DataColumn(TableHead[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    aryLine = strLine.Split(',');
                    SD.DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (aryLine != null && aryLine.Length > 0)
            {
                dt.DefaultView.Sort = TableHead[0] + " " + "asc";
            }

            sr.Close();
            fs.Close();
            return dt;
        }

















    }























}