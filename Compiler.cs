using System.Xml.Linq;
using System.Reflection;
using Neo;
using Neo.VM;

namespace neoml;
static class Compiler
{
    public const string LAZY = "lazy";
    public static Dictionary<string, Type> LANGUAGES = Assembly.GetAssembly(typeof(Compiler))!.GetTypes().Where(v => v.Namespace == "neoml.language").ToDictionary(v => v.Name.ToUpper());
    public static XElement compile(this XElement node)
    {
        node.Elements().ToList().ForEach(v => v.compile());
        if (node.Name.NamespaceName.Any()) LANGUAGES[node.Name.NamespaceName.ToUpper()].GetMethod(node.Name.LocalName.ToUpper())!.Invoke(null, new object?[] { node });
        return node;
    }
    public static byte[] finalize(this XElement node) => node.Descendants().Where(v => !v.Elements().Any()).Aggregate(Enumerable.Empty<byte>(), (sb, v) => sb.emit(v)).ToArray();
    public static XElement set(this XElement node, XName name, object val)
    {
        node.SetAttributeValue(name, val);
        return node;
    }
    public static string? attr(this XElement node, XName name) => node.Attribute(name)?.Value;
    public static IEnumerable<byte> emit(this IEnumerable<byte> sb, XElement node)
    {
        if (node.Name.NamespaceName != "") throw new Exception();
        switch (node.Name.LocalName)
        {
            case "frag":
                return sb.Concat(node.attr("data").HexToBytes());
            case "goto":
                var opcode = Enum.Parse<OpCode>(node.attr("opcode")!);
                var descendants = node.AncestorsAndSelf().Last().Descendants().Where(v => !v.Elements().Any()).ToList();
                var i = descendants.FindIndex(v => v.attr("id") == node.attr("target"));
                var j = descendants.IndexOf(node);
                var n = descendants.Skip(Math.Min(i, j)).Take(Math.Max(i, j) - Math.Min(i, j)).Select(v => v.size()).Sum();
                return sb.Concat(new ScriptBuilder().Emit(opcode, BitConverter.GetBytes(i < j ? -n : n)).ToArray());
            default:
                return sb;
        }
    }
    public static int size(this XElement node)
    {
        switch (node.Name.LocalName)
        {
            case "frag":
                return node.attr("data").HexToBytes().Length;
            case "goto":
                return 5;
            default:
                return 0;
        }
    }
}
