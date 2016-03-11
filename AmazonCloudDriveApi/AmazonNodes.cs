// <copyright file="AmazonNodes.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;
using Azi.Tools;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Part to work with files tree nodes
    /// </summary>
    public partial class AmazonDrive
    {
        private static readonly Regex FilterEscapeChars = new Regex("[ \\+\\-&|!(){}[\\]^'\"~\\*\\?:\\\\]");
        private AmazonNode root;

        /// <inheritdoc/>
        async Task IAmazonNodes.Add(string parentid, string nodeid)
        {
            var url = string.Format("{0}/nodes/{1}/children/{2}", await GetMetadataUrl().ConfigureAwait(false), parentid, nodeid);
            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests to add nodes to parent. Each node can have multiple parents.
        /// </summary>
        /// <param name="parentid">Parent node id.</param>
        /// <param name="nodeids">Nodes id to add.</param>
        /// <returns>Operation Task</returns>
        public async Task Add(string parentid, IEnumerable<string> nodeids)
        {
            var url = string.Format("{0}/nodes/{1}/children", await GetMetadataUrl().ConfigureAwait(false), parentid);
            var op = new AmazonBulkOperation
            {
                op = "add",
                value = nodeids.ToList()
            };

            await http.Send<AmazonBulkOperation, AmazonSharedCollection>(new HttpMethod("PATCH"), url, op).ConfigureAwait(false);
        }

        /// <summary>
        /// Create Shared collection
        /// </summary>
        /// <param name="name">Collection name</param>
        /// <returns></returns>
        public async Task<AmazonSharedCollection> CreateSharedCollection(string name)
        {
            var url = string.Format("{0}nodes", await GetMetadataUrl().ConfigureAwait(false));
            var folder = new NewSharedCollection { name = name, kind = "SHARED_COLLECTION" };
            return await http.Post<NewSharedCollection, AmazonSharedCollection>(url, folder).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.CreateFolder(string parentId, string name)
        {
            var url = string.Format("{0}nodes", await GetMetadataUrl().ConfigureAwait(false));
            var folder = new NewChild { name = name, parents = new string[] { parentId }, kind = "FOLDER" };
            return await http.Post<NewChild, AmazonNode>(url, folder).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetChild(string parentid, string name)
        {
            if (parentid == null)
            {
                parentid = (await GetRoot().ConfigureAwait(false)).id;
            }

            var url = string.Format("{0}nodes?filters={1} AND {2}", await GetMetadataUrl().ConfigureAwait(false), MakeParentFilter(parentid), MakeNameFilter(name));
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            if (result.count == 0)
            {
                return null;
            }

            if (result.count != 1)
            {
                throw new InvalidOperationException("Duplicated node name");
            }

            if (!result.data[0].parents.Contains(parentid))
            {
                return null; // Hack for wrong Amazon output when file location was changed recently
            }

            return result.data[0];
        }

        /// <inheritdoc/>
        async Task<IList<AmazonNode>> IAmazonNodes.GetChildren(string id)
        {
            if (id == null)
            {
                id = (await GetRoot().ConfigureAwait(false)).id;
            }

            var baseurl = string.Format("{0}nodes/{1}/children", await GetMetadataUrl().ConfigureAwait(false), id);
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
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        break;
                    }

                    throw;
                }
            }
            while (!string.IsNullOrWhiteSpace(nextToken));

            return result;
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNode(string id)
        {
            var url = "{0}nodes/{1}";
            var result = await http.GetJsonAsync<AmazonNode>(string.Format(url, await GetMetadataUrl().ConfigureAwait(false), id)).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNodeByMD5(string md5)
        {
            var url = string.Format("{0}nodes?filters={1}", await GetMetadataUrl().ConfigureAwait(false), MakeMD5Filter(md5));
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            if (result.count == 0)
            {
                return null;
            }

            return result.data[0];
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNodeExtended(string id)
        {
            var url = "{0}nodes/{1}?asset=ALL&tempLink=true";
            var result = await http.GetJsonAsync<AmazonNode>(string.Format(url, await GetMetadataUrl().ConfigureAwait(false), id)).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetRoot()
        {
            return await GetRoot();
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.Move(string id, string oldDirId, string newDirId)
        {
            var url = "{0}nodes/{1}/children";
            var data = new
            {
                fromParent = oldDirId,
                childId = id
            };
            return await http.Post<object, AmazonNode>(string.Format(url, await GetMetadataUrl().ConfigureAwait(false), newDirId), data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonNodes.Remove(string parentid, string nodeid)
        {
            var url = string.Format("{0}/nodes/{1}/children/{2}", await GetMetadataUrl().ConfigureAwait(false), parentid, nodeid);
            await http.Send<object>(HttpMethod.Delete, url).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.Rename(string id, string newName)
        {
            var url = "{0}nodes/{1}";
            var data = new
            {
                name = newName
            };
            return await http.Patch<object, AmazonNode>(string.Format(url, await GetMetadataUrl().ConfigureAwait(false), id), data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonNodes.Trash(string id)
        {
            var url = string.Format("{0}trash/{1}", await GetMetadataUrl().ConfigureAwait(false), id);

            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        private async Task<AmazonNode> GetRoot()
        {
            if (root != null)
            {
                return root;
            }

            var url = "{0}nodes?filters=isRoot:true";
            var result = await http.GetJsonAsync<Children>(string.Format(url, await GetMetadataUrl().ConfigureAwait(false))).ConfigureAwait(false);
            if (result.count == 0)
            {
                return null;
            }

            root = result.data[0];
            if (root == null)
            {
                throw new InvalidOperationException("Could not retrieve root");
            }

            return root;
        }

        private static string MakeMD5Filter(string md5)
        {
            return "contentProperties.md5:" + md5;
        }

        private static string MakeNameFilter(string name)
        {
            return "name:" + FilterEscapeChars.Replace(name, "\\$0");
        }

        private static string MakeParentFilter(string id)
        {
            return "parents:" + id;
        }
    }
}