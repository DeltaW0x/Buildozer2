using System.Xml.Linq;

namespace Buildozer.ProjectGen;

public class VsSolution
{
    private XDocument _solutionDocument;
    private XElement _solutionRoot;
    private XElement _solutionConf;

    public VsSolution()
    {
        _solutionDocument = new();
        _solutionRoot = new XElement("Solution");
        _solutionConf = new XElement("Configuration");

        _solutionRoot.Add(_solutionConf);
        _solutionDocument.Add(_solutionRoot);
    }

    public void AddConfiguration(string name)
    {
        _solutionConf.Add(
            new XElement("Platform",
                new XAttribute("Name", name)));
    }
    
    public void AddProject(string path)
    {
        _solutionRoot.Add(
            new XElement("Project",
                new XAttribute("Path", path)));
    }

    public override string ToString()
    {
        return _solutionDocument.ToString();
    }
}