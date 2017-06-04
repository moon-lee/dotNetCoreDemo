using System;
using System.IO;
using System.Xml;

namespace genPanoSkin
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Running Application...");

            /*
                This is the re-program procedures

                1. Read skin xml template

                2. Replace some of nodes 

                3. Clone the element node

                4. Replay until last pano data
             */

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc = GetPanoProject(xmlDoc);

            XmlElement oldRootNode = xmlDoc.DocumentElement;

            /*
                Location Path Expression

                A step consists of:

                    * an axis (defines the tree-relationship between the selected nodes and the current node)
                    * a node-test (identifies a node within an axis)
                    * zero or more predicates (to further refine the selected node-set)

                The syntax for a location step is:

                axisname::nodetest[predicate]

             */
            XmlNode oldTourNode = oldRootNode.SelectSingleNode("tour");
            XmlNode newTourNode = CreateNewPanoProjectFile(xmlDoc);

            oldTourNode.ParentNode.ReplaceChild(newTourNode, oldTourNode);

            WriteXmlfile(xmlDoc);

            Console.WriteLine("Job done...");
        }

        static XmlDocument GetPanoProject(XmlDocument xmlDoc)
        {
            string path = GetP2vrTempFile();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    xmlDoc.Load(sr);
                }
            }
            return xmlDoc;
        }

        static XmlNode CreateNewPanoProjectFile(XmlDocument xmlDoc)
        {
            string path = GetPanoDataFile();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    // Create New Tour Node 
                    XmlNode childRootNode = xmlDoc.CreateElement("tour");

                    // Set Start Tour Node
                    XmlNode startNode = xmlDoc.CreateElement("start");
                    startNode.InnerText = "node1";
                    childRootNode.AppendChild(startNode);

                    Panodata pd = new Panodata();

                    // Skip First Line 
                    sr.ReadLine();

                    while ((line = sr.ReadLine()) != null)
                    {
                        pd = ReadPanoData(pd, line);
                        childRootNode.AppendChild(GeneratePanoXml(xmlDoc, pd));

                    }

                    return childRootNode;
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

        static XmlNode GeneratePanoXml(XmlDocument xmlDoc, Panodata pd)
        {

            XmlNode childNodes = xmlDoc.CreateElement("panorama");

            XmlNode panoId = xmlDoc.CreateElement("id");
            panoId.InnerText = pd.nodeId;

            // Input
            XmlNode inputNode = xmlDoc.CreateElement("input");
            inputNode.InnerXml = String.Format("<type>auto</type><filename>{0}</filename>", pd.filename);

            //Viewingparameter
            XmlNode viewParam = xmlDoc.CreateElement("viewingparameter");
            viewParam.InnerXml = String.Format("<pan><start>{0}</start></pan><tilt><start>0</start></tilt><fov><start>100</start></fov><panonorth>{1}</panonorth><projection>rectilinear</projection>", pd.viewPan, pd.panoNorth);

            //Userdata
            XmlNode userdataNode = xmlDoc.CreateElement("userdata");
            userdataNode.InnerXml = String.Format("<title>{0}</title>", pd.title);

            //Hotspots
            XmlNode hotspotsNode = xmlDoc.CreateElement("hotspots");

            string spotxml = hotspotsNode.InnerXml;

            if (pd.forwardSpot)
            {
                spotxml = String.Format(pd.fwdXml, pd.fwdNode);
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

            hotspotsNode.InnerXml = spotxml;

            childNodes.AppendChild(panoId);
            childNodes.AppendChild(inputNode);
            childNodes.AppendChild(viewParam);
            childNodes.AppendChild(userdataNode);
            childNodes.AppendChild(hotspotsNode);

            return childNodes;
        }

        static void WriteXmlfile(XmlDocument xmlDoc)
        {
            string path = GetNewP2vrFile();

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sr = new StreamWriter(fs))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(sr, settings);
                    xmlDoc.Save(writer);
                }
            }
        }

        static string GetP2vrTempFile()
        {
            string file = Directory.GetCurrentDirectory();
            file += @"\template\PanoProjectTemplate.p2vr";
            return file;
        }

        static string GetPanoDataFile()
        {
            string file = Directory.GetCurrentDirectory();
            file += @"\input\PanoData.csv";
            return file;
        }

        static string GetNewP2vrFile()
        {
            string file = Directory.GetCurrentDirectory();
            file += @"\output\NewPanoProject.p2vr";
            return file;
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

}