using System.Numerics;
using System.Xml.Linq;
using Neo;
using Neo.SmartContract;
using Neo.VM;

namespace neoml.language;

static partial class Assembly
{
    public static void lazy(XElement x) => x.lazy();
    private static void meta(XElement x, string name, string src, string compiler) => x.lazilize("meta").set("name", name).set("src", src).set("compiler", compiler);
    private static void std(XElement x, string std) => x.lazilize("std").set("std", std);
    private static void arg(XElement x, string name, string type) => x.lazilize("arg").set("name", name).set("type", type);
    private static void func(XElement x, string name, string @return, bool safe) => x.lazy("func").set("name", name).set("return", @return).set("safe", safe);
    private static void @event(XElement x, string name) => x.lazilize("event").set("name", name);
    private static void instruction(XElement node, OpCode opcode, byte[]? operand) => node.lazilize("frag").set("data", opcode.hex(operand));
    private static void @int(XElement node, BigInteger val) => node.lazilize("frag").set("data", val.push());
    private static void @string(XElement node, string val) => node.lazilize("frag").set("data", val.push());
    private static void bytes(XElement node, byte[] val) => node.lazilize("frag").set("data", val.push());
    private static void @bool(XElement node, bool val) => node.lazilize("frag").set("data", val.push());
    private static void @null(XElement node) => node.lazilize("frag").set("data", OpCode.PUSHNULL.hex());
    private static void hash160(XElement node, UInt160 val) => node.lazilize("frag").set("data", val.push());
    private static void hash256(XElement node, UInt256 val) => node.lazilize("frag").set("data", val.push());
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