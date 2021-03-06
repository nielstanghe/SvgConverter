﻿
namespace ApertureService
{


    static class Program
    {


        public static ApertureCrap.ApScriptingService GetApertureWebService()
        {
            ApertureCrap.ApScriptingService ass = new ApertureCrap.ApScriptingService();
            ass.Url = "https://www6.cor-asp.ch/ApWebServices/ApScriptingService.asmx";
            // ass.Url = "http://vmwhatever/ApWebServices/ApScriptingService.asmx";
            
            return ass;
        } // End Function GetApertureWebService 


        public static ApertureBounds GetBounds(string d, string projname)
        {
            ApertureCrap.ApScriptingService ass = GetApertureWebService();

            string cmdname = "DrawingBounds";
            string parameters = "<DrawingName>" + d + "</DrawingName>";

            string strBounds = ass.ExecuteCommand(cmdname, projname, parameters);
            return new ApertureBounds(strBounds);
        } // End Function GetBounds 

        
        public static string ClickDrawing(string d, string projname)
        {

            ApertureBounds b = GetBounds(d, projname);
            System.Console.WriteLine(b);

            return ClickDrawing(d, projname, b);
        } // End Function ClickDrawing 


        public static double GetFitFactor(double svgWidth, double svgHeight, double boxWidth, double boxHeight)
        {
            // TODO: Check for arguments (for null and <=0)
            var widthScale = boxWidth / (double)svgWidth;
            var heightScale = boxHeight / (double)svgHeight;
            double scale = (double) System.Math.Min(widthScale, heightScale);
            return scale;
        } // End Function GetFitFactor 


        // Nur mit IE
        // https://www6.cor-asp.ch/FM_COR_Demo/test/DWG.html
        public static string ClickDrawing(string d, string projname, ApertureBounds b)
        {
            string l = "RaumNutzung"; // Layerset

            double w = 800; // Bildgrösse 
            double h = 600; // Bildgrösse 

            double x = 336; // Click-Position
            double y = 292; // Click-Position

            x = 625;
            y = 182;


            

            // SVG
            w = 2098; // Bildgrösse 
            h = 2969; // Bildgrösse 

            w = 790;
            h = 1121;

            x = 190;
            y = 490;

            //double mx = w / 2.0;
            //double my = h / 2.0;

            //x = mx - x;
            //y = my - y;
            

            //double s = GetFitFactor(2098, 2969, 800, 600);
            //double sx = 800.0 / w;
            //double sy = 600.0 / h;

            //x = x * sx;
            //y = y * sy;

            //w = 800;
            //h = 600;



            ApertureCrap.ApScriptingService ass = GetApertureWebService();


            // string mImageLinkTemplate = "{@domain}/ApWebServices/ApDrawingImages.aspx?p={@project}&d={@dwg}&xlr={@r}&ylr={@b}&yul={@t}&xul={@l}&dx={@width}&dy={@height}&L={@layerset}&S={@stylizer}&SEL={@obj}&F=PNG&uu={@cache}";
            string mParamClickTemplate = "<InParams><Drawing>{@dwg}</Drawing><l>{@l}</l><r>{@r}</r><t>{@t}</t><b>{@b}</b><Width>{@width}</Width><Height>{@height}</Height><ClickX>{@x}</ClickX><ClickY>{@y}</ClickY><LayerSet>{@layerset}</LayerSet><RecTypes>0</RecTypes></InParams>";


            string parameters = mParamClickTemplate;
            parameters = parameters
                .Replace("{@dwg}", d)
                .Replace("{@r}", b.sR.Replace(",", "."))

#if true // interchange b & t because Aperture is buggy...
                .Replace("{@b}", b.sT.Replace(",", "."))
                .Replace("{@t}", b.sB.Replace(",", "."))
#else
                .Replace("{@b}", b.sB.Replace(",", "."))
                .Replace("{@t}", b.sT.Replace(",", "."))
#endif

                .Replace("{@l}", b.sL.Replace(",", "."))
                .Replace("{@width}", ToAwsString(w))
                .Replace("{@height}", ToAwsString(h))
                .Replace("{@layerset}", l)
                .Replace("{@y}", ToAwsString(y))
                .Replace("{@x}", ToAwsString(x))
                // .Replace("<", "&lt;")
                // .Replace(">", "&gt;")
                ;


            string cmdname = "_DrawingClick";
            string strRes = ass.ExecuteCommand(cmdname, projname, parameters);
            System.Console.WriteLine(strRes);


			System.Xml.XmlDocument doc = new System.Xml.XmlDocument ();
			doc.LoadXml (strRes);

			System.Xml.XmlNode nd = doc.SelectSingleNode ("//ObjectID");
			System.Console.WriteLine (nd);

			string strApertureObjId = null;
			if(nd != null)
				strApertureObjId = nd.InnerText;

			return strApertureObjId;
        } // End Function ClickDrawing 


