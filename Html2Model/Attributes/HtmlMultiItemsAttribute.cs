using System;

namespace Html2Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HtmlMultiItemsAttribute : Attribute, IHtmlPath
    {
        public HtmlMultiItemsAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public string Attr { get; set; }
    }
}