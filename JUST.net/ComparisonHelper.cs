using System;

namespace JUST
{
    internal static class ComparisonHelper
    {
        public static bool Equals(object x, object y, JUSTContext context)
        {
            var comparisonType = (context.EvaluationMode == EvaluationMode.Strict)
                ? StringComparison.CurrentCulture
                : StringComparison.InvariantCultureIgnoreCase;

            return string.Equals(x?.ToString(), y?.ToString(), comparisonType);
        }

        public static bool Contains(object x, object y, JUSTContext context)
        {
            var comparisonType = (context.EvaluationMode == EvaluationMode.Strict)
                ? StringComparison.CurrentCulture
                : StringComparison.InvariantCultureIgnoreCase;

            return ((x != null) && x.ToString().IndexOf(y?.ToString() ?? string.Empty, comparisonType) >= 0);
        }
    }
}
