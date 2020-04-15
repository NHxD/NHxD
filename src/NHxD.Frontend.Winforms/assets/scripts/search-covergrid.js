var showDebugCommands = false
var showItemProgress = false
var documentContextMenu
var coverContextMenu
var totalProgress
var itemTotalProgresses = []
var itemSubtotalProgresses = []

//
// User lists
//

function __applyFilter(filter)
{
	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		MetadataFilter.filter(searchResult.result[i], filter)
	}
}

//
// Cover download
//

function __onCoversDownloadStarted(selectedCoverIndices)
{
}

function __onCoversDownloadCompleted(selectedCoverIndices)
{
	totalProgress.hide()
}

function __onCoversDownloadCancelled(selectedCoverIndices)
{
	totalProgress.hide()
}

function __onCoverDownloadReportProgress(selectedCoverIndices, itemIndex, galleryId, coverPath, error)
{
	var metadata = searchResult.result.first(function(x) { return x.id === galleryId })

	if (!metadata)
	{
		return
	}

	setCover(metadata, coverPath, error)

	var progressPercentage = parseInt(Math.ceil(itemIndex / searchResult.result.length * 100.0))

	totalProgress.setValue(progressPercentage)
}

function setCover(metadata, coverPath, error)
{
	var img = document.getElementById("item-cover-" + metadata.id)

	if (img)
	{
		if (error)
		{
			img.onerror = null
			img.src = "assets/images/cover/200x200/missing.png"

			if (error !== "SKIP")
			{
				img.title = error
			}
		}
		else
		{
			img.src = coverPath

			var cover = metadata.images.cover

			if (cover.w > 0)
			{
				var adjustedHeight = cover.h * img.width / cover.w

				img.style.height = adjustedHeight + "px"
			}
		}

		// force an update of some context menu items
		if (!coverContextMenu.element.hasClass("display-none"))
		{
			validateCoverMenuItems(metadata)
		}
	}

	var loading = document.getElementById("item-cover-loading-" + metadata.id)

	if (loading)
	{
		loading.addClass("display-none")
	}
}

//
// Page download
//

function __onPagesDownloadStarted(selectedPageIndices, galleryId)
{
	var metadata = searchResult.result.first(function(x) { return x.id === galleryId })

	if (!metadata)
	{
		return
	}

	{
		var progress = itemTotalProgresses[metadata.id.toString()]

		progress.enableAnimation()
		progress.show()
	}

	if (selectedPageIndices !== ""
		&& selectedPageIndices.split(" ").length !== metadata.images.pages.length)
	{
		var progress = itemSubtotalProgresses[metadata.id.toString()]

		progress.hide()
		progress.disableAnimation()
	}
}

{
	var PagesDownloadCompletedNotification = {}

	PagesDownloadCompletedNotification.None = 0
	PagesDownloadCompletedNotification.Always = 1
	PagesDownloadCompletedNotification.FocusedOnly = 2
}

function __onPagesDownloadCompleted(loadCount, loadTotal, cacheCount, pageCount, galleryId)
{
	if (window.external.Notifications.PagesDownloadCompleted === PagesDownloadCompletedNotification.Always)
	{
		self.alert("Gallery #" + galleryId + " download completed")
	}

	var metadata = searchResult.result.first(function(x) { return x.id === galleryId })

	if (!metadata)
	{
		return
	}

	onDownloadCompleted(metadata)

	{
		var progress = itemTotalProgresses[metadata.id.toString()]

		progress.disableAnimation()
	}

	{
		var progress = itemSubtotalProgresses[metadata.id.toString()]

		progress.hide()
		progress.disableAnimation()
	}
}

function __onPagesDownloadCancelled(loadCount, loadTotal, cacheCount, pageCount, galleryId)
{
	var metadata = searchResult.result.first(function(x) { return x.id === galleryId })

	if (!metadata)
	{
		return
	}

	onDownloadCompleted(metadata)

	{
		var progress = itemTotalProgresses[metadata.id.toString()]

		progress.disableAnimation()
	}

	{
		var progress = itemSubtotalProgresses[metadata.id.toString()]

		progress.hide()
		progress.disableAnimation()
	}
}

