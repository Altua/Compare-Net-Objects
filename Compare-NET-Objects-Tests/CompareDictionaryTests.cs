using System;
using System.Collections.Generic;
using System.Diagnostics;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjectsTests.TestClasses;
using NUnit.Framework;

namespace KellermanSoftware.CompareNetObjectsTests
{
    [TestFixture]
    public class CompareDictionaryTests
    {
        #region Class Variables
        private CompareLogic _compare;
        #endregion

        #region Setup/Teardown

        /// <summary>
        /// Code that is run once for a suite of tests
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {

        }

        /// <summary>
        /// Code that is run once after a suite of tests has finished executing
        /// </summary>
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {

        }

        /// <summary>
        /// Code that is run before each test
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            _compare = new CompareLogic();
        }

        /// <summary>
        /// Code that is run after each test
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            _compare = null;
        }
        #endregion

        #region Tests
        [Test]
        public void TestDictionary()
        {
            Person p1 = new Person();
            p1.DateCreated = DateTime.Now;
            p1.Name = "Owen";
            Person p2 = new Person();
            p2.Name = "Greg";
            p2.DateCreated = DateTime.Now.AddDays(-1);

            Dictionary<string, Person> dict1 = new Dictionary<string, Person>();
            dict1.Add("1001", p1);
            dict1.Add("1002", p2);

            Dictionary<string, Person> dict2 = Common.CloneWithSerialization(dict1);

            ComparisonResult result = _compare.Compare(dict1, dict2);

            if (!result.AreEqual)
                throw new Exception(result.DifferencesString);

        }





        [Test]
        public void TestDictionaryNegative()
        {
            Person p1 = new Person();
            p1.DateCreated = DateTime.Now;
            p1.Name = "Owen";
            Person p2 = new Person();
            p2.Name = "Greg";
            p2.DateCreated = DateTime.Now.AddDays(-1);

            Dictionary<string, Person> dict1 = new Dictionary<string, Person>();
            dict1.Add("1001", p1);
            dict1.Add("1002", p2);

            Dictionary<string, Person> dict2 = Common.CloneWithSerialization(dict1);

            dict2["1002"].DateCreated = DateTime.Now.AddDays(1);

            Assert.IsFalse(_compare.Compare(dict1, dict2).AreEqual);

        }





        [Test]
        public void Dictionary_CompareToDictionaryWithOtherOrder_ReturnsTrue()
        {
            DictionaryKey key1 = new DictionaryKey(1);
            DictionaryKey key2 = new DictionaryKey(2);
            Dictionary<DictionaryKey, string> dict1 = new Dictionary<DictionaryKey, string>();
            dict1.Add(key1, "value1");
            dict1.Add(key2, "value2");
            // Added in the opposite order
            Dictionary<DictionaryKey, string> dict2 = new Dictionary<DictionaryKey, string>();
            dict2.Add(key2, "value2");
            dict2.Add(key1, "value1");

            ComparisonResult comparisonResult = _compare.Compare(dict1, dict2);
            bool areEqual = comparisonResult.AreEqual;
            Assert.IsTrue(areEqual);
        }


        private sealed class DictionaryKey
        {
            private readonly int _equality;


            public DictionaryKey(int equality)
            {
                _equality = equality;
            }


            private bool Equals(DictionaryKey other)
            {
                return _equality == other._equality;
            }


            /// <summary>Determines whether the specified object is equal to the current object.</summary>
            /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
            /// <param name="obj">The object to compare with the current object. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                return obj is DictionaryKey && Equals((DictionaryKey) obj);
            }


            public override int GetHashCode()
            {
                // NOTE: We always return the same hash code to force the same buckets for the dictionary
                return 0;
            }
        }
        #endregion
    }
}
