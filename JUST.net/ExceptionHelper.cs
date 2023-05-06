using System;

namespace JUST
{
    internal static class ExceptionHelper
    {
        internal static void HandleException(Exception ex, bool IsStrictMode)
        {
            if (IsStrictMode)
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
