using System;
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

namespace MyCAD1
{
    class DimPloter
    {
        /// <summary>
        /// 水平标注
        /// </summary>
        /// <param name="db"></param>
        /// <param name="P1">起点</param>
        /// <param name="P2">终点</param>
        /// <param name="Pref">标注位置</param>
        /// <param name="dimID">标注样式id</param>
        /// <param name="ang">转角，弧度</param>
        /// <returns></returns>
        public static RotatedDimension Dim0(Database db, Point3d P1, Point3d P2,Point3d Pref
            ,ObjectId dimID,double ang=0)
        {

            RotatedDimension D1;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                D1 = new RotatedDimension(ang, P1, P2,Pref,"", dimID);
                D1.Layer = "标注";
                modelSpace.AppendEntity(D1);
                tr.AddNewlyCreatedDBObject(D1, true);
                tr.Commit();
            }
            return D1;

        }

    }
}
