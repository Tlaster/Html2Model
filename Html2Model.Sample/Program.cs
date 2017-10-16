using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Html2Model.Attributes;
using Newtonsoft.Json;

namespace Html2Model.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync("http://manhua.dmzj.com/update_1.shtml");
                var res = HtmlConvert.DeserializeObject<DmzjModel>(html);
                Console.WriteLine(JsonConvert.SerializeObject(res));
            }
            Console.ReadKey();

        }
    }

    public class CategoryConverter : IHtmlConverter
    {
        public object ReadHtml(INode node, Type targetType, object existingValue)
        {
            return Regex.Match(existingValue + "", "分类：([^-]+)").Groups[1].Value;
        }
    }
    public class DmzjModel
    {
        [HtmlMultiItems(".boxdiv1")]
        public List<ManhuaModel> Manhuas { get; set; }
        [HtmlMultiItems(".pictextst")]
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
}