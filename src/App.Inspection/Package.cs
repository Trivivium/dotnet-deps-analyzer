using System;
using System.Collections.Generic;

namespace App.Inspection
{
    public class Package
    {
        public readonly Namespace Namespace;
        public readonly ICollection<Type> UniqueTypes = new List<Type>();

        public int UniqueTypeCount => 100;  // TODO: Derive this from the actual property when package inspection is implemented.

        public Package(Namespace ns)
        {
            Namespace = ns;
        }
    }

    public class UnknownPackage : Package
    {
        public UnknownPackage(Namespace ns) : base(ns)
        { }
    }
}
