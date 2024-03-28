﻿using System;

namespace JUST
{
    internal static class ComparisonHelper
    {
        public static bool Equals(object x, object y, EvaluationMode evaluationMode)
        {
            var comparisonType = (evaluationMode == EvaluationMode.Strict && x.GetType() != typeof(bool) && y.GetType() != typeof(bool))
                ? StringComparison.CurrentCulture
                : StringComparison.InvariantCultureIgnoreCase;

            return string.Equals(x?.ToString(), y?.ToString(), comparisonType);
        }

        public static bool Contains(object x, object y, EvaluationMode evaluationMode)
        {
            var comparisonType = (evaluationMode == EvaluationMode.Strict && x.GetType() != typeof(bool) && y.GetType() != typeof(bool))
                ? StringComparison.CurrentCulture
                : StringComparison.InvariantCultureIgnoreCase;

            return x != null && x.ToString().IndexOf(y?.ToString() ?? string.Empty, comparisonType) >= 0;
        }
    }
}
