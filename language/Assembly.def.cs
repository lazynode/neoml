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
    public static void LAZY(XElement x) => x.lazy();
    public static void META(XElement x) => META(x, x.a("name") ?? "contract", x.a("src") ?? "", x.a("compiler") ?? "neoml");
    public static void STD(XElement x) => STD(x, x.a("std")!);
    public static void ARG(XElement x) => ARG(x, x.a("name")!, x.a("type")!.fix());
    public static void FUNC(XElement x) => FUNC(x, x.a("name")!, x.a("return")!.fix(), x.a("safe")?.pipe(bool.Parse) ?? false);
    public static void EVENT(XElement x) => EVENT(x, x.a("name")!);
}