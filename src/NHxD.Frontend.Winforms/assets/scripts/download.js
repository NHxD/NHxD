var showDebugCommands = false
var pageThumbnailContextMenu
var coverContextMenu
var totalProgress
var subTotalProgress
var pageProgresses = []
var pageCheckedCount = 0
var firstPageCount = 0//window.external.Download.GetPageFirst()
var lastPageCount = 0//window.external.Download.GetPageLast()
var intervalPageCount = 0//window.external.Download.GetPageInterval()

//
// Cover download
//

function __onCoversDownloadStarted(selectedCoverIndices)
{
}

function __onCoversDownloadCompleted(selectedCoverIndices)
{
}

function __onCoversDownloadCancelled(selectedCoverIndices)
{
}

function __onCoverDownloadReportProgress(selectedCoverIndices, itemIndex, galleryId, coverPath, error)
{
	if (galleryId !== metadata.id)
	{
		return
	}

	var img = document.getElementById("cover")

	if (img)
	{
		if (error)
		{
			//img.onerror = null
			//img.src = "assets/images/cover/200x200/missing.png"

			if (error !== "SKIP")
			{
				img.title = error
			}
		}

		img.src = coverPath

		// force an update of some context menu items.
		if (!coverContextMenu.element.hasClass("display-none"))
		{
			validateCoverMenuItems()
		}
	}
}

//
// Page download
//

function __onPagesDownloadStarted(selectedPageIndices, galleryId)
{
	if (galleryId !== metadata.id)
	{
		return
	}

	resetAllDownloadStatus()
	enableDownloadControls(false)
	totalProgress.show()
	totalProgress.enableAnimation()

	if (selectedPageIndices !== ""
		&& selectedPageIndices.split(" ").length !== metadata.images.pages.length)
	{
		subTotalProgress.show()
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
	if (galleryId !== metadata.id)
	{
		return
	}

	totalProgress.disableAnimation()

	if (window.external.Notifications.PagesDownloadCompleted === PagesDownloadCompletedNotification.FocusedOnly)
	{
		self.alert("Download completed")
	}

	if (cacheCount === pageCount)
	{
		hideDownloadControls()
	}
	else
	{
		enableDownloadControls(true)
	}

	subTotalProgress.hide()
}

function __onPagesDownloadCancelled(loadCount, loadTotal, cacheCount, pageCount, galleryId)
{
	if (galleryId !== metadata.id)
	{
		return
	}

	totalProgress.disableAnimation()

	if (window.external.Notifications.PagesDownloadCompleted === PagesDownloadCompletedNotification.FocusedOnly)
	{
		self.alert("Download cancelled")
	}

	enableDownloadControls(true)

	subTotalProgress.hide()
}

function __onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error)
{
	onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, false)
}

function onDownloadCompleted(isCacheSearch)
{
	hideDownloadControls()
}

function onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, isCacheSearch)
{
	if (galleryId !== metadata.id)
	{
		return
	}

	var page = document.getElementById("page-" + pageIndex)
	var button = document.getElementById("thumbnail-button-" + pageIndex)
	var img = document.getElementById("thumbnail-image-" + pageIndex)

	if (!error && pagePath)
	{
		if (img)
		{
			img.onload = function() { markPageThumbnailAsLoaded(this, this.parentElement) }
			img.onerror = function() { markPageThumbnailAsInvalid(this, this.parentElement) }
			img.src = pagePath.replace(/\//g, "\\")
		}

		button.disabled = false

		button.onclick = function()
		{
			window.external.FileSystem.Cache.OpenPage(JSON.stringify(metadata), pageIndex)
		}

		hideCheckbox(pageIndex)
	}

	//if (downloadSize > 0)
	{
		pageProgresses[pageIndex].setValue(100)
	}

	var status = document.getElementById("status-" + pageIndex)
	var div = status.getElementsByTagName("DIV")[0]

	div.innerText = error || "OK"

	// do this last
	page.addOrRemoveClass("completed", !error)
	page.addOrRemoveClass("cancelled", error && (error !== "SKIP"))
	page.addOrRemoveClass("skipped", error && (error === "SKIP"))

	validateNavigation()
	enableDownloadControls(false)

	var totalProgressPercentage = Math.ceil(cacheCount / pageCount * 100)
	var subTotalProgressPercentage = Math.ceil(loadCount / loadTotal * 100)

	totalProgress.setValue(totalProgressPercentage)
	totalProgress.show()

	if (!isCacheSearch)
	{
		if (cacheCount !== pageCount)
		{
			totalProgress.enableAnimation()
		}

		if (loadTotal !== pageCount)
		{
			subTotalProgress.setValue(subTotalProgressPercentage)
			subTotalProgress.show()
		}
	}

	// force an update of some context menu items.
	if (!coverContextMenu.element.hasClass("display-none"))
	{
		validatePageDownloadMenuItems()
	}
}

//
// User lists
//

function __onWhitelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, whitelist)
	highlightTitles()
}

