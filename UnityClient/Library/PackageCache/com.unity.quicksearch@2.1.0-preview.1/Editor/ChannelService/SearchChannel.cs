#if QUICKSEARCH_SEARCH_CHANNEL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.QuickSearch
{
    enum SearchRequestType
    {
        Query = 0,
        FetchInfo = 1
    }

    [Flags]
    enum FetchOptions
    {
        None = 0,
        Label = 1,
        Description = 1 << 1
    }

    enum ThumbnailCompression
    {
        JPEG = 0,
        PNG = 1
    }

    struct ThumbnailOptions
    {
        public ThumbnailCompression compression;
        public int quality;
        public int width;
        public int height;
    }

    class ClientSearchRequest
    {
        public long requestId;
        public SearchContext context;
        public Dictionary<string, SearchItem> temporaryCache;
        public int maxCount;
        public int currentCount;
    }

    class SearchChannel
    {
        int m_ChannelId;
        Action m_DisconnectChannel;

        public string channelName { get; }

        public SearchChannel(string name, Action<int, byte[]> messageHandler)
        {
            channelName = name;
            m_DisconnectChannel = ChannelService.GetOrCreateChannel(channelName, messageHandler);
            m_ChannelId = ChannelService.ChannelNameToId(name);
        }

        ~SearchChannel()
        {
            m_DisconnectChannel?.Invoke();
        }
    }

    static class SearchChannelsHandler
    {
        static SearchChannel s_SearchChannel;
        static SearchChannel s_ThumbnailChannel;
        static Dictionary<int, ClientSearchRequest> s_SearchRequestsById;

        [InitializeOnLoadMethod]
        static void RegisterChannelService()
        {
            if (!ChannelService.IsRunning())
                ChannelService.Start();

            Console.WriteLine($"ChannelService Running: {ChannelService.GetAddress()}:{ChannelService.GetPort()}");

            // Create search view context
            s_SearchRequestsById = new Dictionary<int, ClientSearchRequest>();

            s_SearchChannel = new SearchChannel("search", HandleChannelMessage);
            s_ThumbnailChannel = new SearchChannel("search_thumbnail", HandleThumbnailChannelMessage);

            Utils.DelayCall(1.0f, CleanupConnections);
        }

        static void CleanupConnections()
        {
            if (s_SearchChannel == null)
                return;

            var connectionIds = GetConnectionIds(s_SearchChannel.channelName);
            var idsToDelete = new List<int>();
            foreach (var kvp in s_SearchRequestsById)
            {
                var connectionId = kvp.Key;
                if (!connectionIds.Contains(connectionId))
                    idsToDelete.Add(connectionId);
            }

            foreach (var connectionId in idsToDelete)
            {
                ClearClientSearchRequest(connectionId);
            }

            Utils.DelayCall(1.0f, CleanupConnections);
        }

        static void ClearClientSearchRequest(int connectionId)
        {
            if (s_SearchRequestsById.TryGetValue(connectionId, out var clientRequest))
            {
                clientRequest.context.Dispose();
                clientRequest.temporaryCache = null;
                s_SearchRequestsById.Remove(connectionId);
            }
        }

        static void HandleChannelMessage(int connectionId, byte[] data)
        {
            var msg = Encoding.UTF8.GetString(data);
            var queryData = Utils.JsonDeserialize(msg) as Dictionary<string, object>;
            if (queryData == null)
                return;

            var searchRequestType = (SearchRequestType)Enum.Parse(typeof(SearchRequestType), queryData["requestType"].ToString());

            switch (searchRequestType)
            {
                case SearchRequestType.Query: HandleQueryRequests(connectionId, queryData);
                    break;
                case SearchRequestType.FetchInfo: HandleFetchInfoRequests(connectionId, queryData);
                    break;
                default: Debug.LogWarning($"Unknown SearchChannel request type: {searchRequestType}.");
                    break;
            }
        }

        static void HandleThumbnailChannelMessage(int connectionId, byte[] data)
        {
            var msg = Encoding.UTF8.GetString(data);
            var fetchData = Utils.JsonDeserialize(msg) as Dictionary<string, object>;
            var requestId = (long)fetchData["requestId"];
            var searchConnectionId = (long)fetchData["connectionId"];
            var itemIds = fetchData["itemIds"] as List<object>;
            var width = (long)fetchData["width"];
            var height = (long)fetchData["height"];
            var quality = (long)fetchData["quality"];
            var compression = (ThumbnailCompression)Enum.Parse(typeof(ThumbnailCompression), fetchData["compression"].ToString());

            if (width == 0 || height == 0)
            {
                Debug.LogWarning("Cannot request a thumbnail with size 0.");
                return;
            }

            if (!s_SearchRequestsById.TryGetValue((int)searchConnectionId, out var request))
                return;

            if (requestId != request.requestId)
                return;

            var cachedSearchItems = itemIds.Cast<string>().Where(id => request.temporaryCache.ContainsKey(id)).Select(id => request.temporaryCache[id]);
            SendThumbnailResults(connectionId, request, cachedSearchItems, new ThumbnailOptions {width = (int)width, height = (int)height, quality = (int)quality, compression = compression});
        }

        static void HandleQueryRequests(int connectionId, Dictionary<string, object> queryData)
        {
            var requestId = (long)queryData["requestId"];
            var query = queryData["query"] as string;
            var fetchOptions = (FetchOptions)Enum.Parse(typeof(FetchOptions), queryData["fetchOptions"].ToString());
            var maxCount = (long)queryData["maxCount"];

            if (!s_SearchRequestsById.TryGetValue(connectionId, out var request))
            {
                request = new ClientSearchRequest
                {
                    // Create a new search context
                    context = new SearchContext(SearchService.Providers.Where(p => p.active), "")
                };
                request.context.asyncItemReceived += (context, items) =>
                {
                    var itemsList = items.ToList();
                    if (request.maxCount > -1)
                    {
                        var remainingItems = request.maxCount - request.currentCount;
                        if (remainingItems < 1)
                            return;
                        itemsList = itemsList.Take(remainingItems).ToList();
                    }
                    request.currentCount += itemsList.Count;
                    CacheResults(request, itemsList);
                    SendResults(connectionId, request, SearchRequestType.Query, fetchOptions, itemsList);
                };
                s_SearchRequestsById.Add(connectionId, request);
            }

            request.context.sessions.StopAllAsyncSearchSessions();
            request.context.searchText = query;
            request.requestId = requestId;
            request.maxCount = (int)maxCount;
            var initialResults = SearchService.GetItems(request.context);
            if (request.maxCount > -1)
                initialResults = initialResults.Take(request.maxCount).ToList();
            request.currentCount = initialResults.Count;
            request.temporaryCache = new Dictionary<string, SearchItem>();
            CacheResults(request, initialResults);
            SendResults(connectionId, request, SearchRequestType.Query, fetchOptions, initialResults);
        }

        static void HandleFetchInfoRequests(int connectionId, Dictionary<string, object> queryData)
        {
            if (!s_SearchRequestsById.TryGetValue(connectionId, out var request))
                return;

            var requestId = (long)queryData["requestId"];
            if (requestId != request.requestId)
                return;

            var fetchOptions = (FetchOptions)Enum.Parse(typeof(FetchOptions), queryData["fetchOptions"].ToString());
            var itemIds = queryData["itemIds"] as List<object>;

            var cachedSearchItems = itemIds.Cast<string>().Where(id => request.temporaryCache.ContainsKey(id)).Select(id => request.temporaryCache[id]);
            SendResults(connectionId, request, SearchRequestType.FetchInfo, fetchOptions, cachedSearchItems);
        }

        static void SendResults(int connectionId, ClientSearchRequest request, SearchRequestType requestType, FetchOptions fetchOptions, IEnumerable<SearchItem> items)
        {
            var json = SearchItemsToJson(items, request, requestType, fetchOptions);
            ChannelService.Send(connectionId, json);
        }

        static void SendThumbnailResults(int connectionId, ClientSearchRequest request, IEnumerable<SearchItem> items, ThumbnailOptions options)
        {
            var allItemsData = new List<byte>();
            AppendInt(allItemsData, (int)request.requestId);
            AppendInt(allItemsData, (int)options.compression);
            foreach (var searchItem in items)
            {
                var thumbnail = FetchThumbnail(searchItem, options, request.context);
                if (thumbnail == null)
                    continue;
                EncodeItemThumbnail(searchItem, thumbnail, options, allItemsData);
            }

            ChannelService.Send(connectionId, allItemsData.ToArray());
        }

        static string SearchItemsToJson(IEnumerable<SearchItem> items, ClientSearchRequest request, SearchRequestType requestType, FetchOptions fetchOptions)
        {
            var jsonData = new Dictionary<string, object>
            {
                ["requestId"] = request.requestId,
                ["items"] = items.Select(i => SearchItemToDictionary(i, request, fetchOptions)).ToArray(),
                ["requestType"] = (int)requestType
            };
            return Utils.JsonSerialize(jsonData);
        }

        static Dictionary<string, object> SearchItemToDictionary(SearchItem item, ClientSearchRequest request, FetchOptions fetchOptions)
        {
            var data = new Dictionary<string, object>
            {
                ["id"] = item.id,
                ["score"] = item.score,
                ["providerPriority"] = item.provider.priority
            };

            FetchAdditionalItemInfo(item, data, fetchOptions, request.context);

            return data;
        }

        static void FetchAdditionalItemInfo(SearchItem item, Dictionary<string, object> data, FetchOptions fetchOptions, SearchContext context)
        {
            if ((fetchOptions & FetchOptions.Label) == FetchOptions.Label)
            {
                data["label"] = item.GetLabel(context);
            }

            if ((fetchOptions & FetchOptions.Description) == FetchOptions.Description)
            {
                data["description"] = item.GetDescription(context);
            }
        }

        static void CacheResults(ClientSearchRequest request, IEnumerable<SearchItem> items)
        {
            foreach (var searchItem in items)
            {
                request.temporaryCache[searchItem.id] = searchItem;
            }
        }

        static Texture2D FetchThumbnail(SearchItem item, ThumbnailOptions options, SearchContext context)
        {
            Texture2D thumbnail = null;
            var requestedWidth = options.width;
            var requestedHeight = options.height;
            var shouldFetchPreview = SearchSettings.fetchPreview;
            if (shouldFetchPreview)
            {
                thumbnail = item.preview;
                shouldFetchPreview = !thumbnail && item.provider.fetchPreview != null;
                if (shouldFetchPreview)
                {
                    thumbnail = item.provider.fetchPreview(item, context, new Vector2(requestedWidth, requestedHeight), FetchPreviewOptions.Preview2D | FetchPreviewOptions.Normal);
                    if (thumbnail)
                    {
                        item.preview = thumbnail;
                    }
                }
            }

            if (!thumbnail)
            {
                thumbnail = item.thumbnail;
                if (!thumbnail && item.provider.fetchThumbnail != null)
                {
                    thumbnail = item.provider.fetchThumbnail(item, context);
                    if (thumbnail && !shouldFetchPreview)
                        item.thumbnail = thumbnail;
                }
            }

            if (thumbnail == null)
                return null;

            if (options.compression == ThumbnailCompression.JPEG && HasAlphaTextureFormat(thumbnail))
            {
                Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, $"Thumbnail for {item.label} has alpha channel. Please use PNG instead.");
            }

            return CopyTextureReadable(thumbnail, options);
        }

        static void EncodeItemThumbnail(SearchItem item, Texture2D thumbnail, ThumbnailOptions options, List<byte> outputData)
        {
            var idData = Encoding.UTF8.GetBytes(item.id);
            var idLength = idData.Length;

            byte[] encodedThumbnail = null;
            if (options.compression == ThumbnailCompression.JPEG)
                encodedThumbnail = thumbnail.EncodeToJPG(options.quality);
            else if (options.compression == ThumbnailCompression.PNG)
                encodedThumbnail = thumbnail.EncodeToPNG();

            // The datablock of an item is:
            // offset        size      data
            // ---------------------------------------------------------
            //   0            4        Length of id buffer
            //   4            N        Id buffer
            //  4+N           4        Length of thumbnail buffer
            //  8+N           4        Actual width of thumbnail
            // 12+N           4        Actual height of thumbnail
            // 16+N           M        Thumbnail buffer

            // Append Id info
            AppendInt(outputData, idLength);
            outputData.AddRange(idData);

            // Append thumbnail info
            AppendInt(outputData, encodedThumbnail.Length);
            AppendInt(outputData, thumbnail.width);
            AppendInt(outputData, thumbnail.height);
            outputData.AddRange(encodedThumbnail);
        }

        static void AppendInt(List<byte> buffer, int value)
        {
            buffer.Add((byte)value);
            buffer.Add((byte)(value >> 8));
            buffer.Add((byte)(value >> 16));
            buffer.Add((byte)(value >> 24));
        }

        static Texture2D CopyTextureReadable(Texture2D texture, ThumbnailOptions options)
        {
            var savedRT = RenderTexture.active;
            var savedViewport = GetRawViewportRect();

            var tmp = RenderTexture.GetTemporary(
                options.width, options.height,
                0,
                SystemInfo.GetGraphicsFormat(DefaultFormat.LDR));
            var mat = GetMaterialForSpecialTexture(texture, null, QualitySettings.activeColorSpace == ColorSpace.Linear);
            if (mat != null)
                Graphics.Blit(texture, tmp, mat);
            else
                Graphics.Blit(texture, tmp);

            RenderTexture.active = tmp;
            var uncompressedTexture = new Texture2D(options.width, options.height, HasAlphaTextureFormat(texture) ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            uncompressedTexture.ReadPixels(new Rect(0, 0, options.width, options.height), 0, 0);
            uncompressedTexture.Apply();
            RenderTexture.ReleaseTemporary(tmp);

            SetRenderTextureNoViewport(savedRT);
            SetRawViewportRect(savedViewport);

            return uncompressedTexture;
        }

        private static PropertyInfo s_RawViewportRect;
        internal static Rect GetRawViewportRect()
        {
            if (s_RawViewportRect == null)
            {
                var t = typeof(ShaderUtil);
                s_RawViewportRect = t.GetProperty("rawViewportRect", BindingFlags.NonPublic | BindingFlags.Static);
            }

            return (Rect)s_RawViewportRect.GetValue(null);
        }

        internal static void SetRawViewportRect(Rect rect)
        {
            if (s_RawViewportRect == null)
            {
                var t = typeof(ShaderUtil);
                s_RawViewportRect = t.GetProperty("rawViewportRect", BindingFlags.NonPublic | BindingFlags.Static);
            }

            s_RawViewportRect.SetValue(null, rect);
        }

        private static MethodInfo s_SetRenderTextureNoViewport;
        internal static void SetRenderTextureNoViewport(RenderTexture rt)
        {
            if (s_SetRenderTextureNoViewport == null)
            {
                var t = typeof(EditorGUIUtility);
                s_SetRenderTextureNoViewport = t.GetMethod("SetRenderTextureNoViewport", BindingFlags.NonPublic | BindingFlags.Static);
            }

            s_SetRenderTextureNoViewport.Invoke(null, new[] { rt });
        }

        private static MethodInfo s_GetMaterialForSpecialTexture;
        internal static Material GetMaterialForSpecialTexture(Texture2D source, Material defaultMaterial, bool normals2Linear, bool useVTMaterialWhenPossible = true)
        {
            if (s_GetMaterialForSpecialTexture == null)
            {
                var t = typeof(EditorGUI);
                s_GetMaterialForSpecialTexture = t.GetMethod("GetMaterialForSpecialTexture", BindingFlags.NonPublic | BindingFlags.Static);
            }

            return (Material)s_GetMaterialForSpecialTexture.Invoke(null, new object[] { source, defaultMaterial, normals2Linear, useVTMaterialWhenPossible });
        }

        static MethodInfo s_HasAlphaTextureFormat;
        internal static bool HasAlphaTextureFormat(Texture2D texture)
        {
            if (s_HasAlphaTextureFormat == null)
            {
                Assembly assembly = typeof(UnityEditor.SerializedProperty).Assembly;
                var type = assembly.GetTypes().First(t => t.FullName == "UnityEditor.TextureUtil");
                s_HasAlphaTextureFormat = type.GetMethod("HasAlphaTextureFormat", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                if (s_HasAlphaTextureFormat == null)
                    return false;
            }

            return (bool)s_HasAlphaTextureFormat.Invoke(null, new object[] { texture.format });
        }

        public static HashSet<int> GetConnectionIds(string channelName)
        {
            var connectionIds = new HashSet<int>();
            var channelClientList = ChannelService.GetChannelClientList();
            foreach (var channelClient in channelClientList)
            {
                var clientChannelName = channelClient.name;
                if (clientChannelName != channelName)
                    continue;

                var clientConnectionId = channelClient.connectionId;
                connectionIds.Add(clientConnectionId);
            }

            return connectionIds;
        }
    }
}
#endif
