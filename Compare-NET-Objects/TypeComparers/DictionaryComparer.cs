// Copyright Altua AS. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
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
            // NOTE regarding the dictionary comparer: We don't support checking
            // for more than one difference due to performance reasons
        }

        private bool CompareEachItem(CompareParms @params)
        {
            List<DictionaryEntry> entries1 = GetEntries((IDictionary)@params.Object1);
            List<DictionaryEntry> entries2 = GetEntries((IDictionary)@params.Object2);
            bool[] isKeyFound = new bool[entries1.Count];

            // Background knowledge guiding this implementation:
            // -------------------------------------------------------------------------------
            // We need to use this library to check equality of both keys and items, i.e.
            // we cannot use IDictionary.Contains to test for presence of the key in the
            // other dictionary. This is because we are not certain that the keys correctly
            // implements Equals() and GetHashCode() (and we're not sure that they
            // actually can, e.g. think of the scenario where we want to look up on instance).
            //
            // Keys are known to be unique.
            //
            // Ordering does not matter (that is defined in the definition of IDictionary),
            // however, ordering is a good heuristic to improve performance. Therefore, we
            // will always begin to check the key at the same index in the other entry list.

            for (int i = 0; i < entries1.Count; i++)
            {

                if (Matches(i, i, entries1, entries2, isKeyFound, @params))
                    continue;

                if (!MatchExists(i, entries1, entries2, isKeyFound, @params))
                    return false;
            }

            return true;
        }


        private bool CompareValues(object value1, object value2, CompareParms @params, string propertyName)
        {
            if (@params.Result.Differences.Count > 0)
                throw new InvalidOperationException("Cannot do dictionary comparisons when other differences exists.");

            CompareParms childParams = new CompareParms
            {
                Result = new ComparisonResult(@params.Config),
                Config = @params.Config,
                ParentObject1 = @params.Object1,
                ParentObject2 = @params.Object2,
                Object1 = value1,
                Object2 = value2,
                BreadCrumb = AddBreadCrumb(@params.Config, @params.BreadCrumb, propertyName),
            };

            try
            {
                RootComparer.Compare(childParams);
                return true;
            }
            catch (DifferenceException exp)
            {
                return false;
            }
        }

        private List<DictionaryEntry> GetEntries(IDictionary dictionary)
        {
            List<DictionaryEntry> entries = new List<DictionaryEntry>(dictionary.Count);

            foreach (dynamic keyValuePair in dictionary)
            {
                object key = keyValuePair.Key;
                object value = keyValuePair.Value;
                DictionaryEntry entry = new DictionaryEntry(key, value);
                entries.Add(entry);
            }

            return entries;
        }


        private bool Matches(int index1, int index2, List<DictionaryEntry> entries1, List<DictionaryEntry> entries2, bool[] isKeyFound, CompareParms @params)
        {
            if (isKeyFound[index2])
                return false;

            var key1 = entries1[index1].Key;
            var key2 = entries2[index2].Key;
            if (!CompareValues(key1, key2, @params, "Key"))
                return false;

            isKeyFound[index2] = true;
            var value1 = entries1[index1].Value;
            var value2 = entries2[index2].Value;
            return CompareValues(value1, value2, @params, "Value");
        }

        private bool MatchExists(int index1, List<DictionaryEntry> entries1, List<DictionaryEntry> entries2, bool[] isKeyFound, CompareParms @params)
        {
            for (int index2 = 0; index2 < entries2.Count; index2++)
            {
                if (index1 == index2)
                    continue;

                if (Matches(index1, index2, entries1, entries2, isKeyFound, @params))
                    return true;

                if (!@params.Result.AreEqual)
                    return false;
            }

            return false;
        }


        private bool VerifyCountAndAddDifference(CompareParms parms)
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

                return false;
            }

            return true;
        }

        /// <summary>
        /// Compare two dictionaries
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            //This should never happen, null check happens one level up
            if (parms.Object1 == null || parms.Object2 == null)
                return;

            // This comparer does not support multiple errors
            if (!parms.Result.AreEqual)
                return;

            // Objects must be the same length
            bool isValidCount = VerifyCountAndAddDifference(parms);

            if (!isValidCount)
                return;

            if (!CompareEachItem(parms))
                AddDifference(parms);
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
