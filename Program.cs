using System.Xml.Linq;
using Neo;
using neoml;

switch (Environment.GetEnvironmentVariable("OUTPUT"))
{
    case "DEBUG":
        Console.OpenStandardInput().pipe(XElement.Load).compile().print();
        break;
    case "BIN":
        Console.OpenStandardInput().pipe(XElement.Load).compile().finalize().write();
        break;
    case "NEF":
        Console.OpenStandardInput().pipe(XElement.Load).compile().nef().write();
        break;
    case "MANIFEST":
        Console.OpenStandardInput().pipe(XElement.Load).compile().manifest().print();
        break;
    case "BASE64":
        Console.OpenStandardInput().pipe(XElement.Load).compile().finalize().pipe(Convert.ToBase64String).print();
        break;
    case "HEX":
    case null:
        Console.OpenStandardInput().pipe(XElement.Load).compile().finalize().ToHexString().print();
        break;
    default:
        throw new Exception();
}
