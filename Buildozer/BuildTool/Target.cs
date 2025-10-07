namespace Buildozer.BuildTool;

public enum TargetPlatform
{
    Linux,
    Windows
}

public enum TargetArchitecture
{
    X64,
    Arm64
}

public enum TargetBuildMode
{
    Debug,
    Develop,
    Release
}

public enum TargetKind
{
    Binary,
    HeaderOnly,
    StaticLibrary,
    SharedLibrary,
    DynamicLibrary,
    ImportedStaticLibrary,
    ImportedSharedLibrary,
    ImportedDynamicLibrary
}

public enum TargetLanguage
{
    C,
    Cxx,
    CSharp,
    CSharpAOT
}

public class Target
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }

    public TargetKind Kind { get; protected set; }
    public TargetLanguage Language { get; protected set; }

    public List<string> PublicDependencies { get; protected set; } = new();
    public List<string> PrivateDependencies { get; protected set; } = new();

    public List<string> SourceFiles { get; protected set; } = new();
    public List<string> PublicIncludeDirs { get; protected set; } = new();
    public List<string> PrivateIncludeDirs { get; protected set; } = new();

    public List<string> PublicCFlags { get; protected set; } = new();
    public List<string> PublicCxxFlags { get; protected set; } = new();
    public List<string> PublicCompilationDefines { get; protected set; } = new();

    public List<string> PrivateCFlags { get; protected set; } = new();
    public List<string> PrivateCxxFlags { get; protected set; } = new();
    public List<string> PrivateCompilationDefines { get; protected set; } = new();

    protected Target(string name) 
    {
        Name = name;
    }

    public virtual void OnConfig() {}

    public virtual void OnPreBuild() {}
    public virtual void OnPostBuild() {}

    public virtual void OnClean() {}
    public virtual void OnInstall() {}
}