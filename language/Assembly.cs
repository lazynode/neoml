using System.Numerics;
using System.Xml.Linq;
using Neo;
using Neo.VM;
namespace neoml.language;

static class Assembly
{
    public static void LAZY(XElement node) { }
    public static void INSTRUCTION(XElement node)
    {
        var opcode = Enum.Parse<OpCode>((node.attr("opcode") ?? "NOP").ToUpper());
        var operand = node.attr("operand")?.HexToBytes();
        new XElement("frag").set("data", new ScriptBuilder().Emit(opcode, operand).ToArray().ToHexString()).to(node);
    }
    public static void LITERAL(XElement node)
    {
        var type = node.attr("type") ?? "null";
        var val = node.attr("val") ?? "";
        switch (type)
        {
            case "int":
                new XElement("frag").set("data", new ScriptBuilder().EmitPush(BigInteger.Parse(val)).ToArray().ToHexString()).to(node);
                break;
            case "string":
                new XElement("frag").set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString()).to(node);
                break;
            case "bytes":
                new XElement("frag").set("data", new ScriptBuilder().EmitPush(val.HexToBytes()).ToArray().ToHexString()).to(node);
                break;
            case "bool":
                new XElement("frag").set("data", new ScriptBuilder().EmitPush(bool.Parse(val)).ToArray().ToHexString()).to(node);
                break;
            case "null":
                new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.PUSHNULL).ToArray().ToHexString()).to(node);
                break;
            default:
                throw new Exception();
        }

    }
    public static void NOP(XElement node)
    {
        new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.NOP).ToArray().ToHexString()).to(node);
    }
    public static void TAG(XElement node)
    {
        var name = node.attr("name") ?? "";
        new XElement("lazy").set("id", name).to(node);
    }
    public static void GOTO(XElement node)
    {
        var cond = node.attr("cond") ?? "";
        var target = node.attr("target") ?? "";
        new XElement("goto").set("opcode", Enum.Parse<OpCode>($"JMP{cond.ToUpper()}_L").ToString()).set("target", target).to(node);
    }
}