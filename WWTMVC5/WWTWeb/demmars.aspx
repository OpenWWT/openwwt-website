<%@ Page Language="C#" ContentType="application/octet-stream" CodeFile="DemMars.aspx.cs" Inherits="WWTMVC5.WWTWeb.DemMars" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="System.Drawing.Text" %>
<%@ Import Namespace="System.Drawing.Imaging" %>
<%@ Import Namespace="System.Drawing.Drawing2D" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Security.Cryptography" %>  
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="WWTWebservices" %>d
<%

    string query = Request.Params["Q"];
    string[] values = query.Split(',');   
    int level = Convert.ToInt32(values[0]);
    int tileX = Convert.ToInt32(values[1]);
    int tileY = Convert.ToInt32(values[2]);

    if (level < 18)
    {
        Stream s = PlateFile2.GetFileStream(@"\\wwtfiles.file.core.windows.net\wwtmars\MarsDem\marsToastDem.plate", -1, level, tileX, tileY);
        
        if (s == null || (int)s.Length == 0)
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.Write("No image");
            Response.End();
            return;
        }

        int length = (int)s.Length;
        byte[] data = new byte[length];
        s.Read(data, 0, length);
        Response.OutputStream.Write(data, 0, length);
        Response.Flush();
        Response.End();
        return;
    }

%>