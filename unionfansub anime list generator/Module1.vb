Imports System.Net
Imports System.Web
Imports System.Xml
Imports HtmlAgilityPack

Public Module Module1

    Private ReadOnly htmlTemplate As XElement =
            <root>
                <html>
                    &lt;meta charset="UTF-8"&gt;
                    &lt;body bgcolor="#252526"&gt;
                    &lt;body link="silver"&gt;
                    <head>
                        &lt;meta name="viewport" content="width=device-width, initial-scale=1"&gt;
                        &lt;style type="text/css"&gt;
                            A {text-decoration: none;}

                            ul {
                                margin: 5px;
                                list-style: none;
                            }

                            ul li {
                                margin: 5px;
                                display: inline-block;
                            }

                            ul li a {
                                padding: 5px;
                                display: inline-block;      
                                border: 1px solid #f2f2f2;
                            }

                            ul li a img {
                                display: block;
                            }

                            ul li a:hover img {
                                transform: scale(2.0);
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.5);
                                border: 1px solid #f2f2f2;
                            }
                        &lt;/style&gt;
                    </head>
                    <body>
                        &lt;center&gt;&lt;h1&gt;
                        &lt;a href="0.htm"&gt;0&lt;/a&gt; &lt;a href="a.htm"&gt;A&lt;/a&gt; 
                        &lt;a href="b.htm"&gt;B&lt;/a&gt; &lt;a href="c.htm"&gt;C&lt;/a&gt; 
                        &lt;a href="d.htm"&gt;D&lt;/a&gt; &lt;a href="e.htm"&gt;E&lt;/a&gt; 
                        &lt;a href="f.htm"&gt;F&lt;/a&gt; &lt;a href="g.htm"&gt;G&lt;/a&gt; 
                        &lt;a href="h.htm"&gt;H&lt;/a&gt; &lt;a href="i.htm"&gt;I&lt;/a&gt; 
                        &lt;a href="j.htm"&gt;J&lt;/a&gt; &lt;a href="k.htm"&gt;K&lt;/a&gt; 
                        &lt;a href="l.htm"&gt;L&lt;/a&gt; &lt;a href="m.htm"&gt;M&lt;/a&gt; 
                        &lt;a href="n.htm"&gt;N&lt;/a&gt; &lt;a href="o.htm"&gt;O&lt;/a&gt; 
                        &lt;a href="p.htm"&gt;P&lt;/a&gt; &lt;a href="q.htm"&gt;Q&lt;/a&gt; 
                        &lt;a href="r.htm"&gt;R&lt;/a&gt; &lt;a href="s.htm"&gt;S&lt;/a&gt; 
                        &lt;a href="t.htm"&gt;T&lt;/a&gt; &lt;a href="u.htm"&gt;U&lt;/a&gt; 
                        &lt;a href="v.htm"&gt;V&lt;/a&gt; &lt;a href="w.htm"&gt;W&lt;/a&gt; 
                        &lt;a href="x.htm"&gt;X&lt;/a&gt; &lt;a href="y.htm"&gt;Y&lt;/a&gt; 
                        &lt;a href="z.htm"&gt;Z&lt;/a&gt;
                        &lt;/h1&gt;&lt;/center&gt;
                    </body>
                </html>
            </root>

    Public Sub Main()
        Dim elementList As New Collection(Of XElement)()

        Using wc As New WebClient() With {.Encoding = Encoding.UTF8}

            Dim listUri As New Uri("http://foro.unionfansub.com/announcements.php?aid=3", UriKind.Absolute)
            Dim listDoc As New HtmlDocument()
            listDoc.LoadHtml(wc.DownloadString(listUri))

            Dim listNodes As HtmlNodeCollection = listDoc.DocumentNode.SelectNodes("//div[@class='listado']/div/span/a")
            Dim animeCount As Integer = listNodes.Count()
            Dim currentIndex As Integer

            For Each listNode As HtmlNode In listNodes
                Dim animeUri As New Uri(String.Format("http://foro.unionfansub.com/{0}", listNode.Attributes("href").Value), UriKind.Absolute)
                Dim animeDoc As New HtmlDocument()
                animeDoc.LoadHtml(wc.DownloadString(animeUri))

                Dim animeTitle As String = listNode.InnerText
                Dim animeGenre As String = animeDoc.DocumentNode.SelectSingleNode("//div[@class='ficha']//div//text()[contains(., 'Género')]/../..").InnerText
                Dim animeYear As String = animeDoc.DocumentNode.SelectSingleNode("//div[@class='ficha']//div//span[@class='produccion']").InnerText
                Dim animeSynopsis As String = String.Empty
                Try
                    animeSynopsis = String.Join(Environment.NewLine,
                                                From node As HtmlNode In animeDoc.DocumentNode.SelectSingleNode("//div[@class='post_body'][1]")?.SelectNodes("./text()[normalize-space()]")
                                                Select node.InnerText).Replace(Environment.NewLine, "").Trim({""""c, " "c})
                Catch ' ex As ArgumentNullException
                    ' Do nothing.
                End Try

                Dim element As New XElement(XmlConvert.EncodeLocalName(animeTitle))
                element.Add("<font face=""consolas"" color=""silver"">")
                element.Add(String.Format("<h2><a href=""{0}"">{1}</a></h2>", animeUri.ToString(), animeTitle), Environment.NewLine,
                                String.Format("{0}<br>", animeGenre), Environment.NewLine,
                                String.Format("<b>Año:</b> {0}<br><br>", animeYear), Environment.NewLine,
                                String.Format("{0}<br><br>", animeSynopsis), Environment.NewLine,
                                "</font><ul>")

                Dim imgNodes As HtmlNodeCollection = animeDoc.DocumentNode.SelectNodes("//div[@class='spoil']//text()[contains(., 'Capturas')]/../..//a") ' animeDoc.DocumentNode.SelectNodes("//span[@class='dimg']//@data-src")
                If (imgNodes IsNot Nothing) Then
                    For Each imgNode As HtmlNode In imgNodes
                        Dim href As String = String.Format("http:{0}", imgNode.Attributes("href").Value)
                        Dim dataSrc As String = String.Format("http:{0}", imgNode.SelectSingleNode("./span[@class='dimg']")?.Attributes("data-src")?.Value)

                        ' Images on this domain may not show correctly...
                        If dataSrc.StartsWith("http://img.unionfansub.com/", StringComparison.OrdinalIgnoreCase) Then
                            ' ToDo: fix it.
                        End If

                        element.Add(String.Format("<li><a href=""{0}""><img src=""{1}""></a></li>", href, dataSrc), Environment.NewLine)
                    Next imgNode
                End If
                element.Add("</ul><br><hr>")
                elementList.Add(element)

                Console.WriteLine("({0:0000} of {1:0000}) {2}", Interlocked.Increment(currentIndex), animeCount, animeTitle)
                ' Thread.Sleep(100)
            Next listNode

        End Using
        Console.Clear()

        If Not Directory.Exists(".\Output") Then
            Directory.CreateDirectory(".\Output")
        End If
        Console.WriteLine("Writing Index.htm file...")
        File.WriteAllText(".\Output\_Index.htm", New XElement(htmlTemplate).Value)

        Dim elementLists As IEnumerable(Of KeyValuePair(Of Char, IEnumerable(Of XElement))) =
            (From el As XElement In elementList
             Group By firstChar = Char.ToLower(CChar(StringExtensions.RemoveDiacritics(el.Name.LocalName.First())))
                 Into g = Group Select New KeyValuePair(Of Char, IEnumerable(Of XElement))(firstChar, g))

        For Each list As KeyValuePair(Of Char, IEnumerable(Of XElement)) In elementLists
            Dim fixedChar As Char = If(Char.IsLetter(list.Key), list.Key, "0"c)
            Dim filename As String = String.Format("{0}.htm", fixedChar)
            Dim templateElement As New XElement(htmlTemplate)
            Dim bodyElement As XElement = templateElement.<html>.<body>.Single()
            For Each el As XElement In list.Value
                bodyElement.Add(el)
            Next
            Console.WriteLine("Writing file '{0}'...", filename)
            File.WriteAllText(Path.Combine(".\Output", filename), templateElement.Value)
        Next list

        Console.WriteLine("")
        Console.WriteLine("Done!. Press any key to exit...")
        Console.ReadKey(intercept:=True)
    End Sub

End Module