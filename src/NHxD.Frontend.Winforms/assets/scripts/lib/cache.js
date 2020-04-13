{
	// require JSON.stringify

	var Cache = {}

	Cache.onCachedPageFound = null
	Cache.onCacheSearchFinished = null
	Cache.onCacheSearchCompleted = null
	Cache.onCacheSearchIncomplete = null
	Cache.onCachedCoverFound = null
	Cache.onCachedCoversSearchFinished = null
	Cache.pageSelector = null

	Cache.init = function(props)
	{
		var propNames =
		[
			"onCachedPageFound",
			"onCacheSearchCompleted",
			"onCacheSearchIncomplete",
			"onCacheSearchFinished",
			"onCachedCoverFound",
			"onCachedCoversSearchFinished",
			"pageSelector"
		]

		for (var i = 0; i < propNames.length; ++i)
		{
			var propName = propNames[i]

			if (typeof props[propName] !== "undefined")
			{
				this[propName] = props[propName]
			}
		}

		if (!this.element)
		{
			return
		}
	}

	Cache.validateAllImageSources = function(searchResult)
	{
		for (var i = 0, len = searchResult.result.length; i < len; ++i)
		{
			var metadata = searchResult.result[i]

			this.validateImageSources(metadata)
		}
	}

	Cache.validateImageSources = function(metadata)
	{
		if (!window.external.FileSystem.Path.DoesPagesFolderExists(metadata.id))
		{
			return
		}

		var metadataJson = window.external.FileSystem.Path.Formatter.IsEnabled() ? JSON.stringify(metadata) : null
		var cachedPageIndices = window.external.FileSystem.Path.GetCachedPageIndices(metadata.id).split(" ")
		var numPages = metadata.images.pages.length
		var cacheCount = 0

		for (var i = 0; i < numPages; ++i)
		{
			if (this.pageSelector)
			{
				if (!this.pageSelector(i))
				{
					continue
				}
			}

			var page = metadata.images.pages[i]
			var cachedFilePath
			
			if (window.external.FileSystem.Path.Formatter.IsEnabled())
			{
				cachedFilePath = window.external.FileSystem.Path.Formatter.GetPage(metadataJson, i)
			}
			else
			{
				cachedFilePath = window.external.FileSystem.Path.Formatter.GetCacheDirectory() + metadata.id + "/" + (i + 1).toString().padLeft(this.getBaseCount(numPages), '0') + this.getImageFileExtension(page)
			}

			if (cachedPageIndices.indexOf((i + 1).toString()) !== -1)
			{
				++cacheCount

				if (this.onCachedPageFound)
				{
					this.onCachedPageFound(i + 1, cachedPageIndices.length, cacheCount, numPages, i, metadata.id, cachedFilePath)
				}
			}
		}

		if (this.onCacheSearchFinished)
		{
			this.onCacheSearchFinished(metadata, cachedPageIndices.length, numPages)
		}
		
		if (cachedPageIndices.length === numPages)
		{
			if (this.onCacheSearchCompleted)
			{
				this.onCacheSearchCompleted(metadata)
			}
		}
		else
		{
			if (this.onCacheSearchIncomplete)
			{
				this.onCacheSearchIncomplete(metadata)
			}
		}
	}

	Cache.validateAllCoverImageSources = function(searchResult)
	{
		for (var i = 0, len = searchResult.result.length; i < len; ++i)
		{
			var metadata = searchResult.result[i]

			this.validateCoverImageSource(metadata)
		}
	}

	Cache.validateCoverImageSource = function(metadata)
	{
		var cachedFilePath

		if (window.external.FileSystem.Path.Formatter.IsEnabled())
		{
			var metadataJson = JSON.stringify(metadata)

			cachedFilePath = window.external.FileSystem.Path.Formatter.GetCover(metadataJson)
		}
		else
		{
			cachedFilePath = window.external.FileSystem.Path.Formatter.GetCacheDirectory() + metadata.id + this.getImageFileExtension(metadata.images.cover)
		}

		if (window.external.FileSystem.File.Exists(cachedFilePath))
		{
			if (this.onCachedCoverFound)
			{
				this.onCachedCoverFound(metadata, cachedFilePath)
			}
		}
	}

	Cache.getBaseCount = function(count)
	{
		return (count < 10) ? 1
			: (count < 100) ? 2
			: (count < 1000) ? 3 : 4
	}

	Cache.getImageFileExtension = function(image)
	{
		return (image.t === 'j') ? ".jpg"
			: (image.t === 'p') ? ".png"
			: (image.t === 'g') ? ".gif" : ""
	}
}
