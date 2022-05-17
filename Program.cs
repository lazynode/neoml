using System.Xml.Linq;
using neoml;

Console.OpenStandardOutput().Write(XElement.Load(Console.OpenStandardInput()).compile().finalize());
