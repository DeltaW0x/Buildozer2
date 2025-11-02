using System.Text;
using System.Xml.Linq;

namespace Buildozer.ProjectGen;

file sealed class UTF8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

public class VsProject
{
    private XDocument _projectDocument;
    private XElement _projectRoot;
    private XNamespace _ns = "http://schemas.microsoft.com/developer/msbuild/2003";

    public VsProject()
    {
        _projectDocument = new XDocument(new XDeclaration("1.0", "utf-8", null));
        _projectRoot = new XElement(_ns + "Project", new XAttribute("DefaultTargets", "Build"));

        _projectDocument.Add(_projectRoot);
    }

    public void AddItemGroup(string? label = null)
    {
        var itemGroup = new XElement(_ns + "ItemGroup");

        if (label != null)
            itemGroup.Add(new XAttribute("Label", label));

        _projectRoot.Add(itemGroup);
    }

    public override string ToString()
    {
        using (var writer = new UTF8StringWriter())
        {
            _projectDocument.Save(writer);
            return writer.ToString();
        }
    }
}