# Html2Model

Batter way to get infomations from html


# Getting Start  
[![NuGet](https://img.shields.io/badge/NuGet%20-1.0.2-blue.svg)](https://www.nuget.org/packages/Html2Model//)

# Sample  
```C#            

    public class CategoryConverter : IHtmlConverter
    {
        public object ReadHtml(INode node, Type targetType, object existingValue)
        {
            return Regex.Match(existingValue + "", "分类：([^-]+)").Groups[1].Value;
        }
    }
    public class TitlesConverter : IHtmlConverter
    {
        public object ReadHtml(INode node, Type targetType, object existingValue)
        {
            return existingValue + " Converter";
        }
    }

    public class DmzjModel
    {
        [HtmlMultiItems(".boxdiv1")]
        public List<ManhuaModel> Manhuas { get; set; }
        
        [HtmlMultiItems(".pictextst")]
        [HtmlConverter(typeof(TitlesConverter))]
        public string[] Titles { get; set; }
    }

    public class ManhuaModel
    {
        [HtmlItem(".pictext")]
        public ManhuaInfoModel ManhuaInfo { get; set; }
        
        [HtmlItem("div.picborder > a > img", Attr = "src")]
        public string Image { get; set; }

        [HtmlItem("div.pictext > ul > li:nth-child(3)")]
        [HtmlConverter(typeof(CategoryConverter))]
        public string Category { get; set; }

        [HtmlItem("div.pictext > ul > li:nth-child(4)")]
        public string Update { get; set; }
        
        [HtmlItem(".numfont")]
        public DateTime UpdateAt { get; set; }
    }

    public class ManhuaInfoModel
    {

        [HtmlItem(".pictextst")]
        public string Title { get; set; }
        
        [HtmlItem(".pictextli", RegexPattern = "作者:([^-]+)", RegexGroup = 1)]
        public string Author { get; set; }
        
        [HtmlItem("div.pictext > ul > li:nth-child(5)")]
        public string State { get; set; }
    }
```  

# License
```
MIT License

Copyright (c) 2017 Tlaster

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
