using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using System.Collections;

namespace TracePlays
{
    public partial class Form1 : Form
    {
        string machDate = string.Empty;
        string urlFootball = string.Empty;
        string urlHockey = string.Empty;
        string gameDate = string.Empty;
        string outputDir = "TP_OUT";
        double progVer = 1.0;

        List<string> searchResultList = new List<string>();
        int searchIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetUrl();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            this.Text = $"Trace Place (v. {progVer})";
        }

        private void GetUrl()
        {
            machDate = dateTimePicker1.Value.Date.ToString("yyyy-MM-dd");
            urlFootball = "https://www.sports.ru/football/match/" + machDate + "/";
            urlHockey = "https://www.sports.ru/hockey/match/" + machDate + "/";
            //textBox1.Text = url;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            TVSearch(0);
            var noDupesList = searchResultList.Distinct().ToList();     // Удаляем дубли (появляются из-за рекурсии)
            searchResultList = noDupesList;
            //MessageBox.Show(noDupes.Count.ToString());

            if (searchResultList.Count > 0)
            {
                if (searchResultList.Count > 1)
                {
                    button6.Visible = true;
                    MessageBox.Show("Найдено совпадений: " + searchResultList.Count.ToString());
                }
                TreeNode SelectedNode = SearchNode(searchResultList[0], treeView1.Nodes[0]); //пытаемся найти в поле Text
                this.treeView1.SelectedNode = SelectedNode;
                this.treeView1.SelectedNode.Expand();
                this.treeView1.Select();
            }
            else { MessageBox.Show("Ничего не найдено!"); }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Перебираем поск по списку
            searchIndex = searchIndex + 1;
            if (searchResultList.Count == searchIndex)  // Если индекс выходит за границу списка, то начать поиск сначала
            {
                searchIndex = 0;
            }
            TreeNode SelectedNode = SearchNode(searchResultList[searchIndex], treeView1.Nodes[0]); //пытаемся найти в поле Text
            this.treeView1.SelectedNode = SelectedNode;
            this.treeView1.SelectedNode.Expand();
            this.treeView1.Select();
        }

        private void TVSearch(int startIndex)
        {
            searchResultList.Clear();
            searchIndex = 0;
            button6.Visible = false;
            try
            {
                string SearchText = this.textBox1.Text;
                if (SearchText == "")
                {
                    return;
                };

                SearchNodeToList(SearchText, treeView1.Nodes[startIndex]);                
            }
            catch { }
        }

        private TreeNode SearchNode(string SearchText, TreeNode StartNode)
        {
            TreeNode node = null;
            while (StartNode != null)
            {
                if (StartNode.Text.ToLower().Contains(SearchText.ToLower()))
                {
                    node = StartNode; //чето нашли, выходим
                    break;
                };
                if (StartNode.Nodes.Count != 0) //у узла есть дочерние элементы
                {
                    node = SearchNode(SearchText, StartNode.Nodes[0]); //ищем рекурсивно в дочерних
                    if (node != null)
                    {
                        break;//чето нашли
                    };
                };
                StartNode = StartNode.NextNode;
            };
            return node;//вернули результат поиска
        }

        private TreeNode SearchNodeToList(string SearchText, TreeNode StartNode)
        {
            TreeNode node = null;
            while (StartNode != null)
            {
                if (StartNode.Text.ToLower().Contains(SearchText.ToLower()))
                {
                    node = StartNode; //чето нашли, выходим
                    searchResultList.Add(node.Text);
                    //break;
                };
                if (StartNode.Nodes.Count != 0) //у узла есть дочерние элементы
                {
                    node = SearchNodeToList(SearchText, StartNode.Nodes[0]); //ищем рекурсивно в дочерних
                    if (node != null)
                    {
                        searchResultList.Add(node.Text);
                        //break;//чето нашли
                    };
                };
                StartNode = StartNode.NextNode;
            };
            return node;//вернули результат поиска
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveToRTF();
            //SaveToXML();
        }