function __onBlacklistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, blacklist)
	highlightTitles()
}

function __onIgnorelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, ignorelist)
	highlightTitles()
}

function __onHidelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, hidelist)
	highlightTitles()
}

//
// UI
//

function onCoverClicked()
{
	if (window.external.Browsers.Details.GetReloadPageOnCoverClicked())
	{
		window.external.Search.ShowDownload(metadata.id)
	}
}

function hideDownloadControls()
{
	var elements =
	[
		document.getElementById("nav-download"),
		document.getElementById("nav-select"),
		document.getElementById("nav-deselect")
	]

	for (var i = 0, len = metadata.images.pages.length; i < len; ++i)
	{
		elements.push(document.getElementById("checkbox-" + i))
		elements.push(document.getElementById("progress-" + i))
		elements.push(document.getElementById("status-" + i))
	}

	for (var i = 0, len = elements.length; i < len; ++i)
	{
		var element = elements[i]

		if (element)
		{
			element.addClass("display-none")
		}
	}
}

function hideCheckbox(pageIndex)
{
	var checkbox = document.getElementById("checkbox-" + pageIndex)

	if (checkbox)
	{
		checkbox.addClass("visibility-hidden")
	}
}

function resetPageDownloadStatus(pageIndex)
{
	var progress = pageProgresses[pageIndex]
	if (progress)
	{
		progress.setValue(0)
	}

	var status = document.getElementById("status-" + pageIndex)
	var div = status.getElementsByTagName("DIV")[0]
	
	div.innerText =	 ""
}

function enableDownloadControls(enable)
{
	var downloadButton = document.getElementById("nav-download")

	downloadButton.disabled = !enable


	var navSelect = document.getElementById("nav-select")
	var navSelectItems = navSelect.getElementsByTagName("BUTTON")

	for (var i = 0, len = navSelectItems.length; i < len; ++i)
	{
		navSelectItems[i].disabled = !enable
	}


	var navDeselect = document.getElementById("nav-deselect")
	var navDeselectItems = navDeselect.getElementsByTagName("BUTTON")

	for (var i = 0, len = navDeselectItems.length; i < len; ++i)
	{
		navDeselectItems[i].disabled = !enable
	}

	for (var i = 0, len = metadata.images.pages.length; i < len; ++i)
	{
		var checkboxtd = document.getElementById("checkbox-" + i)

		if (checkboxtd)
		{
			var checkbox = checkboxtd.getElementsByTagName("INPUT")[0]

			if (checkbox)
			{
				checkbox.disabled = !enable
			}
		}
	}

	if (enable)
	{
		pageCheckedCount = 0
		onPageCheckedCountChanged()
	}
}

function getSelectedPageIndices()
{
	var arr = []

	for (var i = 0, len = metadata.images.pages.length; i < len; ++i)
	{
		var checkboxtd = document.getElementById("checkbox-" + i)
		{
			// NOTE: HIDDEN, not REMOVED.
			if (checkboxtd && !checkboxtd.hasClass("visibility-hidden"))
			{
				var checkbox = checkboxtd.getElementsByTagName("INPUT")[0]

				if (checkbox.checked)
				{
					arr.push(i)
				}
			}
		}
	}

	return arr
}

function resetAllDownloadStatus()
{
	for (var i = 0, len = metadata.images.pages.length; i < len; ++i)
	{
		resetPageDownloadStatus(i)
	}
}

function selectPages(arr, checked)
{
	for (var i = 0, len = arr.length; i < len; ++i)
	{
		var checkboxtd = document.getElementById("checkbox-" + arr[i])

		if (checkboxtd)
		{
			var checkbox = checkboxtd.getElementsByTagName("INPUT")[0]

			if (checkbox)
			{
				if ((checked && checkbox.checked === "checked")
					|| (!checked && !checkbox.checked))
				{
					continue
				}

				checkbox.checked = checked ? "checked" : ""

				var event = document.createEvent("HTMLEvents")
				event.initEvent("change", true, false)
				checkbox.dispatchEvent(event)
			}
		}
	}
}

