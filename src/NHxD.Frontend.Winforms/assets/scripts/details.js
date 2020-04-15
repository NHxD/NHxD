var showDebugCommands = false
var documentContextMenu
var coverContextMenu
var tagContextMenu
var totalProgress
var subTotalProgress

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

	setCover(metadata, coverPath, error)
}

function setCover(metadata, coverPath, error)
{
	var img = document.getElementById("cover")

	if (img)
	{
		if (error)
		{
			img.onerror = null
			img.src = "assets/images/cover/200x200/missing.png"
			img.title = error
		}
		else
		{
			img.src = coverPath
		}

		// force an update of some context menu items
		if (!coverContextMenu.element.hasClass("display-none"))
		{
			validateCoverMenuItems()
		}
	}
	/*
	var loading = document.getElementById("cover-loading")

	if (loading)
	{
		loading.addClass("display-none")
	}
	*/
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

	subTotalProgress.hide()
}

function __onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error)
{
	return onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, false)
}

function onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, downloadedBytes, downloadSize, error, isCacheSearch)
{
	if (galleryId !== metadata.id)
	{
		return
	}

	validateNavigation()

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
// Userlists
//

function __onWhitelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, whitelist)
	applyCustomTagStyle(fieldType, -1, fieldValue)
	highlightTitles()
}

function __onBlacklistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, blacklist)
	applyCustomTagStyle(fieldType, -1, fieldValue)
	highlightTitles()
}

function __onIgnorelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, ignorelist)
	applyCustomTagStyle(fieldType, -1, fieldValue)
	highlightTitles()
}

function __onHidelistChanged(eventType, fieldType, fieldValue)
{
	MetadataKeywordList.synchronizeList(eventType, fieldType, fieldValue, hidelist)
	applyCustomTagStyle(fieldType, -1, fieldValue)
	highlightTitles()
}


function addTagToWhitelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Whitelist.AddTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function addTagToBlacklist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Blacklist.AddTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function addTagToIgnorelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Ignorelist.AddTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function addTagToHidelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Hidelist.AddTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function removeTagFromWhitelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Whitelist.RemoveTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function removeTagFromBlacklist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Blacklist.RemoveTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function removeTagFromIgnorelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Ignorelist.RemoveTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

function removeTagFromHidelist()
{
	var tagType = tagContextMenu.target._tag.type
	var tagName = tagContextMenu.target._tag.name
	var tagId = tagContextMenu.target._tag.id

	window.external.MetadataKeywordLists.Hidelist.RemoveTag(tagType, tagName, tagId)

	tagContextMenu.close()
}

//
// Commands
//

function onCoverClicked()
{
	if (window.external.Browsers.Details.GetReloadPageOnCoverClicked())
	{
		window.external.Search.ShowDetails(metadata.id)
	}
}

function showTagDefinition()
{
	var tagName = tagContextMenu.target._tag.name

	window.external.Dictionary.ShowTagDefinition(tagName)
}

function addTaggedBookmark()
{
	var tagId = tagContextMenu.target._tag.id

	window.external.Bookmark.ShowAddTaggedBookmarkPrompt(tagId, 1)
}

function createArchive(galleryId)
{
	var pluginCount = window.external.Plugins.ArchiveWriters.Count()

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.ArchiveWriters.Create(i, galleryId)
	}

	validateNavigation()
}

function convertMetadata(galleryId)
{
	var pluginCount = window.external.Plugins.MetadataConverters.Count()
	var metadataJson = JSON.stringify(metadata)

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.MetadataConverters.WriteToCache(i, metadataJson)
	}
}

function processMetadata(galleryId)
{
	var pluginCount = window.external.Plugins.MetadataProcessors.Count()
	var metadataJson = JSON.stringify(metadata)

	for (var i = 0; i < pluginCount; ++i)
	{
		window.external.Plugins.MetadataProcessors.Run(i, metadataJson)
	}
}

//
// Initialization
//

