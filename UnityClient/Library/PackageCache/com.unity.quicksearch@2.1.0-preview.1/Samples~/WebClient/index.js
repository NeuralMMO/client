import * as SearchService from "./SearchService/searchService.js"

let queryInputElement = document.getElementById("queryInput");
queryInputElement.addEventListener("input", OnQueryInput);
let listElement = document.getElementById("list");
let searchResultsInfoElement = document.getElementById("searchResultsInfo");

// It is preferable to fetch PNGs rather than JPEG if the textures have transparency. Otherwise, you can use
// JPEG for smaller thumbnails.
let thumbnailOptions = new SearchService.ThumbnailOptions(SearchService.ThumbnailCompression.PNG, 64, 64, 95);

const k_InfoBatchSize = 100;
let itemsMissingInfo = new Set();
const k_ThumbnailBatchSize = 100;
let itemsMissingThumbnails = new Set();

let startTimer = Date.now();
let endTimer = startTimer;

let allItems = [];
const k_MaxItemShown = 3000;

function OnQueryInput(ev) {
    // When doing a new query, we clear everything
    ClearSearchList();
    ClearItemsData();
    startTimer = Date.now();
    endTimer = startTimer;
    UpdateSearchResultsInfo();

    // We initiate the search request, and pass PopulateList as our callback. Also, we don't ask any additional
    // information to reduce the strain on the websocket and reduce potential lag.
    SearchService.Search(queryInputElement.value, PopulateList, SearchService.FetchOptions.NONE, k_MaxItemShown);
}

function PopulateList(items) {
    endTimer = Date.now();

    // This is ineficient but for the purpose of this example
    // when receiving new Search Items we clear the list view,
    // sort all items according to our sorting algorithm and
    // repopulate the list view.
    allItems = allItems.concat(items);
    allItems.sort(SearchService.SearchItem.Compare);
    ClearSearchList();

    let count = 0;
    for (let item of allItems) {
        if (!item.label || !item.description) {
            itemsMissingInfo.add(item.id);
        }
        if (!item.thumbnail) {
            itemsMissingThumbnails.add(item.id);
        }
        let listItemElement = CreateListItemElement(item);
        listElement.appendChild(listItemElement);

        ++count;
        if (count >= k_MaxItemShown) {
            break;
        }
    }

    UpdateSearchResultsInfo();

    // We fetch additional information for new Search Items.
    FetchMissingInfos();
    FetchMissingThumbnails();
}

function FetchMissingData(missingDataSet, maxCount, fetchDataCB) {
    // Fetching a lot of data at once can sometimes cause problems.
    // Lets batch those requests.
    let batch = [];
    let count = 0;
    for (let id of missingDataSet) {
        batch.push(id);
        ++count;
        if (count >= maxCount) {
            break;
        }
    }

    if (batch.length === 0) {
        return;
    }

    fetchDataCB(batch);
}

function FetchMissingInfos() {
    // Requesting missing labels and description
    FetchMissingData(itemsMissingInfo, k_InfoBatchSize, (batch) => SearchService.FetchInfo(batch, UpdateList, SearchService.FetchOptions.LABEL | SearchService.FetchOptions.DESCRIPTION));
}

function FetchMissingThumbnails() {
    // Requesting missing thumbnails
    FetchMissingData(itemsMissingThumbnails, k_ThumbnailBatchSize, (batch) => SearchService.FetchThumbnail(batch, thumbnailOptions, UpdateThumbnails));
}

function UpdateList(items) {
    for (let item of items) {
        if (itemsMissingInfo.has(item.id)) {
            itemsMissingInfo.delete(item.id);
        }

        let existingItem = allItems.find(i => i.id === item.id && i.providerPriority === item.providerPriority);
        if (existingItem) {
            existingItem.Update(item);
        }

        let listItemElement = GetListItemElement(item.id);
        if (!listItemElement) {
            continue;
        }

        if (item.label) {
            UpdateListItemLabel(listItemElement, item.label);
        }
        if (item.description) {
            UpdateListItemDescription(listItemElement, item.description);
        }
    }

    // Since we only asked for a small batch,
    // initiate another fetch.
    setTimeout(FetchMissingInfos, 0);
}