function validateCount(count)
{
	count = parseInt(count)

	if (isNaN(count))
	{
		self.alert("Please enter a valid number between " + 0 + " and " + metadata.images.pages.length)
		return count
	}

	if (count < 0)
	{
		count = 0
	}
	else if (count > metadata.images.pages.length)
	{
		count = metadata.images.pages.length
	}

	return count
}

function setButtonsContent(count, name)
{
	var buttons =
	[
		document.getElementById("quickaction-select-" + name),
		document.getElementById("quickaction-deselect-" + name)
	]

	for (var i = 0; i < buttons.length; ++i)
	{
		buttons[i].innerText = isNaN(count) ? " " : count.toString()
	}
}

function setFirstPageCount(count)
{
	firstPageCount = count
	//window.external.Download.SetPageFirst(firstPageCount)

	setButtonsContent(count, "first")
}

function setLastPageCount(count)
{
	lastPageCount = count
	//window.external.Download.SetPageLast(lastPageCount)

	setButtonsContent(count, "last")
}

function setIntervalPageCount(count)
{
	intervalPageCount = count
	//window.external.Download.SetPageInterval(intervalPageCount)

	setButtonsContent(count, "interval")
}

function selectComboPages(first, last, interval, select)
{
	selectFirstPages(first, select)
	selectLastPages(last, select)
	selectIntervalPages(interval, select)
}

function promptAndSelectFirstPages(select)
{
	var count = self.prompt("First amount of pages to download:", firstPageCount)

	if (!count)
	{
		return
	}

	selectFirstPages(count, select)
}

function selectFirstPages(count, select)
{
	count = validateCount(count)

	if (isNaN(count))
	{
		return
	}

	setFirstPageCount(count)

	var range = []

	for (var i = 0; i < count; ++i)
	{
		range.push(i)
	}

	selectPages(range, select)
}

function promptAndSelectLastPages(select)
{
	var count = self.prompt("Last amount of pages to download:", lastPageCount)

	if (!count)
	{
		return
	}

	selectLastPages(count, select)
}

function selectLastPages(count, select)
{
	count = validateCount(count)

	if (isNaN(count))
	{
		return
	}

	setLastPageCount(count)

	var range = []

	for (var i = 0; i < count; ++i)
	{
		range.push(metadata.images.pages.length - i - 1)
	}

	selectPages(range, select)
}

function promptAndSelectIntervalPages(select)
{
	var count = self.prompt("Interval of pages to download:", intervalPageCount)

	if (!count)
	{
		return
	}

	selectIntervalPages(count, select)
}

var numberRangeRegex = /(\d+)\s*-\s*(\d+)/

function selectIntervalPages(count, select)
{
	var match = count.toString().match(numberRangeRegex)

	if (match)
	{
		var from = parseInt(match[1])
		var to = parseInt(match[2])
		var range = []

		for (var i = from - 1; i < to; ++i)
		{
			range.push(i)
		}

		selectPages(range, select)
	}
	else
	{
		count = validateCount(count)

		if (isNaN(count))
		{
			return
		}

		setIntervalPageCount(count)

		var range = []

		for (var i = count, len = metadata.images.pages.length; i <= len; i += count)
		{
			range.push(i - 1)
		}

		selectPages(range, select)
	}
}

function selectEveryPages(select)
{
	var range = []

	for (var i = 0, len = metadata.images.pages.length; i < len; ++i)
	{
		range.push(i)
	}

	selectPages(range, select)
}

//
// Commands
//

function download(galleryId)
{
	var selectedPageIndices = getSelectedPageIndices()

	if (selectedPageIndices.length === 0)
	{
		self.alert("Please select at least one page to download.")
		return
	}

	window.external.BackgroundWorkers.PageDownloader.DownloadCustom(galleryId, selectedPageIndices.join(" "))
}

function createArchive(galleryId)
{
	var pluginCount = window.external.Plugins.ArchiveWriters.Count()

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.ArchiveWriters.Create(i, galleryId)
	}

	// TODO: plugins aren't currently designed to be asynchronous.

	validateNavigation()
}

