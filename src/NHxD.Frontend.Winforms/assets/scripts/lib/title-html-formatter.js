{
	// TODO: redo this completely to manually parse tokens and interpret them instead of using a blanket regex.

	var titleFormatter = {}

	titleFormatter.titleRegex = /^(\(([^)]+)\)){0,1}\s*(\[([^\]]+)\]){0,1}\s*([^([]*)\s*(\(([^)]+)\)){0,1}\s*(\[([^\]]+)\]){0,1}\s*((?:\[|{)([^\]}]+)(?:\]|})){0,1}\s*(\[([^\]]+)\]){0,1}/gi
	titleFormatter.whitelistRegex = null
	titleFormatter.blacklistRegex = null
	titleFormatter.ignorelistRegex = null
	titleFormatter.hidelistRegex = null
	// with "japanese | english" title:
	//^(\(([^)]+)\)){0,1}\s*(\[([^\]]+)\]){0,1}\s*([^([$|]*)(?:\| ){0,1}\s*([^([$]*)\s*(\(([^)]+)\)){0,1}\s*(\[([^\]]+)\]){0,1}\s*((?:\[|{)([^\]}]+)(?:\]|})){0,1}\s*(\[([^\]]+)\]){0,1}

	document.addEventListener("whitelistchanged", function()
	{
		titleFormatter.whitelistRegex = null
	})

	document.addEventListener("blacklistchanged", function()
	{
		titleFormatter.blacklistRegex = null
	})

	document.addEventListener("ignorelistchanged", function()
	{
		titleFormatter.ignorelistRegex = null
	})

	document.addEventListener("hidelistchanged", function()
	{
		titleFormatter.hidelistRegex = null
	})

	titleFormatter.format = function(title, name)
	{
		// examples:
		// (C97)[Kirintei (Kirin Kakeru, Kouri)][P Dame ni Suru]Fuyuko no Renaishinan (THE iDOLM@STER: Shiny Colors) [Chinese] [無邪気漢化組]
		// (CCTokyo130)[Wata 120 Percent (Menyoujan)]Shining no Erohon | Shining Erotic Book (Shining Blade)[English]=TV= [Decensored]
		// (C90) [Imitation Moon (Narumi Yuu)] Oshiete Rem Sensei - Emilia-tan to Manabu Hajimete no SEX | Teach me, Rem-sensei! An introduction to sex with Emilia-tan (Re:Zero kara Hajimeru Isekai Seikatsu) [English] [EHCOVE]

		var buffer = title

		if (buffer.match(this.titleRegex))
		{
			// TODO: do a regex on each component to guess their correct type (because the order can be random, version may appear before language, etc.)
			//      so do a simple
			//      is it language? /english|japanese|chinese/gi
			//      is it version? /digital/
			//      is it status? /complete|incomplete/
			//      is it censored? /censored|uncensored|decensored/

			buffer = buffer.replace(this.titleRegex,
				"<span class='title-component title-component-comiket'>$1</span>"
				+ "<span class='title-component title-component-group'>$3</span>"
				+ "<span class='title-component title-component-title'>$5</span>"
				+ "<span class='title-component title-component-parody'>$6</span>"
				+ "<span class='title-component title-component-language'>$8</span>"
				+ "<span class='title-component title-component-scanlator'>$10</span>"
				+ "<span class='title-component title-component-version'>$12</span>"
				// or without delimiters:
				// but then, whitelist/blacklist can't parse [anthology] (i.e, just "anthology" would give false positives)
				/*
				"<span class='title-component title-component-comiket'>$2</span>"
				+ "<span class='title-component title-component-group'>$4</span>"
				+ "<span class='title-component title-component-title'>$5</span>"
				+ "<span class='title-component title-component-parody'>$7</span>"
				+ "<span class='title-component title-component-language'>$9</span>"
				+ "<span class='title-component title-component-scanlator'>$11</span>"
				+ "<span class='title-component title-component-version'>$13</span>"
				*/
				)
		}

		var whitelistFilter = whitelist.title[name].join('|')

		if (whitelistFilter.length > 0)
		{
			if (!this.whitelistRegex)
			{
				this.whitelistRegex = new RegExp("(" + whitelistFilter + ")", "gi")
			}

			buffer = buffer.replace(this.whitelistRegex, "<span class='tag-whitelist'>$1</span>")
		}

		var blacklistFilter = blacklist.title[name].join('|')

		if (blacklistFilter.length > 0)
		{
			if (!this.blacklistRegex)
			{
				this.blacklistRegex = new RegExp("(" + blacklistFilter + ")", "gi")
			}

			buffer = buffer.replace(this.blacklistRegex, "<span class='tag-blacklist'>$1</span>")
		}

		return buffer
	}
}
