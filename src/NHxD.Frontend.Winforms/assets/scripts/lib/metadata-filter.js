{
	// require html elements with id named "search-result-item-{Id}"
	// require css "display-none"

	// NOTE: this could be optimized by caching results (like extracted comiket or scanlator info)
	// TODO: it'd be good to allow statements to be quoted.

	var MetadataFilter = {}

	// NOTE: for now, it is a constant, but maybe later, allow the user to change this at runtime.
	MetadataFilter.ignoreCase = true

	MetadataFilter.statementSeparators = [ ',', ';' ]
	// i.e., /[^,;]+|,|;/g
	MetadataFilter.statementSeparatorRegex = new RegExp("[^" + MetadataFilter.statementSeparators.join('') + "]+|" + MetadataFilter.statementSeparators.join('|'), "g")
	MetadataFilter.conditionSeparatorRegex = /([^|&]+|\||&+)/g

	// lazy. used mainly for quick debugging.
	MetadataFilter.whitelist = whitelist
	MetadataFilter.blacklist = blacklist
	MetadataFilter.ignorelist = ignorelist
	MetadataFilter.hidelist = hidelist

	MetadataFilter.tagNames =
	[
		"tag",
		"artist",
		"parody",
		"character",
		"group",
		"category",
		"language"
	]

	MetadataFilter.titlePartNames =
	[
		"comiket",
		"version",
		"censorship"
	]

	MetadataFilter.filter = function(metadata, filter)
	{
		var result = true

		if (filter && this.statementSeparatorRegex.test(filter))
		{
			var statements = filter.match(this.statementSeparatorRegex)

			for (var i = 0; i < statements.length; ++i)
			{
				var statement = statements[i].trim(' ')

				if (this.statementSeparators.any(function(x) { return x === statement }))
				{
					continue
				}

				var kvp = statement.split(':', 2)
				var propertyFilter = kvp[0]

				if (kvp.length < 2)
				{
					//self.alert("syntax error:\r\na property and its value must be separated by a colon\r\n\r\n(e.g., tag:value)")
					break
				}

				if (!this.conditionSeparatorRegex.test(kvp[1]))
				{
					continue
				}

				var conditions = kvp[1].match(this.conditionSeparatorRegex)

				if (!conditions)
				{
					continue
				}

				var conditionOperator

				if (conditions.length > 1)
				{
					var condition = conditions[1].trim(' ')

					if (condition === '&')
					{
						conditionOperator = condition
					}
					else if (condition === '|')
					{
						conditionOperator = condition
					}
				}

				for (var j = 0; j < conditions.length; ++j)
				{
					var condition = conditions[j].trim(' ')

					if (condition === '&')
					{
						conditionOperator = condition
					}
					else if (condition === '|')
					{
						conditionOperator = condition
					}
					else
					{
						result = this.evalItem(metadata, propertyFilter, condition)

						if (!result && conditionOperator === '&')
						{
							break
						}
						else if (result && conditionOperator === '|')
						{
							break
						}
					}
				}
			}
		}

		var item = document.getElementById("search-result-item-" + metadata.id)

		if (item)
		{
			item.addOrRemoveClass("filter", !result)

			if (result)
			{
				// NOTE: force recalculation of the image height because it's bugged on win10/Edge.
				var img = document.getElementById("item-cover-" + metadata.id)

				if (img)
				{
					var cover = metadata.images.cover

					if (cover.w > 0)
					{
						var adjustedHeight = cover.h * img.width / cover.w

						img.style.height = adjustedHeight + "px"
					}
				}
			}
		}
	}

	MetadataFilter.parseTextCondition = function(condition, ignoreCase)
	{
		if (condition.startsWith('^'))
		{
			condition = condition.substr(1)

			if (ignoreCase)
			{
				return StringComparer.startsWithIgnoreCase(condition)
			}
			else
			{
				return StringComparer.startsWith(condition)
			}
		}
		else if (condition.startsWith('$'))
		{
			condition = condition.substr(1)

			if (ignoreCase)
			{
				return StringComparer.endsWithIgnoreCase(condition)
			}
			else
			{
				return StringComparer.endsWith(condition)
			}
		}
		else if (condition.startsWith('~'))
		{
			condition = condition.substr(1)

			if (ignoreCase)
			{
				return StringComparer.containsIgnoreCase(condition)
			}
			else
			{
				return StringComparer.contains(condition)
			}
		}
		else
		{
			if (ignoreCase)
			{
				return StringComparer.equalsIgnoreCase(condition)
			}
			else
			{
				return StringComparer.equals(condition)
			}
		}
	}

	MetadataFilter.parseNumberCondition = function(condition)
	{
		if (condition.startsWith('<='))
		{
			condition = condition.substr(2)

			return NumberComparer.lessThanOrEquals(parseInt(condition))
		}
		else if (condition.startsWith('<'))
		{
			condition = condition.substr(1)

			return NumberComparer.lessThan(parseInt(condition))
		}
		else if (condition.startsWith('>='))
		{
			condition = condition.substr(2)

			return NumberComparer.greaterThan(parseInt(condition))
		}
		else if (condition.startsWith('>'))
		{
			condition = condition.substr(1)

			return NumberComparer.greaterThanOrEquals(parseInt(condition))
		}
		else
		{
			return NumberComparer.equals(parseInt(condition))
		}
	}

	MetadataFilter.parseBooleanCondition = function(condition)
	{
		return BooleanComparer.equals(BooleanComparer.parseBool(condition))
	}

	MetadataFilter.relativeRegex = /^\s*(\d+|last|l|past|p|t|this)\s*(\w+)\s*(ago)?$/i
	MetadataFilter.yRegex = /^\s*(\d+)\s*$/gi
	MetadataFilter.ymRegex = /^\s*(\d+)\D+(\d+)\s*$/gi
	MetadataFilter.ymdRegex = /^\s*(\d+)\D+(\d+)\D+(\d+)\s*$/gi
	MetadataFilter.dateSeparator = "-/\\ ."

	MetadataFilter.parseDate = function(text)
	{
		var m
		var d = null

		if (text === "today")
		{
			d = new Date()
			d.setDate(d.getDate())
			d.hasYear = true
			d.hasMonth = true
			d.hasDate = true
		}
		else if (text === "yesterday")
		{
			d = new Date()
			d.setDate(d.getDate() - 1)
			d.hasYear = true
			d.hasMonth = true
			d.hasDate = true
		}
		else if (m = text.match(MetadataFilter.relativeRegex))
		{
			var a = m[1].toLowerCase()
			var b = m[2].toLowerCase()
			var n = a.startsWith("l") ? 1 : parseInt(a)

			if (b.startsWith("l"))
			{
				n = 1
			}
			else if (b.startsWith("t"))
			{
				n = 0
			}
			else
			{
				n = parseInt(a)
			}

			if (b.startsWith("y"))
			{
				d = new Date()
				d.setUTCFullYear(d.getUTCFullYear() - n)
				d.hasYear = true
			}
			else if (b.startsWith("m"))
			{
				d = new Date()
				d.setUTCFullYear(d.getUTCFullYear(), d.getUTCMonth() - n)
				d.hasYear = true
				d.hasMonth = true
			}
			else if (b.startsWith("d"))
			{
				d = new Date()
				d.setUTCFullYear(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate() - n)
				d.hasYear = true
				d.hasMonth = true
				d.hasDate = true
			}
			/*
			else if (b.startsWith("w"))
			{
				n = 6
				d = new Date()
				d.setUTCFullYear(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate() - n)
				d.hasYear = true
				d.hasMonth = true
				d.hasDate = true
			}
			*/
		}
		else if (m = text.match(MetadataFilter.yRegex))
		{
			var yy = parseInt(m[1])

			d = new Date()
			d.setUTCFullYear(yy)
			d.hasYear = true
		}
		else if (m = text.match(MetadataFilter.ymRegex))
		{
			var yy = parseInt(m[1])
			var mm = parseInt(m[2])

			d = new Date()
			d.setUTCFullYear(yy, mm - 1)
			d.hasYear = true
			d.hasMonth = true
		}
		else if (m = text.match(MetadataFilter.ymdRegex))
		{
			var yy = parseInt(m[1])
			var mm = parseInt(m[2])
			var dd = parseInt(m[3])

			d = new Date()
			d.setUTCFullYear(yy, mm - 1, dd)
			d.hasYear = true
			d.hasMonth = true
			d.hasDate = true

			return d
		}

		if (d == null)
		{
			d = new Date(text)
		}

		return d
	}

	MetadataFilter.parseDateCondition = function(condition)
	{
		if (condition.startsWith('<='))
		{
			condition = condition.substr(2)

			return DateComparer.lessThanOrEquals(this.parseDate(condition))
		}
		else if (condition.startsWith('<'))
		{
			condition = condition.substr(1)

			return DateComparer.lessThan(this.parseDate(condition))
		}
		else if (condition.startsWith('>='))
		{
			condition = condition.substr(2)

			return DateComparer.greaterThanOrEquals(this.parseDate(condition))
		}
		else if (condition.startsWith('>'))
		{
			condition = condition.substr(1)

			return DateComparer.greaterThan(this.parseDate(condition))
		}
		else
		{
			return DateComparer.equals(this.parseDate(condition))
		}
	}

	MetadataFilter.evalItem = function(metadata, propertyFilter, condition)
	{
		var reverseResult = condition.startsWith('!')

		if (reverseResult)
		{
			condition = condition.substr(1)
		}

		if (!condition)
		{
			return false
		}

		var ignoreCase = this.ignoreCase
		var result = false

		if (this.tagNames.any(function(x) { return x === propertyFilter }))
		{
			var compare = this.parseTextCondition(condition, ignoreCase)
			var tags = metadata.tags
				.where(function(x) { return x.type === propertyFilter })
				.select(function(x) { return (x.name) })

			result = tags.any(compare)
		}
		else if (metadata.hasOwnProperty(propertyFilter)
			&& metadata.length)
		{
			var compare = this.parseTextCondition(condition, ignoreCase)

			result = metadata[propertyFilter].any(compare)
		}
		else if (propertyFilter === "title")
		{
			var compare = this.parseTextCondition(condition, ignoreCase)

			result = compare(metadata.title.english)
			|| compare(metadata.title.japanese)
		}
		else if (propertyFilter === "scanlator")
		{
			// TODO: for some reason, nhentai always return an empty "scanlator" property, so try to extract this value from the english title property instead.
		}
		else if (propertyFilter === "date")
		{
			var compare = this.parseDateCondition(condition)

			result = compare(new Date(metadata.upload_date * 1000))
		}
		else if (propertyFilter === "pages")
		{
			var compare = this.parseNumberCondition(condition)

			result = compare(metadata.num_pages)
		}
		else if (propertyFilter === "cachedpages")
		{
			var compare = this.parseNumberCondition(condition)
			var cachedPageIndices = window.external.FileSystem.Path.GetCachedPageIndices(metadata.id).split(" ")

			result = compare(cachedPageIndices.length)
		}
		else if (propertyFilter === "cached")
		{
			var compare = this.parseBooleanCondition(condition)
			var cachedPageIndices = window.external.FileSystem.Path.GetCachedPageIndices(metadata.id).split(" ")

			result = compare(metadata.num_pages === cachedPageIndices.length)
		}
		else if (propertyFilter === "favorites")
		{
			var compare = this.parseNumberCondition(condition)

			result = compare(metadata.num_favorites)
		}
		else if (propertyFilter === "id")
		{
			var compare = this.parseNumberCondition(condition)

			result = compare(metadata.id.toString())
		}
		else if (propertyFilter === "media_id")
		{
			var compare = this.parseNumberCondition(condition)

			result = compare(metadata.media_id.toString())
		}
		else if (propertyFilter === "whitelist" && typeof MetadataKeywordList !== "undefined")
		{
			var compare = this.parseBooleanCondition(condition)

			result = compare(MetadataKeywordList.isInList(this.whitelist, metadata))
		}
		else if (propertyFilter === "blacklist" && typeof MetadataKeywordList !== "undefined")
		{
			var compare = this.parseBooleanCondition(condition)

			result = compare(MetadataKeywordList.isInList(this.blacklist, metadata))
		}
		else if (propertyFilter === "ignorelist" && typeof MetadataKeywordList !== "undefined")
		{
			var compare = this.parseBooleanCondition(condition)

			result = compare(MetadataKeywordList.isInList(this.ignorelist, metadata))
		}
		else if (propertyFilter === "hidelist" && typeof MetadataKeywordList !== "undefined")
		{
			var compare = this.parseBooleanCondition(condition)

			result = compare(MetadataKeywordList.isInList(this.hidelist, metadata))
		}
		else if (this.titlePartNames.any(function(x) { return x === propertyFilter }))
		{
			// TODO: compare comiket, version, digital, censorship, etc.
		}

		if (reverseResult)
		{
			result = !result
		}

		return result
	}
}

