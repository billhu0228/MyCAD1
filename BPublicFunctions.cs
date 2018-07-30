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
        public static string GetXPath(string PromptOpts)
        {
            string xpath = "";
            WFM.OpenFileDialog dialog = new WFM.OpenFileDialog();
            dialog.Title = PromptOpts;
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