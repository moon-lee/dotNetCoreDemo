using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace genPanoSkin
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Running Application...");

            XElement xElem = GetPanoSkinFile();

            // XElement xmlTree2 = new XElement("element",
            //                     from el in xElem.Elements()
            //                     where ((string)el.Element("type") == "mark")
            //                     select el);

            XElement xmlTree2 = new XElement("element",
                                from el in xElem.Elements().Descendants()
                                where (string)el.Element("type") == "mark"
                                select el);

            Console.WriteLine(xmlTree2);
            
            // XElement oldTourElements = xElem.Element("tour");
            // oldTourElements.ReplaceWith(GenerateNewPanoProjectFile());
            SaveXmlfile(xmlTree2);      

            Console.WriteLine("Job done...");
        }

        private static XElement GetPanoSkinFile()
        {

            XElement xElem;

            string path = AppConfig.SkinFile;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    xElem = XElement.Load(sr);
                }
            }

            return xElem;

        }


        static XElement GenerateNewPanoProjectFile()
        {
            string path = AppConfig.PanoDataFile;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    Panodata pd = new Panodata();

                    XElement tourElem = new XElement("tour");
                    XElement startElem = new XElement("start", "node1");
                    tourElem.Add(startElem);

                    XElement childElems = null;
                    // Skip First Line 
                    sr.ReadLine();

                    while ((line = sr.ReadLine()) != null)
                    {
                        pd = ReadPanoData(pd, line);
                        childElems = GeneratePanoElements(childElems,pd);
                        tourElem.Add(childElems);
                    }
   
                   // Console.WriteLine(tourElem);

                    return tourElem;
                }
            }
        }


        static Panodata ReadPanoData(Panodata pd, String panodata)
        {

            var delimiters = new char[] { ',' };
            var items = panodata.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            pd.nodeId = items[1];
            pd.filename = items[2];
            pd.title = items[3];
            pd.panoNorth = int.Parse(items[4].ToString());
            pd.viewPan = int.Parse(items[5].ToString());

            pd.forwardSpot = bool.Parse(items[6].ToString());
            pd.backwardSpot = bool.Parse(items[7].ToString());
            pd.leftSpot = bool.Parse(items[8].ToString());
            pd.rightSpot = bool.Parse(items[9].ToString());
            pd.fwdNode = items[10];
            pd.bwdNode = items[11];

            pd.leftNode = items[12];
            pd.rightNode = items[13];

            Console.WriteLine("Node Id :{0}", pd.nodeId);
            // Console.WriteLine("Image filename :{0}",pd.filename);
            // Console.WriteLine("Pano Title :{0}",pd.title);
            // Console.WriteLine("Pano North :{0}",pd.panoNorth);
            // Console.WriteLine("Viewing Pan :{0}",pd.viewPan);
            // Console.WriteLine("Spot on forward set to {0}",pd.forwardSpot);
            // Console.WriteLine("Spot on backward set to {0}",pd.backwardSpot);
            // Console.WriteLine("Forward Node set to {0}",pd.fwdNode);
            // Console.WriteLine("Backward Node set to {0}",pd.bwdNode);

            return pd;

        }


        static XElement GeneratePanoElements(XElement childElems, Panodata pd)
        {
            XElement panorameElem = new XElement("panorama");
            XElement panoIdElem = new XElement("id", pd.nodeId);

            XElement inputElem = new XElement("input",
                                 new XElement("type", "auto"),
                                 new XElement("filename", pd.filename));

            XElement viewingElem = new XElement("viewingparameter",
                                   new XElement("pan", new XElement("start", pd.viewPan)),
                                   new XElement("tilt", new XElement("start", 0)),
                                   new XElement("fov", new XElement("start", 100)),
                                   new XElement("panonorth", pd.panoNorth),
                                   new XElement("projection", "rectilinear"));

            XElement userdataElem = new XElement("userdata",
                                    new XElement("title", pd.title));

            //Hotspots

            string spotxml = "<hotspots>";

            if (pd.forwardSpot)
            {
                spotxml += String.Format(pd.fwdXml, pd.fwdNode);
            }
            if (pd.backwardSpot)
            {
                spotxml += String.Format(pd.bwdXml, pd.bwdNode);
            }
            if (pd.leftSpot)
            {
                spotxml += String.Format(pd.leftXml, pd.leftNode);
            }
            if (pd.rightSpot)
            {
                spotxml += String.Format(pd.rightXml, pd.rightNode);
            }

            spotxml += "</hotspots>";
            
            XElement hotspotsElem = XElement.Parse(spotxml);

            panorameElem.Add(panoIdElem);
            panorameElem.Add(inputElem);
            panorameElem.Add(viewingElem);
            panorameElem.Add(userdataElem);
            panorameElem.Add(hotspotsElem);

            return childElems = panorameElem;

            // Console.WriteLine(tourElem);
        }


       static void SaveXmlfile(XElement xmlDoc)
        {
            string path = AppConfig.NewSkinFile;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sr = new StreamWriter(fs))
                {
                    XmlWriter writer = XmlWriter.Create(sr);

                    xmlDoc.Save(sr);
                }
            }
        }
    }

    public class Panodata
    {
        public string nodeId;
        public string filename;
        public string title;
        public int panoNorth;
        public int viewPan;
        public Boolean forwardSpot;
        public Boolean backwardSpot;
        public Boolean leftSpot;
        public Boolean rightSpot;
        public string fwdNode;
        public string bwdNode;
        public string leftNode;
        public string rightNode;



        public string fwdXml = "<hotspot><position><pan>0.00</pan><tilt>-35.00</tilt></position><polygon/><type>point</type><id>FwdPoint</id><linktype>node</linktype><url>{{{0}}}</url><target>$fwd</target><skinid>ht_node_forward</skinid></hotspot>";

        public string bwdXml = "<hotspot><position><pan>180.00</pan><tilt>-35.00</tilt></position><polygon/><type>point</type><id>BwdPoint</id><linktype>node</linktype><url>{{{0}}}</url><target>$fwd</target><skinid>ht_node_forward</skinid></hotspot>";

        public string leftXml = "<hotspot><position><pan>90.00</pan><tilt>-35.00</tilt></position><polygon/><type>point</type><id>leftPoint</id><linktype>node</linktype><url>{{{0}}}</url><target>$fwd</target><skinid>ht_node_forward</skinid></hotspot>";
        public string rightXml = "<hotspot><position><pan>-90.00</pan><tilt>-35.00</tilt></position><polygon/><type>point</type><id>rightPoint</id><linktype>node</linktype><url>{{{0}}}</url><target>$fwd</target><skinid>ht_node_forward</skinid></hotspot>";

    }


    public static class AppConfig
    {
        private static string skinFile = @"skin.xml";
        private static string panoDataFile = @"PanoData.csv";
        private static string newSkinFile = @"NewSkin.xml";

        public static string SkinFile { get => skinFile; set => skinFile = value; }
        public static string PanoDataFile { get => panoDataFile; set => panoDataFile = value; }
        public static string NewSkinFile { get => newSkinFile; set => newSkinFile = value; }
    }
}