using System;
using System.Text;

namespace KellermanSoftware.CompareNetObjects
{
    public class BreadCrumb
    {
        private readonly string _name;
        private readonly BreadCrumb _parent;

        public BreadCrumb(BreadCrumb parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        private void BuildString(StringBuilder sb)
        {
            if (_parent != null)
            {
                _parent.BuildString(sb);
                sb.Append(".");
            }

            sb.Append(_name);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            BuildString(sb);
            return sb.ToString();
        }
    }
}
