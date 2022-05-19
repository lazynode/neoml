using System.Xml.Linq;
using Neo;
using Neo.VM;
using Neo.IO;
using Neo.SmartContract;
using Neo.IO.Json;

namespace neoml;
static class Output
{
    public static Dictionary<string, string> TYPEFIX = new() { { "int", "Integer" }, { "integer", "Integer" }, { "Integer", "Integer" }, { "bool", "Boolean" }, { "boolean", "Boolean" }, { "Boolean", "Boolean" }, { "null", "Any" }, { "any", "Any" }, { "Any", "Any" }, { "bytes", "ByteArray" }, { "bytearray", "ByteArray" }, { "ByteArray", "ByteArray" }, { "string", "String" }, { "String", "String" }, { "Hash160", "Hash160" }, { "hash160", "Hash160" }, { "Hash256", "Hash256" }, { "hash256", "Hash256" }, { "publickey", "PublicKey" }, { "PublicKey", "PublicKey" }, { "signature", "Signature" }, { "Signature", "Signature" }, { "array", "Array" }, { "Array", "Array" }, { "map", "Map" }, { "Map", "Map" }, { "interopinterface", "InteropInterface" }, { "InteropInterface", "InteropInterface" }, { "void", "Void" }, { "Void", "Void" } };
    public static byte[] finalize(this XElement node) => node.DescendantsAndSelf().Aggregate(Enumerable.Empty<byte>(), (sb, v) => sb.withassert(v.Name.NamespaceName.Length == 0).emit(v)).ToArray();
    public static string meta(this XElement node, string key, string def = "") => node.filter("meta").SingleOrDefault()?.a(key) ?? def;
    public static JString[] supportedstandards(this XElement node) => node.filter("std").Select(v => (JString)v.a("std")!).ToArray();
    public static JObject abi(this XElement node) => new JObject { ["methods"] = node.filter("func").Select(v => v.func()).ToArray(), ["events"] = node.filter("event").Select(v => v.evt()).ToArray() };
    public static JObject func(this XElement node) => new JObject { ["name"] = node.a("name"), ["offset"] = node.position(), ["safe"] = node.a("safe")?.pipe(bool.Parse) ?? false, ["returntype"] = node.a("return")!.pipe(v => TYPEFIX[v]), ["parameters"] = node.parameters() };
    public static JObject evt(this XElement node) => new JObject { ["name"] = node.a("name"), ["parameters"] = node.parameters() };
    public static int position(this XElement node) => node.root().DescendantsAndSelf().TakeWhile(v => v != node).Select(v => v.size()).Sum();
    public static JObject[] parameters(this XElement node) => node.filter("arg").Select(v => new JObject { ["name"] = v.a("name"), ["type"] = v.a("type")!.pipe(v => TYPEFIX[v]) }).ToArray();
    public static JObject[] permissions(this XElement node) => new JObject[] { new JObject { ["contract"] = "*", ["methods"] = "*" } }; // TODO: IMPL
    public static JObject[] trusts(this XElement node) => new JObject[] { }; //TODO: IMPL
    public static JObject extra(this XElement node) => node.filter("meta").SingleOrDefault()?.Value?.pipe(v => JObject.Parse(v)) ?? new JObject();
    public static MethodToken[] methodtokens(this XElement node) => new MethodToken[] { }; // TODO: IMPL
    public static byte[] nef(this XElement node) => new NefFile() { Compiler = node.meta("compiler", "neoml"), Source = node.meta("src"), Tokens = node.methodtokens(), Script = node.finalize() }.with(v => { v.CheckSum = NefFile.ComputeChecksum(v); }).ToArray();
    public static string manifest(this XElement node) => new JObject() { ["name"] = node.meta("name"), ["groups"] = new JArray(), ["features"] = new JObject(), ["supportedstandards"] = node.supportedstandards(), ["abi"] = node.abi(), ["permissions"] = node.permissions(), ["trusts"] = node.trusts(), ["extra"] = node.extra() }.ToString();
    public static IEnumerable<byte> emit(this IEnumerable<byte> sb, XElement node)
    {
        switch (node.Name.LocalName)
        {
            case "frag":
                return sb.Concat(node.a("data").HexToBytes());
            case "goto":
                return sb.Concat(new ScriptBuilder().Emit(node.a("opcode")!.pipe(Enum.Parse<OpCode>), BitConverter.GetBytes(node.root().DescendantsAndSelf().Where(v => v.a("id") == node.a("target")).Single().position() - node.position())).ToArray());
            default:
                return sb;
        }
    }
    public static int size(this XElement node)
    {
        switch (node.Name.LocalName)
        {
            case "frag":
                return node.a("data").HexToBytes().Length;
            case "goto":
                return 5;
            default:
                return 0;
        }
    }
}
