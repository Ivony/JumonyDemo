﻿<%@ WebHandler Language="C#" Class="DomViewer_html" %>

using System;
using System.Web;
using System.Linq;
using Ivony.Html;
using Ivony.Html.Web;

public class DomViewer_html : ViewHandler
{

  private ISelector selector;
  private IHtmlDocument document;

  protected override void ProcessScope()
  {

    selector = ViewData["Selector"] as ISelector;
    document = ViewData["Document"] as IHtmlDocument;

    foreach ( var node in document.Nodes() )
      ProcessNode( (IHtmlElement) Scope, node );

  }

  private void ProcessNode( IHtmlElement container, IHtmlNode node, bool encodeWhiteSpace = false )
  {

    var specification = node.Document.HtmlSpecification;

    var special = node as IHtmlSpecial;
    if ( special != null )
    {
      container.AddElement( "div" ).SetAttribute( "class", "special" ).InnerText( special.RawHtml );
      return;
    }

    var comment = node as IHtmlComment;
    if ( special != null )
    {
      container.AddElement( "div" ).SetAttribute( "class", "comment" ).InnerText( comment.RawHtml );
      return;
    }

    var textNode = node as IHtmlTextNode;
    if ( textNode != null )
    {
      container.AddElement( "div" ).SetAttribute( "class", "text" ).InnerText( textNode.HtmlText, encodeWhiteSpace );
      return;
    }

    var element = node as IHtmlElement;
    if ( element != null )
    {
      bool selfClosed = specification.IsForbiddenEndTag( element.Name );

      if ( selector != null && selector.IsEligible( element ) )
        container = container.AddElement( "div" ).SetAttribute( "class", "selected" );


      var beginTag = container.AddElement( "div" ).SetAttribute( "class", "beginTag tag" );

      beginTag.AddElement( "span" ).SetAttribute( "class", "brackets" ).InnerText( "<" );
      beginTag.AddElement( "span" ).SetAttribute( "class", "elementName" ).InnerText( element.Name );

      foreach ( var attribute in element.Attributes() )
      {
        beginTag.AddTextNode( " " );
        beginTag.AddElement( "span" ).SetAttribute( "class", "attributeName" ).InnerText( attribute.Name );
        if ( attribute.AttributeValue != null )
        {
          beginTag.AddTextNode( "=" );
          var attributeValue = beginTag.AddElement( "span" ).SetAttribute( "class", "attributeValue" );
          attributeValue.AddElement( "span" ).SetAttribute( "class", "quote" ).InnerText( "\"" );
          attributeValue.AddTextNode( HtmlEncoding.HtmlAttributeEncode( attribute.AttributeValue ) );
          attributeValue.AddElement( "span" ).SetAttribute( "class", "quote" ).InnerText( "\"" );
        }
      }

      beginTag.AddElement( "span" ).SetAttribute( "class", "brackets" ).InnerText( ">" );


      var _encodeWhiteSpace = false;
      if ( specification.IsCDataElement( element.Name ) )
        _encodeWhiteSpace = true;


      if ( !selfClosed && element.Nodes().Any() )
      {
        var childsContainer = container.AddElement( "div" ).SetAttribute( "class", "childs" );
        foreach ( var childNode in element.Nodes() )
          ProcessNode( childsContainer, childNode, _encodeWhiteSpace );
      }

      if ( !selfClosed )
      {
        var endTag = container.AddElement( "div" ).SetAttribute( "class", "endTag tag" );
        endTag
          .AddElement( "span" ).SetAttribute( "class", "brackets" ).InnerText( "<" )
          .AddElement( "span" ).SetAttribute( "class", "slash" ).InnerText( "/" )
          .AddElement( "span" ).SetAttribute( "class", "elementName" ).InnerText( element.Name )
          .AddElement( "span" ).SetAttribute( "class", "brackets" ).InnerText( ">" );
      }


      return;
    }


  }

}