function UpdateThumbnails(items) {
    for (let item of items) {
        if (itemsMissingThumbnails.has(item.id)) {
            itemsMissingThumbnails.delete(item.id);
        }

        let existingItem = allItems.find(i => i.id === item.id && i.providerPriority === item.providerPriority);
        if (existingItem) {
            existingItem.Update(item);
        }

        let listItemElement = GetListItemElement(item.id);
        if (!listItemElement) {
            continue;
        }

        if (item.thumbnail) {
            UpdateListItemThumbnail(listItemElement, item.thumbnail);
        }
    }

    // Since we only asked for a small batch,
    // initiate another fetch.
    setTimeout(FetchMissingThumbnails, 0);
}

function ClearSearchList() {
    // As long as <ul> has a child node, remove it
    ClearElement(listElement);
}

function ClearElement(htmlElement) {
    while (htmlElement.hasChildNodes()) {
        htmlElement.removeChild(htmlElement.firstChild);
    }
}

function ClearItemsData() {
    allItems = [];
    itemsMissingInfo.clear();
    itemsMissingThumbnails.clear();
}

function CreateListItemElement(searchItem) {
    let listItemElement = document.createElement("li");
    listItemElement.id = searchItem.id;
    listItemElement.dataset.score = searchItem.score;

    let thumbnailDiv = document.createElement("div");
    thumbnailDiv.id = "thumbnail";
    thumbnailDiv.className = "search-item-thumbnail";
    listItemElement.appendChild(thumbnailDiv);
    if (searchItem.thumbnail) {
        UpdateListItemThumbnail(listItemElement, searchItem.thumbnail);
    }

    let infoDiv = document.createElement("div");
    infoDiv.className = "search-item-info";
    listItemElement.appendChild(infoDiv);

    let labelDiv = document.createElement("div");
    labelDiv.id = "label";
    labelDiv.className = "search-item-label";
    labelDiv.innerHTML = searchItem.label;
    infoDiv.appendChild(labelDiv);
    if (searchItem.label) {
        UpdateListItemLabel(listItemElement, searchItem.label);
    }

    let descriptionDiv = document.createElement("div");
    descriptionDiv.id = "description";
    descriptionDiv.className = "search-item-description";
    descriptionDiv.innerHTML = searchItem.description;
    infoDiv.appendChild(descriptionDiv);
    if (searchItem.description) {
        UpdateListItemDescription(listItemElement, searchItem.description);
    }

    return listItemElement;
}

function GetListItemElement(id) {
    return document.getElementById(id);
}

function UpdateListItemLabel(listItemElement, newLabel) {
    let labelDiv = listItemElement.querySelector("#label");
    labelDiv.innerHTML = newLabel;
}

function UpdateListItemDescription(listItemElement, newDescription) {
    let descriptionDiv = listItemElement.querySelector("#description");
    descriptionDiv.innerHTML = newDescription;
}

function UpdateListItemThumbnail(listItemElement, thumbnail) {
    let thumbnailDiv = listItemElement.querySelector("#thumbnail");
    ClearElement(thumbnailDiv);
    let image = new Image();
    image.className = "search-item-thumbnail-image";
    image.src = URL.createObjectURL(thumbnail);
    image.onload = function () {
        URL.revokeObjectURL(this.src);
    }
    thumbnailDiv.appendChild(image);
}

function UpdateSearchResultsInfo() {
    let millis = endTimer - startTimer;
    let nbResult = allItems.length;
    let nbItemsShown = listElement.childElementCount;

    searchResultsInfoElement.textContent = `Found ${nbResult}${(nbItemsShown !== nbResult ? `(${nbItemsShown} items shown)` : "")} in ${millis}ms.`;
}
