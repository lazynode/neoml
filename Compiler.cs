using System.Xml.Linq;
using System.Reflection;

namespace neoml;
static class Compiler
{
    public static Dictionary<string, Type> LANGUAGES = Assembly.GetAssembly(typeof(Compiler))!.GetTypes().Where(v => v.Namespace == "neoml.language").ToDictionary(v => v.Name.ToUpper());
    public static XElement compile(this XElement x) => x.withif(x.Name.NamespaceName.Any(), selfcompile).with(childrencompile);
    public static void childrencompile(this XElement x) => x.Elements().ToList().ForEach(v => v.compile());
    public static void selfcompile(this XElement x) => LANGUAGES[x.Name.NamespaceName.ToUpper()].GetMethod(x.Name.LocalName.ToUpper())!.Invoke(null, new object?[] { x });
    public static XElement set(this XElement x, XName name, object? val) => x.with(v => v.SetAttributeValue(name, val));
    public static string? a(this XElement x, XName name) => x.Attribute(name)?.Value;
    public static XElement root(this XElement x) => x.Parent is null ? x : x.Parent.root();
    public static IEnumerable<XElement> filter(this XElement x, string name) => x.DescendantsAndSelf().Where(v => v.Name.LocalName == name);
    public static XElement lazilize(this XElement x, string tag = "lazy") => x.with(v => v.RemoveAll()).with(v => v.Name = tag);
    public static XElement lazy(this XElement x, string tag = "lazy") => x.with(v => v.RemoveAttributes()).with(v => v.Name = tag);
    public static XElement clone(this XElement x, XName tag, params string[] attrs) => attrs.Aggregate(new XElement(tag), (n, v) => n.set(v, x.a(v)));
    public static void addto(this XElement x, XElement parent) => parent.with(v => v.lazilize()).Add(x);
}
