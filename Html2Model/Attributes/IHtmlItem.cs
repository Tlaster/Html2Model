namespace Html2Model.Attributes
{
    internal interface IHtmlItem
    {
        string Path { get; }
        string Attr { get; set; }
        string RegexPattern { get; set; }
        int RegexGroup { get; set; }
    }
}