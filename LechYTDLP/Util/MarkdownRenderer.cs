using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.System;

namespace LechYTDLP.Util
{
    public static class MarkdownRenderer
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static UIElement Render(string? markdown)
        {
            var container = new StackPanel
            {
                Spacing = 6,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (string.IsNullOrWhiteSpace(markdown))
                return container;

            var document = Markdown.Parse(markdown, Pipeline);

            foreach (var block in document)
            {
                var element = RenderBlock(block);
                if (element != null)
                {
                    container.Children.Add(element);
                }
            }

            return container;
        }

        private static UIElement? RenderBlock(Markdig.Syntax.Block block)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    return RenderHeading(heading);

                case ParagraphBlock paragraph:
                    return RenderParagraph(paragraph);

                case QuoteBlock quote:
                    return RenderQuote(quote);

                case ListBlock list:
                    return RenderList(list);

                case FencedCodeBlock fencedCode:
                    return RenderCodeBlock(fencedCode.Lines.ToString());

                case CodeBlock code:
                    return RenderCodeBlock(code.Lines.ToString());

                case ThematicBreakBlock:
                    return new Border
                    {
                        Height = 1,
                        Background = Application.Current.Resources["DividerStrokeColorDefaultBrush"] as Brush,
                        Margin = new Thickness(0, 8, 0, 8),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                default:
                    if (block is ContainerBlock containerBlock)
                    {
                        var panel = new StackPanel { Spacing = 4 };
                        foreach (var child in containerBlock)
                        {
                            var el = RenderBlock(child);
                            if (el != null) panel.Children.Add(el);
                        }
                        return panel;
                    }
                    return null;
            }
        }

        private static UIElement RenderHeading(HeadingBlock heading)
        {
            var rtb = new RichTextBlock
            {
                IsTextSelectionEnabled = true,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            double fontSize = heading.Level switch
            {
                1 => 20,
                2 => 17,
                3 => 15,
                _ => 14
            };

            var margin = heading.Level switch
            {
                1 => new Thickness(0, 14, 0, 4),
                2 => new Thickness(0, 10, 0, 3),
                _ => new Thickness(0, 6, 0, 2)
            };

            rtb.FontSize = fontSize;
            rtb.FontWeight = FontWeights.SemiBold;
            rtb.Margin = margin;

            var p = new Paragraph();
            if (heading.Inline != null)
            {
                ProcessInlines(heading.Inline, p.Inlines);
            }
            rtb.Blocks.Add(p);

            return rtb;
        }

        private static UIElement RenderParagraph(ParagraphBlock paragraph)
        {
            var rtb = new RichTextBlock
            {
                IsTextSelectionEnabled = true,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 2, 0, 3)
            };

            var p = new Paragraph();
            if (paragraph.Inline != null)
            {
                ProcessInlines(paragraph.Inline, p.Inlines);
            }
            rtb.Blocks.Add(p);

            return rtb;
        }

        private static UIElement RenderQuote(QuoteBlock quote)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(3, 0, 0, 0),
                BorderBrush = Application.Current.Resources["AccentFillColorDefaultBrush"] as Brush,
                Background = Application.Current.Resources["SubtleFillColorSecondaryBrush"] as Brush,
                CornerRadius = new CornerRadius(0, 4, 4, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 6, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var panel = new StackPanel
            {
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            foreach (var child in quote)
            {
                var el = RenderBlock(child);
                if (el != null) panel.Children.Add(el);
            }
            border.Child = panel;
            return border;
        }

        private static UIElement RenderList(ListBlock list)
        {
            var panel = new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(6, 2, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            int index = 1;
            foreach (var item in list)
            {
                if (item is ListItemBlock listItem)
                {
                    var grid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var bullet = new TextBlock
                    {
                        Text = list.IsOrdered ? $"{index}." : "•",
                        FontSize = list.IsOrdered ? 13 : 14,
                        FontWeight = list.IsOrdered ? FontWeights.SemiBold : FontWeights.Bold,
                        Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as Brush,
                        Margin = new Thickness(0, 1, 8, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Grid.SetColumn(bullet, 0);
                    grid.Children.Add(bullet);

                    var itemPanel = new StackPanel
                    {
                        Spacing = 2,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    foreach (var child in listItem)
                    {
                        var el = RenderBlock(child);
                        if (el != null) itemPanel.Children.Add(el);
                    }
                    Grid.SetColumn(itemPanel, 1);
                    grid.Children.Add(itemPanel);

                    panel.Children.Add(grid);
                    index++;
                }
            }

            return panel;
        }

        private static UIElement RenderCodeBlock(string code)
        {
            var border = new Border
            {
                Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush,
                BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as Brush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var tb = new TextBlock
            {
                Text = code.TrimEnd(),
                FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                FontSize = 12,
                IsTextSelectionEnabled = true,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = tb;
            return border;
        }

        private static void ProcessInlines(ContainerInline container, InlineCollection targetInlines)
        {
            foreach (var inline in container)
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        targetInlines.Add(new Run { Text = literal.Content.ToString() });
                        break;

                    case EmphasisInline emphasis:
                        var span = new Span();
                        if (emphasis.DelimiterCount >= 2)
                        {
                            span.FontWeight = FontWeights.Bold;
                        }
                        if (emphasis.DelimiterCount == 1 || emphasis.DelimiterCount == 3)
                        {
                            span.FontStyle = Windows.UI.Text.FontStyle.Italic;
                        }
                        ProcessInlines(emphasis, span.Inlines);
                        targetInlines.Add(span);
                        break;

                    case CodeInline code:
                        var codeSpan = new Span
                        {
                            FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New")
                        };
                        codeSpan.Inlines.Add(new Run
                        {
                            Text = code.Content,
                            Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as Brush
                        });
                        targetInlines.Add(codeSpan);
                        break;

                    case LinkInline link:
                        var hyperlink = new Hyperlink();
                        var url = link.Url;
                        hyperlink.Click += async (s, e) =>
                        {
                            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                            {
                                await Launcher.LaunchUriAsync(uri);
                            }
                        };
                        ProcessInlines(link, hyperlink.Inlines);
                        if (hyperlink.Inlines.Count == 0 && !string.IsNullOrEmpty(url))
                        {
                            hyperlink.Inlines.Add(new Run { Text = url });
                        }
                        targetInlines.Add(hyperlink);
                        break;

                    case LineBreakInline:
                        targetInlines.Add(new LineBreak());
                        break;

                    case ContainerInline childContainer:
                        ProcessInlines(childContainer, targetInlines);
                        break;

                    default:
                        var text = inline.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            targetInlines.Add(new Run { Text = text });
                        }
                        break;
                }
            }
        }
    }
}