function onDownloadCompleted(metadata)
{

}

function __onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error)
{
	onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, false)
}

function onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, isCacheSearch)
{
	var metadata = searchResult.result.first(function(x) { return x.id === galleryId })

	if (!metadata)
	{
		return
	}

	var totalProgressPercentage = Math.ceil(cacheCount / pageCount * 100)
	var subTotalProgressPercentage = Math.ceil(loadCount / loadTotal * 100)

	{
		var progress = itemTotalProgresses[metadata.id.toString()]

		progress.setValue(totalProgressPercentage)

		if (!isCacheSearch)
		{
			progress.enableAnimation()
			progress.show()
		}
		else
		{
			if (totalProgressPercentage > 0)
			{
				progress.show()
			}
		}
	}

	if (loadTotal !== pageCount)
	{
		var progress = itemSubtotalProgresses[metadata.id.toString()]

		progress.setValue(subTotalProgressPercentage)

		if (!isCacheSearch)
		{
			progress.show()
		}
	}

	// force an update of some context menu items.
	if (!coverContextMenu.element.hasClass("display-none"))
	{
		validatePageDownloadMenuItems(metadata)
	}
}

//
// User lists
//

function __onWhitelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, whitelist)
	titleFormatter.whitelistRegex = null
	highlightTitles()
}

function __onBlacklistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, blacklist)
	titleFormatter.blacklistRegex = null
	highlightTitles()
}

function __onIgnorelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, ignorelist)
	updateBlurs()
}

function __onHidelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, hidelist)
	updateHidden()
}

//
// Misc.
//

function addBookmark()
{
	if (search.tag_id !== -1)
	{
		window.external.Bookmark.ShowAddTaggedBookmarkPrompt(search.tag_id, search.page_index)
	}
	else
	{
		window.external.Bookmark.ShowAddSearchBookmarkPrompt(search.query, search.page_index)
	}
}

function highlightMetadataKeywordLists()
{
	MetadataKeywordList.applyToSearchResult(searchResult, function(i)
		{
			return document.getElementById("item-button-" + searchResult.result[i].id)
		})
}

function highlightTitles()
{
	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]
		var title = document.getElementById("item-title-" + metadata.id)

		// HACK: assume it is set to english (could use primary...)
		title.innerHTML = titleFormatter.format(title.innerText, "english")
	}
}

function updateBlurs()
{
	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]
		var button = document.getElementById("item-button-" + metadata.id)

		if (MetadataKeywordList.isInList(ignorelist, metadata))
		{
			button.addClass("tag-ignorelist")
		}
		else
		{
			button.removeClass("tag-ignorelist")
		}
	}
}

function updateHidden()
{
	//var dirtyIndices = []

	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]
		var button = document.getElementById("search-result-item-" + metadata.id)

		if (button)
		{
			if (MetadataKeywordList.isInList(hidelist, metadata))
			{
				button.addClass("display-none")
				//dirtyIndices.push(i)
			}
			else
			{
				button.removeClass("display-none")
			}
		}
		// TODO: restore item. we could clone any other item and replace the values accordingly, but is it really worth all the trouble?
		/*else
		{
			if (MetadataKeywordList.isInList(hidelist, metadata))
			{
			}
		}
		*/
	}

	/*for (var i = 0, len = dirtyIndices.length; i > len; --i)
	{
		// TODO: store the item somewhere, so that it can be restored later, if needed.
		unusedResult.push(searchResult.result[dirtyIndices[i - 1]])

		searchResult.result.slice(dirtyIndices[i - 1], dirtyIndices[i - 1] + 1)
	}*/
}

//
// Initialization
//

