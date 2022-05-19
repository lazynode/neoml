using System.Numerics;
using System.Xml.Linq;
using Neo;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace neoml.language;

static partial class Assembly
{
    private static void META(XElement x, string name, string src, string compiler) => x.lazilize("meta").set("name", name).set("src", src).set("compiler", compiler);
    private static void STD(XElement x, string std) => x.lazilize("std").set("std", std);
    private static void ARG(XElement x, string name, string type) => x.lazilize("arg").set("name", name).set("type", type);
    private static void FUNC(XElement x, string name, string @return, bool safe) => x.lazilize("func").set("name", name).set("return", @return).set("safe", safe);
    private static void EVENT(XElement x, string name) => x.lazilize("event").set("name", name);

    public static void INSTRUCTION(XElement node)
    {
        var opcode = Enum.Parse<OpCode>((node.a("opcode") ?? "NOP").ToUpper());
        var operand = node.a("operand")?.HexToBytes();
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(opcode, operand).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void LITERAL(XElement node)
    {
        var type = node.a("type") ?? "null";
        var val = node.a("val") ?? "";
        var child = new XElement("frag");
        switch (type)
        {
            case "int":
                child.set("data", new ScriptBuilder().EmitPush(BigInteger.Parse(val)).ToArray().ToHexString());
                break;
            case "string":
                child.set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString());
                break;
            case "bytes":
                child.set("data", new ScriptBuilder().EmitPush(val.HexToBytes()).ToArray().ToHexString());
                break;
            case "bool":
                child.set("data", new ScriptBuilder().EmitPush(bool.Parse(val)).ToArray().ToHexString());
                break;
            case "null":
                child.set("data", new ScriptBuilder().Emit(OpCode.PUSHNULL).ToArray().ToHexString());
                break;
            case "hash160":
                child.set("data", new ScriptBuilder().EmitPush(UInt160.Parse(val)).ToArray().ToHexString());
                break;
            case "hash256":
                child.set("data", new ScriptBuilder().EmitPush(UInt256.Parse(val)).ToArray().ToHexString());
                break;
            case "address":
                child.set("data", new ScriptBuilder().EmitPush(val.ToScriptHash(ProtocolSettings.Default.AddressVersion)).ToArray().ToHexString());
                break;
            case "publickey":
                child.set("data", new ScriptBuilder().EmitPush(((ECPoint.Parse(val, ECCurve.Secp256r1)).EncodePoint(true))).ToArray().ToHexString());
                break;
            default:
                throw new Exception();
        }
        child.compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void COMPOUND(XElement node)
    {
        var type = node.a("type") ?? "array";
        var size = node.a("size")?.pipe(int.Parse) ?? node.Elements().Count();
        var items = new XElement("lazy", node.Descendants().Where(v => !v.Elements().Any()).Reverse().ToList()); // TODO
        var child = new XElement("frag");
        switch (type)
        {
            case "array":
                child.set("data", new ScriptBuilder().EmitPush(size).Emit(OpCode.PACK).ToArray().ToHexString());
                break;
            case "map":
                child.set("data", new ScriptBuilder().EmitPush(size).Emit(OpCode.PACKMAP).ToArray().ToHexString());
                break;
            case "struct":
                child.set("data", new ScriptBuilder().EmitPush(size).Emit(OpCode.PACKSTRUCT).ToArray().ToHexString());
                break;
            default:
                throw new Exception();
        }
        child.compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(items);
        node.Add(child);
    }
    public static void NOP(XElement node)
    {
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.NOP).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void TAG(XElement node)
    {
        var name = node.a("name") ?? "";
        var child = new XElement("lazy").set("id", name).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void GOTO(XElement node)
    {
        var cond = node.a("cond") ?? "";
        var target = node.a("target") ?? "";
        var child = new XElement("goto").set("opcode", Enum.Parse<OpCode>($"JMP{cond.ToUpper()}_L").ToString()).set("target", target).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void SKIP(XElement node)
    {
        var cond = node.a("cond") ?? "";
        Guid end = Guid.NewGuid();
        var tag = new XElement(ns + "tag").set("name", end).compile();
        var child = new XElement(ns + "goto").set("target", end).set("cond", cond).compile();
        node.RemoveAttributes();
        node.Name = "lazy";
        node.AddFirst(child);
        node.Add(tag);
    }
    public static void IF(XElement node)
    {
        node.RemoveAttributes();
        node.Name = ns + "skip";
        node.set("cond", "ifnot").compile();
    }
    public static void UNLESS(XElement node)
    {
        node.RemoveAttributes();
        node.Name = ns + "skip";
        node.set("cond", "if").compile();
    }
    public static void SYSCALL(XElement node)
    {
        var name = node.a("name");
        var child = new XElement("frag").set("data", new ScriptBuilder().EmitSysCall(new InteropDescriptor() { Name = name }.Hash).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void CONTRACTCALL(XElement node)
    {
        var flag = Enum.Parse<CallFlags>(node.a("flag") ?? "All");
        var method = node.a("method") ?? "";
        var scripthash = UInt160.Parse(node.a("hash"));
        var arg3 = new XElement("frag").set("data", new ScriptBuilder().EmitPush(flag).ToArray().ToHexString()).compile();
        var arg2 = new XElement("frag").set("data", new ScriptBuilder().EmitPush(method).ToArray().ToHexString()).compile();
        var arg1 = new XElement("frag").set("data", new ScriptBuilder().EmitPush(scripthash).ToArray().ToHexString()).compile();
        var main = new XElement(ns + "syscall").set("name", "System.Contract.Call").compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(arg3);
        node.Add(arg2);
        node.Add(arg1);
        node.Add(main);
    }
    public static void CALL(XElement node)
    {
        var target = node.a("target") ?? "";
        var child = new XElement("goto").set("opcode", "CALL_L").set("target", target).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void DOWHILE(XElement node)
    {
        Guid start = Guid.NewGuid();
        var tag = new XElement(ns + "tag").set("name", start).compile();
        var child = new XElement(ns + "goto").set("target", start).set("cond", "if").compile();
        node.RemoveAttributes();
        node.Name = "lazy";
        node.AddFirst(tag);
        node.Add(child);
    }
}