using System;
using System.Collections.Generic;
using System.Reflection;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace uSeoToolkit.Umbraco.Common.Core.Extensions
{
    public static class TreeCollectionBuilderExtensions
    {
        public static TreeCollectionBuilder RemoveTreeController<TController>(this TreeCollectionBuilder collection)
            where TController : TreeControllerBase
        {
            return RemoveTreeController(collection, typeof(TController));
        }

        public static TreeCollectionBuilder RemoveTreeController(this TreeCollectionBuilder collection, Type controllerType)
        {
            var type = typeof(TreeCollectionBuilder);
            
            var field = type.GetField("_trees", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return collection;
            }

            var trees = (List<Tree>)field.GetValue(collection);
            if (trees == null)
            {
                return collection;
            }

            if (typeof(TreeControllerBase).IsAssignableFrom(controllerType) == false)
            {
                throw new ArgumentException($"Type {controllerType} does not inherit from {nameof(TreeControllerBase)}.");
            }

            var exists = trees.FindIndex(x => x.TreeControllerType == controllerType);
            if (exists > -1)
            {
                trees.RemoveAt(exists);
            }

            return collection;
        }
    }
}
