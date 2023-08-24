using System;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace student_buchkod.Views;

public partial class MainWindow : Window
{
    TextBox _textBox;
    InlinePanel siteWindow;
    Frame frame;
    
    public MainWindow()
    {
        InitializeComponent();
        _textBox = this.FindControl<TextBox>("inputURL");
        _textBox.KeyUp += RequestURL;
        siteWindow = this.FindControl<InlinePanel>("siteContent");

        // forbid change window size
        MaxWidth = 1200;
        MaxHeight = 800;
        MinWidth = 1200;
        MinHeight = 800;

        frame = new Frame(siteWindow, _textBox, setTitle, true);
        Home();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Resets History and displays home page at browser://home/
    /// </summary>
    void Home(object? sender = null, RoutedEventArgs? e = null) {
        History.Reset();
        siteWindow.Clear();
        Update();
    }
    /// <summary>
    /// Redirects to previously visited page
    /// </summary>
    void Back(object? sender = null, RoutedEventArgs? e = null) {
        _ = frame.DisplayURL(History.Pop());
    }
    /// <summary>
    /// Reloads last request with updated cookies
    /// </summary>
    void Update(object? sender = null, RoutedEventArgs? e = null) {
        _ = frame.DisplayURL(History.current);
    }
    /// <summary>
    /// Gets requested URL from <c>_textBox</c>, creates GET <c>Request r</c>,
    /// displays <c>Request r</c> in the current frame
    /// </summary>
    void RequestURL(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) {
            if (_textBox.Text=="" || _textBox.Text==null) {
                _textBox.Text = "browser://home";
            }
            else if (!_textBox.Text.Contains("://")) {
                _textBox.Text = "https://" + _textBox.Text;
            }
            Uri uri = new Uri(_textBox.Text);
            if(uri.ToString()==History.current.url.ToString()) {
                History.Pop();
            }
            Request r;
            try {
                r = new Request(uri,
                    new Uri("browser://home"), HttpMethod.Get);
                History.Push(r);
            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
                r = new Request(new Uri("browser://error/"),
                    new Uri("browser://home"), HttpMethod.Get);
            }
            _ = frame.DisplayURL(r);
        }
    }
    /// <summary>
    /// Sets current site page title, displays as MainWindow title
    /// </summary>
    public void setTitle(string title) {
        Title = title;
    }
}