        private void SaveToRTF()
        {
            string rtfFile = "Игры на " + gameDate + ".rtf";
            string pathOUT = ".\\"+ outputDir + "\\" + rtfFile;
            try
            {
                Directory.CreateDirectory(outputDir);
                richTextBox1.SaveFile(pathOUT);
                Process.Start(pathOUT);
                Process.Start("explorer", ".\\" + outputDir);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private void SaveToXML()
        {
            string xmlFile = "xml.xml";
            string pathOUT = ".\\" + outputDir + "\\" + xmlFile;
            /*var rootElement = new XElement("root", CreateXmlElement(treeView1.Nodes));
            var document = new XDocument(rootElement);
            document.Save(pathOUT);*/
            // Этот пример записывает xml в строку
            var buffer = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };

            // Измените вызов метода Create, если надо писать в файл, например.
            using (var writer = XmlTextWriter.Create(buffer, settings))
            {
                writer.WriteStartElement("nodes");
                Traverse(treeView1.Nodes[0], writer);
                writer.WriteEndDocument();
            }
            File.WriteAllText(pathOUT, buffer.ToString());
            //MessageBox.Show(buffer.ToString());
        }

        private void Traverse(TreeNode root, XmlWriter w)
        {
            if (root == null) return;

            //if (root.Checked)
            {
                w.WriteStartElement("node");
                w.WriteAttributeString("id", root.Text);
                Traverse(root.FirstNode, w);
                w.WriteEndElement();
            }
            Traverse(root.NextNode, w);
        }

        private static List<XElement> CreateXmlElement(TreeNodeCollection treeViewNodes)
        {
            var elements = new List<XElement>();
            foreach (TreeNode treeViewNode in treeViewNodes)
            {
                //MessageBox.Show(treeViewNode.Text);
                try
                {
                    var element = new XElement(treeViewNode.Text.Replace(" ", "-"));
                    if (treeViewNode.GetNodeCount(true) == 1)
                        element.Value = treeViewNode.Nodes[0].Name;
                    else
                        element.Add(CreateXmlElement(treeViewNode.Nodes));
                    elements.Add(element);
                }
                catch { }
            }
            return elements;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String s = richTextBox1.SelectedText;
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
            richTextBox1.SelectedText = s;
            //richTextBox1.Rtf.
        }

        public string getRequest(string url)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.AllowAutoRedirect = false;//Запрещаем автоматический редирект
                //httpWebRequest.Headers["User-Agent"] = "Mozilla/5.0";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:35.0) Gecko/20100101 Firefox/35.0";
                httpWebRequest.Method = "GET"; //Можно не указывать, по умолчанию используется GET.
                httpWebRequest.Referer = "http://google.com"; // Реферер. Тут можно указать любой URL
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var stream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet)))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return String.Empty;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //richTextBox1.Clear();
            button4.Text = "Ждите...";
            button4.Enabled = false;            
            button5.Enabled = false;
            button6.Visible = false;
            gameDate = dateTimePicker1.Value.ToString("dd.MM.yyy");
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("Футбол " + gameDate);
            treeView1.Nodes.Add("Хоккей " + gameDate);
            treeView1.Enabled = false;
            GetUrl();
            GetPlays(urlFootball, 0);
            GetPlays(urlHockey, 1);
            button4.Enabled = true;
            button4.Text = "Получить игры на дату";
            treeView1.Enabled = true;
        }

        private void GetPlays(string url, int nodeIndex)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(getRequest(url));
            //StreamWriter sw = new StreamWriter(".\\1.xml", true, Encoding.Default);    // false - перезаписывает файл, true - дополняет файл

            //HtmlNodeCollection leagues = doc.DocumentNode.SelectNodes("//div[@class='light-gray-title  corners-3px']/a");
            HtmlNodeCollection leagues = doc.DocumentNode.SelectNodes("//div[@style='clear:both']/a");      // Блок с лигами
            //HtmlNodeCollection coll = doc.DocumentNode.SelectNodes("//div[@class='stat onlines-box']");
            HtmlNodeCollection coll = doc.DocumentNode.SelectNodes("//table[@class='stat-table matches-table']");   // Блок с играми в одной лиге
            int i = -1;
            string xmlString = string.Empty;
            if (leagues != null)
            {
                foreach (HtmlNode node in leagues)
                {
                    i++;
                    treeView1.Nodes[nodeIndex].Nodes.Add(node.InnerText);
                    //sw.WriteLine("<League Name=\""+ node.InnerText + "\">");
                    xmlString += "<League Name=\"" + node.InnerText + "\">" +"\n";
                    
                    HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                    string str = "";
                    string gameName = string.Empty;
                    string gameTime = string.Empty;
                    string gameScore = string.Empty;
                    string gameTeam1 = string.Empty;
                    string gameTeam2 = string.Empty;
                    string gameTeam1Points = string.Empty;
                    string gameTeam2Points = string.Empty;
                    doc1.LoadHtml(coll[i].InnerHtml.ToString());
                    HtmlNodeCollection collMatches = doc1.DocumentNode.SelectNodes("//tr[@data-match-id]");
                    if (coll != null)
                    {
                        if (collMatches != null)
                        {
                            foreach (HtmlNode nod in collMatches)   // Получаем блоки с играми в данной лиге
                            {
                                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                                doc2.LoadHtml(nod.InnerHtml.ToString());
                                str = "";
                                HtmlNodeCollection collGameTime = doc2.DocumentNode.SelectNodes("//td[@class='alLeft gray-text']");
                                HtmlNodeCollection collCom1 = doc2.DocumentNode.SelectNodes("//td[@class='owner-td']");
                                HtmlNodeCollection collCom2 = doc2.DocumentNode.SelectNodes("//td[@class='guests-td']");
                                HtmlNodeCollection coll1 = doc2.DocumentNode.SelectNodes("//span[@class='s-left']");
                                HtmlNodeCollection coll2 = doc2.DocumentNode.SelectNodes("//span[@class='s-right']");

                                char[] charsToTrim = { '\n' };   // Для отрезки перевода строки
                                if (collGameTime != null)
                                {
                                    str = collGameTime[0].InnerText + " ";
                                    gameTime = collGameTime[0].InnerText;
                                    if (collGameTime[1].InnerText.Contains("Отмен") || collGameTime[1].InnerText.Contains("Перене"))
                                    {
                                        str = collGameTime[1].InnerText + " ";
                                        gameTime = collGameTime[1].InnerText;
                                        //xmlString += "<MatchTime>" + collGameTime[1].InnerText + "</MatchTime>" + "\n";
                                    }
                                }
                                if (collCom1 != null)   // Получаем команду 1
                                {
                                    foreach (HtmlNode node1 in collCom1)
                                    {
                                        gameTeam1 = node1.InnerText.Trim(charsToTrim);
                                        str += gameTeam1 + " - ";
                                        //xmlString += "<MatchName>" + team1 + " - ";
                                    }
                                }
                                if (collCom2 != null)   // Получаем команду 2
                                {
                                    foreach (HtmlNode node2 in collCom2)
                                    {
                                        gameTeam2 = node2.InnerText.Trim(charsToTrim);
                                        str += gameTeam2;
                                        //xmlString += team2 + "</MatchName>" + "\n";
                                    }
                                }
                                if (coll1 != null)   // Получаем счет команды 1
                                {
                                    gameTeam1Points = coll1[0].InnerText;
                                    str += " (" + gameTeam1Points + ":";
                                    //xmlString += "<MatchScore>" + team1Points + ":";
                                }
                                if (coll2 != null)   // Получаем счет команды 2
                                {
                                    gameTeam2Points = coll2[0].InnerText;
                                    str += gameTeam2Points + ")";
                                    //xmlString += team2Points + "</MatchScore>" + "\n";
                                }
                                treeView1.Nodes[nodeIndex].Nodes[i].Nodes.Add(str);
                                //xmlString += "</Match>" + "\n";
                                gameName = gameTeam1 + " - " + gameTeam2;
                                if (gameTeam1Points != "" || gameTeam2Points != "")
                                    gameScore = gameTeam1Points + ":" + gameTeam2Points;
                                xmlString += "<Match>" + "\n";
                                xmlString += "<MatchName>" + gameName + "</MatchName>" + "\n";
                                xmlString += "<MatchDate>" + gameDate + "</MatchDate>" + "\n";
                                xmlString += "<MatchTime>" + gameTime + "</MatchTime>" + "\n";
                                xmlString += "<MatchScore>" + gameScore + "</MatchScore>" + "\n";
                                xmlString += "</Match>" + "\n";
                            }
                        }
                        else { MessageBox.Show("null"); /*i1--;*/ }
                        button5.Enabled = true;
                    }
                    //sw.WriteLine("</League>");
                    xmlString += "</League>" + "\n";
                    //MessageBox.Show(xmlString);
                    //richTextBox1.AppendText(xmlString);   // Показать сгенерированный XML
                }

            }
            else
                richTextBox1.AppendText("Ничего не найдено!\n");            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            int titleSize = Convert.ToInt32(numericUpDown2.Value)*2;
            int textSize = Convert.ToInt32(numericUpDown1.Value) * 2;
            foreach (TreeNode nd in treeView1.Nodes)
            {
                string ndText = @"{\rtf1 \ansicpg1251 {\b \ul \fs" + titleSize + " " + nd.Text + @"\line} }";
                //richTextBox1.SelectedRtf = ndText;
                int i = 0;
                foreach (TreeNode node in treeView1.Nodes[nd.Index].Nodes)
                {                    
                    foreach (TreeNode nodes in node.Nodes)
                    {                        
                        if (nodes.Checked)
                        {
                            i++;
                            if (i == 1) // Если это первая строка, то добавить вид спорта и дату
                                richTextBox1.SelectedRtf = ndText;
                            if (!richTextBox1.Text.Contains(node.Text)) // Добавить лигу
                            {
                                /*if (richTextBox1.Text.Length != 0)
                                    richTextBox1.AppendText("\n");*/
                                /*else
                                    richTextBox1.Rtf = @"{\rtf1 \ansicpg1251 {\b \fs52 " + nd.Text + @"\line} }";*/

                                /*richTextBox1.AppendText("- " +node.Text + " -\n");
                                int rtb1Lenght = richTextBox1.Text.Length - node.Text.Length - 3;
                                richTextBox1.Select(rtb1Lenght, node.Text.Length);
                                richTextBox1.SelectionFont = new Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size, FontStyle.Bold);
                                */
                                richTextBox1.SelectedRtf = @"{\rtf1 \ansicpg1251 {\fs"+ textSize/3 + @" \line}{\b \ul \fs" + textSize + " " + node.Text + @"\line} }";
                            }
                            richTextBox1.AppendText(nodes.Text + "\n");
                            //richTextBox1.SelectedRtf = @"{\rtf1 \ansicpg1251 {\line \fs26 " + nodes.Text + @"} }";
                        }
                        
                    }           
                }
                if (i != 0)
                    richTextBox1.AppendText("\n");
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeViewChangeCheckBox(e.Node);
        }
        private void treeViewChangeCheckBox(TreeNode Node)
        {
            for (int i = 0; i < Node.Nodes.Count; i++)
            {
                Node.Nodes[i].Checked = Node.Checked;
                treeViewChangeCheckBox(Node.Nodes[i]);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            /*int newFontSize = Convert.ToInt32(numericUpDown1.Value); //размер текста
            //FontStyle style = (FontStyle.Bold | FontStyle.Italic | FontStyle.Underline); //жирный, курсив, подчеркнутый
            Font style = richTextBox1.Font;
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, (float)newFontSize,style.Style);*/
            button5.PerformClick();
            //richTextBox1.Font.Size = numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e) //размер заголовков
        {
            button5.PerformClick();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SaveToRTF();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button6.Visible = false;
        }
    }
}