function TagContextMenu_OnBeforeOpenContextMenu(target)
{
	var tagType = target._tag.type
	var tagName = target._tag.name
	var isInWhitelist = whitelist.tags[tagType].indexOf(tagName) !== -1
	var isInBlacklist = blacklist.tags[tagType].indexOf(tagName) !== -1
	var isInIgnorelist = ignorelist.tags[tagType].indexOf(tagName) !== -1
	var isInHidelist = hidelist.tags[tagType].indexOf(tagName) !== -1

	document.getElementById("menu-command-remove-whitelist").addOrRemoveClass("display-none", !isInWhitelist)
	document.getElementById("menu-command-remove-blacklist").addOrRemoveClass("display-none", !(isInBlacklist))
	document.getElementById("menu-command-remove-ignorelist").addOrRemoveClass("display-none", !(isInIgnorelist))
	document.getElementById("menu-command-remove-hidelist").addOrRemoveClass("display-none", !(isInHidelist))
	document.getElementById("menu-command-add-whitelist").addOrRemoveClass("display-none", !(!isInWhitelist && !isInBlacklist))
	document.getElementById("menu-command-add-blacklist").addOrRemoveClass("display-none", !(!isInBlacklist && !isInWhitelist))
	document.getElementById("menu-command-add-ignorelist").addOrRemoveClass("display-none", !(!isInIgnorelist))
	document.getElementById("menu-command-add-hidelist").addOrRemoveClass("display-none", !(!isInHidelist))
	document.getElementById("menu-command-add-tag-bookmark").addOrRemoveClass("display-none", false)
	document.getElementById("menu-command-show-definition").addOrRemoveClass("display-none", !(tagType === "tag"))

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

	document.getElementById("menu-command-show-details").addOrRemoveClass("display-none", true)
	document.getElementById("menu-command-show-download").addOrRemoveClass("display-none", false)

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

function applyCustomTagStyle(tagType, tagId, tagName, button)
{
	var tags = metadata.tags.first(function(x) { return x.type === tagType && x.name === tagName })

	if (!tags)
	{
		return
	}

	// HACK: override tag id...
	tagId = tags.id

	button = button || document.getElementById("tag-button-" + tagId)

	if (!button)
	{
		return
	}

	//MetadataKeywordList.applyToElement(metadata, button)

	var isInWhitelist = whitelist.tags[tagType].indexOf(tagName) !== -1
	var isInBlacklist = blacklist.tags[tagType].indexOf(tagName) !== -1
	var isInIgnorelist = ignorelist.tags[tagType].indexOf(tagName) !== -1
	var isInHidelist = hidelist.tags[tagType].indexOf(tagName) !== -1

	button.addOrRemoveClass("tag-whitelist", isInWhitelist)
	button.addOrRemoveClass("tag-blacklist", isInBlacklist)
	button.addOrRemoveClass("tag-ignorelist", isInIgnorelist)
	button.addOrRemoveClass("tag-hidelist", isInHidelist)
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

function createTagElements()
{
	var tagGroups =
	[
		{ name: "Languages", type: "language", category: "m" },
		{ name: "Artists", type: "artist", category: "m" },
		{ name: "Groups", type: "group", category: "m" },
		{ name: "Tags", type: "tag", category: "m" },
		{ name: "Parodies", type: "parody", category: "m" },
		{ name: "Characters", type: "character", category: "m" },
		{ name: "Categories", type: "category", category: "m" },
		{ name: "Scanlator", type: "scanlator", value: metadata.scanlator, category: "s" },
		{ name: "Upload Date", type: "upload_date", value: metadata.upload_date, category: "s", formatter: function(x) { return (new Date(x * 1000)).toLocaleDateString() } },
		{ name: "Pages", type: "num_pages", value: metadata.num_pages, category: "s", formatter: function(x) { return Number(x).toLocaleString().replace(".00", "") } },
		{ name: "Favorites", type: "num_favorites", value: metadata.num_favorites, category: "s", formatter: function(x) { return Number(x).toLocaleString().replace(".00", "") } },
		{ name: "Id", type: "id", value: metadata.id, category: "s" },
		{ name: "Media Id", type: "media_id", value: metadata.media_id, category: "s" }
	]

	var table = document.getElementById("tags")
	var tbody = table.getElementsByTagName("TBODY")[0]

	for (var i = 0; i < tagGroups.length; ++i)
	{
		var tagGroup = tagGroups[i]

		var tr = document.createElement("TR")

		tr.id = "tag-row-" + tagGroup.type
		tr.className = "tag-row"

		var td = document.createElement("TD")

		td.id = "tag-type-" + tagGroup.type
		td.className = "tag-type"

		var span = document.createElement("SPAN")
		var txt = document.createTextNode(tagGroup.name)

		span.appendChild(txt)
		td.appendChild(span)
		tr.appendChild(td)

		td = document.createElement("TD")
		td.id = "tag-value-" + tagGroup.type
		td.className = "tag-value"
		span = document.createElement("UL")
		span.id = "tag-group-" + tagGroup.type
		span.className = "tag-group"

		if (tagGroup.category === "m")
		{
			var lastChild = null

			for (var j = 0, metadata_tags_length = metadata.tags.length; j < metadata_tags_length; ++j)
			{
				var tag = metadata.tags[j]

				if (tag.type !== tagGroup.type)
				{
					continue
				}

				var container = document.createElement("LI")

				container.addClass("tag-container")
				container.addClass("tag-container-" + tag.id)
				container.addClass("tag-container-type-" + tag.type)

				container.addEventListener("contextmenu", function(e)
				{
					if (e.target && e.target.nodeName === "BUTTON")
					{
						ContextMenus.show(tagContextMenu, e.target)
					}
				})

				var div = document.createElement("SPAN")

				div.addClass("tag-button-container")

				var button = document.createElement("BUTTON")

				button.id = "tag-button-" + tag.id
				button.addClass("tag-button")
				button.addClass("tag-button-" + tag.id)
				button.addClass("tag-button-type-" + tag.type)

				button.title = tag.count + (tag.count === 1 ? " gallery" : " galleries")
				button._tag = tag

				button.addEventListener("click", function(e)
				{
					if (window.external.Settings.ShouldBlockBlacklistActions())
					{
						var isInBlacklist = blacklist.tags[this._tag.type].indexOf(this._tag.name) !== -1

						if (isInBlacklist)
						{
							var dialogResult = window.confirm("The tag \"" + this._tag.name + "\" is currently blacklisted. Are you sure you want to open this link?")

							if (!dialogResult)
							{
								return
							}
						}
					}

					window.external.Search.RunTaggedSearch(this._tag.id, 1)
				})

				var txt = document.createTextNode(tag.name)
				button.appendChild(txt)

				div.appendChild(button)
				container.appendChild(div)
				span.appendChild(container)

				applyCustomTagStyle(tag.type, tag.id, tag.name, button)

				lastChild = container
			}

			if (lastChild != null)
			{
				lastChild.addClass("tag-container-last")
			}
		}
		else if (tagGroup.category === "s")
		{
			var container = document.createElement("LI")

			container.id = "tag-container-" + tag.id
			container.className = "tag-container tag-container-type-" + tagGroup.type

			var val = tagGroup.value

			if (tagGroup.formatter)
			{
				val = tagGroup.formatter(val)
			}
			txt = document.createTextNode(val)
			container.appendChild(txt)
			span.appendChild(container)
		}

		td.appendChild(span)
		tr.appendChild(td)

		tbody.appendChild(tr)
	}
}

function highlightTitles()
{
	var prettyTitle = document.getElementById("title-pretty")
	var englishTitle = document.getElementById("title-english")
	var japaneseTitle = document.getElementById("title-japanese")

	prettyTitle.innerHTML = titleFormatter.format(prettyTitle.innerText, "pretty")
	englishTitle.innerHTML = titleFormatter.format(englishTitle.innerText, "english")
	japaneseTitle.innerHTML = titleFormatter.format(japaneseTitle.innerText, "japanese")
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
}

function initDocument()
{
	document.onpagerefresh = function(event)
	{
		window.external.Search.ShowDetails(metadata.id)
	}
}

function initZoom()
{
	zoom.init(window.external.Browsers.Details.GetZoomRatio())

	zoom.onzoomchanged = function(ratio)
	{
		window.external.Browsers.Details.SetZoomRatio(ratio)

		// GUI elements shouldn't be scaleable.
		if (ratio)
		{
			tagContextMenu.element.style.zoom = (1.0 / ratio).toString()
			coverContextMenu.element.style.zoom = (1.0 / ratio).toString()
		}
	}
}

function initContextMenus()
{
	tagContextMenu = Object.create(ContextMenu)
	tagContextMenu.init({
		"name": "tag",
		"horizontalAlignment": "cursor",
		"verticalAlignment": "bottom",
		"horizontalSide": "inside",
		"verticalSide": "outside",
		"onbeforeopen": function(target)
		{
			TagContextMenu_OnBeforeOpenContextMenu(target)
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
		"contextMenus": [ documentContextMenu, tagContextMenu, coverContextMenu ]
	})

	document.body.addEventListener("contextmenu", function(e)
	{
		if (e.target.id !== "cover"
			&& e.target.tagName !== "BUTTON"
			/*&& e.target.className.indexOf("tag-container") !== -1*/
			)
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
			"onCacheSearchCompleted": function(metadata)
			{

			},
			"onCacheSearchFinished": function(metadata, cachedCount, totalPages)
			{
				var progressPercentage = Math.ceil(cachedCount / totalPages * 100)
				
				totalProgress.setValue(progressPercentage)
			},
			"onCachedPageFound": function(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath)
			{
				onPageDownloadReportProgress(loadCount, loadTotal, cacheCount, pageCount, pageIndex, galleryId, pagePath, 0, 0, "", true)
			}
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
	highlightTitles()
	createTagElements()
	validateNavigation()
	initCover()
	initCache()
	initZoom()
})()
