using Codeplex.Data;
using System;
using System.Collections.Generic;

namespace GistsApi
{
  public static class DynamicJsonExtensions
  {
    public static IEnumerable<T> DeserializeMembers<T>(this DynamicJson dynamicJson, Func<dynamic, T> resultSelector)
    {
      foreach (var name in dynamicJson.GetDynamicMemberNames())
      {
        yield return resultSelector(((dynamic)dynamicJson)[name]);
      }
    }
  }
}
