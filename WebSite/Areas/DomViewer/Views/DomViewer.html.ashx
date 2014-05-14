<%@ WebHandler Language="C#" Class="DomViewer_html" %>

using System;
using System.Web;
using System.Linq;
using Ivony.Html;
using Ivony.Html.Web;

public class DomViewer_html : ViewHandler
{

    private ISelector selector;
    private IHtmlDocument document;
    private int index = 0;
    private IHtmlElement code;
    private IHtmlElement gutter;

    protected override void ProcessScope()
    {
        selector = ViewData["Selector"] as ISelector;
        document = ViewData["Document"] as IHtmlDocument;
        code = Scope.FindFirst("td.code");
        gutter = Scope.FindFirst("td.gutter");
        
        foreach (var node in document.Nodes())
            ProcessNode(node, string.Empty);
    }
    private void AddLineNumber()
    {
        index++;
        gutter.AddElement("div").SetAttribute("class", string.Format("line number{0} alt{1}", index, index % 2)).InnerText(index.ToString());
    }
    private void AddCode(string className, string spaces, string value)
    {
        var line = code.AddElement("div").SetAttribute("class", string.Format("line number{0} alt{1}", index, index % 2));
        if (!string.IsNullOrEmpty(spaces))
        {
            line.AddElement("code").SetAttribute("class", "spaces").AddTextNode(spaces);
        }
        line.AddElement("code").SetAttribute("class", className).InnerText(value);
    }
    private void ProcessNode(IHtmlNode node,string spaces)
    {
        var specification = node.Document.HtmlSpecification;

        var special = node as IHtmlSpecial;
        if (special != null)
        {
            AddLineNumber();
            AddCode("plain", spaces, special.RawHtml);
            return;
        }
        //注释
        var comment = node as IHtmlComment;
        if (comment != null)
        {
            string[] contentLines = comment.ToString().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in contentLines)
            {
                AddLineNumber();
                AddCode("comments", spaces, s.TrimStart());
            }
            return; 
        }
        //文本
        var textNode = node as IHtmlTextNode;
        if (textNode != null && !string.IsNullOrWhiteSpace(textNode.HtmlText))
        {
            string[] contentLines = textNode.HtmlText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in contentLines)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                AddLineNumber();
                AddCode("plain", spaces, s);
            }
            return;
        }

        var element = node as IHtmlElement;
        if (element != null)
        {
            bool selfClosed = specification.IsForbiddenEndTag(element.Name);
            AddLineNumber();
            var line = code.AddElement("div").SetAttribute("class", string.Format("line number{0} alt{1}", index, index % 2));
            if (selector != null && selector.IsEligible(element))
            {
                line.SetAttribute("class", string.Format("line number{0} alt{1} selected", index, index % 2));
            }
            if (!string.IsNullOrEmpty(spaces))
            {
                line.AddElement("code").SetAttribute("class", "spaces").AddTextNode(spaces);
            }
            line.AddElement("code").SetAttribute("class", "brackets").InnerText("<");
            line.AddElement("code").SetAttribute("class", "keyword").InnerText(element.Name);
            
            foreach (var attribute in element.Attributes())
            {
                line.AddTextNode("&nbsp;");
                line.AddElement("code").SetAttribute("class", "attributeName").InnerText(attribute.Name);
                if (attribute.AttributeValue != null)
                {
                    line.AddElement("code").SetAttribute("class", "plain").InnerText("=");
                    line.AddElement("code").SetAttribute("class", "quote").InnerText("\"");
                    line.AddElement("code").SetAttribute("class", "attributeValue").InnerText(attribute.AttributeValue.Replace("\n","").Replace("\r",""),true);
                    line.AddElement("code").SetAttribute("class", "quote").InnerText("\"");
                }
            }

            line.AddElement("code").SetAttribute("class", "brackets").InnerText(">");

            if (!selfClosed && element.Nodes().Any())
            {
                foreach (var childNode in element.Nodes())
                    ProcessNode(childNode, spaces + "&nbsp;&nbsp;");
            }

            if (!selfClosed)
            {
                AddLineNumber();
                var endTag = code.AddElement("div").SetAttribute("class", string.Format("line number{0} alt{1}", index, index % 2));
                if (!string.IsNullOrEmpty(spaces))
                {
                    endTag.AddElement("code").SetAttribute("class", "spaces").AddTextNode(spaces);
                }
                endTag.AddElement("code").SetAttribute("class", "brackets").InnerText("</")
                    .AddElement("code").SetAttribute("class", "keyword").InnerText(element.Name)
                    .AddElement("code").SetAttribute("class", "brackets").InnerText(">");
            }
        }
    }
}