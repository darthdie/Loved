﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Loved.Controls {
    public class EditableTextBlockAdorner : Adorner {
        private readonly VisualCollection _collection;

        private readonly TextBox _textBox;

        private readonly TextBlock _textBlock;

        public EditableTextBlockAdorner(EditableTextBlock adornedElement)
            : base(adornedElement) {
            _collection = new VisualCollection(this);
            _textBox = new TextBox();
            _textBlock = adornedElement;
            var binding = new Binding("Text") { Source = adornedElement };
            _textBox.SetBinding(TextBox.TextProperty, binding);
            _textBox.AcceptsReturn = true;
            _textBox.MaxLength = adornedElement.MaxLength;
            _textBox.KeyUp += _textBox_KeyUp;
            _collection.Add(_textBox);
        }

        void _textBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                _textBox.Text = _textBox.Text.Replace("\r\n", string.Empty);
                var expression = _textBox.GetBindingExpression(TextBox.TextProperty);
                if (null != expression) {
                    expression.UpdateSource();
                }
            }
        }

        protected override Visual GetVisualChild(int index) {
            return _collection[index];
        }

        protected override int VisualChildrenCount {
            get { return _collection.Count; }
        }

        protected override Size ArrangeOverride(Size finalSize) {
            _textBox.Arrange(new Rect(0, GetYOffset(), _textBlock.DesiredSize.Width + 50, _textBlock.DesiredSize.Height * 1.3));
            _textBox.Focus();
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            drawingContext.DrawRectangle(null, new Pen {
                Brush = Brushes.Gold,
                Thickness = 2
            }, new Rect(0, GetYOffset(), _textBlock.DesiredSize.Width + 50, _textBlock.DesiredSize.Height * 1.3));
        }

        private double GetXOffset() {
            return 0;
        }

        private double GetYOffset() {
            return -(5);
        }

        public event RoutedEventHandler TextBoxLostFocus {
            add { _textBox.LostFocus += value; }
            remove { _textBox.LostFocus -= value; }
        }

        public event KeyEventHandler TextBoxKeyUp {
            add { _textBox.KeyUp += value; }
            remove { _textBox.KeyUp -= value; }
        }
    }
}
