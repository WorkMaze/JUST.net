using System;

namespace JUST
{
    internal static class ExceptionHelper
    {
        internal static void HandleException(Exception ex, EvaluationMode evaluationMode)
        {
            if ( (evaluationMode & EvaluationMode.Strict) == EvaluationMode.Strict)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                };
                throw ex;
            }
        }
    }
}
