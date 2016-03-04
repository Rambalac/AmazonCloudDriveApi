// <copyright file="IAmazonNodes.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Azi.Amazon.CloudDrive.JsonObjects;

namespace Azi.Amazon.CloudDrive
{
    /// <summary>
    /// Files and folders manipulating part of API
    /// </summary>
    public interface IAmazonNodes
    {
        /// <summary>
        /// Requests to add node to parent. Each node can have multiple parents.
        /// </summary>
        /// <param name="parentid">Parent node id.</param>
        /// <param name="nodeid">Node id to add.</param>
        /// <returns>Operation Task</returns>
        Task Add(string parentid, string nodeid);

        /// <summary>
        /// Create Folder node
        /// </summary>
        /// <param name="parentId">Parent node id</param>
        /// <param name="name">Folder name</param>
        /// <returns>New Folder node info</returns>
        Task<AmazonNode> CreateFolder(string parentId, string name);

        /// <summary>
        /// Requests for node info with specified name and parent
        /// </summary>
        /// <param name="parentid">Parent node Id to look in</param>
        /// <param name="name">Name of node to look for</param>
        /// <returns>Node info</returns>
        Task<AmazonNode> GetChild(string parentid, string name);

        /// <summary>
        /// Requests for all nodes info belonging to specified parent
        /// </summary>
        /// <param name="id">Parent node id. If null Root folder is presumed</param>
        /// <returns>List of nodes info</returns>
        Task<IList<AmazonNode>> GetChildren(string id = null);

        /// <summary>
        /// Requests for node information by its Id
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node info or null</returns>
        Task<AmazonNode> GetNode(string id);

        /// <summary>
        /// Requests for file node info with specified MD5
        /// </summary>
        /// <param name="md5">MD5 as low case hex without separators</param>
        /// <returns>Found node info or null</returns>
        Task<AmazonNode> GetNodeByMD5(string md5);

        /// <summary>
        /// Requests for node extended information by its Id. Extended info includes temp link and assets.
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node info or null</returns>
        Task<AmazonNode> GetNodeExtended(string id);

        /// <summary>
        /// Requests for Root folder node info. Cached without expiration.
        /// </summary>
        /// <returns>Root folder node info</returns>
        Task<AmazonNode> GetRoot();

        /// <summary>
        /// Requests to change specified parent id to another.
        /// Nodes can have multiple parents, method will change only specified one.
        /// </summary>
        /// <param name="id">Node id to move</param>
        /// <param name="oldDirId">Existing parent node id to remove</param>
        /// <param name="newDirId">Another Folder node id to add as parent</param>
        /// <returns>Moved node info</returns>
        Task<AmazonNode> Move(string id, string oldDirId, string newDirId);

        /// <summary>
        /// Request to remove node from parent. Nodes can have multiple parents.
        /// if you remove node from all parents it will not be trashed.
        /// If you remove file node from all parents without trash and decide to upload file with the same MD5 and duplication check then file will be rejected as Conflict.
        /// </summary>
        /// <param name="parentid">Parent node id to remove from.</param>
        /// <param name="nodeid">Node id to remove.</param>
        /// <returns>Operation Task</returns>
        Task Remove(string parentid, string nodeid);

        /// <summary>
        /// Requests to change name of node, file or folder.
        /// </summary>
        /// <param name="id">Node id to rename</param>
        /// <param name="newName">New name</param>
        /// <returns>Node info with new name</returns>
        Task<AmazonNode> Rename(string id, string newName);

        /// <summary>
        /// Requests to move node to trash. Nodes can be in several parents, nodes will become unavailable in all parents.
        /// </summary>
        /// <param name="id">Node id to trash</param>
        /// <returns>Operation Task</returns>
        Task Trash(string id);
    }
}