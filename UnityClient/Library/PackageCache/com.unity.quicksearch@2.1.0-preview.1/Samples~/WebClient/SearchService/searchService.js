import { ChannelClient } from "./channelClient.js"

const k_Port = 56865;
let s_SearchClient = new ChannelClient("search", false, k_Port, OnMessageReceived);
let s_SearchThumbnailClient = new ChannelClient("search_thumbnail", true, k_Port, OnBinaryMessageReceived);

let s_OnItemsReceived = null;
let s_OnItemsInfoReceived = null;
let s_OnItemsThumbnailReceived = null;
let s_RequestId = 0;

const SearchRequestType = {
    QUERY: 0,
    FETCHINFO: 1
}

/**
 * Callback used to handle search requests.
 * @callback onItemsReceivedHandler
 * @param {SearchItem[]} searchItems The list of Search Items received.
 */

/**
* Callback used to handle FetchInfo requests.
* @callback onInfoReceivedHandler
* @param {SearchItem[]} searchItems The list of Search Items received.
*/

/**
* Callback used to handle FetchThumbnail requests.
* @callback onThumbnailReceivedHandler
* @param {SearchItem[]} searchItems The list of Search Items received.
*/

/**
 * Enum for information fetching.
 * This can be used as a bitfield to request multiple search item information at the same time.
 * @readonly
 * @enum {number}
 * @exports
 */
export const FetchOptions = {
    /** @type {number} */
    NONE: 0,
    /** @type {number} */
    LABEL: 1,
    /** @type {number} */
    DESCRIPTION: 1 << 1
};

/**
 * Enum for compression type.
 * @readonly
 * @enum {number}
 * @exports
 */
export const ThumbnailCompression = {
    /**
     * Request thumbnails as JPEG. Does not support transparency.
     * @type {number}
     * */
    JPEG: 0,
    /**
     * Request thumbnail as PNG.
     * @type {number}
     * */
    PNG: 1
};

/**
 * Structure that holds options when requesting thumbnails.
 * @class ThumbnailOptions
 * @exports
 */
export class ThumbnailOptions {
    /**
     * Compression option.
     * @type {ThumbnailCompression}
     * @memberof ThumbnailOptions
     */
    compression;
    /**
     * Width of the requested thumbnail.
     * @type {number}
     * @memberof ThumbnailOptions
     */
    width;
    /**
     * Height of the requested thumbnail
     * @type {number}
     * @memberof ThumbnailOptions
     */
    height;
    /**
     * Compression quality. Between 0-100. Valid for JPEG only.
     * @type {number}
     * @memberof ThumbnailOptions
     */
    quality;

    /**
     * Creates an instance of ThumbnailOptions.
     * @param {ThumbnailCompression} compression
     * @param {number} width
     * @param {number} height
     * @param {number} [quality=100]
     * @memberof ThumbnailOptions
     */
    constructor(compression, width, height, quality = 100) {
        this.compression = compression;
        this.width = width;
        this.height = height;
        this.quality = quality;
    }
}

/**
 * Structure that holds a Search Item.
 *
 * @export
 * @class SearchItem
 */
export class SearchItem {
    /**
     * Search Item identifier.
     * @type {string}
     * @memberof SearchItem
     */
    id;
    /**
     * Display label.
     * @type {string}
     * @memberof SearchItem
     */
    label;
    /**
     * The Search Item's score. The lower the score, the better it matches the query.
     * @type {number}
     * @memberof SearchItem
     */
    score;
    /**
     * Provider's priority. A lower number means a higher priority.
     * @type {number}
     * @memberof SearchItem
     */
    providerPriority;
    /**
     * Display description.
     * @type {string}
     * @memberof SearchItem
     */
    description;
    /**
     * The Search Item's thumbnail.
     * @type {Blob}
     * @memberof SearchItem
     */
    thumbnail;

    /**
     * Creates an instance of SearchItem.
     * @param {*} data An object containing the initial values of the Search Item.
     * @memberof SearchItem
     */
    constructor(data) {
        Object.assign(this, {
            id: "",
            label: "",
            score: 0,
            providerPriority: 0,
            description: "",
            thumbnail: null
        }, data);
    }

