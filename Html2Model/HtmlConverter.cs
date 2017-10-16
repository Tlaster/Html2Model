using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Dom;

namespace Html2Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HtmlConverterAttribute : Attribute
    {
        public HtmlConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public Type ConverterType { get; }
    }
    public interface IHtmlConverter
    {
        object ReadHtml(INode node, Type targetType, object existingValue);
    }
}
