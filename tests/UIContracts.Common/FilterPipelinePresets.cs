using System.Reflection;
using System.Text;

namespace UIContracts.Common;

/// <summary>
/// Embedded JSON presets for filter-pipeline UI contract tests.
/// </summary>
public static class FilterPipelinePresets
{
    public const string SingleCriterionIsActiveTrue = "SingleCriterion_IsActiveTrue.json";
    public const string MultiColumnAndDeptItCountryUsa = "MultiColumn_And_DeptIT_CountryUSA.json";
    public const string OrGroupNameStartsWith = "OrGroup_NameStartsWith.json";
    public const string DisabledCriterionIgnored = "DisabledCriterion_Ignored.json";

    public static string Load(string resourceFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded preset not found: {resourceFileName}");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load preset stream: {resourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string SingleCriterionIsActiveTrueJson => Load(SingleCriterionIsActiveTrue);
    public static string MultiColumnAndDeptItCountryUsaJson => Load(MultiColumnAndDeptItCountryUsa);
    public static string OrGroupNameStartsWithJson => Load(OrGroupNameStartsWith);
    public static string DisabledCriterionIgnoredJson => Load(DisabledCriterionIgnored);
}
