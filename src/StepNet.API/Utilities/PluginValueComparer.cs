using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace StepNet.API.Utilities
{
    public class PluginValueComparer : ValueComparer<List<Plugin>>
    {
        public PluginValueComparer()
            : base(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()))
            )
        { }
    }
}