    /**
     * Update this Search Item.
     * @param {SearchItem} data An object containing the new label, description or thumbnail.
     * @memberof SearchItem
     */
    Update(data) {
        if (data.label) {
            this.label = data.label;
        }
        if (data.description) {
            this.description = data.description;
        }
        if (data.thumbnail) {
            this.thumbnail = data.thumbnail;
        }
    }

    /**
     * Compares two Search Items.
     * @static
     * @param {SearchItem} a The first Search Item
     * @param {SearchItem} b The second Search Item
     * @returns {number} 0 if equals, <0 if lesser and >0 if greater.
     * @memberof SearchItem
     */
    static Compare(a, b) {
        let priorityCompare = SearchItem.CompareProviderPriority(a, b);
        if (priorityCompare !== 0) {
            return priorityCompare;
        }
        let scoreCompare = SearchItem.CompareScore(a, b);
        if (scoreCompare !== 0) {
            return scoreCompare;
        }
        return a.id.localeCompare(b.id);
    }

    /**
     * Compares two Search Items by their provider priorities.
     * @static
     * @param {SearchItem} a The first Search Item
     * @param {SearchItem} b The second Search Item
     * @returns {number} 0 if equals, <0 if lesser (higher priority) and >0 if greater (lower priority).
     * @memberof SearchItem
     */
    static CompareProviderPriority(a, b) {
        return a.providerPriority - b.providerPriority;
    }

    /**
     * Compares two Search Items by their score.
     * @static
     * @param {SearchItem} a The first Search Item
     * @param {SearchItem} b The second Search Item
     * @returns {number} 0 if equals, <0 if lesser and >0 if greater.
     * @memberof SearchItem
     */
    static CompareScore(a, b) {
        return a.score - b.score;
    }
}

/**
 * Initiate a search request.
 * @export
 * @param {string} query The search query. Anything that quicksearch supports.
 * @param {onItemsReceivedHandler} onItemsReceivedCB Callback called when items are received. Not all items are received at once. The handler must support to be called multiple times.
 * @param {FetchOptions} [fetchOptions=FetchOptions.NONE] The additional information to fetch.
 * @param {number} [maxCount=-1] The maximum amount of items to fetch.
 */
export function Search(query, onItemsReceivedCB, fetchOptions = FetchOptions.NONE, maxCount = -1) {
    let data = CreateQueryRequest(query, fetchOptions, maxCount);
    s_OnItemsReceived = onItemsReceivedCB;
    let json = JSON.stringify(data)
    s_SearchClient.send(json);
}

/**
 * Fetch additional information for Search Items.
 * @exports
 * @param {number[]} itemIds An array of Search Item ids.
 * @param {onInfoReceivedHandler} onItemsInfoReceivedCB Callback called when items' informations are received. All the informations are received at once.
 * @param {FetchOptions} [fetchOptions=FetchOptions.LABEL | FetchOptions.DESCRIPTION] The additional information to fetch.
 */
export function FetchInfo(itemIds, onItemsInfoReceivedCB, fetchOptions = FetchOptions.LABEL | FetchOptions.DESCRIPTION) {
    let data = CreateFetchInfoRequest(itemIds, fetchOptions);
    s_OnItemsInfoReceived = onItemsInfoReceivedCB;
    let json = JSON.stringify(data)
    s_SearchClient.send(json);
}

/**
 * Fetch thumbnails for Search Items.
 * @param {number[]} itemIds An array of Search Item ids.
 * @param {ThumbnailOptions} thumbnailOptions The thumbnail options.
 * @param {onThumbnailReceivedHandler} onItemsThumbnailReceivedCB Callback called when thumbnails are received. All thumbnails are received at once.
 */
export function FetchThumbnail(itemIds, thumbnailOptions, onItemsThumbnailReceivedCB) {
    let data = CreateFetchThumbnailRequest(itemIds, thumbnailOptions);
    s_OnItemsThumbnailReceived = onItemsThumbnailReceivedCB;
    let json = JSON.stringify(data)
    s_SearchThumbnailClient.send(json);
}

