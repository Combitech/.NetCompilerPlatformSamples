using System;

namespace MyTypes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsJsonAttribute() : Attribute {  }
