using System;


namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Compare Parameters
    /// </summary>
    public class CompareParms
    {
        /// <summary>
        /// The breadcrumb in the tree
        /// </summary>
        public string BreadCrumb { get; set; }


        /// <summary>
        /// The configuration settings
        /// </summary>
        public ComparisonConfig Config { get; set; }


        /// <summary>
        /// The first object to be compared
        /// </summary>
        public object Object1 { get; set; }


        /// <summary>
        /// The type of the first object
        /// </summary>
        public Type Object1Type { get; set; }


        /// <summary>
        /// The second object to be compared
        /// </summary>
        public object Object2 { get; set; }


        /// <summary>
        /// The type of the second object
        /// </summary>
        public Type Object2Type { get; set; }


        /// <summary>
        /// A reference to the parent object1
        /// </summary>
        public object ParentObject1 { get; set; }


        /// <summary>
        /// A reference to the parent object2
        /// </summary>
        public object ParentObject2 { get; set; }


        /// <summary>
        /// Details about the comparison
        /// </summary>
        public ComparisonResult Result { get; set; }


        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var parms = obj as CompareParms;
            if (parms == null)
                return false;

            if (Object1 != parms.Object1)
                return false;
            if (Object2 != parms.Object2)
                return false;

            return true;
        }


        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int) 2166136261;

                hash = Object1 == null ? hash : hash *16777619 ^ Object1.GetHashCode();
                hash = Object2 == null ? hash : hash * 16777619 ^ Object2.GetHashCode();
                return hash;
            }
        }
    }
}
