using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;

namespace GistsApi
{
  public static class DynamicJsonHelper
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
