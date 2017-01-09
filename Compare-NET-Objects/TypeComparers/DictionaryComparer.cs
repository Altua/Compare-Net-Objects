// Copyright Altua AS. All rights reserved.

using System;
using System.Collections;
using System.Globalization;


namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two dictionaries
    /// </summary>
    public class DictionaryComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public DictionaryComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }


        private bool CheckIfDictionaryCountIsDifferent(CompareParms parms)
        {
            IDictionary iDict1 = parms.Object1 as IDictionary;
            IDictionary iDict2 = parms.Object2 as IDictionary;

            if (iDict1 == null)
                throw new ArgumentException("parms.Object1");

            if (iDict2 == null)
                throw new ArgumentException("parms.Object2");

            if (iDict1.Count != iDict2.Count)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = new WeakReference(parms.ParentObject1),
                    ParentObject2 = new WeakReference(parms.ParentObject2),
                    PropertyName = parms.BreadCrumb,
                    Object1Value = iDict1.Count.ToString(CultureInfo.InvariantCulture),
                    Object2Value = iDict2.Count.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Count",
                    Object1 = new WeakReference(iDict1),
                    Object2 = new WeakReference(iDict2)
                };

                AddDifference(parms.Result, difference);

                return true;
            }
            return false;
        }


        private void CompareEachItem(CompareParms parms)
        {
            IDictionary dict1 = (IDictionary) parms.Object1;
            IDictionary dict2 = (IDictionary) parms.Object2;

            string keyBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Key");
            string valueBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Value");

            // Note: We currently just check this one way. A further optimization could be to run
            // the comparison in the other direction as well. We will never get a false negative, since
            // since we have already checked the count, but if the user has allowed more than one difference
            // not all differences will be shown in a single run.
            foreach (object key in dict1.Keys)
            {
                object value1 = dict1[key];
                object value2;

                if (!TryGetValue(dict2, key, out value2))
                {
                    Difference difference = new Difference
                    {
                        ParentObject1 = new WeakReference(parms.ParentObject1),
                        ParentObject2 = new WeakReference(parms.ParentObject2),
                        PropertyName = keyBreadCrumb,
                        Object1Value = key.ToString(),
                        Object2Value = "",
                        ChildPropertyName = "Key",
                        Object1 = new WeakReference(key),
                        Object2 = null,
                    };

                    AddDifference(parms.Result, difference);
                }
                else
                {
                    CompareParms childParms = new CompareParms
                    {
                        Result = parms.Result,
                        Config = parms.Config,
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        Object1 = value1,
                        Object2 = value2,
                        BreadCrumb = valueBreadCrumb
                    };

                    RootComparer.Compare(childParms);
                }

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }


        private static bool TryGetValue(IDictionary dictionary, object key, out object value)
        {
            try
            {
                value = dictionary[key];
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }


        /// <summary>
        /// Compare two dictionaries
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            //This should never happen, null check happens one level up
            if (parms.Object1 == null || parms.Object2 == null)
                return;

            // Objects must be the same length
            CheckIfDictionaryCountIsDifferent(parms);

            if (parms.Result.ExceededDifferences)
                return;

            CompareEachItem(parms);
        }


        /// <summary>
        /// Returns true if both types are dictionaries
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsIDictionary(type1) && TypeHelper.IsIDictionary(type2);
        }
    }
}
