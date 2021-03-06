﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two properties (Note inherits from BaseComparer instead of TypeComparer
    /// </summary>
    public class PropertyComparer : BaseComparer
    {
        private readonly IndexerComparer _indexerComparer;
        private readonly RootComparer _rootComparer;


        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public PropertyComparer(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
            _indexerComparer = new IndexerComparer(rootComparer);
        }


        /// <summary>
        /// Compare a single property of a class
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="info"></param>
        private void CompareProperty(CompareParms parms, PropertyInfo info)
        {
            //If we can't read it, skip it
            if (info.CanRead == false)
                return;

            //Skip if this is a shallow compare
            if (!parms.Config.CompareChildren && TypeHelper.CanHaveChildren(info.PropertyType))
                return;

            //Skip if it should be excluded based on the configuration
            if (ExcludeLogic.ShouldExcludeMember(parms.Config, info))
                return;

            //If we should ignore read only, skip it
            if (!parms.Config.CompareReadOnly && info.CanWrite == false)
                return;

            //If we ignore types then we must get correct PropertyInfo object
            PropertyInfo secondObjectInfo = GetSecondObjectInfo(parms, info);

            //If the property does not exist, and we are ignoring the object types, skip it
            if (parms.Config.IgnoreObjectTypes && secondObjectInfo == null)
                return;

            object objectValue1;
            object objectValue2;
            bool isIndexer;
            if (!IsValidIndexer(parms.Config, info, parms.BreadCrumb, out isIndexer))
            {
                if (isIndexer)
                    return;

                objectValue1 = info.GetValue(parms.Object1, null);
                objectValue2 = secondObjectInfo != null ? secondObjectInfo.GetValue(parms.Object2, null) : null;
            }
            else
            {
                _indexerComparer.CompareIndexer(parms, info);
                return;
            }

            var currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, info.Name);

            CompareParms childParms = new CompareParms
            {
                Result = parms.Result,
                Config = parms.Config,
                ParentObject1 = parms.Object1,
                ParentObject2 = parms.Object2,
                Object1 = objectValue1,
                Object2 = objectValue2,
                BreadCrumb = currentBreadCrumb
            };

            _rootComparer.Compare(childParms);
        }


        private static IEnumerable<PropertyInfo> GetCurrentProperties(CompareParms parms)
        {
            IEnumerable<PropertyInfo> currentProperties = null;

            //Interface Member Logic
            if (parms.Config.InterfaceMembers.Count > 0)
            {
                Type[] interfaces = parms.Object1Type.GetInterfaces();

                foreach (var type in parms.Config.InterfaceMembers)
                {
                    if (interfaces.Contains(type))
                    {
                        currentProperties = Cache.GetPropertyInfo(parms.Result, type);
                        break;
                    }
                }
            }

            if (currentProperties == null)
                currentProperties = Cache.GetPropertyInfo(parms.Result, parms.Object1Type);
            return currentProperties;
        }


        private static PropertyInfo GetSecondObjectInfo(CompareParms parms, PropertyInfo info)
        {
            PropertyInfo secondObjectInfo = null;
            if (parms.Config.IgnoreObjectTypes)
            {
                IEnumerable<PropertyInfo> secondObjectPropertyInfos = Cache.GetPropertyInfo(parms.Result, parms.Object2Type);

                foreach (var propertyInfo in secondObjectPropertyInfos)
                {
                    if (propertyInfo.Name != info.Name)
                        continue;

                    secondObjectInfo = propertyInfo;
                    break;
                }
            }
            else
                secondObjectInfo = info;
            return secondObjectInfo;
        }


        private bool IsValidIndexer(ComparisonConfig config, PropertyInfo info, BreadCrumb breadCrumb, out bool indexer)
        {
            ParameterInfo[] indexers = info.GetIndexParameters();

            if (indexers.Length == 0)
            {
                indexer = false;
                return false;
            }

            indexer = true;

            if (config.SkipInvalidIndexers)
                return false;

            if (indexers.Length > 1)
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with more than one indexer for object " + breadCrumb);
            }

            if (indexers[0].ParameterType != typeof(Int32))
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with a non integer indexer for object " + breadCrumb);
            }

#if !NEWPCL
            var type = info.ReflectedType;
#else
            var type = info.DeclaringType;
#endif
            if (type == null)
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Cannot compare objects with a null indexer for object " + breadCrumb);
            }

            if (type.GetProperty("Count") == null
                || type.GetProperty("Count").PropertyType != typeof(Int32))
            {
                if (config.SkipInvalidIndexers)
                    return false;

                throw new Exception("Indexer must have a corresponding Count property that is an integer for object " + breadCrumb);
            }

            return true;
        }


        /// <summary>
        /// Compare the properties of a class
        /// </summary>
        public void PerformCompareProperties(CompareParms parms)
        {
            IEnumerable<PropertyInfo> currentProperties = GetCurrentProperties(parms);

            foreach (PropertyInfo info in currentProperties)
            {
                CompareProperty(parms, info);

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }
    }
}
