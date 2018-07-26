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



        public static LineAngularDimension2 DimAng(Database db,  Line L1, Line L2, Point3d Pref, ObjectId dimID)
        {
            LineAngularDimension2 AD1;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                AD1 = new LineAngularDimension2(L1.StartPoint, L1.EndPoint, L2.StartPoint, L2.EndPoint, Pref, "", dimID);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                tr.Commit();
            }
            return AD1;
        }



        public static AlignedDimension DimAli(Database db, Point3d P1, Point3d P2, Point3d Pref, ObjectId dimID)
        {
            AlignedDimension AD1;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                AD1 = new AlignedDimension(P1, P2, Pref, "", dimID);
                AD1.Layer = "标注";
                modelSpace.AppendEntity(AD1);
                tr.AddNewlyCreatedDBObject(AD1, true);

                tr.Commit();
            }
            return AD1;
        }





        /// <summary>
        /// 绘制标高符号
        /// </summary>
        /// <param name="bgdata">标高数据</param>
        /// <param name="refpt">标高点</param>
        /// <param name="ms"></param>
        /// <param name="tr"></param>
        /// <param name="blockTbl"></param>
        /// <param name="s"></param>



        public static void BiaoGao(double bgdata, Point3d refpt, BlockTableRecord ms, Transaction tr, BlockTable blockTbl, double s = 100)
        {
            ObjectId blkRecId = blockTbl["BG"];
            double factor = s / 100;
            using (BlockReference acBlkRef = new BlockReference(refpt, blkRecId))
            {
                //acBlkRef.SetAttributes();
                acBlkRef.ScaleFactors = new Scale3d(factor);
                ms.AppendEntity(acBlkRef);
                tr.AddNewlyCreatedDBObject(acBlkRef, true);
                BlockTableRecord zheshiyuankuai;
                zheshiyuankuai = tr.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId gezhongshuxingID in zheshiyuankuai)
                {
                    DBObject gezhongshuxing = tr.GetObject(gezhongshuxingID, OpenMode.ForRead) as DBObject;
                    if (gezhongshuxing is AttributeDefinition)
                    {
                        AttributeDefinition acAtt = gezhongshuxing as AttributeDefinition;
                        using (AttributeReference acAttRef = new AttributeReference())
                        {
                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);
                            acAttRef.TextString = String.Format("{0:f2}", bgdata);
                            //acAttRef.Height = acAttRef.Height * factor;
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);

                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
            }
        }






        /// <summary>
        /// 左右横坡
        /// </summary>
        /// <param name="db">CAD数据库</param>
        /// <param name="hpdata">横坡数值，不含%</param>
        /// <param name="refpt">插入点3d</param>
        /// <param name="isLeft">是否向左</param>


        public static void HengPo(Database db, double hpdata, Point3d refpt, bool isLeft,double scale=100)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                ObjectId blkRecId;
                if (isLeft) { blkRecId = blockTbl["ZP"]; }
                else
                { blkRecId = blockTbl["YP"]; }
                BlockReference acBlkRef = new BlockReference(refpt, blkRecId);
                acBlkRef.Layer = "标注";
                acBlkRef.ScaleFactors = new Scale3d(0.01*scale);
                modelSpace.AppendEntity(acBlkRef);
                tr.AddNewlyCreatedDBObject(acBlkRef, true);


                BlockTableRecord zheshiyuankuai;
                zheshiyuankuai = tr.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId gezhongshuxingID in zheshiyuankuai)
                {
                    DBObject gezhongshuxing = tr.GetObject(gezhongshuxingID, OpenMode.ForRead) as DBObject;
                    if (gezhongshuxing is AttributeDefinition)
                    {
                        AttributeDefinition acAtt = gezhongshuxing as AttributeDefinition;
                        using (AttributeReference acAttRef = new AttributeReference())
                        {
                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);
                            acAttRef.TextString = String.Format("{0:f2}%", hpdata);
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
                tr.Commit();
            }            
        }
    }
}
