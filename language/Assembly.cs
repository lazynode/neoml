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
    public static void INT(XElement node)
    {
        var val = BigInteger.Parse(node.attr("val") ?? "0");
        new XElement("frag").set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString()).to(node);
    }
    public static void STRING(XElement node)
    {
        var val = node.attr("val") ?? "";
        new XElement("frag").set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString()).to(node);
    }
    public static void BYTES(XElement node)
    {
        var val = (node.attr("val") ?? "").HexToBytes();
        new XElement("frag").set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString()).to(node);
    }
    public static void BOOL(XElement node)
    {
        var val = bool.Parse(node.attr("val") ?? "false");
        new XElement("frag").set("data", new ScriptBuilder().EmitPush(val).ToArray().ToHexString()).to(node);
    }
    public static void NULL(XElement node)
    {
        new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.PUSHNULL).ToArray().ToHexString()).to(node);
    }
    public static void NOP(XElement node)
    {
        new XElement("frag").set("data", new ScriptBuilder().Emit(OpCode.NOP).ToArray().ToHexString()).to(node);
    }
}