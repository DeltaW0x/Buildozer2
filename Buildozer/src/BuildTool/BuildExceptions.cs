using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.BuildTool
{
    public class InvalidBuildLanguageException : Exception
    {
        public InvalidBuildLanguageException()
        {
        }

        public InvalidBuildLanguageException(string? message) : base(message)
        {
        }
    }

    public class InvalidBuildWarningLevelException : Exception
    {
        public InvalidBuildWarningLevelException()
        {
        }

        public InvalidBuildWarningLevelException(string? message) : base(message)
        {
        }
    }
}
