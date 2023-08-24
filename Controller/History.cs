using System;
using System.Net.Http;

namespace student_buchkod;

/// <summary>
/// History class memorizes visit history pages,
/// was designed for "back button"
/// </summary>
public static class History {
    // implemented as a modified linked list
    // such that Pop returns next after popped <c>Request</c>

    private static Node? root = null;
    private static Request home = new Request(new Uri("browser://home/"),
    new Uri("browser://home/"), HttpMethod.Get);

    /// <summary>
    /// Returns the last <c>Request</c> added to History
    /// </summary>
    public static Request current {
        get {
            if(root==null) return home;
            return root.value;
        }
    }

    /// <summary>
    /// Returns penult <c>Request</c> added to History
    /// </summary>
    public static Request previous {
        get {
            if(root==null || root.child==null) return home;
            return root.child.value;
        }
    }

    /// <summary>
    /// Remove the last added <c>Request</c> and return <c>Request</c> before the
    /// removed one
    /// </summary>
    public static Request Pop() {
        // when we pop we delete current page
        // and return value of new current page
        if (root==null) return home;
        if (root.child==null) {
            // return to home page
            root = null;
            return home;
        }
        root = root.child;
        return root.value;
    }

    /// <summary>
    /// Adds new <c>Request</c> to the History
    /// </summary>
    public static void Push(Request s) {
        root = new Node(s, root);
    }

    /// <summary>
    /// Deletes all elements from History
    /// </summary>
    public static void Reset() {
        root = null;
    }
    class Node {
        public Request value;
        public Node? child;
        public Node(Request value, Node? child = null) {
            this.value = value;
            this.child = child;
        }
    }
}