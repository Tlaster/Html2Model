using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Html2Model.Attributes;

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
            }
        }
    }

    public class DmzjModel
    {
        [HtmlMultiItems(".boxdiv1")]
        public List<ManhuaModel> Manhuas { get; set; }
    }

    public class ManhuaModel
    {
        [HtmlItem(".pictextst")]
        public string Title { get; set; }

        [HtmlItem("div.picborder > a > img", Attr = "src")]
        public string Image { get; set; }

        [HtmlItem(".pictextli")]
        public string Author { get; set; }

        [HtmlItem("div.pictext > ul > li:nth-child(3)")]
        public string Category { get; set; }

        [HtmlItem("div.pictext > ul > li:nth-child(4)")]
        public string Update { get; set; }
        
        [HtmlItem("div.pictext > ul > li:nth-child(5)")]
        public string State { get; set; }

        [HtmlItem(".numfont")]
        public DateTime UpdateAt { get; set; }
    }
}