function convertMetadata(galleryId)
{
	var pluginCount = window.external.Plugins.MetadataConverters.Count()

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.MetadataConverters.WriteToCache(i, JSON.stringify(metadata))
	}

	// TODO: plugins aren't currently designed to be asynchronous.
}

function processMetadata(galleryId)
{
	var pluginCount = window.external.Plugins.MetadataProcessors.Count()

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.MetadataProcessors.Run(i, JSON.stringify(metadata))
	}

	// TODO: plugins aren't currently designed to be asynchronous.
}

//
// Misc.
//

function markPageThumbnailAsLoaded(img, button)
{
	if (button)
	{
		button.disabled = false
	}
}

function markPageThumbnailAsInvalid(img, button)
{
	if (button)
	{
		button.disabled = true
	}

	if (img)
	{
		img.onerror = null
		img.src = "assets/images/cover/200x200/missing.png"
	}
}

//
// Initialization
//

function createDownloadElements()
{
	var appDir = fileSystem.appDir
	var table = document.getElementById("page-links")
	var tbody = table.getElementsByTagName("TBODY")[0]

	var numPages = metadata.images.pages.length

	for (var i = 0; i < numPages; ++i)
	{
		var page = metadata.images.pages[i]

		var tr = document.createElement("TR")

		tr.id ="page-" + i
		tr.className ="page"

		var td = document.createElement("TD")

		td.id = "thumbnail-" + i.toString()
		td.className = "thumbnail"

		var span = document.createElement("SPAN")

		span.id = "thumbnail-span-" + i
		span.className = "thumbnail-span"

		var button = document.createElement("BUTTON")

		button.id = "thumbnail-button-" + i
		button.className = "thumbnail-button"
		button.disabled = true
		button.pageIndex = i

		button.addEventListener("contextmenu", function(e)
		{
			if (e.target && e.target.nodeName === "BUTTON")
			{
				ContextMenus.show(pageThumbnailContextMenu, e.target)
			}
		})

		var img = document.createElement("IMG")

		img.id = "thumbnail-image-" + i
		img.className = "thumbnail-image"
		img.disabled = true
		img.alt = (i + 1).toString()
		img.title = (i + 1).toString().padLeft(Cache.getBaseCount(numPages), "0") + Cache.getImageFileExtension(page)

		button.appendChild(img)
		span.appendChild(button)
		td.appendChild(span)
		tr.appendChild(td)

		td = document.createElement("TD")
		td.id = "checkbox-" + i.toString()
		td.className = "checkbox"

		var checkbox = document.createElement("INPUT")

		checkbox.type = "checkbox"
		checkbox.checked = ""	//"checked"
		checkbox.addEventListener("change", function(event)
		{
			event = event || window.event

			pageCheckedCount += event.target.checked ? 1 : -1

			onPageCheckedCountChanged()
		})
		td.appendChild(checkbox)
		tr.appendChild(td)

		td = document.createElement("TD")
		td.id = "index-" + i.toString()
		td.className = "index"
		span = document.createElement("SPAN")

		var txt = document.createTextNode((i + 1).toString())

		span.appendChild(txt)
		td.appendChild(span)
		tr.appendChild(td)

		td = document.createElement("TD")
		td.id = "progress-" + i.toString()
		td.className = "progress progress-display-inline"

		var progress = document.createElement("DIV")	// PROGRESS

		progress.className = "progress-container"
		span = document.createElement("DIV")
		span.className = "progress-fill"
		progress.appendChild(span)
		td.appendChild(progress)
		tr.appendChild(td)

		var pageProgress = Object.create(Progress)

		pageProgress.init({
			"element": td,
			autoHide: false
		})

		pageProgresses.push(pageProgress)

		td = document.createElement("TD")
		td.id = "status-" + i.toString()
		td.className = "status"
		div = document.createElement("DIV")
		span = document.createElement("SPAN")
		txt = document.createTextNode("")
		span.appendChild(txt)
		div.appendChild(span)
		td.appendChild(div)
		tr.appendChild(td)

		tbody.appendChild(tr)
	}
}