// NOTE: misnomers - these classes test for equality instead of comparison.
{

	var StringComparer = {}

	StringComparer.equals = function(y) { return function(x) { return x == y } }
	StringComparer.startsWith = function(y) { return function(x) { return x && y && x.toString().startsWith(y.toString()) } }
	StringComparer.endsWith = function(y) { return function(x) { return x && y && x.toString().endsWith(y.toString()) } }
	StringComparer.contains = function(y) { return function(x) { return x && y && x.toString().indexOf(y.toString()) !== -1 } }
	StringComparer.equalsIgnoreCase = function(y) { return function(x) { return x && y && x.toString().toUpperCase() == y.toString().toUpperCase() } }
	StringComparer.startsWithIgnoreCase = function(y) { return function(x) { return x && y && x.toString().toUpperCase().startsWith(y.toString().toUpperCase()) } }
	StringComparer.endsWithIgnoreCase = function(y) { return function(x) { return x && y && x.toString().toUpperCase().endsWith(y.toString().toUpperCase()) } }
	StringComparer.containsIgnoreCase = function(y) { return function(x) { return x && y && x.toString().toUpperCase().indexOf(y.toString().toUpperCase()) !== -1 } }
}

{
	var BooleanComparer = {}

	BooleanComparer.equals = function(y) { return function(x) { return x == y } }

	BooleanComparer.parseBool = function(value)
	{
		value = value.toLowerCase()

		return value === "1"
			|| value === "on"
			|| value === "yes"
			|| value === "true"
	}
}

