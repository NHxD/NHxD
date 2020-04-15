{
	// require html elements with id named "search-result-item-{Id}"
	// require css "display-none"
	// require StringComparer.containsIgnoreCase

	var MetadataKeywordList = {}

	MetadataKeywordList.init = function()
	{
		// don't bother to precache elements - it'd be a waste of memory.
	}

	MetadataKeywordList.applyToElement = function(metadata, button)
	{
		if (!button)
		{
			return
		}

		if (this.isInList(whitelist, metadata))
		{
			button.addClass("tag-whitelist")
		}
		else if (this.isInList(blacklist, metadata))
		{
			button.addClass("tag-blacklist")
		}

		if (this.isInList(ignorelist, metadata))
		{
			button.addClass("tag-ignorelist")
		}

		if (this.isInList(hidelist, metadata))
		{
			button.addClass("tag-hidelist")
		}
	}

	MetadataKeywordList.applyToSearchResult = function(searchResult, elementSelector)
	{
		for (var i = 0; i < searchResult.result.length; ++i)
		{
			var metadata = searchResult.result[i]

			this.applyToElement(metadata, elementSelector(i))
		}
	}

	MetadataKeywordList.isInList = function(_list, _metadata)
	{
		// NOTE: IE7 doesn't support object property enumeration (i.e., for (var prop in object)).
		var props =
		[
			[ _list.title.pretty, _metadata.title.pretty ],
			[ _list.title.english, _metadata.title.english ],
			[ _list.title.japanese, _metadata.title.japanese ],
			[ _list.scanlator, _metadata.scanlator ],
			[ _list.upload_date, _metadata.upload_date ],
			[ _list.num_pages, _metadata.num_pages ],
			[ _list.num_favorites, _metadata.num_favorites ],
			[ _list.tags.tag, _metadata.tags.where(function(x) { return x.type === "tag" }).select(function(x) { return x.name }) ],
			[ _list.tags.artist, _metadata.tags.where(function(x) { return x.type === "artist" }).select(function(x) { return x.name }) ],
			[ _list.tags.group, _metadata.tags.where(function(x) { return x.type === "group" }).select(function(x) { return x.name }) ],
			[ _list.tags.parody, _metadata.tags.where(function(x) { return x.type === "parody" }).select(function(x) { return x.name }) ],
			[ _list.tags.character, _metadata.tags.where(function(x) { return x.type === "character" }).select(function(x) { return x.name }) ],
			[ _list.tags.category, _metadata.tags.where(function(x) { return x.type === "category" }).select(function(x) { return x.name }) ],
			[ _list.tags.language, _metadata.tags.where(function(x) { return x.type === "language" }).select(function(x) { return x.name }) ],
		]

		for (var i = 0, len = props.length; i < len; ++i)
		{
			var kvp = props[i]

			if (kvp[0].isInside(kvp[1], function(x, y) { return StringComparer.containsIgnoreCase(x)(y) }))
			{
				return true
			}
		}
		
		return false
	}

	MetadataKeywordList.synchronizeList = function(eventType, fieldType, fieldValue, list)
	{
		var mappings =
		[
			{ type: "language", target: list.tags.language },
			{ type: "artist", target: list.tags.artist },
			{ type: "group", target: list.tags.group },
			{ type: "tag", target: list.tags.tag },
			{ type: "parody", target: list.tags.parody },
			{ type: "character", target: list.tags.character },
			{ type: "category", target: list.tags.category },

			{ type: "english", target: list.title.english },
			{ type: "japanese", target: list.title.japanese },
			{ type: "pretty", target: list.title.pretty },

			{ type: "scanlator", target: list.scanlator },
			{ type: "upload_date", target: list.upload_date },
			{ type: "num_pages", target: list.num_pages },
			{ type: "num_favorites", target: list.num_favorites },
			{ type: "id", target: list.id },
			{ type: "media_id", target: list.media_id }
		]

		var mapping = mappings.first(function(x) { return x.type === fieldType })

		if (mapping && mapping.target)
		{
			if (eventType === "remove")
			{
				var idx = mapping.target.indexOf(fieldValue)

				if (idx !== -1)
				{
					mapping.target.splice(idx, 1)
				}
			}
			else if (eventType === "add")
			{
				mapping.target.push(fieldValue)
			}
		}
	}
}
