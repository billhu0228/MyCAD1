using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(MyCAD2.Class1))]

namespace MyCAD2
{
    public class Class1
    {

        [CommandMethod("T2")]
        public void Test()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                // if the bloc table has the block definition
                if (bt.Has("prz_podl"))
                {
                    // create a new block reference
                    var br = new BlockReference(Point3d.Origin, bt["prz_podl"]);

                    // add the block reference to the curentSpace and the transaction
                    var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    curSpace.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    // set the dynamic property value
                    foreach (DynamicBlockReferenceProperty prop in br.DynamicBlockReferencePropertyCollection)
                    {
                        if (prop.PropertyName == "par_l")
                        {
                            prop.Value = 500.0;
                        }
                    }
                }
                // save changes
                tr.Commit();
            } // <- end using: disposing the transaction and all objects opened with it (block table) or added to it (block reference)
        }

        [CommandMethod("tst")]
        public static void MyFunc()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record for read
                BlockTableRecord acBlkTblRec;

                // Request which table record to open
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter which space to create the line in ";
                pKeyOpts.Keywords.Add("Model");
                pKeyOpts.Keywords.Add("Paper");
                pKeyOpts.Keywords.Add("Current");
                pKeyOpts.AllowNone = false;
                pKeyOpts.AppendKeywordsToMessage = true;

                PromptResult pKeyRes = acDoc.Editor.GetKeywords(pKeyOpts);

                if (pKeyRes.StringResult == "Model")
                {
                    // Get the ObjectID for Model space from the Block table
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;
                }
                else if (pKeyRes.StringResult == "Paper")
                {
                    // Get the ObjectID for Paper space from the Block table
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;
                }
                else
                {
                    // Get the ObjectID for the current space from the database
                    acBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId,
                                                    OpenMode.ForWrite) as BlockTableRecord;
                }

                // Create a line that starts at 2,5 and ends at 10,7
                using (Line acLine = new Line(new Point3d(2, 5, 0),new Point3d(10, 7, 0)))
                {
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new line to the database
                acTrans.Commit();
            }
        }
    }
}
