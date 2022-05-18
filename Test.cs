using Xunit;
using System.Xml.Linq;
using neoml;
using Neo.SmartContract;
using Neo.VM;
using Neo;

public class Test
{
    [Fact]
    public void TestOutput() => Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.GetDirectories("examples").First().GetFiles().Select(v => XElement.Load(v.OpenRead())).ToList().ForEach(v =>
    {
        using ApplicationEngine engine = ApplicationEngine.Run(v.compile().finalize(), new NeoSystem(ProtocolSettings.Default, null, null).GetSnapshot().CreateSnapshot(), container: null, settings: ProtocolSettings.Default, gas: ApplicationEngine.TestModeGas);
        Assert.Equal(VMState.HALT, engine.State);
        Assert.Equal(v.attr("result") ?? "", engine.ResultStack.Peek().ToJson().ToString());
    });
    [Fact]
    public void TestBytecode() => Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.GetDirectories("examples").First().GetFiles().Select(v => XElement.Load(v.OpenRead())).ToList().ForEach(v => Assert.Equal(Convert.FromBase64String(v.attr("bytecode") ?? ""), v.compile().finalize()));
}
