using Xunit;
using System.Xml.Linq;
using neoml;

public class Test
{
    [Fact]
    public void TestAllExamples() => Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.GetDirectories("examples").First().GetFiles().Select(v => XElement.Load(v.OpenRead())).ToList().ForEach(v => Assert.Equal(v.compile().finalize(), Convert.FromBase64String(v.attr("bytecode")??"")));
}
