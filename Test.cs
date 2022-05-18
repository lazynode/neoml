using Xunit;
using System.Xml.Linq;
using neoml;
using Neo.SmartContract;
using Neo.VM;
using Neo;

public class Test
{
    [Theory()]
    [InlineData("1.literal")]
    [InlineData("2.instruction")]
    [InlineData("3.syscall")]
    [InlineData("4.contractcall")]
    [InlineData("5.goto")]
    [InlineData("6.if")]
    [InlineData("7.call")]
    [InlineData("8.while")]
    [InlineData("9.dowhile")]
    public void TestOutput(string filename) => Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.GetDirectories("examples").First().GetFiles(filename).Select(v => XElement.Load(v.OpenRead())).ToList().ForEach(v =>
    {
        using ApplicationEngine engine = ApplicationEngine.Run(v.finalize(), new NeoSystem(ProtocolSettings.Default, null, null).GetSnapshot().CreateSnapshot(), container: null, settings: ProtocolSettings.Default, gas: ApplicationEngine.TestModeGas);
        Assert.Equal(VMState.HALT, engine.State);
        Assert.Equal(v.attr("result") ?? "", engine.ResultStack.Peek().ToJson().ToString());
        Assert.Equal(Convert.FromBase64String(v.attr("bytecode") ?? "").ToHexString(), v.finalize().ToHexString());
    });
}
