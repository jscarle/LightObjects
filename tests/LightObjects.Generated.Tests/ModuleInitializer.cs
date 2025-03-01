using DiffEngine;
using VerifyTests.DiffPlex;

namespace LightObjects.Generated.Tests;

public static class ModuleInitializer
{
    public static void Initialize()
    {
        DiffRunner.Disabled = true;
        VerifyDiffPlex.Initialize(OutputType.Compact);
        VerifierSettings.InitializePlugins();
        VerifierSettings.ScrubLinesContaining("DiffEngineTray");
        VerifierSettings.IgnoreStackTrace();
    }
}
