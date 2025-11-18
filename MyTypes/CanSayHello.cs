using System;

namespace MyTypes;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CanSayHello : Attribute { }
