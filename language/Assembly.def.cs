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
    public static void LAZY(XElement node) => node.lazy();
    // public static void META(XElement node) => META(node, node.attr("name"), node.attr("src"), node.attr("compiler"));

}