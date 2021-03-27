using System;

namespace App.Inspection.Exceptions
{
    public class InspectionException : Exception
    {
        public InspectionException(string? message) : base(message)
        { }

        public InspectionException(string? message, Exception exception) : base(message, exception)
        { }
    }
}
