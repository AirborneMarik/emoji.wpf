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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Emoji.Wpf
{
    public partial class TextBoxZ : System.Windows.Controls.TextBox
    {
        public TextBoxZ()
        {
            InitializeComponent();
        }

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
            Grid g = Template.FindName("Grid_INTERNAL", this) as Grid;
            m_tb = Template.FindName("TextBlock_INTERNAL", this) as TextBlock;
            m_tb.Padding = new Thickness(0);
            m_tb.Margin = new Thickness(0);
            m_tb.LineHeight = 1.0;
            m_tb.TextWrapping = TextWrapping.WrapWithOverflow;
            ScrollViewer scrollViewer = Template.FindName("PART_ContentHost", this) as ScrollViewer;
            Grid grid = new Grid();
            grid.Margin = new Thickness(0);
            g.Children.Remove(m_tb);
            UIElement uIElement = scrollViewer.Content as UIElement;
            
            scrollViewer.Content = null;
            this.Width = this.Width - 100;
            m_tb.Width = this.Width;
            grid.Children.Add(m_tb);
            grid.Children.Add(uIElement);
            scrollViewer.Content = grid;

            // Build a list of TextBox properties that are not inherited from Control. These
            // are the properties we want to bind to our child RichTextBox.
            var exclude = GetReadWritePropertyNames(typeof(System.Windows.Controls.Control));
            var propset = GetReadWritePropertyNames(typeof(TextBox));
            propset.ExceptWith(exclude);

#if DEBUG
            var tmp1 = GetReadWritePropertyNames(typeof(TextBlock));
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
                "Background",
            });

            // Iterate over all RichTextBox properties; for each found match, create a
            // two-way binding with one of our properties.
            foreach (var dpd in GetReadWriteProperties(typeof(TextBlock))
                                   .Where(x => propset.Contains(x.Name))
                                   .Select(x => DependencyPropertyDescriptor.FromProperty(x))
                                   .Where(x => x != null))
            {
                m_tb.SetBinding(dpd.DependencyProperty, new Binding(dpd.Name)
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

        private TextBlock m_tb;
    }
}
