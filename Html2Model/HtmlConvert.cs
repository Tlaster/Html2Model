using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            var instance = Expression.Lambda<Func<object>>(
                Expression.New(type.GetConstructor(Type.EmptyTypes))
            ).Compile()();
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
                
                
                var htmlPath = isHtmlItem ? propertyInfo.GetCustomAttribute<HtmlItemAttribute>() as IHtmlPath :
                    isHtmlMultiItems ? propertyInfo.GetCustomAttribute<HtmlMultiItemsAttribute>() as IHtmlPath: null;
                if (htmlPath == null)
                    throw new NullReferenceException();
                var selector = htmlPath.Path;
                var attr = htmlPath.Attr;
                if (string.IsNullOrEmpty(selector))
                    continue;
                if (isHtmlItem)
                {
                    var value = string.IsNullOrEmpty(attr)
                        ? element.QuerySelector(selector).Text()
                        : element.QuerySelector(selector).GetAttribute(attr);
                    object targetValue = null;
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
                            targetValue = Convert.ChangeType(value.Trim(), propertyInfo.PropertyType);
                            break;
                        case TypeCode.DBNull:
                        case TypeCode.Empty:
                            throw new NotSupportedException();
                        default:
                            targetValue = DeserializeObject(element.QuerySelector(selector), propertyInfo.PropertyType);
                            break;
                    }
                    propertyInfo.SetValue(instance, targetValue);
                }
                else if (isHtmlMultiItems)
                {
                    var elements = element.QuerySelectorAll(selector);
                    var itemType = ReflectionHelper.GetCollectionItemType(propertyInfo.PropertyType);
                    if (itemType == null) continue;
                    var list = new List<object>();
                    foreach (var value in elements)
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
                                targetValue = Convert.ChangeType(
                                    (string.IsNullOrEmpty(attr) ? value.Text() : value.GetAttribute(attr)).Trim(),
                                    itemType);
                                break;
                            case TypeCode.DBNull:
                            case TypeCode.Empty:
                                throw new NotSupportedException();
                            default:
                                targetValue = DeserializeObject(value, itemType);
                                break;
                        }
                        list.Add(targetValue);
                    }
                    var targetEnumerable = typeof(Enumerable)
                        .GetMethod("Cast", new[] {typeof(IEnumerable)})
                        .MakeGenericMethod(itemType)
                        .Invoke(null, new object[] {list});
                    if (propertyInfo.PropertyType.IsArray)
                    {
                        propertyInfo.SetValue(instance, typeof(Enumerable)
                            .GetMethod("ToArray")
                            .Invoke(null, new[] {targetEnumerable}));
                    }
                    else if (typeof(List<>).MakeGenericType(itemType) == propertyInfo.PropertyType)
                    {
                        try
                        {
                            propertyInfo.SetValue(instance, typeof(Enumerable)
                                .GetMethod("ToList")
                                .MakeGenericMethod(itemType)
                                .Invoke(null, new[] {targetEnumerable}));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }//TODO:More types
                }
                
            }
            return instance;
        }
    }
}