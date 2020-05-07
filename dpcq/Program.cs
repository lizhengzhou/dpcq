using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace dpcq
{
    class Program
    {
        [Obsolete]
        static void Main(string[] args)
        {

            if (!Directory.Exists("斗破苍穹-蜡笔小勇"))
            {
                Directory.CreateDirectory("斗破苍穹-蜡笔小勇");
            }

            var client = new RestClient();

            var bookId = 13015;

            //获取清单
            var request = new RestRequest("https://16ting.com/ajax/ajax_book.aspx", Method.POST, DataFormat.Json);

            request.AddParameter("bookId", bookId);
            request.AddParameter("Act", "audioshowall");

            var pagesResponse = client.Execute<PagesModel>(request);

            if (pagesResponse.Data != null)
            {
                var pagesStr = pagesResponse.Data.audiodata;
                var matches = Regex.Matches(pagesStr, "<a title='斗破苍穹-蜡笔小勇[0-9]{4}' href='//16ting.com/book/13015-[0-9]{6}.htm'>");
                foreach (Match match in matches)
                {
                    //遍历每个章节
                    if (match.Success)
                    {
                        Console.WriteLine(match.Value);

                        var titleMatch = Regex.Match(match.Value, "[0-9]{4}'");
                        var urlMatch = Regex.Match(match.Value, "//16ting.com/book/13015-[0-9]{6}.htm");
                        var audioIdMatch = Regex.Match(match.Value, "-[0-9]{6}");

                        if (titleMatch.Success && urlMatch.Success && audioIdMatch.Success)
                        {
                            var title = titleMatch.Value.TrimEnd('\'');

                            if (File.Exists("斗破苍穹-蜡笔小勇\\" + title + ".mp3")) continue;

                            var url = "https:" + urlMatch.Value;
                            var audioId = audioIdMatch.Value.Replace("-", "");

                            request = new RestRequest(url);

                            var response = client.Execute(request);

                            if (!string.IsNullOrEmpty(response.Content))
                            {
                                var vcodeMatch = Regex.Match(response.Content, "vcode=\"[0-9,a-z]{32}\"");
                                var stampMatch = Regex.Match(response.Content, "timespans=\"[0-9]{10}\"");
                                if (vcodeMatch.Success && stampMatch.Success)
                                {
                                    var vcode = vcodeMatch.Value.Replace("vcode=", "").Replace("\"", "");
                                    var stamp = stampMatch.Value.Replace("timespans=", "").Replace("\"", "");

                                    request = new RestRequest("https://play.16ting.com/play/pc.aspx?callback=jQuery111307392365635385074_1584849005259&a="
                                        + audioId + "&b=" + bookId + "&t=" + stamp + "&c=" + vcode + "&_=1584849005260");

                                    response = client.Execute(request);

                                    if (!string.IsNullOrEmpty(response.Content))
                                    {
                                        var jsonMatch = Regex.Match(response.Content, "{\\S*}");
                                        if (jsonMatch.Success)
                                        {
                                            var msg = SimpleJson.DeserializeObject<MsgModel>(jsonMatch.Value);
                                            if (msg != null)
                                            {
                                                if (msg.status == "N")
                                                {
                                                    Console.WriteLine(msg.msg);

                                                    Console.ReadKey();
                                                }
                                                else
                                                {
                                                    request = new RestRequest(msg.msg);
                                                    request.AddHeader("accept", "*/*");


                                                    client.DownloadData(request).SaveAs("斗破苍穹-蜡笔小勇\\" + title + ".mp3");


                                                    Thread.Sleep(60000);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        public class PagesModel
        {
            public string audiodata { get; set; }
        }

        public class MsgModel
        {
            public string status { get; set; }

            public string msg { get; set; }

        }

    }
}
