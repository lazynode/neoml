using System.Xml.Linq;
using Neo;
using Neo.VM;
using Neo.IO;
using Neo.SmartContract;
using Neo.IO.Json;

namespace neoml;
static class Output
{
    public static string fix(this string type) => type switch { "int" => "Integer", "integer" => "Integer", "Integer" => "Integer", "bool" => "Boolean", "boolean" => "Boolean", "Boolean" => "Boolean", "null" => "Any", "any" => "Any", "Any" => "Any", "bytes" => "ByteArray", "bytearray" => "ByteArray", "ByteArray" => "ByteArray", "string" => "String", "String" => "String", "Hash160" => "Hash160", "hash160" => "Hash160", "Hash256" => "Hash256", "hash256" => "Hash256", "publickey" => "PublicKey", "PublicKey" => "PublicKey", "signature" => "Signature", "Signature" => "Signature", "array" => "Array", "Array" => "Array", "map" => "Map", "Map" => "Map", "interopinterface" => "InteropInterface", "InteropInterface" => "InteropInterface", "void" => "Void", "Void" => "Void", _ => "Any" };
    public static byte[] bytes(this OpCode opcode, byte[]? operand = null) => new ScriptBuilder().Emit(opcode, operand).ToArray();
    public static string hex(this OpCode opcode, byte[]? operand = null) => opcode.bytes(operand).ToHexString();
    public static string push<T>(this T val) => new ScriptBuilder().EmitPush(val).ToArray().ToHexString();
    public static byte[] finalize(this XElement x) => x.DescendantsAndSelf().Aggregate(Enumerable.Empty<byte>(), (sb, v) => v.withassert(v.Name.NamespaceName.Length == 0).emit(sb)).ToArray();
    public static string meta(this XElement x, string key) => x.filter("meta").SingleOrDefault()?.a(key)!;
    public static JString[] supportedstandards(this XElement x) => x.filter("std").Select(v => (JString)v.a("std")!).ToArray();
    public static JObject abi(this XElement x) => new JObject { ["methods"] = x.filter("func").Select(v => v.func()).ToArray(), ["events"] = x.filter("event").Select(v => v.evt()).ToArray() };
    public static JObject func(this XElement x) => new JObject { ["name"] = x.a("name"), ["offset"] = x.position(), ["safe"] = x.a("safe")?.pipe(bool.Parse) ?? false, ["returntype"] = x.a("return")!, ["parameters"] = x.parameters() };
    public static JObject evt(this XElement x) => new JObject { ["name"] = x.a("name"), ["parameters"] = x.parameters() };
    public static int position(this XElement x) => x.root().DescendantsAndSelf().TakeWhile(v => v != x).Select(v => v.size()).Sum();
    public static JObject[] parameters(this XElement x) => x.filter("arg").Select(v => new JObject { ["name"] = v.a("name"), ["type"] = v.a("type")! }).ToArray();
    public static JObject[] permissions(this XElement x) => new JObject[] { new JObject { ["contract"] = "*", ["methods"] = "*" } }; // TODO: IMPL
    public static JObject[] trusts(this XElement x) => new JObject[] { }; //TODO: IMPL
    public static JObject extra(this XElement x) => x.filter("meta").SingleOrDefault()?.Value?.pipe(v => JObject.Parse(v)) ?? new JObject();
    public static MethodToken[] methodtokens(this XElement x) => new MethodToken[] { }; // TODO: IMPL
    public static byte[] nef(this XElement x) => new NefFile() { Compiler = x.meta("compiler"), Source = x.meta("src"), Tokens = x.methodtokens(), Script = x.finalize() }.with(v => { v.CheckSum = NefFile.ComputeChecksum(v); }).ToArray();
    public static string manifest(this XElement x) => new JObject() { ["name"] = x.meta("name"), ["groups"] = new JArray(), ["features"] = new JObject(), ["supportedstandards"] = x.supportedstandards(), ["abi"] = x.abi(), ["permissions"] = x.permissions(), ["trusts"] = x.trusts(), ["extra"] = x.extra() }.ToString();
    public static IEnumerable<byte> emit(this XElement x, IEnumerable<byte> sb) => x.Name.LocalName switch { "frag" => sb.Concat(x.a("data").HexToBytes()), "goto" => sb.Concat(new ScriptBuilder().Emit(x.a("opcode")!.pipe(Enum.Parse<OpCode>), BitConverter.GetBytes(x.root().DescendantsAndSelf().Where(v => v.a("id") == x.a("target")).Single().position() - x.position())).ToArray()), _ => sb };
    public static int size(this XElement x) => x.Name.LocalName switch { "frag" => x.a("data").HexToBytes().Length, "goto" => 5, _ => 0 };
}
