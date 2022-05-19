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
    public static XNamespace ns = nameof(Assembly);
    public static void LAZY(XElement x) => lazy(x);
    public static void META(XElement x) => meta(x, x.a("name") ?? "contract", x.a("src") ?? "", x.a("compiler") ?? "neoml", x.Value);
    public static void STD(XElement x) => std(x, x.a("std")!);
    public static void ARG(XElement x) => arg(x, x.a("name")!, x.a("type")!.fix());
    public static void FUNC(XElement x) => func(x, x.a("name")!, x.a("return")!.fix(), x.a("safe")?.pipe(bool.Parse) ?? false);
    public static void EVENT(XElement x) => @event(x, x.a("name")!);
    public static void INSTRUCTION(XElement x) => instruction(x, x.a("opcode")!.ToUpper().pipe(Enum.Parse<OpCode>), x.a("operand")?.HexToBytes());
    public static void INT(XElement x) => @int(x, x.a("val")!.pipe(BigInteger.Parse));
    public static void STRING(XElement x) => @string(x, x.a("val")!);
    public static void BYTES(XElement x) => bytes(x, x.a("val")!.HexToBytes());
    public static void BOOL(XElement x) => @bool(x, x.a("val")!.pipe(bool.Parse));
    public static void NULL(XElement x) => @null(x);
    public static void HASH160(XElement x) => hash160(x, x.a("val")!.pipe(UInt160.Parse));
    public static void HASH256(XElement x) => hash256(x, x.a("val")!.pipe(UInt256.Parse));
}