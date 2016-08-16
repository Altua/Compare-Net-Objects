using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects.TypeComparers;


namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// The base comparer which contains all the type comparers
    /// </summary>
    public class RootComparer : BaseComparer
    {
        private Dictionary<CompareParms, bool> _cache = new Dictionary<CompareParms, bool>();
        private Dictionary<CompareParms, bool> _cacheInProgress = new Dictionary<CompareParms, bool>();


        #region Properties

        /// <summary>
        /// A list of the type comparers
        /// </summary>
        internal List<BaseTypeComparer> TypeComparers { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// Compare two objects
        /// </summary>
        public bool Compare(CompareParms parms)
        {
            bool result = false;
            try
            {
                bool cachedResult = false;
                if (_cache.TryGetValue(parms, out cachedResult))
                    return cachedResult;

                if (_cacheInProgress.ContainsKey(parms))
                    return true;

                _cacheInProgress.Add(parms, true);

                if (parms.Object1 == null && parms.Object2 == null)
                    result = true;
                else
                {
                    Type t1 = parms.Object1 != null ? parms.Object1.GetType() : null;
                    Type t2 = parms.Object2 != null ? parms.Object2.GetType() : null;

                    BaseTypeComparer customComparer =
                        parms.Config.CustomComparers.FirstOrDefault(o => o.IsTypeMatch(t1, t2));

                    if (customComparer != null)
                    {
                        customComparer.CompareType(parms);
                        result = parms.Result.AreEqual;
                    }
                    else
                    {
                        BaseTypeComparer typeComparer = TypeComparers.FirstOrDefault(o => o.IsTypeMatch(t1, t2));

                        if (typeComparer != null)
                        {
                            if (parms.Config.IgnoreObjectTypes || !TypesDifferent(parms, t1, t2))
                            {
                                typeComparer.CompareType(parms);
                            }
                            result = parms.Result.AreEqual;
                        }
                        else
                        {
                            if (EitherObjectIsNull(parms))
                                result = false;
                            else if (!parms.Config.IgnoreObjectTypes && t1 != null)
                                throw new NotSupportedException("Cannot compare object of type " + t1.Name);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                if (!parms.Config.IgnoreObjectDisposedException)
                    throw;

                result = true;
            }
            finally
            {
                
            }

            _cache.Add(parms, result);
            _cacheInProgress.Remove(parms);
            return result;
        }


        private bool TypesDifferent(CompareParms parms, Type t1, Type t2)
        {
            //Objects must be the same type and not be null
            if (!parms.Config.IgnoreObjectTypes
                && parms.Object1 != null
                && parms.Object2 != null
                && t1 != t2)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = new WeakReference(parms.ParentObject1),
                    ParentObject2 = new WeakReference(parms.ParentObject2),
                    PropertyName = parms.BreadCrumb,
                    Object1Value = t1.FullName,
                    Object2Value = t2.FullName,
                    ChildPropertyName = "GetType()",
                    MessagePrefix = "Different Types",
                    Object1 = new WeakReference(parms.Object1),
                    Object2 = new WeakReference(parms.Object2)
                };

                AddDifference(parms.Result, difference);
                return true;
            }

            return false;
        }


        private bool EitherObjectIsNull(CompareParms parms)
        {
            //Check if one of them is null
            if (parms.Object1 == null || parms.Object2 == null)
            {
                AddDifference(parms);
                return true;
            }

            return false;
        }

        #endregion

        public void Clean()
        {
            _cache.Clear();
            _cacheInProgress.Clear();
        }
    }
}
