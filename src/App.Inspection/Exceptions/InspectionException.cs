using System;

namespace App.Inspection.Exceptions
{
    public class InspectionException : Exception
    {
        public InspectionException(string? message) : base(message)
        { }
    }
}
