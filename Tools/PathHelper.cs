using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathHelper
{
    public static string SaveDatabasePath { get { return Application.persistentDataPath + "/Database"; } }
    public static string TemplateDatabasePath { get { return Application.streamingAssetsPath + "/Database/template.db"; } }

}
