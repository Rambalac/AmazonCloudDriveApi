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
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}/nodes/{parentid}/children/{nodeid}";
            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.CreateFolder(string parentId, string name)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes";
            var folder = new NewChild { name = name, parents = new[] { parentId }, kind = "FOLDER" };
            return await http.Post<NewChild, AmazonNode>(url, folder).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetChild(string parentid, string name)
        {
            if (parentid == null)
            {
                parentid = (await GetRoot().ConfigureAwait(false)).id;
            }

            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes?filters={MakeParentFilter(parentid)} AND {MakeNameFilter(name)}";
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

            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var baseurl = $"{meta}nodes/{id}/children";

            return await GetAllNodes(baseurl, n => n.parents.Contains(id)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNode(string id)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes/{id}";
            var result = await http.GetJsonAsync<AmazonNode>(url).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNodeByMD5(string md5)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes?filters={MakeMD5Filter(md5)}";
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            return result.count == 0 ? null : result.data[0];
        }

        /// <inheritdoc/>
        async Task<IList<AmazonNode>> IAmazonNodes.GetNodesByMD5(string md5)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var baseurl = $"{meta}nodes?filters={MakeMD5Filter(md5)}";

            return await GetAllNodes(baseurl, t => true).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetNodeExtended(string id)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes/{id}?asset=ALL&tempLink=true";
            var result = await http.GetJsonAsync<AmazonNode>(url).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.GetRoot()
        {
            return await GetRoot().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.Move(string id, string oldDirId, string newDirId)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes/{newDirId}/children";
            var data = new
            {
                fromParent = oldDirId,
                childId = id
            };
            return await http.Post<object, AmazonNode>(url, data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonNodes.Remove(string parentid, string nodeid)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}/nodes/{parentid}/children/{nodeid}";
            await http.Send<object>(HttpMethod.Delete, url).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<AmazonNode> IAmazonNodes.Rename(string id, string newName)
        {
            var data = new
            {
                name = newName
            };
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes/{id}";
            return await http.Patch<object, AmazonNode>(url, data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task IAmazonNodes.Trash(string id)
        {
            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}trash/{id}";

            await http.Send<object>(HttpMethod.Put, url).ConfigureAwait(false);
        }

        private static string MakeMD5Filter(string md5) => "contentProperties.md5:" + md5.ToLowerInvariant();

        private static string MakeNameFilter(string name) => "name:" + Uri.EscapeDataString(FilterEscapeChars.Replace(name, "\\$0")).Replace("%5C", @"\");

        private static string MakeParentFilter(string id) => "parents:" + id;

        private async Task<AmazonNode> GetRoot()
        {
            if (root != null)
            {
                return root;
            }

            var meta = await GetMetadataUrl().ConfigureAwait(false);
            var url = $"{meta}nodes?filters=isRoot:true";
            var result = await http.GetJsonAsync<Children>(url).ConfigureAwait(false);
            if (result.count == 0)
            {
                throw new InvalidOperationException("Could not retrieve root");
            }

            if (result.count > 1)
            {
                throw new InvalidOperationException("Multiple roots?");
            }

            root = result.data[0];
            if (root == null)
            {
                throw new InvalidOperationException("Root is null");
            }

            return root;
        }

        private async Task<IList<AmazonNode>> GetAllNodes(string baseurl, Func<AmazonNode, bool> filter)
        {
            var result = new List<AmazonNode>();
            string nextToken = null;
            do
            {
                var url = string.IsNullOrWhiteSpace(nextToken) ? baseurl : baseurl + "?startToken=" + nextToken;
                try
                {
                    var nodes = await http.GetJsonAsync<Children>(url);
                    result.AddRange(nodes.data.Where(filter));
                    nextToken = nodes.nextToken;
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
    }
}