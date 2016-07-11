using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SZTQuery {
    public partial class MainForm : Form {
        private delegate void QueryDelegate();
        private QueryDelegate query;
        public MainForm() {
            InitializeComponent();
            this.txtResult.Text = "正在查询......";
            query = new QueryDelegate(DoQuery);
            query.BeginInvoke(null,null);
        }

        private void DoQuery() {
            try {
                string cardno = this.txtCardID.Text;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://query.shenzhentong.com:8080/sztnet/qryCard.do");
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 200000;
                byte[] btBodys = Encoding.UTF8.GetBytes(string.Format("cardno={0}", cardno));
                httpWebRequest.ContentLength = btBodys.Length;
                httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.Default);
                string responseContent = streamReader.ReadToEnd();
                httpWebResponse.Close();
                streamReader.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseContent);
                HtmlNode node = doc.DocumentNode.SelectSingleNode("//table[@class='tableact']/tr");
                if (node != null) {
                    StringBuilder text = new StringBuilder();
                    int i = 0;
                    foreach (HtmlNode tdNode in node.ChildNodes) {
                        i++;
                        if (i % 2 == 0) {
                            text.AppendLine(tdNode.InnerText);
                        }
                        else {
                            text.Append(tdNode.InnerText);
                        }
                    }
                    AddMsg( text.ToString());
                    string dirPath = Environment.CurrentDirectory;
                    string filePath=Path.Combine(dirPath,"Record.txt");
                    FileInfo file = new FileInfo(filePath);
                    FileStream fileStream;
                    if (!file.Exists) {
                        fileStream = file.Create();
                    }
                    else {
                        fileStream = file.Open(FileMode.Append, FileAccess.Write);
                    }
                    StreamWriter sw = new StreamWriter(fileStream);
                    sw.WriteLine("--------------------------------------------");
                    sw.WriteLine("查询时间："+DateTime.Now);
                    sw.WriteLine(text.ToString());
                    sw.Close();
                    fileStream.Close();
                }
                else {
                    AddMsg( "查询异常！");
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                AddMsg(ex.ToString());
            }
        }
        private void AddMsg(string msg) {
            Invoke(new MethodInvoker(delegate() {
                        this.txtResult.Text=msg;
                    }));
        }

        private void txtResult_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.txtResult.Text = string.Empty;
        }

        private void txtCardID_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                query.BeginInvoke(null, null);
            }
        }
    }
}
