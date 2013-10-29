﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;

namespace Loved.Controls {
    public class InlineExpression {
        public static readonly DependencyProperty InlineExpressionProperty = DependencyProperty.RegisterAttached(
            "InlineExpression", typeof(string), typeof(TextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetInlineExpression(TextBlock textBlock, string value) {
            textBlock.SetValue(InlineExpressionProperty, value);

            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(value))
                return;

            var descriptions = GetInlineDescriptions(value);
            if (descriptions.Length == 0)
                return;

            var inlines = GetInlines(textBlock, descriptions);
            if (inlines.Length == 0)
                return;

            textBlock.Inlines.AddRange(inlines);
        }

        public static string GetInlineExpression(TextBlock textBlock) {
            return (string)textBlock.GetValue(InlineExpressionProperty);
        }

        enum InlineType {
            Run,
            LineBreak,
            Span,
            Bold,
            Italic,
            Hyperlink,
            Underline
        }

        class InlineDescription {
            public InlineType Type { get; set; }
            public string Text { get; set; }
            public InlineDescription[] Inlines { get; set; }
            public string StyleName { get; set; }
        }

        private static Inline[] GetInlines(FrameworkElement element, IEnumerable<InlineDescription> inlineDescriptions) {
            var inlines = new List<Inline>();
            foreach (var description in inlineDescriptions) {
                var inline = GetInline(element, description);
                if (inline != null)
                    inlines.Add(inline);
            }

            return inlines.ToArray();
        }

        private static Inline GetInline(FrameworkElement element, InlineDescription description) {
            Style style = null;
            if (!string.IsNullOrEmpty(description.StyleName)) {
                style = element.FindResource(description.StyleName) as Style;
                if (style == null)
                    throw new InvalidOperationException("The style '" + description.StyleName + "' cannot be found");
            }

            Inline inline = null;
            switch (description.Type) {
                case InlineType.Run:
                    var run = new Run(description.Text);
                    inline = run;
                    break;
                case InlineType.LineBreak:
                    var lineBreak = new LineBreak();
                    inline = lineBreak;
                    break;
                case InlineType.Span:
                    var span = new Span();
                    inline = span;
                    break;
                case InlineType.Bold:
                    var bold = new Bold();
                    inline = bold;
                    break;
                case InlineType.Italic:
                    var italic = new Italic();
                    inline = italic;
                    break;
                case InlineType.Hyperlink:
                    var hyperlink = new Hyperlink();
                    inline = hyperlink;
                    break;
                case InlineType.Underline:
                    var underline = new Underline();
                    inline = underline;
                    break;
            }

            if (inline != null) {
                var span = inline as Span;
                if (span != null) {
                    var childInlines = new List<Inline>();
                    foreach (var inlineDescription in description.Inlines) {
                        var childInline = GetInline(element, inlineDescription);
                        if (childInline == null)
                            continue;

                        childInlines.Add(childInline);
                    }

                    span.Inlines.AddRange(childInlines);
                }

                if (style != null)
                    inline.Style = style;
            }

            return inline;
        }

        private static InlineDescription[] GetInlineDescriptions(string inlineExpression) {
            if (inlineExpression == null)
                return new InlineDescription[0];

            inlineExpression = inlineExpression.Trim();
            if (inlineExpression.Length == 0)
                return new InlineDescription[0];

            inlineExpression = inlineExpression.Insert(0, @"<root>");
            inlineExpression = inlineExpression.Insert(inlineExpression.Length, @"</root>");

            var xmlTextReader = new XmlTextReader(new StringReader(inlineExpression));
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlTextReader);

            var rootElement = xmlDocument.DocumentElement;
            if (rootElement == null)
                return new InlineDescription[0];

            var inlineDescriptions = new List<InlineDescription>();

            foreach (XmlNode childNode in rootElement.ChildNodes) {
                var description = GetInlineDescription(childNode);
                if (description == null)
                    continue;

                inlineDescriptions.Add(description);
            }

            return inlineDescriptions.ToArray();
        }

        private static InlineDescription GetInlineDescription(XmlNode node) {
            var element = node as XmlElement;
            if (element != null)
                return GetInlineDescription(element);
            var text = node as XmlText;
            if (text != null)
                return GetInlineDescription(text);
            return null;
        }

        private static InlineDescription GetInlineDescription(XmlElement element) {
            InlineType type;
            var elementName = element.Name.ToLower();
            switch (elementName) {
                case "run":
                    type = InlineType.Run;
                    break;
                case "linebreak":
                    type = InlineType.LineBreak;
                    break;
                case "span":
                    type = InlineType.Span;
                    break;
                case "bold":
                    type = InlineType.Bold;
                    break;
                case "italic":
                    type = InlineType.Italic;
                    break;
                case "hyperlink":
                    type = InlineType.Hyperlink;
                    break;
                case "underline":
                    type = InlineType.Underline;
                    break;
                default:
                    return null;
            }

            string styleName = null;
            var attribute = element.GetAttributeNode("style");
            if (attribute != null)
                styleName = attribute.Value;

            string text = null;
            var childDescriptions = new List<InlineDescription>();

            if (type == InlineType.Run || type == InlineType.LineBreak) {
                text = element.InnerText;
            }
            else {
                foreach (XmlNode childNode in element.ChildNodes) {
                    var childDescription = GetInlineDescription(childNode);
                    if (childDescription == null)
                        continue;

                    childDescriptions.Add(childDescription);
                }
            }

            var inlineDescription = new InlineDescription {
                Type = type,
                StyleName = styleName,
                Text = text,
                Inlines = childDescriptions.ToArray()
            };

            return inlineDescription;
        }

        private static InlineDescription GetInlineDescription(XmlText text) {
            var value = text.Value;
            if (string.IsNullOrEmpty(value))
                return null;

            var inlineDescription = new InlineDescription {
                Type = InlineType.Run,
                Text = value
            };
            return inlineDescription;
        }
    }
}
