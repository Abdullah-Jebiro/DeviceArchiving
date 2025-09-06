using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Data.SqlClient;

namespace DeviceArchiving.Service;

public class DatabaseFileChecker
{
    public static void CheckAndConnect(string pathDb)
    {

        if (!File.Exists(pathDb))
        {
            throw new FileNotFoundException("ملف قاعدة البيانات غير موجود.", pathDb);
        }
    
    }
}