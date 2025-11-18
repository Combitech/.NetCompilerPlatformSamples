using Microsoft.CodeAnalysis;
using MyTypes;
using System.Linq;

namespace MyGenerator.EventSubscribing;

internal static class EventAttributeUtils
{
	internal static bool HasEventReceiverAttribute(IMethodSymbol method)
	{
		return method != null && method.GetAttributes().Any(attr => attr.AttributeClass?.Name == nameof(EventReceiverAttribute) || attr.AttributeClass?.Name + "Attribute" == nameof(EventReceiverAttribute));
	}
}
