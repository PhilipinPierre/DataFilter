using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Applies a compiled <see cref="FilterPipeline"/> to a <see cref="FilterContext"/>.
/// </summary>
public static class FilterPipelineContextExtensions
{
    /// <summary>
    /// Compiles the pipeline and replaces all descriptors on the context (see <see cref="FilterContext.ReplaceDescriptors"/>).
    /// </summary>
    public static void ApplyToContext(this FilterPipeline pipeline, FilterContext context)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (context == null) throw new ArgumentNullException(nameof(context));

        context.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline));
    }
}
