using System;

using Foundation;
using UIKit;

namespace App
{
    public partial class WebViewController : UIViewController
    {
        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        protected WebViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Intercept URL loading to handle native calls from browser
            WebView.ShouldStartLoad += HandleShouldStartLoad;

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1 { Text = "Text goes here" };
            var template = new RazorView { Model = model };
            var page = template.GenerateString();

            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Resources folder
            WebView.LoadHtmlString(page, NSBundle.MainBundle.BundleUrl);

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        bool HandleShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            // If the URL is not our own custom scheme, just let the webView load the URL as usual
            const string scheme = "hybrid:";

            if (request.Url.Scheme != scheme.Replace(":", ""))
                return true;

            // This handler will treat everything between the protocol and "?"
            // as the method name.  The querystring has all of the parameters.
            var resources = request.Url.ResourceSpecifier.Split('?');
            var method = resources[0];
            var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

            if (method == "UpdateLabel")
            {
                var textbox = parameters["textbox"];

                // Add some text to our string here so that we know something
                // happened on the native part of the round trip.
                var prepended = string.Format("C# says: {0}", textbox);

                // Build some javascript using the C#-modified result
                var js = string.Format("SetLabelText('{0}');", prepended);

                webView.EvaluateJavascript(js);
            }

            return false;
        }
    }
}

