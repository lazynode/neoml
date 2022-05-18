using System.Xml.Linq;
using Neo;
using neoml;

switch (Environment.GetEnvironmentVariable("OUTPUT"))
{
    case "BIN":
        Console.OpenStandardInput().pipe(XElement.Load).finalize().write();
        break;
    case "NEF":
        Console.OpenStandardInput().pipe(XElement.Load).nef().write();
        break;
    case "MANIFEST":
        Console.OpenStandardInput().pipe(XElement.Load).manifest().print();
        break;
    case "BASE64":
        Console.OpenStandardInput().pipe(XElement.Load).finalize().pipe(Convert.ToBase64String).print();
        break;
    case "HEX":
    case null:
        Console.OpenStandardInput().pipe(XElement.Load).finalize().ToHexString().print();
        break;
    default:
        throw new Exception();
}
