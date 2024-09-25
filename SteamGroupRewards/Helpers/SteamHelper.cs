using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace RestoreMonarchy.SteamGroupRewards.Helpers
{
    internal static class SteamHelper
    {
        private const string BaseUrl = "https://steamcommunity.com/groups/{0}/memberslistxml/?xml=1&p={1}";

        internal static ulong[] GetAllGroupMembers(string groupName)
        {
            var allMembers = new List<ulong>();
            int currentPage = 1;
            bool hasNextPage = true;

            while (hasNextPage)
            {
                string url = string.Format(BaseUrl, groupName, currentPage);
                string xmlContent = GetXmlContent(url);

                XDocument doc = XDocument.Parse(xmlContent);
                XElement memberList = doc.Root;

                if (memberList == null)
                {
                    throw new Exception("Failed to parse XML content");
                }

                foreach (XElement member in memberList.Element("members").Elements("steamID64"))
                {
                    if (ulong.TryParse(member.Value, out ulong steamId))
                    {
                        allMembers.Add(steamId);
                    }
                }

                XElement totalPagesElement = memberList.Element("totalPages");
                XElement currentPageElement = memberList.Element("currentPage");

                if (totalPagesElement != null && currentPageElement != null)
                {
                    int totalPages = int.Parse(totalPagesElement.Value);
                    currentPage = int.Parse(currentPageElement.Value);
                    hasNextPage = currentPage < totalPages;
                    currentPage++;
                }
                else
                {
                    hasNextPage = false;
                }
            }

            return allMembers.ToArray();
        }

        private static string GetXmlContent(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
