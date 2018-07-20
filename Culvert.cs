using System;
using System.Collections.Generic;
using WFM = System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MOE=Microsoft.Office.Interop.Excel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAD1
{
    struct BPad
    {
        public string description;
        public double width;
        public double height;
    };
    struct BRefill
    {
        public double slop;
        public double sep;
        public string description;
    };
    struct BOctWall
    {
        public double width;
        public double thick;
        public double cutTop;
        public double cutBot;
    }

    class Culvert
    {
        public int numSeg;
        public double Length,SegLength,SideLength, Ang, Slop;
        public double ShoulderW, ShoulderH;
        public double mouthT;        
        public double[] Sect = new double[12];
        public BPad CPad0,CPad1;
        public BRefill CRefill;
        public BOctWall CWall;
        public double[] WallAng=new double[4];        
        public double Area;
        public double Volume;
        public double[] BG = new double[4];
        public double[] delt = new double[3];
        public double[] RoadWidth;
        public Culvert()
        {
            Sect = new double[12] { 2500, 2500, 2000, 250, 250, 250, 50, 50, 200, 100, 0.01, 0 };
            numSeg = 8;
            SegLength = 2000;
            Length = 16000.0;
            Ang = 3.0;
            Slop = -0.01;
            ShoulderH = 300;
            ShoulderW = 200;
            CPad0 = new BPad()
            {
                description = "C12/15 B.P.",
                width = 2900,
                height = 100,
            };
            CPad1 = new BPad()
            {
                description = "Graveleux Lateritique",
                width = 2900 + 600,
                height = 400,
            };
            CWall = new BOctWall()
            {
                width=2820,
                thick = 250,
                cutTop = 200,
                cutBot = 300,                
            };
            WallAng = new double[]{30,30,30,30};
            CRefill = new BRefill() { slop = 1, description = "REMBLAI.G.L", sep = 500 };
            mouthT = 250;
            BG = new double[] {19.30,22.89,22.83,22.70 };
            RoadWidth = new double[] { 5000, 5000 };
            CalVA();
        }

        public Culvert(string strFileName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            object missing = System.Reflection.Missing.Value;
            List<string> colnames = new List<string>() { "No.", "H1", "H2", "H3", "t1", "t2", "t3", "f1", "f2",
                "f3", "f4", "s1", "a1", "Length", "numSeg", "SegL", "Ang", "Slop", "ShoulderH", "ShoulderW",
                "Pad0Des", "Pad0W", "Pad0H", "Pad1Des", "Pad1W", "Pad1H", "WallWidth", "WallT", "cutTop", "cutBot",
                "WA1", "WA2", "WA3", "WA4", "RefillS", "RefillDes", "RefillSep", "MouthT","BG0","BG1","BG2","BG3","RW0","RW1" };
            MOE.Application excel = new MOE.Application
            {
                Visible = false,
                UserControl = true
            };
            MOE.Workbook wb = excel.Application.Workbooks.Open(strFileName, missing, true, missing, missing, missing,
                missing, missing, missing, true, missing, missing, missing, missing, missing);
            MOE.Worksheet ws = new MOE.Worksheet();
            for (int i = 1; i <= wb.Worksheets.Count; i++)
            {
                if (wb.Worksheets[i].Name=="Parameters")
                {
                    ws = (MOE.Worksheet)wb.Worksheets[i];
                    break;
                }
                else
                {
                    ed.WriteMessage("Bad");
                    return;
                }
            }
            MOE.Range rgUsed = ws.UsedRange;
            MOE.Range rgFound;
            int iRowNum = 2;
            
            foreach(string thename in colnames)
            {
                rgFound = (MOE.Range)rgUsed.Find(thename, Type.Missing, MOE.XlFindLookIn.xlValues,
                    MOE.XlLookAt.xlPart, MOE.XlSearchOrder.xlByRows, MOE.XlSearchDirection.xlNext, false, false);
                int iColNum = rgFound.Column;
                var value = ws.Cells[iRowNum, iColNum].Value2;
                switch (thename)
                {
                    case "No.":
                        break;
                    case "H1":
                        Sect[0] = value;
                        break;
                    case "H2":
                        Sect[1] = value;
                        break;
                    case "H3":
                        Sect[2] = value;
                        break;
                    case "t1":
                        Sect[3] = value;
                        break;
                    case "t2":
                        Sect[4] = value;
                        break;
                    case "t3":
                        Sect[5] = value;
                        break;
                    case "f1":
                        Sect[6] = value;
                        break;
                    case "f2":
                        Sect[7] = value;
                        break;
                    case "f3":
                        Sect[8] = value;
                        break;
                    case "f4":
                        Sect[9] = value;
                        break;
                    case "s1":
                        Sect[10] = value;
                        break;
                    case "a1":
                        Sect[11] = value;
                        break;
                    case "Length":
                        Length = value;
                        break;
                    case "numSeg":
                        numSeg = (int)value;
                        break;
                    case "SegL":
                        SegLength = value;
                        break;
                    case "Ang":
                        Ang = value;
                        break;
                    case "Slop":
                        Slop = value;
                        break;
                    case "ShoulderH":
                        ShoulderH = value;
                        break;
                    case "ShoulderW":
                        ShoulderW = value;
                        break;
                    case "Pad0Des":
                        CPad0.description = value;
                        break;
                    case "Pad0W":
                        CPad0.width = value;
                        break;
                    case "Pad0H":
                        CPad0.height = value;
                        break;
                    case "Pad1Des":
                        CPad1.description = value;
                        break;
                    case "Pad1W":
                        CPad1.width = value;
                        break;
                    case "Pad1H":
                        CPad1.height = value;
                        break;
                    case "WallWidth":
                        CWall.width = value;
                        break;
                    case "WallT":
                        CWall.thick = value;
                        break;
                    case "cutTop":
                        CWall.cutTop = value;
                        break;
                    case "cutBot":
                        CWall.cutBot = value;
                        break;
                    case "WA1":
                        WallAng[0] = value;
                        break;
                    case "WA2":
                        WallAng[1] = value;
                        break;
                    case "WA3":
                        WallAng[2] = value;
                        break;
                    case "WA4":
                        WallAng[3] = value;
                        break;
                    case "RefillS":
                        CRefill.slop = value;
                        break;
                    case "RefillDes":
                        CRefill.description = value;
                        break;
                    case "RefillSep":
                        CRefill.sep = value;
                        break;
                    case "MouthT":
                        mouthT = value;
                        break;
                    case "BG0":
                        BG[0] = value;
                        break;
                    case "BG1":
                        BG[1] = value;
                        break;
                    case "BG2":
                        BG[2] = value;
                        break;
                    case "BG3":
                        BG[3] = value;
                        break;
                    case "RW0":
                        RoadWidth[0] = value;
                        break;
                    case "RW1":
                        RoadWidth[1] = value;
                        break;
                }
            }
            CalVA();
            wb.Close(true, null, null);
            excel.Quit();
            return;
        }
        private void CalVA()
        {
            SideLength = Length - numSeg * SegLength;
            Area = Sect[0] * Sect[2] - (Sect[0] - 2 * Sect[5]) * (Sect[2] - Sect[3] - Sect[4])
                + Sect[6] * Sect[7] + Sect[8] * Sect[9] + 0.5 * Sect[1] * Sect[10];
            if (Sect[11] != 0) { Area += Sect[11] * Sect[3] * 2 + Sect[6] * Sect[7]; }
            Volume = Area * Length;
            delt[0] = (BG[1] - BG[0]) * 1000.0;
            delt[1] = (BG[2] - BG[0]) * 1000.0;
            delt[2] = (BG[3] - BG[0]) * 1000.0;

            return;
        }
        public Point2d[] GetAnchPoints()
        {
            double[] ys = GetYRange2();
            Point2d[] aPts=new Point2d[3];
            aPts[0] =new Point2d(0, 0);
            aPts[1] = new Point2d(0,ys[0]+1500+CPad0.height+CPad1.height);
            aPts[2] = new Point2d(0.5*Length+CWall.width+10000, ys[0] + 1500 + CPad0.height + CPad1.height);
            return aPts;
        }
        public double[] GetXRange()
        {
            return new double[2] { -0.5 * Length - CWall.width, 0.5 * Length + CWall.width };            
        }
        public double[] GetYRange()
        {
            double y1, y2, y3, y4;
            double sl = CWall.width;
            y1 = -0.5 * Sect[1] + Math.Tan(Ang / 180 * Math.PI) * -0.5 * Length;
            y2 = -0.5 * Sect[1] + Math.Tan(Ang / 180 * Math.PI) * +0.5 * Length;
            y3 = +0.5 * Sect[1] + Math.Tan(Ang / 180 * Math.PI) * +0.5 * Length;
            y4 = +0.5 * Sect[1] + Math.Tan(Ang / 180 * Math.PI) * -0.5 * Length;
            y1 += -sl * Math.Tan(WallAng[0] / 180 * Math.PI);
            y2 += -sl * Math.Tan(WallAng[0] / 180 * Math.PI);
            y3 += +sl * Math.Tan(WallAng[0] / 180 * Math.PI);
            y4 += +sl * Math.Tan(WallAng[0] / 180 * Math.PI);
            return new double[2] { Math.Min(y1, y2) - 1500.0, Math.Max(y3, y4) + 1500 };
        }
        public double[] GetYRange2()
        {
            double[] Yrg1 = GetYRange();
            double ymax, ymin;
            ymax = Yrg1[0] - 3000;//标注线
            ymin = ymax - 1500-delt[1]-Sect[3]-CPad0.height-CPad1.height-1500;//标高顶
            return new double[2] { ymin,ymax};
        }
    }
}
