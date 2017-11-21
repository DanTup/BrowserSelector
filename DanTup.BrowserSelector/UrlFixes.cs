using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanTup.BrowserSelector
{
    public static class UrlFixes
    {
        public static string AddMissedSlashesAfterProtocol(string url)
        {
            var protocolEndPos = url.IndexOf(':');
            if (protocolEndPos == -1)
            {
                throw new ArgumentException($"Not found a colon ':' after protocol in url: " + url);
            }
            if (protocolEndPos == url.Length)
            {
                //The passed url contains only a protocol. Just add slashes and return the result
                return url + "//";
                //throw new ArgumentException($"The passed url contains only a protocol: " + url);
            }
            var protocolLength = protocolEndPos + 1;
            var urlAfterProtocol = url.Substring(protocolLength);
            string slashesToAdd;
            if (urlAfterProtocol[0] == '/')
            {
                if (urlAfterProtocol.Length == 1)
                {
                    //The passed url contains only a protocol with one slash. Just add one more slash and return the result
                    return url + '/';
                    //throw new ArgumentException($"The passed url contains only a protocol: " + url);
                }
                if (urlAfterProtocol[1] == '/')
                {
                    //all slashes are present. The passed url is valid so return it as is
                    return url;
                }
                else
                {
                    slashesToAdd = "/";
                }
            }
            else
            {
                slashesToAdd = "//";
            }
            var fixedUrl = url.Substring(0, protocolLength) + slashesToAdd + urlAfterProtocol;
            return fixedUrl;
        }
    }
}
