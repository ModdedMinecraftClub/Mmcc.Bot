using System;

namespace Mmcc.Bot.Common;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ExcludeFromMediatrAssemblyScanAttribute : Attribute
{
}
