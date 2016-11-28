using System;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Detailed information about the difference
    /// </summary>
    public class Difference
    {
        /// <summary>
        /// Name of Expected Object
        /// </summary>
        public string ExpectedName { get; set; }

        /// <summary>
        /// Name of Actual Object
        /// </summary>
        public string ActualName { get; set; }

        /// <summary>
        /// Returns the parent property name
        /// </summary>
        public string ParentPropertyName
        {
            get
            {
                if (PropertyName.EndsWith("]") && PropertyName.Contains("["))
                {
                    int lastLeftSquare = PropertyName.LastIndexOf('[');

                    return PropertyName.Substring(0, lastLeftSquare);
                }

                if (PropertyName.Contains("."))
                {
                    int lastPeriod = PropertyName.LastIndexOf('.');

                    if (lastPeriod > 0)
                        return PropertyName.Substring(0,  lastPeriod);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// The breadcrumb of the property leading up to the value
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The child property name
        /// </summary>
        public string ChildPropertyName { get; set; }

        /// <summary>
        /// Object1 Value as a string
        /// </summary>
        public string Object1Value { get; set; }

        /// <summary>
        /// Object2 Value as a string
        /// </summary>
        public string Object2Value { get; set; }

        /// <summary>
        /// The type of the first object
        /// </summary>
        public string Object1TypeName { get; set; }

        /// <summary>
        /// The type of the second object
        /// </summary>
        public string Object2TypeName { get; set; }

        /// <summary>
        /// A reference to the parent of object1
        /// </summary>
        public WeakReference ParentObject1 { get; set; }

        /// <summary>
        /// A reference to the parent of object2
        /// </summary>
        public WeakReference ParentObject2 { get; set; }

        /// <summary>
        /// Object1 as a reference
        /// </summary>
        public WeakReference Object1 { get; set; }

        /// <summary>
        /// Object2 as a reference
        /// </summary>
        public WeakReference Object2 { get; set; }

        /// <summary>
        /// Prefix to put on the beginning of the message
        /// </summary>
        public string MessagePrefix { get; set; }

        /// <summary>
        /// Item and property name only
        /// </summary>
        /// <returns></returns>
        public string GetShortItem()
        {
            string message;

            if (!String.IsNullOrEmpty(PropertyName))
            {
                if (String.IsNullOrEmpty(ChildPropertyName))
                {
                    message = String.Format("{0}", PropertyName);
                }
                else
                {
                    message = String.Format("{0}.{1}",
                        PropertyName,
                        ChildPropertyName);
                }
            }
            else
            {
                message = String.Format("{0} != {1}",
                    ExpectedName,
                    ActualName);
            }

            if (!String.IsNullOrEmpty(MessagePrefix))
                message = String.Format("{0}: {1}", MessagePrefix, message);

            message = message.Replace("..", ".");
            message = message.Replace(".[", "[");

            return message;            
        }


        /// <summary>
        /// Nicely formatted string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string type = ParentObject1.IsAlive ? ParentObject1.Target.GetType().ToString() : string.Empty;
            string path = string.Join(".", PropertyName, ChildPropertyName).Replace("..", ".").Replace(".[", "[");

            string message = $@"
Location:
    Type:       {type}
    Path:       {path}
Values:
    Expected:   {Object1TypeName} <{Object1Value}>
    Actual:     {Object2TypeName} <{Object2Value}>
";

            if (!string.IsNullOrEmpty(MessagePrefix))
                message = $"{MessagePrefix}: {message}";

            return message;
        }
    }
}