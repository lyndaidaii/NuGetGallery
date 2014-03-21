﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace TestNuspec
{
    public class XsltHelper
    {
        public XPathNavigator Split(string original)
        {
            char[] trimChar = { ',', ' ', '\t', '|', ';' };

            IEnumerable<string> fields = original
                .Split(trimChar)
                .Select((w) => w.Trim(trimChar))
                .Where((w) => w.Length > 0);

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("list");
            xmlDoc.AppendChild(root);

            foreach (string s in fields)
            {
                XmlElement element = xmlDoc.CreateElement("item");
                element.InnerText = s;
                root.AppendChild(element);
            }

            return xmlDoc.CreateNavigator();
        }
    }

    class Program
    {
        static XslCompiledTransform CreateTransform(string path)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            transform.Load(XmlReader.Create(new StreamReader(path)));
            return transform;
        }

        public static XDocument NormalizeNuspecNamespace(XDocument original)
        {
            string path = @"C:\private\NuGet3\NuGet.Services.Metadata\src\GatherMergeRewrite\GatherMergeRewrite\xslt\normalizeNuspecNamespace.xslt";

            XDocument result = new XDocument();

            using (XmlWriter writer = result.CreateWriter())
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(XmlReader.Create(new StreamReader(path)));
                xslt.Transform(original.CreateReader(), writer);
            }

            return result;
        }

        static XDocument LoadNuspec()
        {
            XDocument nuspec = XDocument.Load(new StreamReader(@"C:\data\nupkgs\dotnetrdf.0.5.0.nuspec"));
            return nuspec;
        }

        public static XDocument GetNuspec(Package package)
        {
            foreach (PackagePart part in package.GetParts())
            {
                if (part.Uri.ToString().EndsWith(".nuspec"))
                {
                    XDocument nuspec = XDocument.Load(part.GetStream());
                    return nuspec;
                }
            }
            throw new FileNotFoundException("nuspec");
        }

        public static Package GetPackage(Stream stream)
        {
            Package package = Package.Open(stream);
            return package;
        }

        public static XDocument GetNuspecFromPackage(string filename)
        {
            Stream stream = new FileStream(filename, FileMode.Open);
            Package package = GetPackage(stream);
            XDocument awkwardNuspec = GetNuspec(package);
            XDocument nuspec = NormalizeNuspecNamespace(awkwardNuspec);
            return nuspec;
        }

        static void Main(string[] args)
        {
            string path = @"C:\private\NuGet3\NuGet.Services.Metadata\src\GatherMergeRewrite\GatherMergeRewrite\xslt\nuspec.xslt";

            XslCompiledTransform transform = CreateTransform(path);

            string baseAddress = "http://tempuri.org/base/";

            XsltArgumentList arguments = new XsltArgumentList();
            arguments.AddParam("base", "", baseAddress);
            arguments.AddExtensionObject("urn:helper", new XsltHelper());

            //foreach (KeyValuePair<string, string> arg in transformArgs)
            //{
            //    arguments.AddParam(arg.Key, "", arg.Value);
            //}

            //XDocument nuspec = GetNuspecFromPackage(@"C:\data\nupkgs\dotnetrdf.0.5.0.nupkg");
            XDocument nuspec = GetNuspecFromPackage(@"C:\data\nupkgs\dotnetrdf.0.8.0.nupkg");
            //XDocument nuspec = GetNuspecFromPackage(@"C:\data\nupkgs\dotnetrdf.1.0.3.nupkg");

            XDocument rdfxml = new XDocument();
            using (XmlWriter writer = rdfxml.CreateWriter())
            {
                transform.Transform(nuspec.CreateReader(), arguments, writer);
            }

            Console.WriteLine(rdfxml);

            //Console.WriteLine();

            RdfXmlParser rdfXmlParser = new RdfXmlParser();
            XmlDocument doc = new XmlDocument();
            doc.Load(rdfxml.CreateReader());
            IGraph graph = new Graph();
            rdfXmlParser.Load(graph, doc);

            string subject = graph.GetTriplesWithPredicateObject(
                    graph.CreateUriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type")),
                    graph.CreateUriNode(new Uri("http://nuget.org/schema#Package")))
                .First()
                .Subject
                .ToString();

            CompressingTurtleWriter turtleWriter = new CompressingTurtleWriter();
            turtleWriter.DefaultNamespaces.AddNamespace("nuget", new Uri("http://nuget.org/schema#"));
            turtleWriter.DefaultNamespaces.AddNamespace("package", new Uri(subject + "#"));

            turtleWriter.PrettyPrintMode = true;
            turtleWriter.CompressionLevel = 10;
            turtleWriter.Save(graph, Console.Out);
        }
    }
}