// Private functions
function OnMessageReceived(ev) {
    let searchData = JSON.parse(ev.data);

    let requestId = searchData["requestId"];
    if (requestId !== s_RequestId) {
        return;
    }

    let searchItems = [];
    for (let item of searchData["items"]) {
        searchItems.push(new SearchItem(item));
    }

    let requestType = searchData["requestType"];
    if (requestType === SearchRequestType.QUERY) {
        if (s_OnItemsReceived) {
            s_OnItemsReceived(searchItems);
        }
    }
    else if (requestType === SearchRequestType.FETCHINFO) {
        if (s_OnItemsInfoReceived) {
            s_OnItemsInfoReceived(searchItems);
        }
    }
}

function OnBinaryMessageReceived(ev) {
    let buffer = new Uint8Array(ev.data);

    let requestId = ReadInt(buffer, 0);
    let compression = ReadInt(buffer, 4);

    if (requestId !== s_RequestId) {
        return;
    }

    let searchItems = DecodeThumbnailBufferToSearchItems(buffer, 8, compression);
    if (s_OnItemsThumbnailReceived) {
        s_OnItemsThumbnailReceived(searchItems);
    }
}

function CreateQueryRequest(query, fetchOptions, maxCount) {
    return {
        query: query,
        requestId: ++s_RequestId,
        fetchOptions: fetchOptions,
        maxCount: maxCount,
        requestType: SearchRequestType.QUERY
    };
}

function CreateFetchInfoRequest(itemIds, fetchOptions) {
    return {
        requestType: SearchRequestType.FETCHINFO,
        fetchOptions: fetchOptions,
        requestId: s_RequestId, // Do not ++ s_RequestId, we want to keep the same id that requested the searchItems
        itemIds: itemIds
    };
}

function CreateFetchThumbnailRequest(itemIds, thumbnailOptions) {
    return {
        connectionId: s_SearchClient.clientId, // This is needed to match the connectionId of the search_thumbnail route to the searc route
        requestId: s_RequestId, // Do not ++ s_RequestId, we want to keep the same id that requested the searchItems
        itemIds: itemIds,
        width: thumbnailOptions.width,
        height: thumbnailOptions.height,
        quality: thumbnailOptions.quality,
        compression: thumbnailOptions.compression
    };
}

function DecodeThumbnailBufferToSearchItems(buffer, startOffset, compression) {
    // The datablock of an item is:
    // offset        size      data
    // ---------------------------------------------------------
    //   0            4        Length of id buffer
    //   4            N        Id buffer
    //  4+N           4        Length of thumbnail buffer
    //  8+N           4        Actual width of thumbnail
    // 12+N           4        Actual height of thumbnail
    // 16+N           M        Thumbnail buffer

    let bytesRead = startOffset;
    let totalBytes = buffer.length;

    let imageType = compression === ThumbnailCompression.JPEG ? "image/jpeg" : "image/png";

    let searchItems = [];
    while (bytesRead < totalBytes) {
        let idBufferLength = ReadInt(buffer, bytesRead);
        bytesRead += 4;
        let idBuffer = buffer.slice(bytesRead, idBufferLength + bytesRead);
        bytesRead += idBufferLength;
        let id = new TextDecoder().decode(idBuffer);

        let thumbnailBufferLength = ReadInt(buffer, bytesRead);
        bytesRead += 4;
        let actualWidth = ReadInt(buffer, bytesRead);
        bytesRead += 4;
        let actualHeight = ReadInt(buffer, bytesRead);
        bytesRead += 4;
        let thumbnailBuffer = buffer.subarray(bytesRead, thumbnailBufferLength + bytesRead);
        bytesRead += thumbnailBufferLength;

        searchItems.push(new SearchItem({ id: id, thumbnail: new Blob([thumbnailBuffer], { type: imageType })}));
    }
    return searchItems;
}

function ReadInt(buffer, offset) {
    let value = 0;
    for (let i = 0; i < 4; ++i) {
        value |= (buffer[offset + i] << (8*i));
    }
    return value;
}
