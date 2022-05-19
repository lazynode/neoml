using System.Xml.Linq;
using System.Reflection;

namespace neoml;
static class Compiler
{
    public static Dictionary<string, Type> LANGUAGES = Assembly.GetAssembly(typeof(Compiler))!.GetTypes().Where(v => v.Namespace == "neoml.language").ToDictionary(v => v.Name.ToUpper());
    public static XElement compile(this XElement node) => node.with(childrencompile).withif(node.Name.NamespaceName.Any(), selfcompile);
    public static void childrencompile(this XElement node) => node.Elements().ToList().ForEach(v => v.compile());
    public static void selfcompile(this XElement node) => LANGUAGES[node.Name.NamespaceName.ToUpper()].GetMethod(node.Name.LocalName.ToUpper())!.Invoke(null, new object?[] { node });
    public static XElement set(this XElement node, XName name, object? val) => node.with(v => v.SetAttributeValue(name, val));
    public static string? a(this XElement node, XName name) => node.Attribute(name)?.Value;
    public static XElement root(this XElement node) => node.Parent is null ? node : node.Parent.root();
    public static XElement leftest(this XElement node) => node.Elements().Any() ? node.Elements().First().leftest() : node;
    public static IEnumerable<XElement> filter(this XElement node, string name) => node.DescendantsAndSelf().Where(v => v.Name.LocalName == name);
    public static XElement lazilize(this XElement node) => node.with(v => v.RemoveAll()).with(v => v.Name = "lazy");
    public static XElement lazy(this XElement node) => node.with(v => v.RemoveAttributes()).with(v => v.Name = "lazy");
    public static XElement clone(this XElement node, XName tag, params string[] attrs) => attrs.Aggregate(new XElement(tag), (n, v) => n.set(v, node.a(v)));
    public static void addto(this XElement node, XElement parent) => parent.with(v => v.lazilize()).Add(node);
}
