using System;

namespace MyTypes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InheritAttribute(Type targetType) : Attribute { public readonly Type TargetType = targetType; }