function initDocument()
{
	document.onpagerefresh = function(event)
	{
		Navigation.gotoPage(search.page_index)
	}

	document.onnavigateback = function(event)
	{
		// TODO: window.external.Gallery.GetUrlAt(-1)
		Navigation.gotoPreviousPage()
	}

	document.onnavigateforward = function(event)
	{
		// TODO: window.external.Gallery.GetUrlAt(1)
		Navigation.gotoNextPage()
	}

	document.ondocumentcancel = function(event)
	{
		window.external.BackgroundWorkers.CoverDownloader.CancelSearchResult(JSON.stringify(searchResult))
	}
}

function initZoom()
{
	zoom.init(window.external.Browsers.Gallery.GetZoomRatio())

	zoom.onzoomchanged = function(ratio)
	{
		window.external.Browsers.Gallery.SetZoomRatio(ratio)

		// GUI elements shouldn't be scaleable.
		if (ratio)
		{
			documentContextMenu.element.style.zoom = (1.0 / ratio).toString()
			coverContextMenu.element.style.zoom = (1.0 / ratio).toString()
		}
	}
}

function initCover(metadata, i)
{
	// NOTE: BUTTON tag takes precedence over embedded IMG tag.

	var cover = document.getElementById("item-button-" + metadata.id)

	cover.addEventListener("contextmenu", function(e)
	{
		cover.searchResultIndex = i

		if (e.target && e.target.nodeName === "BUTTON")
		{
			ContextMenus.show(coverContextMenu, e.target)
		}
	})
}

function initCovers()
{
	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]

		initCover(metadata, i)
	}
}

function validateCoverMenuItems(metadata)
{
	var doesCoverExists = window.external.FileSystem.Path.DoesCoverExists(metadata.id)

	document.getElementById("menu-command-open-cover").disabled = !doesCoverExists
}

function validatePageDownloadMenuItems(metadata)
{
	var doesPagesFolderExists = window.external.FileSystem.Path.DoesPagesFolderExists(metadata.id)
	var doesAnyPageExists = window.external.FileSystem.Path.AnyPageExists(metadata.id)
	var doesArchiveExists = window.external.FileSystem.Path.DoesComicBookArchiveExists(metadata.id)

	document.getElementById("menu-command-browse").disabled = !doesPagesFolderExists
	document.getElementById("menu-command-read").disabled = !doesAnyPageExists
	document.getElementById("menu-command-open-archive").disabled = !doesArchiveExists
}

function DocumentContextMenu_OnBeforeOpenContextMenu(target)
{
	return false
}

function CoverContextMenu_OnBeforeOpenContextMenu(target)
{
	var metadata = searchResult.result[target.searchResultIndex]

	validateCoverMenuItems(metadata)
	validatePageDownloadMenuItems(metadata)

	document.getElementById("menu-command-show-details").addOrRemoveClass("display-none", false)
	document.getElementById("menu-command-show-download").addOrRemoveClass("display-none", false)

	// HACK: interaction with background workers isn't fully implemented/tested yet, so always show/hide these two commands.
	document.getElementById("menu-command-download-all").addOrRemoveClass("display-none", false)
	document.getElementById("menu-command-download-cancel").addOrRemoveClass("display-none", true)

	return false
}

function initContextMenus()
{
	documentContextMenu = Object.create(ContextMenu)
	documentContextMenu.init({
		"name": "document",
		"horizontalAlignment": "cursor",
		"verticalAlignment": "cursor",
		"onbeforeopen": function(target)
		{
			DocumentContextMenu_OnBeforeOpenContextMenu(target)
		}
	})

	coverContextMenu = Object.create(ContextMenu)
	coverContextMenu.init({
		"name": "cover",
		"horizontalAlignment": "cursor",
		"verticalAlignment": "cursor",
		"onbeforeopen": function(target)
		{
			CoverContextMenu_OnBeforeOpenContextMenu(target)
		}
	})

	ContextMenus.init(
	{
		"contextMenus": [ documentContextMenu, coverContextMenu ]
	})

	document.body.addEventListener("contextmenu", function(e)
	{
		if (e.target.className === "search-result-container")
		{
			ContextMenus.show(documentContextMenu, e.target)
		}
	})
}

