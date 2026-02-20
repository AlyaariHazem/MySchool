using Ganss.Xss;

namespace Backend.Services
{
    public class HtmlSanitizationService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitizationService()
        {
            _sanitizer = new HtmlSanitizer();
            
            // Allow common HTML tags used in rich text editors
            _sanitizer.AllowedTags.UnionWith(new[] { 
                "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
                "h1", "h2", "h3", "h4", "h5", "h6",
                "ul", "ol", "li",
                "table", "thead", "tbody", "tr", "th", "td",
                "div", "span", "blockquote",
                "a", "img"
            });

            // Allow common attributes
            _sanitizer.AllowedAttributes.UnionWith(new[] {
                "style", "class", "id",
                "href", "target", "rel",
                "src", "alt", "width", "height",
                "colspan", "rowspan", "align", "valign"
            });

            // Allow safe CSS properties
            _sanitizer.AllowedCssProperties.UnionWith(new[] {
                "color", "background-color", "font-size", "font-weight",
                "font-family", "text-align", "text-decoration",
                "margin", "padding", "border", "width", "height"
            });

            // Allow data URIs for images (base64 encoded images)
            _sanitizer.AllowedSchemes.Add("data");
            
            // Allow http and https for links and images
            _sanitizer.AllowedSchemes.Add("http");
            _sanitizer.AllowedSchemes.Add("https");
        }

        public string Sanitize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            return _sanitizer.Sanitize(html);
        }
    }
}
