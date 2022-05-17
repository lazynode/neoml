using System.Xml.Linq;
using Neo;
using neoml;

switch (Environment.GetEnvironmentVariable("OUTPUT"))
{
    case "BIN":
        Console.OpenStandardOutput().Write(XElement.Load(Console.OpenStandardInput()).compile().finalize());
        break;
    case "NEF":
        Console.OpenStandardOutput().Write(XElement.Load(Console.OpenStandardInput()).compile().finalize());
        break;
    case "MANIFEST":
        Console.WriteLine(XElement.Load(Console.OpenStandardInput()).compile().manifest());
        break;
    case "HEX":
    default:
        Console.WriteLine(XElement.Load(Console.OpenStandardInput()).compile().finalize().ToHexString());
        break;
}
