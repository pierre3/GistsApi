using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WpfGists.ViewModel
{
  public class FileNameCollection : Collection<string>
  {
    public FileNameCollection()
      :base()
    {}
    public FileNameCollection(IList<string> collection)
      :base(collection)
    {}
    public override string ToString()
    {
      return Items.Select(s => "\"" + s + "\"").Aggregate((a, b) => a + " " + b);
    }
  }
}