{
	var NumberComparer = {}

	NumberComparer.equals = function(y) { return function(x) { return x == y } }
	NumberComparer.lessThan = function(y) { return function(x) { return x < y } }
	NumberComparer.greaterThan = function(y) { return function(x) { return x > y } }
	NumberComparer.lessThanOrEquals = function(y) { return function(x) { return x <= y } }
	NumberComparer.greaterThanOrEquals = function(y) { return function(x) { return x >= y } }
}

{
	var DateComparer = {}

	DateComparer.equals = function(y) { var that = this; return function(x) { return that.compareDate(x, y, NumberComparer.equals) } }
	DateComparer.lessThan = function(y) { var that = this; return function(x) { return that.compareDate(x, y, NumberComparer.lessThan) } }
	DateComparer.lessThanOrEquals = function(y) { var that = this; return function(x) { return that.compareDate(x, y, NumberComparer.lessThanOrEquals) } }
	DateComparer.greaterThan = function(y) { var that = this; return function(x) { return that.compareDate(x, y, NumberComparer.greaterThan) } }
	DateComparer.greaterThanOrEquals = function(y) { var that = this; return function(x) { return that.compareDate(x, y, NumberComparer.greaterThanOrEquals) } }

	DateComparer.compareDate = function(x, y, comparer)
	{
		if (y.hasDate)
		{
			var d1 = new Date()
			d1.setUTCFullYear(y.getUTCFullYear(), y.getUTCMonth(), y.getUTCDate())

			var d2 = new Date()
			d2.setUTCFullYear(x.getUTCFullYear(), x.getUTCMonth(), x.getUTCDate())

			return comparer(d1)(d2)
		}

		if (y.hasMonth)
		{
			var d1 = new Date()
			d1.setUTCFullYear(y.getUTCFullYear(), y.getUTCMonth())

			var d2 = new Date()
			d2.setUTCFullYear(x.getUTCFullYear(), x.getUTCMonth())

			return comparer(d1)(d2)
		}

		if (y.hasYear)
		{
			var d1 = new Date()
			d1.setUTCFullYear(y.getUTCFullYear())

			var d2 = new Date()
			d2.setUTCFullYear(x.getUTCFullYear())

			return comparer(d1)(d2)
		}

		return false
	}
}
