using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KellermanSoftware.CompareNetObjects
{
    public class DifferenceException : Exception
    {
        public DifferenceException(Difference difference)
        {
            Difference = difference ?? throw new ArgumentNullException(nameof(difference));
        }

        public Difference Difference { get; }

    }
}