function onPageCheckedCountChanged()
{
	var isNoneSelected = (pageCheckedCount === 0)
	var isAllSelected = (pageCheckedCount === metadata.images.pages.length)

	var navSelect = document.getElementById("nav-select")
	var navSelectItems = navSelect.getElementsByTagName("BUTTON")

	for (var i = 0, len = navSelectItems.length; i < len; ++i)
	{
		navSelectItems[i].disabled = isAllSelected
	}

	var navDeselect = document.getElementById("nav-deselect")
	var navDeselectItems = navDeselect.getElementsByTagName("BUTTON")

	for (var i = 0, len = navDeselectItems.length; i < len; ++i)
	{
		navDeselectItems[i].disabled = isNoneSelected
	}

	var selectAllButton = document.getElementById("quickaction-select-all")

	selectAllButton.disabled = isAllSelected

	var deselectAllButton = document.getElementById("quickaction-deselect-all")

	deselectAllButton.disabled = isNoneSelected

	var downloadButton = document.getElementById("nav-download")

	downloadButton.disabled = isNoneSelected
}

function highlightTitles()
{
	var englishTitle = document.getElementById("title-english")

	englishTitle.innerHTML = titleFormatter.format(englishTitle.innerText, "english")
}

function validateNavigation()
{
	var doesPagesFolderExists = window.external.FileSystem.Path.DoesPagesFolderExists(metadata.id)
	var doesAnyPageExists = window.external.FileSystem.Path.AnyPageExists(metadata.id)
	var doesArchiveExists = window.external.FileSystem.Path.DoesComicBookArchiveExists(metadata.id)

	document.getElementById("nav-browse").addOrRemoveClass("display-none", !doesPagesFolderExists)
	document.getElementById("nav-read").addOrRemoveClass("display-none", !doesAnyPageExists)
	document.getElementById("nav-open-archive").addOrRemoveClass("display-none", !doesArchiveExists)

	document.getElementById("nav-commands").removeClass("display-none")
	document.getElementById("nav-select").removeClass("display-none")
	document.getElementById("nav-deselect").removeClass("display-none")

	onPageCheckedCountChanged()	// HACK
}

function initDocument()
{
	document.onpagerefresh = function(event)
	{
		window.external.Search.ShowDownload(metadata.id)
	}

	document.ondocumentcancel = function(event)
	{
		window.external.BackgroundWorkers.PageDownloader.Cancel(metadata.id)
	}
}

function initZoom()
{
	zoom.init(window.external.Browsers.Download.GetZoomRatio())

	zoom.onzoomchanged = function(ratio)
	{
		window.external.Browsers.Download.SetZoomRatio(ratio)

		// GUI elements shouldn't be scaleable.
		if (ratio)
		{
			coverContextMenu.element.style.zoom = (1.0 / ratio).toString()
		}
	}
}

function initCover()
{
	var cover = document.getElementById("cover")

	cover.addEventListener("contextmenu", function(e)
	{
		if (e.target && e.target.nodeName === "IMG")
		{
			ContextMenus.show(coverContextMenu, e.target)
		}
	})
}

function PageThumbnailContextMenu_OnBeforeOpenContextMenu(target)
{
	var doesPageExists = window.external.FileSystem.Path.DoesPageExists(metadata.id, target.pageIndex)

	document.getElementById("menu-command-open-page").disabled = !doesPageExists
	document.getElementById("menu-command-select-page").disabled = !doesPageExists

	return false
}

function validateCoverMenuItems()
{
	var doesCoverExists = window.external.FileSystem.Path.DoesCoverExists(metadata.id)

	document.getElementById("menu-command-open-cover").disabled = !doesCoverExists

	document.getElementById("menu-command-select-cover").disabled = !doesCoverExists
}

function validatePageDownloadMenuItems()
{
	var doesPagesFolderExists = window.external.FileSystem.Path.DoesPagesFolderExists(metadata.id)
	var doesAnyPageExists = window.external.FileSystem.Path.AnyPageExists(metadata.id)
	var doesArchiveExists = window.external.FileSystem.Path.DoesComicBookArchiveExists(metadata.id)

	document.getElementById("menu-command-browse").disabled = !doesPagesFolderExists
	document.getElementById("menu-command-read").disabled = !doesAnyPageExists
	document.getElementById("menu-command-open-archive").disabled = !doesArchiveExists

	document.getElementById("menu-command-select-pages").disabled = !doesPagesFolderExists
	document.getElementById("menu-command-select-anypage").disabled = !doesAnyPageExists
	document.getElementById("menu-command-select-archive").disabled = !doesArchiveExists
}

