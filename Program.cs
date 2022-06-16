using System.Xml.Linq;
using neoml;

Console.OpenStandardInput().pipe(XElement.Load).compile().output(Environment.GetEnvironmentVariable("FMT"));
