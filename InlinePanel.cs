using System;
using Avalonia;
using Avalonia.Controls;

namespace student_buchkod;

// Aim: create a panel in which all elements will act same as in inline html
// StackPanel and WrapPanel can't be used because they simply create blocks as
// separate cells like in the table. They are working pretty good with an infinite width
// but when width setted, separate blocks wrap in their "cells"
// instead of whole construction wrap as a single element
// expected that WBr elements will be procceded by this panel same as wbr in html


/// <summary>
/// StackPanel with Horizontal orientation which wraps blocks to new line if
/// <c>PanelViewWidth</c> can be overflowed or Separator occures
/// </summary>
public class InlinePanel : StackPanel
{
  /// <summary>
  /// <c>PanelViewWidth</c> sets the width for <c>Separator</c> children,
  /// and sets width which should not be overflowed, if overflow can occur block
  /// must be moved to the next line
  /// </summary>
  public double PanelViewWidth = 1200;
  double currentLineHeight = 0;
  double currentLineWidth = 0;
  double previousLength = 0;

  public InlinePanel() {
    // created because Avalonia UI does not compile
    // without explictly created constructor with 0 arguments
  }
  /// <summary>
  /// Initialize <c>InlinePanel</c> and assigns <c>PanelViewWidth</c>
  /// </summary>
  public InlinePanel (double PanelViewWidth) {
    // height is unlimited
    this.PanelViewWidth = PanelViewWidth;
  }
  protected override Size ArrangeOverride(Size finalSize)
  {
    Spacing = 0;
    currentLineHeight = 0;
    currentLineWidth = 0;
    previousLength = 0;
    double width = 0;
    double height = 0;
    Rect rcChild = new Rect();
    foreach (var child in Children)
    {
      if (child is Control control)
      {
        double childWidth = control.DesiredSize.Width;
        double childHeight = control.DesiredSize.Height;

        if (!child.IsVisible) continue;

        if (rcChild.X + previousLength + childWidth > PanelViewWidth ||
        (child is Separator && (rcChild.Y!=0 || rcChild.X!=0)))
        {
          // write to next line
          rcChild = rcChild.WithX(0);
          rcChild = rcChild.WithY(rcChild.Y + currentLineHeight);
          currentLineWidth = 0;
          height += currentLineHeight;
          currentLineHeight = 0;
        }
        else
        {
          // append to current line
          rcChild = rcChild.WithX(rcChild.X + previousLength);
        }
        currentLineWidth += childWidth;
        previousLength = childWidth;
        currentLineHeight = Math.Max(currentLineHeight, childHeight);
        rcChild = rcChild.WithWidth(childWidth);
        rcChild = rcChild.WithHeight(currentLineHeight);

        child.Arrange(rcChild);
        width = Math.Max(width, currentLineWidth);
        currentLineHeight = Math.Max(currentLineHeight, childHeight);
      }
    }
    Width = Math.Max(currentLineWidth, width);
    Height = currentLineHeight + height;
    return finalSize;
  }
  /// <summary>
  /// Deletes all children
  /// </summary>
  public void Clear() {
    Children.Clear();
  }
}