function CoverContextMenu_OnBeforeOpenContextMenu(target)
{
	var doesMetadataExists = window.external.FileSystem.Path.DoesMetadataExists(metadata.id)

	validateCoverMenuItems()
	validatePageDownloadMenuItems()

	document.getElementById("menu-command-select-metadata").disabled = !doesMetadataExists

	document.getElementById("menu-command-show-details").addOrRemoveClass("display-none", false)
	document.getElementById("menu-command-show-download").addOrRemoveClass("display-none", true)

	// HACK: interaction with background workers isn't fully implemented/tested yet, so always show/hide these two commands.
	document.getElementById("menu-command-download-all").addOrRemoveClass("display-none", false)
	document.getElementById("menu-command-download-cancel").addOrRemoveClass("display-none", true)

	document.getElementById("menu-command-plugin-archive-create").addOrRemoveClass("display-none", window.external.Plugins.ArchiveWriters.Count() === 0)
	document.getElementById("menu-command-plugin-metadata-convert").addOrRemoveClass("display-none", window.external.Plugins.MetadataConverters.Count() === 0)
	document.getElementById("menu-command-plugin-metadata-process").addOrRemoveClass("display-none", window.external.Plugins.MetadataProcessors.Count() === 0)

	document.getElementById("menu-command-select-cover").addOrRemoveClass("display-none",!showDebugCommands)
	document.getElementById("menu-command-select-metadata").addOrRemoveClass("display-none",!showDebugCommands)
	document.getElementById("menu-command-select-pages").addOrRemoveClass("display-none",!showDebugCommands)
	document.getElementById("menu-command-select-anypage").addOrRemoveClass("display-none",!showDebugCommands)
	document.getElementById("menu-command-select-archive").addOrRemoveClass("display-none",!showDebugCommands)

	return false
}

function DocumentContextMenu_OnBeforeOpenContextMenu(target)
{
	var y = document.documentElement.scrollTop
	var h = document.documentElement.scrollHeight - document.documentElement.clientHeight

	document.getElementById("menu-command-document-scroll-top").addOrRemoveClass("display-none", (y <= 0))
	document.getElementById("menu-command-document-scroll-bottom").addOrRemoveClass("display-none", (y >= h))

	return false
}

function initContextMenus()
{
	pageThumbnailContextMenu = Object.create(ContextMenu)
	pageThumbnailContextMenu.init({
		"name": "page-thumbnail",
		"horizontalAlignment": "cursor",
		"verticalAlignment": "cursor",
		"onbeforeopen": function(target)
		{
			PageThumbnailContextMenu_OnBeforeOpenContextMenu(target)
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

	ContextMenus.init(
	{
		"contextMenus": [ documentContextMenu, pageThumbnailContextMenu, coverContextMenu ]
	})

	document.body.addEventListener("contextmenu", function(e)
	{
		if (e.target.id !== "cover"
			&& e.target.tagName !== "BUTTON"
			//&& e.target.className !== "thumbnail"
			&& e.target.className !== "thumbnail-button")
		{
			ContextMenus.show(documentContextMenu, e.target)
		}
	})
}

function initProgress()
{
	totalProgress = Object.create(Progress)
	totalProgress.init({
		"element": document.getElementById("progress-total"),
		autoHide: false
	})

	subTotalProgress = Object.create(Progress)
	subTotalProgress.init({
		"element": document.getElementById("progress-subtotal"),
		autoHide: false
	})
}

function initCache()
{
	Cache.init(
		{
			"onCacheSearchIncomplete": function(metadata)
			{
				enableDownloadControls(true)
			},
			"onCacheSearchCompleted": function(metadata)
			{
				onDownloadCompleted(true)
			},
			"onCacheSearchFinished": function(metadata, cachedCount, totalPages)
			{
				//var progressPercentage = Math.ceil(cachedCount / totalPages * 100)
				
				//totalProgress.setValue(progressPercentage)
			},
			"onCachedPageFound": function(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath)
			{
				onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, 0, 0, "", true)
			}/*,
			"pageSelector": function(pageIndex)
			{
				var button = document.getElementById("thumbnail-button-" + pageIndex)

				return button && !button.disabled
			}*/
		})

	Cache.validateImageSources(metadata)
}

(function()
{
	console.create()
	Browser.init()
	initDocument()
	initProgress()
	initContextMenus()
	setButtonsContent(firstPageCount, "first")
	setButtonsContent(lastPageCount, "last")
	setButtonsContent(intervalPageCount, "interval")
	highlightTitles()
	self.setTimeout(function() { createDownloadElements(); initCache() }, 1)
	validateNavigation()
	initCover()
	initZoom()
})()
