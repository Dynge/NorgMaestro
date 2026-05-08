using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class LanguageServerStateDiagnosticsTests
{
    [Fact]
    public void ShouldReportUnresolvedLinks()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-diagnostics").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601030101.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source\n@end\n\nSee {:missing-note:}[Missing]"
            );

            LanguageServerState state = new();
            state.UpdateDocument(new(Path.GetFullPath(sourcePath)));

            var diagnostics = state.GetDiagnostics();

            diagnostics.Should().HaveCount(1);
            diagnostics.Values.Single().Should().ContainSingle();
            diagnostics.Values.Single()[0].Message.Should().Contain("Unresolved note link");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldDropStaleReferencesAfterSave()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-stale").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601030102.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source\n@end\n\nSee {:missing-note:}[Missing]"
            );

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.References.Should().HaveCount(1);

            File.WriteAllText(sourcePath, "@document.meta\ntitle: Source\n@end\n\nNo links now");
            state.UpdateDocument(sourceUri);

            state.References.Should().BeEmpty();
            state.GetDiagnostics().Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldUseConfiguredUnresolvedLinkSeverity()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-diagnostics-severity").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601030103.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source\n@end\n\nSee {:missing-note:}[Missing]"
            );

            LanguageServerState state = new();
            state.Initialize(
                new Uri(Path.GetFullPath(tempDir)),
                null,
                new InitializationOptions
                {
                    Diagnostics = new DiagnosticsInitializationOptions
                    {
                        UnresolvedLinkSeverity = "hint"
                    }
                }
            );
            state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));

            var diagnostics = state.GetDiagnostics();

            diagnostics.Values.Single()[0].Severity.Should().Be(DiagnosticSeverity.Hint);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldResetUnresolvedLinkSeverityOnReinitializeWithoutOptions()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-diagnostics-reset-severity").FullName;
        try
        {
            LanguageServerState state = new();
            Uri root = new(Path.GetFullPath(tempDir));

            state.Initialize(
                root,
                null,
                new InitializationOptions
                {
                    Diagnostics = new DiagnosticsInitializationOptions
                    {
                        UnresolvedLinkSeverity = "error"
                    }
                }
            );
            state.UnresolvedLinkSeverity.Should().Be(DiagnosticSeverity.Error);

            state.Initialize(root, null, null);
            state.UnresolvedLinkSeverity.Should().Be(DiagnosticSeverity.Warning);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldNormalizeConfiguredSeverityValue()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-diagnostics-normalize-severity").FullName;
        try
        {
            LanguageServerState state = new();
            state.Initialize(
                new Uri(Path.GetFullPath(tempDir)),
                null,
                new InitializationOptions
                {
                    Diagnostics = new DiagnosticsInitializationOptions
                    {
                        UnresolvedLinkSeverity = "  InFo  "
                    }
                }
            );

            state.UnresolvedLinkSeverity.Should().Be(DiagnosticSeverity.Information);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
