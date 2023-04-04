//
//  Emoji.Wpf — Emoji support for WPF
//
//  Copyright © 2017—2021 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using Stfu.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace Emoji.Wpf
{
    public class TextPlus
    {
        public string Text { get; set; }

        public int caretIndex { get; set; }
    }

    public partial class TextBox : System.Windows.Controls.TextBox
    {
        public TextBox()
        {
            InitializeComponent();
        }

        public string Pre
        {
            get
            {
                string pre = "";
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;

                    m_rtb.Selection.Select(m_rtb.Document.ContentStart, selectStart);

                    pre = m_rtb.Selection.Text;

                    //m_rtb.CaretPosition = selectStart;
                    m_rtb.Selection.Select(selectStart, selectEnd);
                }
                return pre;
            }
        }

        public string Post
        {
            get
            {
                string post = "";
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;

                    m_rtb.Selection.Select(selectEnd, m_rtb.Document.ContentEnd);

                    post = m_rtb.Selection.Text;

                    m_rtb.CaretPosition = selectStart;
                    m_rtb.Selection.Select(selectStart, selectEnd);
                }
                return post;
            }
            set
            {
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (value != "" && m_rtb != null)
                {
                    Inline inline = (m_rtb.Document.Blocks.LastBlock as Paragraph).Inlines.LastInline;
                    Paragraph p = (Paragraph)inline.Parent;
                    Run newRun = new Run(value);
                    //m_rtb.CaretPosition = inline.ElementEnd;
                    //m_rtb.Selection.Select(inline.ElementEnd, inline.ElementEnd);
                    //m_rtb.Focus();
                    p.Inlines.InsertAfter(inline, newRun);
                    //(m_rtb.Document.Blocks.LastBlock as Paragraph).Inlines.Add(value);
                    //m_rtb.CaretPosition = newRun.ElementStart;
                    //m_rtb.Selection.Select(newRun.ElementStart, newRun.ElementStart);                    
                }
            }
        }

        public new bool Focus()
        {
            bool b = base.Focus();
            m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
            if (m_rtb != null)
            {
                b = m_rtb.Focus();
            }
            return b;
        }
        /*
        public string Selected
        {
            get
            {
                string selected = "";
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;

                    selected = m_rtb.Selection.Text;

                    //m_rtb.CaretPosition = selectStart;
                    //m_rtb.Selection.Select(selectStart, selectEnd);
                }
                return selected;
            }
        }
        
        public new int SelectionLength
        {
            get {
                int len = 0;
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;
                    m_rtb.Selection.Select(selectStart, selectEnd);
                    string str = m_rtb.Selection.Text;

                    len = str.Length;
                }

                return len;
            }
        }
        
        public new int SelectionStart
        {
            get {
                int len = 0;
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;

                    TextPointer start = m_rtb.Document.ContentStart;
                    TextPointer caret = m_rtb.CaretPosition;
                    m_rtb.Selection.Select(start, caret);
                    string str = m_rtb.Selection.Text;
                    m_rtb.Selection.Select(selectStart, selectEnd);
                    len = str.Length;
                }
                return len;
            }
        }
        
        public new int CaretIndex
        {
            get
            {
                int len = 0;
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    TextPointer selectStart = m_rtb.Selection.Start;
                    TextPointer selectEnd = m_rtb.Selection.End;

                    TextPointer start = m_rtb.Document.ContentStart;
                    TextPointer caret = m_rtb.CaretPosition;

                    m_rtb.Selection.Select(start, caret);
                    string str = m_rtb.Selection.Text;
                    m_rtb.Selection.Select(selectStart, selectEnd);
                    len = str.Length;                    
                }
                return len;
            }
            set
            {
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                if (m_rtb != null)
                {
                    try
                    {
                        TextPointer start = m_rtb.Document.ContentStart;
                        m_rtb.CaretPosition = start.GetPositionAtOffset(value);
                    }
                    catch
                    {
                        //m_rtb.CaretPosition = m_rtb.Document.ContentStart;
                    }
                }
            }
        }
        */
        private string txt(FlowDocument flowDocument)
        {
            int numBlocks = flowDocument.Blocks.Count;
            int i = 0;
            string t = "";
            foreach (Paragraph paragraph in flowDocument.Blocks)
            {
                i++;
                foreach (Inline inline in paragraph.Inlines)
                {
                    t += new TextSelection(inline.ElementStart, inline.ElementEnd).Text;
                }
                if (i < numBlocks)
                {
                    t += "\n";
                }
            }
            return t;
        }

        public void paintEmoji()
        {
            BeginChange();
            if (m_rtb != null)
            {
                m_rtb.Document.SubstituteGlyphs(SubstituteOptions.None);
            }
                //(ColonSyntax ? SubstituteOptions.ColonSyntax : SubstituteOptions.None) |
                //(ColorBlend ? SubstituteOptions.ColorBlend : ));

            EndChange();
        }
        public new string Text
        {
            get
            {
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                string text = "";
                if (m_rtb != null)
                {
                    //TextPointer st = m_rtb.Selection.Start;
                    //TextPointer end = m_rtb.Selection.End;

                    //m_rtb.SelectAll();
                    //text = m_rtb.Selection.Text;
                    text = m_rtb.Text;
                    Inline inline = (m_rtb.Document.Blocks.LastBlock as Paragraph).Inlines.LastInline;
                    m_rtb.CaretPosition = inline.ElementEnd;
                    m_rtb.Selection.Select(inline.ElementEnd, inline.ElementEnd);
                }
                return text;
            }
            set
            {
                m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
                
                if (m_rtb != null)
                {
                    //this.Text = value;
                    //string v = value;
                    //string pr = this.Pre;
                    
                    //int offset = v.Length - this.Post.Length;
                    //TextPointer caret = m_rtb.Selection.Start;
                    //if (caret.GetInsertionPosition(LogicalDirection.Forward) != null)
                    //{
                    //    caret = caret.GetInsertionPosition(LogicalDirection.Forward);
                    //}
                    m_rtb.Document.Blocks.Clear();
                    m_rtb.Document.Blocks.Add(new Paragraph(new Run(value)));
                    //m_rtb.CaretPosition = m_rtb.
                    Inline inline = (m_rtb.Document.Blocks.LastBlock as Paragraph).Inlines.LastInline;                    
                    m_rtb.CaretPosition = inline.ElementEnd;
                    m_rtb.Selection.Select(inline.ElementEnd, inline.ElementEnd);
                    m_rtb.Focus();
                    //this.CaretIndex = value.Length;
                    this.Select(value.Length, 0);
                }
            }
        }
        /*
        protected TextPlus GetTextPlus(RichTextBox richTextBox)
        {
            TextPlus textPlus = new TextPlus();

            string str = "";
            foreach (Paragraph paragraph in richTextBox.Document.Blocks)
            {
                foreach (Inline inline in paragraph.Inlines)
                {
                    string s = (new TextRange(inline.ContentStart, inline.ContentEnd)).Text.Trim();
                    if (s == "")
                    {
                        EmojiInline emojiInline = inline as EmojiInline;
                        if (emojiInline != null)
                        {
                            str += emojiInline.Text;
                        }
                    }
                    else
                    {
                        str += s;
                    }
                }
            }
            textPlus.Text = str;
            textPlus.caretIndex = textPlus.Text.Length; //need to fix
            return textPlus;
        }
        */
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"Property Changed: {e.Property}");
#endif
            base.OnPropertyChanged(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_rtb = Template.FindName("RichTextBox_INTERNAL", this) as RichTextBox;
            
            if (m_rtb.SpellCheck.IsEnabled == false)
            {
                m_rtb.SpellCheck.IsEnabled = true;
            }
            // Build a list of TextBox properties that are not inherited from Control. These
            // are the properties we want to bind to our child RichTextBox.
            var exclude = GetReadWritePropertyNames(typeof(System.Windows.Controls.Control));
            var propset = GetReadWritePropertyNames(typeof(TextBox));
            propset.ExceptWith(exclude);

#if DEBUG
            var tmp1 = GetReadWritePropertyNames(typeof(RichTextBox));
            tmp1.ExceptWith(exclude);
            var tmp2 = propset.ToHashSet();
            tmp2.ExceptWith(tmp1);
            tmp1.ExceptWith(propset);

            Console.WriteLine("RichTextBox properties not in TextBox: " + string.Join(" ", tmp1));
            Console.WriteLine("TextBox properties not in RichTextBox: " + string.Join(" ", tmp2));
#endif

            // Add some Control properties that we want to inherit
            propset.UnionWith(new List<string>()
            {
                "Foreground",
            });

            // Iterate over all RichTextBox properties; for each found match, create a
            // two-way binding with one of our properties.
            foreach (var dpd in GetReadWriteProperties(typeof(RichTextBox))
                                   .Where(x => propset.Contains(x.Name))
                                   .Select(x => DependencyPropertyDescriptor.FromProperty(x))
                                   .Where(x => x != null))
            {
                m_rtb.SetBinding(dpd.DependencyProperty, new Binding(dpd.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });
            }
        }

        private static IEnumerable<PropertyDescriptor> GetReadWriteProperties(Type t)
            => TypeDescriptor.GetProperties(t, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) })
                             .Cast<PropertyDescriptor>()
                             .Where(x => !x.IsReadOnly);

        private static HashSet<string> GetReadWritePropertyNames(Type t)
            => GetReadWriteProperties(t).Select(x => x.Name).ToHashSet();

        private RichTextBox m_rtb;
        //private int m_caretIndex;
        //private string m_text;
    }
}
