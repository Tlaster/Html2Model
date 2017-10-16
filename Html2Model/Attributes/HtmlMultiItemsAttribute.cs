using System;

namespace Html2Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HtmlMultiItemsAttribute : Attribute, IHtmlItem
    {
        public HtmlMultiItemsAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public string Attr { get; set; }
        public string RegexPattern { get; set; }
        public int RegexGroup { get; set; }
    }
}