using System.Xml.Linq;
using Neo;
using Neo.VM;
using Neo.IO;
using Neo.SmartContract;
using Neo.IO.Json;

namespace neoml;
static class Output
{
    public const string LAZY = "lazy";
    public static byte[] finalize(this XElement node) => node.leaves().Aggregate(Enumerable.Empty<byte>(), (sb, v) => sb.withassert(v.Name.NamespaceName.Length == 0).emit(v)).ToArray();
    public static string meta(this XElement node, string key, string def = "") => node.leaves().Where(v => v.Name.LocalName == "meta").SingleOrDefault()?.attr(key) ?? def;
    public static JString[] supportedstandards(this XElement node) => node.leaves().Where(v => v.Name.LocalName == "std").Select(v => (JString)v.attr("std")!).ToArray();
    public static JObject abi(this XElement node) => new JObject { }; // TODO
    public static JObject[] permissions(this XElement node) => new JObject[] { }; // TODO
    public static JObject[] trusts(this XElement node) => new JObject[] { }; // TODO
    public static JObject extra(this XElement node) => node.leaves().Where(v => v.Name.LocalName == "meta").SingleOrDefault()?.Value?.pipe(v => JObject.Parse(v)) ?? new JObject();
    public static MethodToken[] methodtokens(this XElement node) => new MethodToken[] { };
    public static byte[] nef(this XElement node) => new NefFile() { Compiler = node.meta("compiler", "neoml"), Source = node.meta("src"), Tokens = node.methodtokens(), Script = node.finalize() }.with(v => { v.CheckSum = NefFile.ComputeChecksum(v); }).ToArray();
    public static string manifest(this XElement node) => new JObject() { ["name"] = node.meta("name"), ["groups"] = new JArray(), ["features"] = new JObject(), ["supportedstandards"] = node.supportedstandards(), ["abi"] = node.abi(), ["permissions"] = node.permissions(), ["trusts"] = node.trusts(), ["extra"] = node.extra() }.ToString();
    // ["abi"] = new JObject
    // {
    //     ["methods"] = methodsExported.Select(p => new JObject
    //     {
    //         ["name"] = p.Name,
    //         ["offset"] = GetAbiOffset(p.Symbol),
    //         ["safe"] = p.Safe,
    //         ["returntype"] = p.ReturnType,
    //         ["parameters"] = p.Parameters.Select(p => p.ToJson()).ToArray()
    //     }).ToArray(),
    //     ["events"] = eventsExported.Select(p => new JObject
    //     {
    //         ["name"] = p.Name,
    //         ["parameters"] = p.Parameters.Select(p => p.ToJson()).ToArray()
    //     }).ToArray()
    // },
    public static IEnumerable<byte> emit(this IEnumerable<byte> sb, XElement node)
    {
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
