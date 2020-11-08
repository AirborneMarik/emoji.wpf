﻿//
//  Emoji.Wpf — Emoji support for WPF
//
//  Copyright © 2017—2020 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace Emoji.Wpf
{
    public sealed class TextSelection : TextRange
    {
        internal TextSelection(TextPointer start, TextPointer end)
          : base(start, end) { }

        /// <summary>
        /// Override selection to text conversion in order to convert back all
        /// EmojiInline instances to their equivalent UTF-8 sequences.
        /// </summary>
        public new string Text
        {
            get
            {
                var buf = new StringBuilder();

                for (TextPointer p = Start, next = null;
                     p != null && p.CompareTo(End) < 0;
                     p = next)
                {
                    next = p.GetNextContextPosition(LogicalDirection.Forward);
                    if (next == null)
                        break;

                    switch (p.GetPointerContext(LogicalDirection.Forward))
                    {
                        case TextPointerContext.ElementStart:
                            if (p.GetAdjacentElement(LogicalDirection.Forward) is EmojiInline emoji)
                                buf.Append(emoji.Text);
                            break;
                        case TextPointerContext.ElementEnd:
                        case TextPointerContext.EmbeddedElement:
                            break;
                        case TextPointerContext.Text:
                            // Get text from the Run but don’t go past end
                            buf.Append(new TextRange(p, next.CompareTo(End) < 0 ? next : End).Text);
                            break;
                    }
                }

                return buf.ToString();
            }
        }
    }

    public partial class RichTextBox : System.Windows.Controls.RichTextBox
    {
        public RichTextBox()
        {
            CommandManager.AddPreviewExecutedHandler(this, PreviewExecuted);
            SetValue(Block.LineHeightProperty, 1.0);
            Selection = new TextSelection(Document.ContentStart, Document.ContentStart);
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            Selection = new TextSelection(base.Selection.Start, base.Selection.End);
        }

        /// <summary>
        /// Disable Ctrl-Z to prevent the widget internals from adding data to the
        /// FlowDocument. This is unfortunate but there are too many crashes caused
        /// by this. We probably need a custom undo/redo stack.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z)
                e.Handled = true;
            else
                base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Intercept Copy and Cut commands to make sure the clipboard contains the
        /// proper emoji characters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is RichTextBox rtb)
            {
                if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
                {
                    var selection = rtb.Selection.Text;
                    if (e.Command == ApplicationCommands.Cut)
                        rtb.Cut();
                    Clipboard.SetText(selection);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Replace Emoji characters with EmojiInline objects inside the document.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (m_pending_change)
                return;

            m_pending_change = true;

            base.OnTextChanged(e);

            /* This will prevent our operation from polluting the undo buffer, but it
             * will create an infinite undo stack... need to fix this. */
            BeginChange();

            TextPointer cur = Document.ContentStart;
            while (cur.CompareTo(Document.ContentEnd) < 0)
            {
                TextPointer next = cur.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next == null)
                    break;

                TextRange word = new TextRange(cur, next);
                if (EmojiData.MatchOne.IsMatch(word.Text))
                {
                    Inline inline = new EmojiInline()
                    {
                        Text = word.Text,
                        FontSize = FontSize,
                        FallbackBrush = Foreground,
                    };

                    // Preserve caret position
                    bool caret_was_next = (0 == next.CompareTo(CaretPosition));
                    next = Replace(word, inline);
                    if (caret_was_next)
                        CaretPosition = next;
                }

                cur = next;
            }

            EndChange();

            m_pending_change = false;

            // FIXME: this could be done on-demand by detecting GetValue() calls maybe
            SetValue(TextProperty, new TextSelection(Document.ContentStart, Document.ContentEnd).Text);
#if DEBUG
            var xaml = XamlWriter.Save(Document);
            SetValue(XamlTextProperty, xaml);
#endif
        }

        private bool m_pending_change = false;

        public TextPointer Replace(TextRange range, Inline inline)
        {
            var run = range.Start.Parent as Run;
            if (run == null)
                return range.End;

            var before = new TextRange(run.ContentStart, range.Start).Text;
            var after = new TextRange(range.End, run.ContentEnd).Text;
            var inlines = run.SiblingInlines;

            /* Insert new inlines in reverse order after the run */
            if (!string.IsNullOrEmpty(after))
                inlines.InsertAfter(run, new Run(after));

            inlines.InsertAfter(run, inline);

            if (!string.IsNullOrEmpty(before))
                inlines.InsertAfter(run, new Run(before));

            TextPointer ret = inline.ContentEnd; // FIXME
            inlines.Remove(run);
            return ret;
        }

        public new TextSelection Selection { get; private set; }

        public string Text => (string)GetValue(TextProperty);

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(RichTextBox),
            new PropertyMetadata(""));

#if DEBUG
        public string XamlText => (string)GetValue(XamlTextProperty);

        public static readonly DependencyProperty XamlTextProperty = DependencyProperty.Register(
            nameof(XamlText), typeof(string), typeof(RichTextBox),
            new PropertyMetadata(""));
#endif
    }
}

