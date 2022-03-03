using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LayoutTest1
{
    public class ListBoxItemsBag
    {
        public List<ListBoxItem> Bag = new List<ListBoxItem>();
        public int Count { get => Bag.Count; }
    }
}
