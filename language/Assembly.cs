using System.Numerics;
using System.Xml.Linq;
using Neo;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace neoml.language;

static class Assembly
{
    public static XNamespace ns = nameof(Assembly);
    public static void LAZY(XElement node) {}
    public static void FUNC(XElement node) => node.Name = "func";
    public static void ARG(XElement node) => node.Name = "arg";
    public static void EVT(XElement node) => node.Name = "evt";
    public static void META(XElement node) => node.Name = "meta";
    public static void STD(XElement node) => node.Name = "std";
    public static void INSTRUCTION(XElement node)
    {
        var opcode = Enum.Parse<OpCode>((node.attr("opcode") ?? "NOP").ToUpper());
        var operand = node.attr("operand")?.HexToBytes();
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(opcode, operand).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void LITERAL(XElement node)
    {
        var type = node.attr("type") ?? "null";
        var val = node.attr("val") ?? "";
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
    public static void NOP(XElement node)
    {
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.NOP).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void TAG(XElement node)
    {
        var name = node.attr("name") ?? "";
        var child = new XElement("lazy").set("id", name).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void GOTO(XElement node)
    {
        var cond = node.attr("cond") ?? "";
        var target = node.attr("target") ?? "";
        var child = new XElement("goto").set("opcode", Enum.Parse<OpCode>($"JMP{cond.ToUpper()}_L").ToString()).set("target", target).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void SKIP(XElement node)
    {
        var cond = node.attr("cond") ?? "";
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
        var name = node.attr("name");
        var child = new XElement("frag").set("data", new ScriptBuilder().EmitSysCall(new InteropDescriptor() { Name = name }.Hash).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = "lazy";
        node.Add(child);
    }
    public static void CONTRACTCALL(XElement node)
    {
        var flag = Enum.Parse<CallFlags>(node.attr("flag") ?? "All");
        var method = node.attr("method") ?? "";
        var scripthash = UInt160.Parse(node.attr("hash"));
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
        var target = node.attr("target") ?? "";
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
    public static void WHILE(XElement node)
    {
        Guid start = Guid.NewGuid();
        Guid stop = Guid.NewGuid();
        var tagstart = new XElement(ns + "tag").set("name", start).compile();
        var tagstop = new XElement(ns + "tag").set("name", stop).compile();
        var gotostart = new XElement(ns + "goto").set("target", start).set("cond", "if").compile();
        var gotostop = new XElement(ns + "goto").set("target", stop).compile();
        node.AddFirst(tagstart);
        node.Add(tagstop);
        node.AddFirst(gotostop);
        node.Add(gotostart);
    }
}