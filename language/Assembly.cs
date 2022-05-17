using System.Numerics;
using System.Xml.Linq;
using Neo;
using Neo.VM;
namespace neoml.language;

static class Assembly
{
    public static XNamespace ns = nameof(Assembly);
    public static void LAZY(XElement node) { }
    public static void INSTRUCTION(XElement node)
    {
        var opcode = Enum.Parse<OpCode>((node.attr("opcode") ?? "NOP").ToUpper());
        var operand = node.attr("operand")?.HexToBytes();
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(opcode, operand).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = Compiler.LAZY;
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
            default:
                throw new Exception();
        }
        child.compile();
        node.RemoveAll();
        node.Name = Compiler.LAZY;
        node.Add(child);
    }
    public static void NOP(XElement node)
    {
        var child = new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.NOP).ToArray().ToHexString()).compile();
        node.RemoveAll();
        node.Name = Compiler.LAZY;
        node.Add(child);
    }
    public static void TAG(XElement node)
    {
        var name = node.attr("name") ?? "";
        var child = new XElement("lazy").set("id", name).compile();
        node.RemoveAll();
        node.Name = Compiler.LAZY;
        node.Add(child);
    }
    public static void GOTO(XElement node)
    {
        var cond = node.attr("cond") ?? "";
        var target = node.attr("target") ?? "";
        var child = new XElement("goto").set("opcode", Enum.Parse<OpCode>($"JMP{cond.ToUpper()}_L").ToString()).set("target", target).compile();
        node.RemoveAll();
        node.Name = Compiler.LAZY;
        node.Add(child);
    }
    public static void SKIP(XElement node)
    {
        var cond = node.attr("cond") ?? "";
        Guid end = Guid.NewGuid();
        var tag = new XElement("lazy").set("id", end).compile();
        var child = new XElement(ns + "goto").set("target", end).set("cond", cond).compile();
        node.RemoveAttributes();
        node.Name = Compiler.LAZY;
        node.AddFirst(child);
        node.Add(tag);
    }
    // public static void ELSE(XElement node)
    // {
    //     Guid end = Guid.NewGuid();
    //     node.Add(new XElement(Compiler.lazy).attr("id", end));
    //     node.AddFirst(new XElement(ns + "goto").attr("target", $"../lazy[@id='{end}']").attr("cond", "if"));
    // }
}