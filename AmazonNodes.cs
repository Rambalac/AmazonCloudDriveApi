using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpClient = Azi.Tools.HttpClient;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Files and folders manipulating part of API
    /// </summary>
    public class AmazonNodes
    {
        private readonly AmazonDrive amazon;
        private HttpClient http => amazon.http;
        private readonly static Regex filterEscapeChars = new Regex("[ \\+\\-&|!(){}[\\]^'\"~\\*\\?:\\\\]");
        private AmazonNode root;


        internal AmazonNodes(AmazonDrive amazonDrive)
        {
            this.amazon = amazonDrive;
        }


        /// <summary>
        /// Requests for node information by its Id
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node info or null</returns>
        public async Task<AmazonNode> GetNode(string id)
        {
            var url = "{0}nodes/{1}";
            var result = await http.GetJsonAsync<AmazonNode>(string.Format(url, await amazon.GetMetadataUrl().ConfigureAwait(false), id)).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Requests for all nodes info belonging to specified parent
        /// </summary>
        /// <param name="id">Parent node id. If null Root folder is presumed</param>
        /// <returns>List of nodes info</returns>
        public async Task<IList<AmazonNode>> GetChildren(string id = null)
        {
            if (id == null) id = (await GetRoot().ConfigureAwait(false)).id;
            var baseurl = string.Format("{0}nodes/{1}/children", await amazon.GetMetadataUrl().ConfigureAwait(false), id);
            var result = new List<AmazonNode>();
            string nextToken = null;
            do
            {
                var url = string.IsNullOrWhiteSpace(nextToken) ? baseurl : baseurl + "?startToken=" + nextToken;
                try
                {
                    var children = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
                    result.AddRange(children.data.Where(n => n.parents.Contains(id))); // Hack for wrong Amazon output when file location was changed recently
                    nextToken = children.nextToken;
                }
                catch (HttpWebException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound) break;
                    throw;
                }
            } while (!string.IsNullOrWhiteSpace(nextToken));
            return result;
        }

        private string MakeNameFilter(string name)
        {
            return "name:" + filterEscapeChars.Replace(name, "\\$0");
        }

        private string MakeParentFilter(string id)
        {
            return "parents:" + id;
        }

        private string MakeMD5Filter(string md5)
        {
            return "contentProperties.md5:" + md5;
        }

        /// <summary>
        /// Requests for node info with specified name and parent
        /// </summary>
        /// <param name="parentid">Parent node Id to look in</param>
        /// <param name="name">Name of node to look for</param>
        /// <returns>Node info</returns>
        public async Task<AmazonNode> GetChild(string parentid, string name)
        {
            if (parentid == null) parentid = (await GetRoot().ConfigureAwait(false)).id;
            var url = string.Format("{0}nodes?filters={1} AND {2}", await amazon.GetMetadataUrl().ConfigureAwait(false), MakeParentFilter(parentid), MakeNameFilter(name));
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            if (result.count == 0) return null;
            if (result.count != 1) throw new InvalidOperationException("Duplicated node name");

            if (!result.data[0].parents.Contains(parentid)) return null; // Hack for wrong Amazon output when file location was changed recently

            return result.data[0];
        }

        /// <summary>
        /// Requests to add node to parent. Each node can have multiple parents.
        /// </summary>
        /// <param name="parentid">Parent node id.</param>
        /// <param name="nodeid">Node id to add.</param>
        /// <returns>Operation Task</returns>
        public async Task Add(string parentid, string nodeid)
        {
            var url = string.Format("{0}/nodes/{1}/children/{2}", await amazon.GetMetadataUrl().ConfigureAwait(false), parentid, nodeid);
            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Request to remove node from parent. Nodes can have multiple parents. 
        /// if you remove node from all parents it will not be trashed. 
        /// If you remove file node from all parents without trash and decide to upload file with the same MD5 and duplication check then file will be rejected as Conflict.
        /// </summary>
        /// <param name="parentid">Parent node id to remove from.</param>
        /// <param name="nodeid">Node id to remove.</param>
        /// <returns>Operation Task</returns>
        public async Task Remove(string parentid, string nodeid)
        {
            var url = string.Format("{0}/nodes/{1}/children/{2}", await amazon.GetMetadataUrl().ConfigureAwait(false), parentid, nodeid);
            await http.Send<object>(HttpMethod.Delete, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests to move node to trash. Nodes can be in several parents, nodes will become unavailable in all parents.
        /// </summary>
        /// <param name="id">Node id to trash</param>
        /// <returns>Operation Task</returns>
        public async Task Trash(string id)
        {
            var url = string.Format("{0}trash/{1}", await amazon.GetMetadataUrl().ConfigureAwait(false), id);

            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Create Folder node
        /// </summary>
        /// <param name="parentId">Parent node id</param>
        /// <param name="name">Folder name</param>
        /// <returns></returns>
        public async Task<AmazonNode> CreateFolder(string parentId, string name)
        {
            var url = string.Format("{0}nodes", await amazon.GetMetadataUrl().ConfigureAwait(false));
            var folder = new NewChild { name = name, parents = new string[] { parentId }, kind = "FOLDER" };
            return await http.Post<NewChild, AmazonNode>(url, folder).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests for Root folder node info. Cached without expiration.
        /// </summary>
        /// <returns>Root folder node info</returns>
        public async Task<AmazonNode> GetRoot()
        {
            if (root != null) return root;

            var url = "{0}nodes?filters=isRoot:true";
            var result = await http.GetJsonAsync<Children>(string.Format(url, await amazon.GetMetadataUrl().ConfigureAwait(false))).ConfigureAwait(false);
            if (result.count == 0) return null;
            root = result.data[0];
            if (root == null) throw new InvalidOperationException("Could not retrieve root");
            return root;
        }

        /// <summary>
        /// Requests to change name of node, file or folder.
        /// </summary>
        /// <param name="id">Node id to rename</param>
        /// <param name="newName">New name</param>
        /// <returns>Node info with new name</returns>
        public async Task<AmazonNode> Rename(string id, string newName)
        {
            var url = "{0}nodes/{1}";
            var data = new
            {
                name = newName
            };
            return await http.Patch<object, AmazonNode>(string.Format(url, await amazon.GetMetadataUrl().ConfigureAwait(false), id), data).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests to change specified parent id to another. 
        /// Nodes can have multiple parents, method will change only specified one.
        /// </summary>
        /// <param name="id">Node id to move</param>
        /// <param name="oldDirId">Existing parent node id to remove</param>
        /// <param name="newDirId">Another Folder node id to add as parent</param>
        /// <returns></returns>
        public async Task<AmazonNode> Move(string id, string oldDirId, string newDirId)
        {
            var url = "{0}nodes/{1}/children";
            var data = new
            {
                fromParent = oldDirId,
                childId = id
            };
            return await http.Post<object, AmazonNode>(string.Format(url, await amazon.GetMetadataUrl().ConfigureAwait(false), newDirId), data).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests for file node info with specified MD5
        /// </summary>
        /// <param name="md5">MD5 as low case hex without separators</param>
        /// <returns>Found node info or null</returns>
        public async Task<AmazonNode> GetNodeByMD5(string md5)
        {
            var url = string.Format("{0}nodes?filters={1}", await amazon.GetMetadataUrl().ConfigureAwait(false), MakeMD5Filter(md5));
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            if (result.count == 0) return null;
            return result.data[0];
        }
    }
}