function initProgress()
{
	showItemProgress = window.external.Browsers.Gallery.GetItemDownloadProgressVisible()

	totalProgress = Object.create(Progress)
	totalProgress.init({
		"element": document.getElementById("progress-total"),
		autoHide: true
	})

	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]
		var progress = Object.create(Progress)
		progress.init({
			"element": document.getElementById("progress-total-" + metadata.id),
			autoHide: false
		})

		if (showItemProgress)
		{
			progress.element.removeClass("display-none-toggle")
		}
		else
		{
			progress.element.addClass("display-none-toggle")
		}

		itemTotalProgresses[metadata.id.toString()] = progress
	}

	for (var i = 0, len = searchResult.result.length; i < len; ++i)
	{
		var metadata = searchResult.result[i]
		var progress = Object.create(Progress)
		progress.init({
			"element": document.getElementById("progress-subtotal-" + metadata.id),
			autoHide: false
		})

		itemSubtotalProgresses[metadata.id.toString()] = progress
	}

	document.addEventListener("keydown", function(event)
	{
		event = event || window.event
		var keyCode = event.keyCode || event.which

		if (keyCode === 72)	// 'h'
		{
			showItemProgress = !showItemProgress
			window.external.Browsers.Gallery.SetItemDownloadProgressVisible(showItemProgress)

			for (var i = 0, len = searchResult.result.length; i < len; ++i)
			{
				var metadata = searchResult.result[i]
				var itemTotalProgress = itemTotalProgresses[metadata.id.toString()]

				if (showItemProgress)
				{
					itemTotalProgress.element.removeClass("display-none-toggle")
				}
				else
				{
					itemTotalProgress.element.addClass("display-none-toggle")
				}

				var itemSubtotalProgress = itemSubtotalProgresses[metadata.id.toString()]

				if (itemSubtotalProgress && itemSubtotalProgress.value !== 100)
				{
					if (showItemProgress)
					{
						itemSubtotalProgress.element.removeClass("display-none-toggle")
					}
					else
					{
						itemSubtotalProgress.element.addClass("display-none-toggle")
					}
				}
			}
		}
	})
}

function Navigation_gotoPage(pageIndex)
{
	if (search.tag_id !== -1)
	{
		window.external.Search.RunTaggedSearch(search.tag_id, pageIndex)
	}
	else
	{
		window.external.Search.RunSearch(search.query, pageIndex)
	}
}

function initCache()
{
	Cache.init(
		{
			"onCacheSearchCompleted": function(metadata)
			{
				onDownloadCompleted(metadata)
			},
			"onCacheSearchFinished": function(metadata, cachedCount, totalPages)
			{
				var progressPercentage = Math.ceil(cachedCount / totalPages * 100)
				var progress = itemTotalProgresses[metadata.id.toString()]

				progress.setValue(progressPercentage)
			},
			"onCachedPageFound": function(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath)
			{
				onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, 0, 0, "", true)
			},
			"onCachedCoverFound": function(metadata, coverPath)
			{
				setCover(metadata, coverPath, "")
			}
			/*,
			"onCachedCoversSearchFinished": function(cachedCount, totalCovers)
			{
				var progressPercentage = Math.ceil(cachedCount / totalCovers * 100)
				var progress = totalProgress

				progress.setValue(progressPercentage)
			}
			*/
		})

	Cache.validateAllImageSources(searchResult)
	Cache.validateAllCoverImageSources(searchResult)
}

(function()
{
	console.create()
	Browser.init()
	initDocument()
	initProgress()
	Navigation.init(search, searchResult, Navigation_gotoPage)
	window.external.Browsers.Gallery.ApplyFilter()
	highlightMetadataKeywordLists()
	highlightTitles()
	updateHidden()
	initContextMenus()
	initCovers()
	initCache()
	initZoom()
})()
