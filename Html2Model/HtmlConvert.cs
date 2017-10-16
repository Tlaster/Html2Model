using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Html2Model.Attributes;
using Html2Model.Helpers;

namespace Html2Model
{
    public static class HtmlConvert
    {
        public static T DeserializeObject<T>(string html)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            return (T) DeserializeObject(doc, typeof(T));
        }

        public static object DeserializeObject(string html, Type type)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            return DeserializeObject(doc, type);
        }

        private static object DeserializeObject(IParentNode element, Type type)
        {
            var properties =
                type.GetProperties()
                    .Where(item => item.CanWrite && item.CanRead);
            var instance = CreateInstance(type);
            foreach (var propertyInfo in properties)
            {
                var isHtmlItem = Attribute.IsDefined(propertyInfo, typeof(HtmlItemAttribute));
                var isHtmlMultiItems = Attribute.IsDefined(propertyInfo, typeof(HtmlMultiItemsAttribute));
                if (!isHtmlMultiItems && !isHtmlItem)
                    continue;


//                if (isHtmlMultiItems && !typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
//                    throw new NotSupportedException($"{type.AssemblyQualifiedName}.{propertyInfo.Name} must implment IEnumerable");
//                if (isHtmlItem && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
//                    throw new NotSupportedException($"{type.AssemblyQualifiedName}.{propertyInfo.Name} must not implment IEnumerable");
//                if (isHtmlMultiItems && typeof(IDictionary).IsAssignableFrom(propertyInfo.PropertyType))
//                    throw new NotSupportedException($"Current not support IDictionary at {type.AssemblyQualifiedName}.{propertyInfo.Name}");


                if (isHtmlItem)
                {
                    DeserializeHtmlItem(ref instance, propertyInfo, element);
                }
                else if (isHtmlMultiItems)
                {
                    DeserializeHtmlMultiItems(ref instance, propertyInfo, element);
                }
            }
            return instance;
        }

        private static object CreateInstance(Type type)
        {
            return Expression.Lambda<Func<object>>(
                Expression.New(type.GetConstructor(Type.EmptyTypes))
            ).Compile()();
        }

        private static void DeserializeHtmlMultiItems(ref object instance, PropertyInfo propertyInfo, IParentNode element)
        {
            var attributes = propertyInfo.GetCustomAttributes<HtmlMultiItemsAttribute>().Cast<IHtmlItem>().ToList();
            if (attributes?.Any() != true)
                throw new NullReferenceException();
            var tuple = GetFirstOfDefaultNodes(element, attributes);
            if (tuple.Elements == null || tuple.HtmlItem == null)
                return;
            var itemType = ReflectionHelper.GetCollectionItemType(propertyInfo.PropertyType);
            if (itemType == null) return;
            var list = new List<object>();
            var converter = CheckForConverter(propertyInfo);
            foreach (var value in tuple.Elements)
            {
                object targetValue = null;
                switch (Type.GetTypeCode(itemType))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Char:
                    case TypeCode.DateTime:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.String:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        var text = (string.IsNullOrEmpty(tuple.HtmlItem.Attr) ? value.Text() : value.GetAttribute(tuple.HtmlItem.Attr)).Trim();
                        if (!string.IsNullOrEmpty(tuple.HtmlItem.RegexPattern) && !string.IsNullOrEmpty(text))
                            text = Regex.Match(text, tuple.HtmlItem.RegexPattern).Groups[tuple.HtmlItem.RegexGroup].Value;
                        targetValue = Convert.ChangeType(text, itemType);
                        break;
                    case TypeCode.DBNull:
                    case TypeCode.Empty:
                        throw new NotSupportedException();
                    default:
                        targetValue = DeserializeObject(value, itemType);
                        break;
                }
                if (converter != null)
                    targetValue = converter.ReadHtml(value, itemType, targetValue);
                list.Add(targetValue);
            }
            var targetEnumerable = typeof(Enumerable)
                .GetMethod("Cast", new[] { typeof(IEnumerable) })
                .MakeGenericMethod(itemType)
                .Invoke(null, new object[] { list });
            if (propertyInfo.PropertyType.IsArray)
                propertyInfo.SetValue(instance, typeof(Enumerable)
                    .GetMethod("ToArray")
                    .MakeGenericMethod(itemType)
                    .Invoke(null, new[] { targetEnumerable }));
            else if (typeof(List<>).MakeGenericType(itemType) == propertyInfo.PropertyType)
                try
                {
                    propertyInfo.SetValue(instance, typeof(Enumerable)
                        .GetMethod("ToList")
                        .MakeGenericMethod(itemType)
                        .Invoke(null, new[] { targetEnumerable }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
        }

        private static void DeserializeHtmlItem(ref object instance, PropertyInfo propertyInfo, IParentNode element)
        {
            var attributes = propertyInfo.GetCustomAttributes<HtmlItemAttribute>().Cast<IHtmlItem>().ToList();
            if (attributes?.Any() != true)
                throw new NullReferenceException();
            object targetValue;
            var tuple = GetFirstOfDefaultNode(element, attributes);
            if (tuple.Element == null || tuple.HtmlItem == null)
                return;
            var converter = CheckForConverter(propertyInfo);
            switch (Type.GetTypeCode(propertyInfo.PropertyType))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    var value = (string.IsNullOrEmpty(tuple.HtmlItem.Attr)
                        ? tuple.Element.Text()
                        : tuple.Element.GetAttribute(tuple.HtmlItem.Attr)).Trim();
                    if (!string.IsNullOrEmpty(tuple.HtmlItem.RegexPattern))
                        value = Regex.Match(value, tuple.HtmlItem.RegexPattern).Groups[tuple.HtmlItem.RegexGroup].Value;
                    targetValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                    break;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new NotSupportedException();
                default:
                    targetValue = DeserializeObject(tuple.Element, propertyInfo.PropertyType);
                    break;
            }
            if (converter != null)
                targetValue = converter.ReadHtml(tuple.Element, propertyInfo.PropertyType, targetValue);
            propertyInfo.SetValue(instance, targetValue);
        }

        private static IHtmlConverter CheckForConverter(MemberInfo propertyInfo)
        {
            if (!Attribute.IsDefined(propertyInfo, typeof(HtmlConverterAttribute))) return null;
            if (CreateInstance(propertyInfo.GetCustomAttribute<HtmlConverterAttribute>().ConverterType) is
                IHtmlConverter converter)
                return converter;
            return null;
        }

        private static (IElement Element, IHtmlItem HtmlItem) GetFirstOfDefaultNode(IParentNode element, IEnumerable<IHtmlItem> attributes)
        {
            IElement node = null;
            IHtmlItem htmlItem = null;
            foreach (var attribute in attributes)
            {
                node = element.QuerySelector(attribute.Path);
                if (node == null) continue;
                htmlItem = attribute;
                break;
            }
            return (node, htmlItem);
        }


        private static (IHtmlCollection<IElement> Elements, IHtmlItem HtmlItem) GetFirstOfDefaultNodes(IParentNode element, IEnumerable<IHtmlItem> attributes)
        {
            IHtmlCollection<IElement> node = null;
            IHtmlItem htmlItem = null;
            foreach (var attribute in attributes)
            {
                node = element.QuerySelectorAll(attribute.Path);
                if (node == null || !node.Any()) continue;
                htmlItem = attribute;
                break;
            }
            return (node, htmlItem);
        }
    }
}