        // To ApertureWebServiceString
        public static string ToAwsString(double d)
        {
            return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        } // End Function ToAwsString 


        public static string ModifyUrl(string url)
        {
            System.Collections.Specialized.NameValueCollection originalQuery = System.Web.HttpUtility.ParseQueryString(new System.Uri(url).Query);
            System.Uri uri = new System.Uri(originalQuery["path"].Replace("$amp$", "&"));
            System.UriBuilder builder = new System.UriBuilder(uri);
            
            System.Collections.Specialized.NameValueCollection nameValues = System.Web.HttpUtility.ParseQueryString(uri.Query); //.Get("param1");
            nameValues.Remove("uu");
            nameValues.Remove("F");
            nameValues.Remove("dx");
            nameValues.Remove("dy");


            // string projname = "COR_Demo_Portal";
            string projname = nameValues["p"];
            // string d = "0001_GB01_OG14_0000";
            string d = nameValues["d"];


            ApertureBounds apb = GetBounds(d, projname);
            System.Console.WriteLine(apb);

            nameValues["xul"] = apb.XUL.ToString();
            nameValues["yul"] = apb.YUL.ToString();

            nameValues["xlr"] = apb.XLR.ToString();
            nameValues["ylr"] = apb.YLR.ToString();

            builder.Query = nameValues.ToString();
            System.Uri result = builder.Uri;
            string newUrl = result.OriginalString;
            newUrl = newUrl.Replace(":80/", "/");

            return newUrl;
        }


        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
#if false
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
#endif

            string url = @"https://www6.cor-asp.ch/COR_Basic_Demo/ajax/PdfLegende.ashx?name=0001_GB01_OG14_0000&path=https://www6.cor-asp.ch/ApWebServices/ApDrawingPDFs.aspx?p=COR_Demo_Portal$amp$d=0001_GB01_OG14_0000$amp$xlr=236.36265042399265$amp$ylr=165.386215$amp$yul=134.932205$amp$xul=184.29521757600736$amp$dx=1867$amp$dy=1092$amp$L=RaumNutzung$amp$S=Nutzungsart$amp$SEL=0000000002FX0003D4$amp$F=PNG$amp$uu=2&l=Nutzungsart&dwg=0001_GB01_OG14_0000&lycode=Raum%20Nutzungsart&BE_ID=12435&legendvisible=false";

            string urlMod = ModifyUrl(url);
            System.Console.WriteLine(urlMod);


            string projname = "COR_Demo_Portal";
            string d = "0001_GB01_OG14_0000";
            
            projname = "PortalName";
            d = "AP_DWG";
            
            ApertureBounds apb = GetBounds(d, projname);
            System.Console.WriteLine(apb);

            ClickDrawing(d, projname);
            // TransformPath ();
        } // End Sub Main 


    } // End Class Program 


} // End Namespace ApertureService 
