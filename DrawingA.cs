using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;


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
            string aa = GetXPath();
            if (aa != "")
            {
                TheDalot = new Dalot();
                ed.WriteMessage(aa);
            }
            else
            {
                ed.WriteMessage("\n建模使用默认数据。");
                TheDalot = new Dalot();
            }
            Extents2d extA, extB, extC;

            //绘图
            extA =TheDalot.PlotA(db, TheDalot.BasePoint, 100);
            double dAB = extA.MinPoint.Y-TheDalot.BasePoint.Y;
            dAB -= 1500;//图名
            dAB -= 1500;//顶标注
            dAB -= 1500; //H2
            dAB -= (TheDalot.H2 - TheDalot.H0)*1000;
            TheDalot.PlotB(db, TheDalot.BasePoint.Convert2D(0, dAB), 100);
            TheDalot.PlotC(db, TheDalot.BasePoint.Convert2D(30000, dAB), 75);


            // 成图
            //TheDalot.CreatPaperSpace(db, ed, new int[] { 100, 100, 75 });



        }











        private static string GetXPath()
        {
            string xpath = "";
            WFM.OpenFileDialog dialog = new WFM.OpenFileDialog();
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
