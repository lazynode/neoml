using System.Xml.Linq;
using System.Reflection;
using Neo;
namespace neoml;
static class Compiler
{
    public const string LAZY = "lazy";
    public static Dictionary<string, Type> LANGUAGES = Assembly.GetAssembly(typeof(Compiler))!.GetTypes().Where(v => v.Namespace == "neoml.language").ToDictionary(v => v.Name.ToUpper());
    public static void compile(this XElement node)
    {
        node.Elements().ToList().ForEach(compile);
        if (node.Name.NamespaceName.Any()) LANGUAGES[node.Name.NamespaceName.ToUpper()].GetMethod(node.Name.LocalName.ToUpper())!.Invoke(null, new object?[] { node });
        node.Name = LAZY;
        node.RemoveAttributes();
    }
    public static byte[] eval(this XElement node)
    {
        node.compile();
        return node.Descendants().Where(v => !v.Elements().Any()).Aggregate(Enumerable.Empty<byte>(), (sb, v) => sb.emit(v)).ToArray();
    }
    public static XElement set(this XElement node, XName name, object val)
    {
        node.SetAttributeValue(name, val);
        return node;
    }
    public static string? attr(this XElement node, XName name) => node.Attribute(name)?.Value;
    public static void to(this XElement node, XElement parent) => parent.Add(node);
    public static IEnumerable<byte> emit(this IEnumerable<byte> sb, XElement node)
    {
        if (node.Name.NamespaceName != "") throw new Exception();
        switch (node.Name.LocalName)
        {
            case "frag":
                return sb.Concat(node.attr("data").HexToBytes());
            case "jump":
                // todo
                throw new Exception();
            default:
                return sb;
        }
    }
}
