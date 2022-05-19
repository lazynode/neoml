using Xunit;
using System.Xml.Linq;
using neoml;
using Neo.SmartContract;
using Neo.VM;
using Neo;

public class Test
{
    [Theory()]
    [InlineData("1.literal.helloworld.xml")]
    [InlineData("2.instruction.add.xml")]
    [InlineData("3.syscall.convert_publickey_to_hash160.xml")]
    [InlineData("4.contractcall.atoi.xml")]
    [InlineData("5.goto.judgeweekday.xml")]
    [InlineData("6.if.ispowerof2.xml")]
    [InlineData("7.call.fibonacci.xml")]
    [InlineData("8.dowhile.pseudorandomeven.xml")]
    [InlineData("9.function.simplenep17.xml")]
    public void TestOutput(string filename) => Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.GetDirectories("examples").First().GetFiles(filename).First().pipe(v => v.OpenRead().pipe(XElement.Load)).with(v =>
    {
        var expected = v.a("result");
        var bytecode = v.a("bytecode");
        using ApplicationEngine engine = ApplicationEngine.Run(v.compile().finalize(), new NeoSystem(ProtocolSettings.Default, null, null).GetSnapshot().CreateSnapshot(), container: null, settings: ProtocolSettings.Default, gas: ApplicationEngine.TestModeGas);
        Assert.Equal(VMState.HALT, engine.State);
        Assert.Equal(expected, engine.ResultStack.Peek().ToJson().ToString());
        Assert.Equal(Convert.FromBase64String(bytecode!).ToHexString(), v.finalize().ToHexString());
    });
}
