using System;

namespace MyTypes;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventReceiverAttribute : Attribute { }
