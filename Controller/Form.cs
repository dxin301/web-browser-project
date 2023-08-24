using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace student_buchkod;

/// <summary>Handles form tag</summary>
public class Form {
    /// <summary>URL to which submission request must be sent</summary>
    Uri action;
    string enctype = "application/x-www-form-urlencoded";
    HttpMethod httpMethod = HttpMethod.Get;
    /// <summary>Frame which contains this Form and all its inputs</summary>
    Frame initiator;
    /// <summary>Request which initiated creation of this Form</summary>
    Request initialRequest;

    Dictionary<Control, (string?,string)> buttons = new Dictionary<Control, (string?, string)>();
    List<Input> inputs = new List<Input>();
    public Form (Uri action, Frame initiator, Request initialRequest, string? enctype = null,
    HttpMethod? httpMethod = null) {
        this.action = action;
        this.initiator = initiator;
        this.initialRequest = initialRequest;
        if (enctype!=null) this.enctype = enctype;
        if (httpMethod!=null) this.httpMethod = httpMethod;
    }

    /// <summary>
    /// Submit this Form
    /// </summary>
    /// <param name="f">Object which called this class (primarly submit button)</param>
    /// <param name="s">Click event and its information, not used</param>
    public void submit(Object? f = null, RoutedEventArgs? s = null) {
        Dictionary<string,string> data = new Dictionary<string, string>();
        // handle button, add it to input
        if (f is Button && buttons[(Control)f].Item1!=null) {
            var buttonData = buttons[(Control)f];
            data[buttonData.Item1] = buttonData.Item2;
        }
        // add input values
        foreach (Input input in inputs) {
            if (input.control is TextBox) {
                TextBox inputText = (TextBox)input.control;
                if (inputText.Text==null) inputText.Text = "";
                if ((input.required && inputText.Text=="") ||
                !Regex.IsMatch(inputText.Text, $"^{input.regex}$")) {
                    inputText.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                if (input.name!=null) {
                    data[input.name] = inputText.Text;
                }
                inputText.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else if (input.control is CheckBox) {
                CheckBox inputCheckBox = (CheckBox)input.control;
                if (input.required && inputCheckBox.IsChecked!=true) {
                    inputCheckBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                if (input.name!=null && inputCheckBox.IsChecked==true) {
                    data[input.name] = input.value;
                }
                inputCheckBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else if (input.control is ComboBox) {
                ComboBox inputComboBox = (ComboBox)input.control;
                if (input.required && inputComboBox.SelectedItem==null) {
                    inputComboBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
                if (input.name!=null) {
                    if (inputComboBox.SelectedItem==null) data[input.name] = "";
                    else data[input.name] = (string) inputComboBox.SelectedItem;
                }
                inputComboBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else if (input.control is NumericUpDown) {
                NumericUpDown numericUpDown = (NumericUpDown) input.control;
                // always contains value, so no check for required
                if (input.name!=null) {
                    data[input.name] = numericUpDown.Value.ToString();
                }
            }
            else if (input.control==null) {
                if (input.name!=null) {
                    data[input.name] = input.value;
                }
            }
            else throw new Exception("Unknown input field control");
        }
        
        // manage http content
        HttpContent httpContent;
        Request request;
        RefPolicy? refPolicy =  initialRequest.refPolicyOutHeader;
        if (refPolicy==null) refPolicy = RefPolicy.strict_origin_when_cross_origin;
        // GET
        if (httpMethod==HttpMethod.Get) {
            string content = "";
            foreach(var key in data.Keys) {
                content += "&" + HttpUtility.UrlEncode(key) + "="
                + HttpUtility.UrlEncode(data[key]);
            }
            if (content.Length>0) content = content.Substring(1);
            content = "?" + content;
            request = new Request(new Uri(action, content), initialRequest.url, HttpMethod.Get,
            refPolicy: (RefPolicy)refPolicy);
            _ = initiator.DisplayURL(request);
            return;
        }
        // NOT GET
        else if (enctype=="text/plain") {
            string content = "";
            foreach(var key in data.Keys) {
                content += "&" + key + "=" + data[key];
            }
            if (content.Length>0) content = content.Substring(1);
            httpContent = new StringContent(content);
        }
        else if (enctype=="multipart/form-data") {
            httpContent = new MultipartFormDataContent();
            foreach(var key in data.Keys) {
                ((MultipartFormDataContent)httpContent)
                .Add(new StringContent(data[key]), key);
            }
        }
        else httpContent = new FormUrlEncodedContent(data);
        
        request = new Request(action, initialRequest.url, httpMethod,
        httpContent, enctype, refPolicy: (RefPolicy)refPolicy);
        _ = initiator.DisplayURL(request);
    }
    /// <summary>Assign input which is part of current Form</summary>
    /// <remarks>Not for buttons even not for: <code>input type="submit"</code></remarks>
    public void addInput(string? name = null, Control? control = null, Object? defValue = null,
    string regex = "(.*)", bool required = false, string value = "") {
        Input newInput = new Input{ control = control, name = name,
        defValue = defValue, regex = regex, required = required, value = value};
        inputs.Add(newInput);
    }
    /// <summary>Assign button which is part of current Form</summary>
    public void addButton(Button control, string? name, string value = "") {
        buttons[control] = (name,value);
        control.Click += this.submit;
    }
    /// <summary>
    /// Set all input values to their initial
    /// </summary>
    /// <param name="f">Not needed, designed to support Avalonia calls</param>
    /// <param name="s">Not needed, designed to support Avalonia calls</param>
    public void resetForm(object? f = null, RoutedEventArgs? s = null) {
        foreach(Input input in inputs) {
            Control el = input.control;
            if (el is TextBox) {
                ((TextBox)el).Text = (string)input.defValue;
            }
            else if (el is CheckBox) {
                ((CheckBox)el).IsChecked = (bool)input.defValue;
            }
            else if (el is ComboBox) {
                ((ComboBox)el).SelectedIndex = (int)input.defValue;
            }
            else if (el is NumericUpDown) {
                ((NumericUpDown)el).Value = (double)input.defValue;
            }
        }
    }
}

public class Input {
    public Control? control;
    public string? name;
    public Object? defValue;
    public string value = "";
    public string regex = "(.*)";
    public bool required = false;
}
